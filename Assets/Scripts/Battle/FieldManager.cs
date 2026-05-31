using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;
using static UnityEngine.EventSystems.EventTrigger;

/// <summary>
/// 戦闘中のUI管理など
/// </summary>
public class FieldManager : MonoBehaviour,IItemManager,IBaseUIPage
{
    [SerializeField] private HandManager handManager;
    [SerializeField] private DeckDataInBattle deckData;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private PlayerBattleManager playerBattleManager;
    [SerializeField] private WindowManager windowManager;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private ConsumablesManager consumablesManager;
    [SerializeField] private GearManager gearManager;
    [SerializeField] private BattleMessage battleMessage;
    [SerializeField] private EnergyUI energyUI;
    [SerializeField] private EnemyCardZoneManager enemyCardZoneManager;
    [SerializeField] private InputBlocker inputBlocker;
    [SerializeField] private ParentManagerInDragging parentManagerInDragging;
    [SerializeField] private CardEffectExecute cardEffectExecute;
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private BattleDropsManager battleDropsManager;
    [SerializeField] private ItemArrangeManager itemArrangeManager;
    [SerializeField] private CardSpecialEffectManager cardSpecialEffectManager;

    [SerializeField] private RectTransform canvasRectTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Transform descriptionParent;//説明パネルをセットする親
    [SerializeField] private Transform popupMessageParent;//ポップアップメッセージをセットする親

    //右クリックで出る説明パネルのプレハブ
    [SerializeField] private GameObject descriptionPrefab;
    //ポップアップメッセージのプレハブ
    [SerializeField] private GameObject popupMessageWithTogglePrefab;

    private DescriptionPanel descriptionPanel=null;//インスタンス化したときにセット

    private ItemBase draggingItem;//ドラッグ中のアイテム
    //ドラッグ中重なっている効果対象可能なカードゾーン、枠が可視化されている。
    private EffectableCardZoneBase visualizedCardZone;

    private List<ICardZone> zones;//CardZone全体のリスト

    [SerializeField] private int turnDrawNum;//ターンごとに引くカードの数

    //カードを使用するときに消費するエネルギー(AP)とその最大値
    private int energy;
    private int energyMax;

    //バトル中の効果に影響するデータ
    [SerializeField] private DataPerCard dataPerCard;
    [SerializeField] private DataPerTurn dataPerTurn;
    [SerializeField] private DataPerBattle dataPerBattle;

    [SerializeField] UIPageManager uiPageManager;
    public UIPageManager UIPageManager { get { return uiPageManager; } }

    [SerializeField] private InventoryAndRewardUIPage inventoryAndRewardUIPage;

    [SerializeField] private ScenarioManager scenarioManager;

    /// <summary>
    /// 初期化処理。敵データの生成
    /// </summary>
    /// <param name="encountType">どのような種類のバトルか</param>
    public void Init(EncountTypeDefine.EnecountType encountType)
    {
        //データ初期化
        dataPerCard.ResetData();
        dataPerTurn.ResetData();
        dataPerBattle.ResetData();

        battleMessage.Init();
        inputBlocker.Init();

        itemArrangeManager.Init(this, mainCamera);

        //エネルギー初期化
        energyMax = BattleConstants.EnergyMax;
        energy = energyMax;
        energyUI.SetEnergy(energy,energyMax);//UI反映

        InstantiateDescriptions();

        
        handManager.Init();//HandManagerの初期化
        deckData.Init();//デッキ初期化
        gearManager.Init();//装備データ初期化、生成
        enemyManager.Init(encountType);//敵初期化、生成
        playerBattleManager.Init();//Playerバトルデータ初期化、生成
        weaponManager.Init();//武器データ初期化、生成
        consumablesManager.Init();//消耗品データ初期化、生成

        //シールドを0に初期化
        playerBattleManager.ResetPlayerShield();

        deckData.ShuffleCard();//デッキシャッフル

        TurnStart(true);//ターン開始

        visualizedCardZone = null;

        draggingItem = null;
    }

    public void Update()
    {
        //右クリックでアイテムドラッグをキャンセル
        if (Input.GetMouseButtonDown(1))
        {
            if (draggingItem != null)
            {
                if(visualizedCardZone is not null)
                {
                    visualizedCardZone.VisualizeCardZoneOff();
                    visualizedCardZone = null;//可視化しているゾーンは存在しない
                }

                draggingItem.ItemMover.EndDraggingForcibly();//ドラッグ終了
                parentManagerInDragging.ReturnToBaseParent();
                draggingItem.BackToBasePos();
                draggingItem = null;
            }
        }
    }

    #region ターン開始終了処理
    /// <summary>
    /// ターン終了ボタンが押された時。
    /// TODO:コストが残っているかの判定を行う
    /// </summary>
    public void OnTurnEndButtonPressed()
    {
        //盲目を持っているかの確認
        bool isHaveBlindness = false;
        SearchAndUsePlayerState<StateBlindness>(a => isHaveBlindness = true);

        if (SettingManager.IsShowCostRemainedPopup&&
            handManager.IsPlayableCardRemained(energy, isHaveBlindness))
        {//使用可能なカードが残っているなら,かつポップアップを表示する設定なら
            //残っているのポップアップを表示する

            GameObject popupMessageObj = Instantiate(popupMessageWithTogglePrefab, popupMessageParent.position, Quaternion.identity, popupMessageParent);
            PopupMessageWithToggleUI messageUI = popupMessageObj.GetComponent<PopupMessageWithToggleUI>();

            //挙動を定義するクラスを生成してセット
            var costRemained = new CostRemainedPopupController(this, messageUI, inputBlocker);

            //UIの初期化
            messageUI.Init(costRemained);
        }
        else
        {//使用可能なカードが残っていないなら
            //ターン終了
            TurnEnd();
        }
    }


    /// <summary>
    /// ターンエンド時の処理
    /// 敵が行動する以前の処理を行う
    /// 敵の行動を行う
    /// TurnEnd()=>TurnEndAfterEnemiesAction()の順
    /// </summary>
    public void TurnEnd()
    {
        //敵のシールドリセット
        enemyManager.ResetEnemiesShield();

        Sequence sequence = DOTween.Sequence();//演出は全てこの上

        //ターン終了時バフデバフ効果発生
        //自身のもの
        playerBattleManager.SearchAndUsePlayerState<IStatePlayerTurnEnd>(a =>
        {
            a.TurnEndEffect(cardEffectExecute, sequence, playerBattleManager.GetPlayer());
        });
        if (playerBattleManager.GetPlayer().GetHp() <= 0)
        {//Hpが0以下なら
            DefeatBattle();
        }

        inputBlocker.InputBlockingUp();//敵の行動中はプレイヤーの入力を受け付けないようにする

        //敵の行動
        enemyManager.ExecuteEnemiesAction(sequence);

        //ターン終了時バフデバフ効果発生
        //敵のもの
        Enemy[] enemies = enemyManager.GetEnemies();
        foreach (Enemy enemy in enemies)
        {
            if (enemy is null) continue;
            enemy.SearchAndUseState<IStateEnemyTurnEnd>(a =>
            {
                a.TurnEndEffect(cardEffectExecute, sequence, enemy);
            });
        }

        sequence.AppendCallback(() =>
        {
            inputBlocker.InputBlockingDown();//敵の行動が終わったらプレイヤーの入力を受け付けるようにする

            Sequence checkDeadSequence = DOTween.Sequence();

            foreach (Enemy enemy in enemies)
            {
                if (enemy is null) continue;
                enemy.CheckDead(checkDeadSequence);//敵の死亡チェック
            }
        });
    }

    /// <summary>
    /// ターンエンド時処理
    /// 敵の行動が終了するとコールバックで呼ばれる
    /// 手札を捨てる
    /// バフ、デバフの清算も行う
    /// </summary>
    public void TurnEndAfterEnemiesAction()
    {
        //敵の行動で発生したエフェクトをすべて削除演出する
        DestroyPlayerEffect();
        DestroyEnemyEffect();

        //プレイヤーのシールドリセット
        playerBattleManager.ResetPlayerShield();

        //状態異常のスタックを、減るべきものは減らす。
        playerBattleManager.DecreaseState();
        enemyManager.DecreaseState();

        playerBattleManager.RefreshWeapon();//武器リフレッシュ

        //手札をすべて捨てる
        handManager.TrashCardAll();

        //TODO:次のターンを開始する条件を判定

        //バトル中に発生したメッセージを初期化
        battleMessage.ClearAllMessage();

        //ターンごとのデータをリセット
        dataPerTurn.ResetData();

        TurnStart(false);
    }

    /// <summary>
    /// ターン開始時処理
    /// 手札を引き直し、バフデバフの清算
    /// </summary>
    /// <param name="isFirstTurn">最初のターンであるか</param>
    private void TurnStart(bool isFirstTurn)
    {
        //エネルギー回復
        energy = energyMax;
        energyUI.SetEnergy(energy, energyMax);

        //敵の行動を更新
        enemyManager.UpdateEnemiesNextAction(isFirstTurn);

        Sequence sequence = DOTween.Sequence();//演出は全てこの上

        //ターン開始時状態異常発動
        playerBattleManager.SearchAndUsePlayerState<IStatePlayerTurnStart>(a =>
        {
            a.TurnStartEffect(playerBattleManager.GetPlayer());
        });
        //ターン開始時バフデバフ効果発生
        //敵のもの
        Enemy[] enemies = enemyManager.GetEnemies();
        foreach (Enemy enemy in enemies)
        {
            if (enemy is null) continue;
            enemy.SearchAndUseState<IStateEnemyTurnStart>(a =>
            {
                a.TurnStartEffect(cardEffectExecute, sequence, enemy);
            });
        }

        //ターン開始時演出が終わったら
        sequence.AppendCallback(() =>
        {
            //ターンごとのカウントを持つ状態異常のカウントをリセットする
            playerBattleManager.ResetIStateCountPerTurn();
            enemyManager.ResetIStateCountPerTurn();

            //TODO:ターン開始で死亡したかの判定

            //プレイヤーのターン開始時処理
            //溜めた処理をInvokeする
            playerBattleManager.OnTurnStart(() =>
            {
                //行動で発生したエフェクトをすべて削除演出する
                DestroyPlayerEffect();
                DestroyEnemyEffect();
            });

            CardDrowByDrawNum();//カードを引く

            if (isFirstTurn)
            {
                enemyManager.InitialEffectExecute();//敵の戦闘開始時行動を実行
            }
        });
    }

    /// <summary>
    /// プレイヤーに発生しているエフェクト全てに削除演出を行う
    /// </summary>
    private void DestroyPlayerEffect()
    {
        playerBattleManager.DestroyEffects();
    }

    /// <summary>
    /// Enemyに発生しているエフェクト全てに削除演出を行う
    /// </summary>
    private void DestroyEnemyEffect()
    {
        enemyManager.DestroyEffects();
    }
    #endregion

    #region カード武器ドラッグ処理

    public void StartDragging(ItemBase item)
    {
        draggingItem = item;
        parentManagerInDragging.SetParent(item);
    }
    
    /// <summary>
    /// ドラッグ終了時処理
    /// </summary>
    public bool EndDragging()
    {
        parentManagerInDragging.ReturnToBaseParent();
        if (draggingItem == null)
        {//ドラッグしていないなら終了
            return false;
        }
        else if (!IsPlayable(draggingItem))
        {// プレイできないカードならメッセージを表示
            battleMessage.PlayerMessage("APが足りない！");
        }

        if (visualizedCardZone != null&&draggingItem!=null&&draggingItem is Card)
        {//可視化されているCardZoneがあるなら,カードをドラッグしているなら
            Card draggingCard = (Card)draggingItem;

            draggingCard.SetExecuting(true);//カードを実行中状態へ

            dataPerTurn.SetUsedCard(draggingCard);//使用したカードを記録

            Sequence sequence= DOTween.Sequence();//ダメージ効果は全てこの上に乗る

            //カード特殊演出開始
            cardSpecialEffectManager.PlayCardSpecialEffect(draggingCard.specialEffect);

            //カードのコストを支払う
            //エネルギー消費効果を実装するため、効果発生より前に消費
            energy -= draggingCard.actualCost;
            energyUI.SetEnergy(energy, energyMax);//UI反映

            //カードセット効果発生
            if (draggingCard.actualEffectTarget.effectTarget == TargetDefine.EffectTarget.EnemyAll)
            {//全体攻撃のカードなら
                enemyCardZoneManager.SetAllEnemyCardZone(draggingCard,sequence);//敵全体に効果
                enemyCardZoneManager.VisualizeAllEnemyCardZone(false);//全てのカードゾーンをオフ
                visualizedCardZone = null;//可視化しているゾーンは存在しない
            }
            else
            {//単体のカードなら
             //可視化されているカードゾーンが効果対象なので、対象のカードゾーンの効果を発生させる
                visualizedCardZone.Set(draggingCard, sequence);
                visualizedCardZone.VisualizeCardZoneOff();
                visualizedCardZone = null;//可視化しているゾーンは存在しない
            }

            //カード使用後効果の発生
            playerBattleManager.SearchAndUsePlayerState<IStateAfterUsingCard>(
                a=>a.AfterUsingCard(cardEffectExecute,sequence,draggingCard,playerBattleManager.GetPlayer())
            );

            if (draggingCard.usingStateNew.Count > 0)
            {
                //Cardの使用した状態異常を参照して消費。情報はタプルで取得している
                foreach ((IState, int) stateTouple in draggingCard.usingStateNew)
                {
                    ConsumePlayerState(stateTouple.Item1, stateTouple.Item2);
                }
            }

            if (draggingCard.IsItemHasTag(CardTagDefine.CardTag.Once)||
                draggingCard.IsItemHasTag(CardTagDefine.CardTag.Temporary))
            {//"一回","一時的"のタグを持っているカードは1度使うと廃棄される
                handManager.DiscardCard(draggingCard.pos);
            }
            else
            {
                handManager.TrashCard(draggingCard.pos);//カードを捨て札に
            }

            //ドラッグしているカードは存在しない
            draggingItem = null;

            //エフェクト演出が終わるまで待つ。その後、エフェクトを消す。
            sequence.AppendInterval(BattleConstants.EffectCardDisappearWait).
                AppendCallback(() =>
                {
                    DestroyPlayerEffect();
                    DestroyEnemyEffect();
                    //カード効果中に蓄積されるデータをリセットする
                    dataPerCard.ResetData();
                });
        }
        else if (visualizedCardZone != null && draggingItem != null && draggingItem is Weapon)
        {//可視化されているCardZoneがあるなら,武器をドラッグしているなら
            Sequence sequence = DOTween.Sequence();//ダメージ効果は全てこの上に乗る
            Weapon draggingWeapon = (Weapon)draggingItem;

            draggingWeapon.SetExecuting(true);

            //武器のコストを支払う
            energy -= draggingWeapon.actualCost;
            energyUI.SetEnergy(energy, energyMax);//UI反映

            if (draggingWeapon.IsItemHasTag(CardTagDefine.CardTag.EnemyAll))
            {//敵全体が対象
                enemyCardZoneManager.SetAllEnemyCardZone(draggingWeapon,sequence);//敵全体に効果
                enemyCardZoneManager.VisualizeAllEnemyCardZone(false);//全てのカードゾーンをオフ
                visualizedCardZone = null;//可視化しているゾーンは存在しない
            }
            else
            {
                //可視化されているカードゾーンが効果対象なので、対象のカードゾーンの効果を発生させる
                visualizedCardZone.Set(draggingWeapon, sequence);
                visualizedCardZone.VisualizeCardZoneOff();
                visualizedCardZone = null;//可視化しているゾーンは存在しない
            }

            //武器使用時効果発生
            playerBattleManager.SearchAndUsePlayerState<IStateAfterUsingWeapon>(
                a => a.AfterUsingWeapon(cardEffectExecute, sequence, draggingWeapon, playerBattleManager.GetPlayer())
            );

            //使用した状態異常を消費
            if (draggingWeapon.usingStateNew.Count > 0)
            {
                //武器の使用した状態異常を参照して消費。情報はタプルで取得している
                foreach ((IState, int) stateTouple in draggingWeapon.usingStateNew)
                {
                    ConsumePlayerState(stateTouple.Item1, stateTouple.Item2);
                }
            }

            draggingWeapon.BackToBasePos();//武器をもとの位置に戻す

            draggingWeapon.SetExecuting(false);

            //ドラッグしているアイテムは存在しない
            draggingItem = null;

            //エフェクト演出が終わるまで待つ。その後、エフェクトを消す。
            sequence.AppendInterval(BattleConstants.EffectCardDisappearWait).
                AppendCallback(() =>
                {
                    DestroyPlayerEffect();
                    DestroyEnemyEffect();

                    draggingWeapon.RefreshWeapon();//ドラッグしている武器の状態更新
                });
        }
        else if(visualizedCardZone != null && draggingItem != null && draggingItem is Consumable)
        {
            Sequence sequence = DOTween.Sequence();//ダメージ効果は全てこの上に乗る
            Consumable draggingConsumable = (Consumable)draggingItem;

            //コストを支払う
            energy -= draggingConsumable.Cost;
            energyUI.SetEnergy(energy, energyMax);//UI反映

            //可視化されているカードゾーンが効果対象なので、対象のカードゾーンの効果を発生させる
            visualizedCardZone.Set(draggingConsumable, sequence);
            visualizedCardZone.VisualizeCardZoneOff();
            visualizedCardZone = null;//可視化しているゾーンは存在しない

            consumablesManager.UseConsumable(draggingConsumable);//消耗品を使用

            draggingItem = null;

            //エフェクト演出が終わるまで待つ。その後、エフェクトを消す。
            sequence.AppendInterval(BattleConstants.EffectCardDisappearWait).
                AppendCallback(() =>
                {
                    DestroyPlayerEffect();
                    DestroyEnemyEffect();
                });
        }

        //接触判定で何も起こらなければ元の場所に戻る
        //カード効果が発生したときはDraggingCardが消えている
        if (draggingItem != null)
        {
            draggingItem.BackToBasePos();
        }

        draggingItem = null;

        //カード効果リセット
        handManager.RefreshCard();

        return true;
    }

    /// <summary>
    /// アイテムのドラッグ時処理
    /// </summary>
    public void UpdateItemDragging()
    {
        EffectableCardZoneBase targetCardZone = GetOverlappedEffectableCardZone();//重なっているカードゾーン

        if (!IsPlayable(draggingItem)) 
        {
            return; 
        }

        if (targetCardZone != null && targetCardZone.isEffectableToThis(draggingItem))
        {//対象のカードゾーンがあるなら (マウスが重なっていて、カード効果発動可能)
            if ((draggingItem is Card c && c.actualEffectTarget.effectTarget == TargetDefine.EffectTarget.EnemyAll)||
                (draggingItem is Weapon w && w.effectTarget.effectTarget == TargetDefine.EffectTarget.EnemyAll))
            {//全体攻撃のカード等なら
                enemyCardZoneManager.VisualizeAllEnemyCardZone(true);//全ての敵カードゾーンを可視化
                visualizedCardZone = targetCardZone;
            }
            else
            {
                if (visualizedCardZone == null)
                {//最初の一つの可視化カードゾーンは光らせるだけ
                    targetCardZone.VisualizeCardZoneOn();
                    visualizedCardZone = targetCardZone;
                }
                else if (!targetCardZone.visualizeFlag)
                {//対象のカードゾーンが可視化されていないなら
                 //可視化するカードゾーンを切り替え、visualizedCardZoneを次のカードゾーンへ
                    visualizedCardZone.VisualizeCardZoneOff();
                    targetCardZone.VisualizeCardZoneOn();

                    visualizedCardZone = targetCardZone;
                }
            }
        }
        else
        {//対象のカードゾーンにマウスが重なっていないなら
            if (visualizedCardZone != null)
            {//前に可視化していたら、オフ
                enemyCardZoneManager.VisualizeAllEnemyCardZone(false);//全ての敵カードゾーンをオフ
                visualizedCardZone.VisualizeCardZoneOff();
                visualizedCardZone = null;
            }
        }
    }
    
    /// <summary>
    /// draggingCardがEffectableCardZoneBaseに接触していたなら、最初のそれを返す
    /// そうでないなら、Nullを返す
    /// </summary>
    private EffectableCardZoneBase GetOverlappedEffectableCardZone()
    {
        // 重なっているオブジェクトの情報を全て取得する
        // (判定が必要なオブジェクトには全てBoxCollider2Dが付与されているのでそれを利用して判定)

        //マウスのスクリーン座標を取得する
        // マウス位置を取得
        Vector2 tapPos = Input.mousePosition;
        Vector3 mouseScreenPoint = new Vector3(tapPos.x, tapPos.y, 10);

        // メインカメラから上記で取得した座標に向けてRayを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPoint);

        EffectableCardZoneBase targetZone = null;
        foreach (RaycastHit2D hit in Physics2D.RaycastAll(ray.origin, ray.direction, 10.0f))
        {
            if (!hit.collider)
            {//コライダーのないオブジェクトなら
                continue;
            }

            // 当たったオブジェクトがドラッグ中のカードと同一なら次へ
            // 当たったオブジェクトがドラッグ中の武器と同一なら次へ
            var hitObj = hit.collider.gameObject;
            if (draggingItem!=null&&hitObj == draggingItem.gameObject)
            {
                continue;
            }

            // オブジェクトがカードエリアなら、次へ
            var hitZone = hitObj.GetComponent<EffectableCardZoneBase>();
            if (hitZone != null)
            {
                targetZone = hitZone;
                break;
            }
        }

        return targetZone;
    }

    public void QuickMove(ItemBase item)
    {
        //バトル画面でのShift＋Clickは動作しない
    }

    /// <summary>
    /// プレイヤーの状態異常を使用した際に値を減少させる
    /// </summary>
    /// <param name="state">減少させるState</param>
    /// <param name="value">減少させる量</param>
    public void ConsumePlayerState(IState state ,int value)
    {
        if (state is IStateHasCount  cState)
        {//カウントが減少するタイプのStateなら
            //カウントを減少させる
            playerBattleManager.DecreaseCountOfCountState(cState);
        }
        else
        {
            if (state is StateContinueTypeEternalBase)
            {
                //永続的な状態異常なら
                //消費する場合、即座に削除する
                playerBattleManager.DeleteState(state);
            }
            else
            {
                //IStateの値を減らす
                playerBattleManager.ConsumeState(state, value);
            }
        }
    }

    #endregion

    #region バトル終了、開始処理

    /// <summary>
    /// 敵が全ていなくなり、敵に勝利した場合の処理
    /// </summary>
    public void WinToEnemy()
    {
        //ドロップカード取得
        List<int> droppedCards = battleDropsManager.GetDroppedCards(dataPerBattle.DefeatedEnemyTier, MapTileDefine.MapTile.Enemy);
        //ドロップアイテムを取得
        List<PhysicalItemDataSO> droppedItems = battleDropsManager.GetDroppedItems(dataPerBattle.DefeatedEnemyTier, MapTileDefine.MapTile.Enemy);

        //敵を倒したことを記録
        QuestLineData.MemoryDefeatedEnemy(dataPerBattle.DefeatedEnemy);

        //ドロップメニューが閉じた後はEndBattleでバトル終了処理。
        //ドロップメニューに必要なドロップ品を渡している。
        inventoryAndRewardUIPage.Init(droppedCards, droppedItems, StashPanel.DropFrameType.Monster, () => {
            EndBattle();
        } );

        inventoryAndRewardUIPage.PushSelf();
    }

    /// <summary>
    /// プレイヤーのHPが0になり、敗北した場合の処理
    /// </summary>
    public void DefeatBattle()
    {
        inputBlocker.InputBlockingUp();//入力を受け付けなくする
        //敗北メッセージを表示
        windowManager.DefeatMessage();
    }

    /// <summary>
    /// バトル終了ウィンドウを閉じるときに、バトル中に生成したものを削除する
    /// </summary>
    public void EndBattle()
    {
        //inputBlocker.InputBlockingDown();//入力を受け付け(UIPageManagerが行うため削除)

        handManager.TrashCardAll();//手札をすべて削除
        enemyManager.DeleteEnemies();//Enemyをすべて削除
        //playerのオブジェクト、Stateを削除。終了指示
        playerBattleManager.EndBattle();
        weaponManager.DeleteWeapons();//weaponをすべて削除
        consumablesManager.EndBattle();//消耗品をすべて削除,消耗品データをインベントリに戻す

        dungeonManager.EndBattle();
    }

    #endregion


    #region プレイヤーに対しての処理

    /// <summary>
    /// プレイヤーにダメージを与える
    /// 敵からのダメージはEnemyManagerから渡されてくる
    /// </summary>
    /// <param name="value">ダメージの値</param>
    public void DamageToPlayer(int value)
    {
        playerBattleManager.DamageToPlayer(value);
    }

    /// <summary>
    /// プレイヤーに状態異常を与える
    /// </summary>
    /// <param name="state">状態異常のIState</param>
    public void StateToPlayer(IState state)
    {
        playerBattleManager.StateToPlayer(state);
    }

    /// <summary>
    /// Playerのインスタンスを取得
    /// </summary>
    /// <returns>現在のPlayer</returns>
    public Player GetPlayer()
    {
        return playerBattleManager.GetPlayer();
    }

    /// <summary>
    /// プレイヤーの所有する状態異常のリストからTに該当するものを検索し、
    /// デリゲートで指示したものを実行させる
    /// </summary>
    /// <typeparam name="T">検索する状態異常のタイプ</typeparam>
    /// <param name="useAction">a => a.UseA()のように、Tのメソッドを使用する指示</param>
    public void SearchAndUsePlayerState<T>(Action<T> useAction) where T : class, IState
    {
        playerBattleManager.SearchAndUsePlayerState<T>(useAction);
    }

    #endregion


    #region Description関連

    /// <summary>
    /// FieldManagerから呼び出されるカードなどの説明文をインスタンス化する
    /// 既に存在していた場合は何もしない
    /// </summary>
    private void InstantiateDescriptions()
    {
        if(descriptionPanel == null)
        {//カード説明のインスタンス化
            GameObject cardDesObj = Instantiate(descriptionPrefab, descriptionParent.position, Quaternion.identity, descriptionParent);
            descriptionPanel = cardDesObj.GetComponent<DescriptionPanel>();
            descriptionPanel.Init(this);
        }
    }

    /// <summary>
    /// カードの詳細情報を表示する画面を出す。
    /// </summary>
    /// <param name="serialNum">カードのシリアル番号</param>
    public void ShowDescription(int serialNum)
    {
        inputBlocker.InputBlockingUp();//バトルに触れないようにする
        descriptionPanel.OpenPanel(serialNum);
    }

    public void ShowDescription(PhysicalItemDataSO pItemData)
    {
        inputBlocker.InputBlockingUp();//バトルに触れないようにする
        descriptionPanel.OpenPanel(pItemData);
    }

    /// <summary>
    /// カードの詳細情報を表示する画面を閉じる
    /// パネルから呼出
    /// </summary>
    public void CloseCardDescription()
    {
        inputBlocker.InputBlockingDown();//UIブロック解除
    }
    #endregion


    /// <summary>
    /// カードを初期手札の枚数だけ引く
    /// </summary>
    private void CardDrowByDrawNum()
    {
        handManager.CardsDraw(turnDrawNum);
        //TODO:装備やパークによって手札量増減
    }

    /// <summary>
    /// 渡されたItemが現在使用可能かを判定する
    /// </summary>
    /// <param name="item">アイテム</param>
    /// <returns>使用可能なアイテムか</returns>
    public bool IsPlayable(ItemBase item)
    {
        if (item is Card)
        {
            if (((Card)item).actualCost > energy)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if (item is Weapon)
        {
            if (((Weapon)item).actualCost > energy)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        else if (item is Consumable)
        {
            if (((Consumable)item).Cost > energy)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        return false;
    }


    /// <summary>
    /// カードUI情報を再読み込みする
    /// </summary>
    public void RefreshCard()
    {
        handManager.RefreshCard();
    }

    /// <summary>
    /// 武器UI情報を再読み込みする
    /// </summary>
    public void RefreshWeapon()
    {
        weaponManager.RefreshWeapon();
    }

    public bool UseEnergy(int value)
    {
        if (energy >= value)
        {//エネルギーが足りているなら
            energy -= value;
            energyUI.SetEnergy(energy, energyMax);//UI反映
            return true;
        }
        else
        {//足りないなら
            return false;
        }
    }

    /// <summary>
    /// エネルギーを回復
    /// </summary>
    /// <param name="value">回復量</param>
    public void GainEnergy(int value)
    {
        energy += value;
        energyUI.SetEnergy(energy, energyMax);//UI反映
    }

    public int GEtEnergy()
    {
        return energy;
    }

    public ScenarioManager GetScenarioManager()
    {
        return scenarioManager;
    }


    #region UIPageManagerの実装

    public void OnPushed()
    {

    }

    public void OnPopped()
    {
        //popされない前提なので、実装しない
    }

    public void OnCovered()
    {
        inputBlocker.InputBlockingUp();
    }

    public void OnBecomeTopPage()
    {
        inputBlocker.InputBlockingDown();
    }

    public void PushSelf()
    {
        //pushされない前提なので、実装しない
    }

    #endregion

    #region ForTest
    //テスト用。再ドローする
    public void TestReDraw()
    {
        handManager.TrashCardAll();//カードを捨てる
        CardDrowByDrawNum();//カードを引く
    }

    /// <summary>
    /// バトルに即座に勝利する
    /// アイテムドロップ画面を開く
    /// </summary>
    public void TestWinBattle()
    {
        enemyManager.TestDestroyAllEnemy();
        /*
        List<int> droppedCards = battleDropsManager.GetDroppedCards(dataPerBattle.DefeatedEnemy,MapTileDefine.MapTile.Enemy);

        foreach (var card in droppedCards)
        {
            Debug.Log("ドロップカードID: " + card);
        }

        inputBlocker.InputBlockingUp();//入力を受け付けなくする
        itemArrangeManager.OpenItemDropInventory(droppedCards);
        */
    }
    #endregion
}
