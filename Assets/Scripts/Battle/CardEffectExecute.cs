using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Assertions.Must;
using System;
using static UnityEngine.EventSystems.EventTrigger;
using UnityEngine.TextCore.LowLevel;

/// <summary>
/// カード効果の発生を処理するクラス
/// </summary>
public class CardEffectExecute : MonoBehaviour
{
    //敵に対して発生するカードを使用した場合発生しうるカード効果は
    //敵に対して発生するもの＋自分に対して発生するもの
    //なので敵に対してを計算した後、自分に対してのものも計算する

    [SerializeField] PlayerBattleManager playerBattleManager;
    [SerializeField] HandManager handManager;
    [SerializeField] EnemyManager enemyManager;
    [SerializeField] FieldManager fieldManager;

    [SerializeField] CardDataSO shadowBoxingSO;//シャドーボクシングのカード効果のため保持
    [SerializeField] CardDataSO dragonShieldSO;//竜鱗守護のカード
    [SerializeField] CardDataSO fireNailSO;//火炎爪のカード
    [SerializeField] CardDataSO mysticKnifeSO;//幻想ナイフのカード

    //バトル中の効果に影響するデータ
    [SerializeField] private DataPerCard dataPerCard;
    [SerializeField] private DataPerTurn dataPerTurn;
    [SerializeField] private DataPerBattle dataPerBattle;

    /// <summary>
    /// 判定を持つカード効果はSequenceに乗せることなく
    /// 判定を行う。そのため、Execute先頭で実行する。
    /// </summary>
    /// <param name="effect">処理対象の効果</param>
    /// <param name="attributes">カードの属性(エフェクトに使用)</param>
    /// <param name="cardPos">使用されたカードの手札の中の位置</param>
    /// <param name="cardPos">エフェクトのSequence</param>
    /// <param name="enemy">敵</param>
    /// <returns>判定を行うカード効果の場合、判定に失敗したらfalse</returns>
    private bool ExecuteDecision(ActualEffect effect, List<AttributeDefine> attributes, Sequence sequence, int cardPos, Enemy enemy = null)
    {
        int value = effect.actualEffectValue;

        //IState useState = null;//カード効果が使用する状態異常

        bool branchFlag = false;//判定効果で分岐が発生したかのフラグ

        //敵に対する効果
        if (enemy is not null)
        {
            switch (effect.effect.cardEffect)
            {
                case CardEffectDefine.CardEffect.RandomXDamage3Times://ランダムな敵に3回ダメージを与える
                    for (int i = 0; i < 3; i++)
                    {
                        Enemy targetEnemy = enemyManager.GetRandomEnemy();
                        if (targetEnemy == null) return true;//敵がもういないなら終了
                                                             //通常ダメージを3回発生させる
                        Execute(new ActualEffect(new CardEffectDefine(CardEffectDefine.CardEffect.Damage, value), value, null), targetEnemy, attributes, sequence, cardPos);
                        sequence.AppendCallback(() =>
                        {
                            targetEnemy.CheckDead(sequence);//敵が死んでいるかを確認して、死んでいたなら撃破時演出
                        });
                    }
                    return true;//エフェクトを発生させない
                case CardEffectDefine.CardEffect.IsLongRangeEnemy:
                    //遠距離の敵ならtrue
                    if (enemy.GetPos() == 2)
                    {
                        //遠距離の敵ならtrueを返す
                        return true;
                    }
                    else
                    {//遠距離の敵ではないのでfalseを返す
                        return false;
                    }
                case CardEffectDefine.CardEffect.IsShortRangeEnemy:
                    //近距離の敵ならtrue
                    if (enemy.GetPos() == 0)
                    {
                        //近距離の敵ならtrueを返す
                        return true;
                    }
                    else
                    {//近距離の敵ではないのでfalseを返す
                        return false;
                    }
            }
        }

        switch (effect.effect.cardEffect)
        {
            //判定効果(演出は行わないのでreturnする)
            case CardEffectDefine.CardEffect.UseStateConstant:
                //十分な量のStateを持っていなければfalseを返す
                branchFlag = false;
                SearchAndUsePlayerState<IStateConsumable>(a =>
                {
                    if (a.GetType() == effect.UseState.GetType())
                    {//同じタイプの状態異常を見つけたら
                        branchFlag = a.IsHaveEnoughState(value);//値が十分にあるかを判定
                    }
                }, StateCopy.CopyInstanceAndSetState(effect.UseState,value));
                return branchFlag;
            case CardEffectDefine.CardEffect.UseStateEternal:
                //十分な量のStateを持っていなければfalseを返す
                branchFlag = false;
                SearchAndUsePlayerState<IStateConsumable>(a =>
                {
                    if (a.GetType() == effect.UseState.GetType())
                    {//同じタイプの状態異常を見つけたら
                        branchFlag = a.IsHaveEnoughState(value);//値が十分にあるかを判定
                    }
                }, StateCopy.CopyInstanceAndSetState(effect.UseState, value));
                return branchFlag;
            case CardEffectDefine.CardEffect.UseAllPlay:
                //StatePlayを持っていなければfalseを返す
                branchFlag = false;
                int consumedValue = 0;
                SearchAndUsePlayerState<StatePlay>(a =>
                {
                    consumedValue = a.value;//消費する値を記録
                });
                SearchAndUsePlayerState<StatePlay>(a =>
                {
                    if (a.value > 0)
                    {//同じタイプの状態異常を見つけたら
                        branchFlag = true;
                    }
                }, StateCopy.CopyInstanceAndSetState(new StatePlay(), consumedValue));
                return branchFlag;
            case CardEffectDefine.CardEffect.IsShuffleNum:
                if (dataPerBattle.ShuffleNum >= value)
                {//十分な回数シャッフルしているなら
                    return true;
                }
                else
                {
                    return false;
                }
            case CardEffectDefine.CardEffect.UseEnergy:
                //エネルギーを値分消費して、消費できたならtrue
                if (fieldManager.UseEnergy(value))
                {
                    return true;
                }
                else
                {
                    return false;
                }

        }

        return true;
    }

    /// <summary>
    /// 敵に対して
    /// 効果を持つカード効果の実行部分。Sequence上で実行することでカード効果演出とタイミングを合わせたい
    /// Effectもここで発生させる
    /// </summary>
    /// <param name="effect">処理対象の効果</param>
    /// <param name="attributes">カードの属性(エフェクトに使用)</param>
    /// <param name="cardPos">使用されたカードの手札の中の位置</param>
    /// <param name="enemy">敵</param>
    private void EnemyExecuteAction(ActualEffect effect, List<AttributeDefine> attributes, Sequence sequence, int cardPos, Enemy enemy)
    {
        int value = effect.actualEffectValue;//効果の値

        bool avoidedFlag = false;//回避が発生したかのフラグ。演出に変化アリ

        bool enemyEffectFlag = true;//敵に対する演出を実行するかのフラグ

        //敵に対する効果
        if (enemy is not null)
        {
            switch (effect.effect.cardEffect)
            {
                case CardEffectDefine.CardEffect.Damage://単純なダメージ
                case CardEffectDefine.CardEffect.PureDamage://防御無視ダメージ
                    //回避を持っているならダメージが無効化され、回避エフェクト
                    //などの、ダメージ演出を置き換えるものを調べる
                    SpecialAttackEffectDefine.SpecialEffect se = SpecialAttackEffectDefine.SpecialEffect.NoChange;
                    enemy.SearchAndUseState<IStateEnemyWhenAttacked>(a => {
                        se = a.ChangeEnemyAttackedOperation(se,enemy);
                    });

                    switch (se)
                    {
                        case SpecialAttackEffectDefine.SpecialEffect.NoChange:
                            //ダメージ量増減計算
                            enemy.SearchAndUseState<IStateEnemyDamagedValue>(a => {
                                value = a.ChangeDamagedValue(enemy, value,attributes);
                            });

                            if (effect.effect.cardEffect == CardEffectDefine.CardEffect.Damage)
                            {//通常ダメージ
                                enemy.Damage(value,true,sequence);
                            }
                            else if(effect.effect.cardEffect == CardEffectDefine.CardEffect.PureDamage)
                            {//防御無視ダメージ
                             //防御力を無視してダメージを与える
                                enemy.Damage(value,false,sequence);
                            }
                            break;
                        case SpecialAttackEffectDefine.SpecialEffect.Avoid:
                            //ダメージが回避されているのでダメージなし
                            avoidedFlag = true;
                            break;
                    }
                    break;
                case CardEffectDefine.CardEffect.CauseState://敵に状態を付与
                                                            //カード効果の使用する状態効果と同じ効果をインスタンス化して付与
                    enemy.GetState(StateCopy.CopyInstanceAndSetState(effect.UseState, value));
                    break;
                case CardEffectDefine.CardEffect.BlockToEnemy://敵にシールドを付与
                    enemy.GetShield(value); 
                    break;
                default:
                    enemyEffectFlag = false;//敵に対する演出を行わない
                    PlayerExecuteAction(effect, attributes, sequence, cardPos);
                    break;
            }
        }

        if (enemyEffectFlag)
        {//敵に対する演出効果が発生しているなら
            //Enemyへのエフェクトを発生させる(Playerに発生するものは既に終了している)
            CardEffectDefine.CardEffectType effectType = CardEffectDefine.CardEffectType.Error;
            //エフェクト種類の選択。回避が発生したなら専用のエフェクト
            if (avoidedFlag)
            {
                effectType = CardEffectDefine.CardEffectType.Avoided;
            }
            else
            {//回避が発生していないのでカード効果に対応したエフェクト
                effectType = CardEffectDefine.GetCardEffectType(effect.effect.cardEffect);
            }

            //エフェクトの発生
            if (enemy == null) return;//敵が途中で消えた場合に対応

            //最初の属性をもとにエフェクト発生
            enemy.Effect(effectType, attributes[0].attribute, value);
        }
    }

    private void PlayerExecuteAction(ActualEffect effect, List<AttributeDefine> attributes, Sequence sequence, int cardPos)
    {
        int value = effect.actualEffectValue;

        IState useState = null;//カード効果が使用する状態異常

        bool playerEffectFlag = true;//演出を実行するかのフラグ

        bool avoidedFlag = false;//回避が発生したかのフラグ。演出に変化アリ


        switch (effect.effect.cardEffect)
        {

            case CardEffectDefine.CardEffect.Block://シールドを付与
                playerBattleManager.ShieldToPlayer(value);
                break;

            case CardEffectDefine.CardEffect.SelfDamage://自分にダメージを与える
                
                //回避を持っているならダメージが無効化され、回避エフェクト
                //などの、ダメージ演出を置き換えるものを調べる
                SpecialAttackEffectDefine.SpecialEffect se = SpecialAttackEffectDefine.SpecialEffect.NoChange;
                SearchAndUsePlayerState<IStatePlayerWhenAttacked>(a => {
                    se = a.ChangePlayerAttackedEffect(se);
                    a.ChangePlayerAttackedOperation(playerBattleManager.GetPlayer(),value);
                });

                switch (se)
                {
                    case SpecialAttackEffectDefine.SpecialEffect.NoChange:
                        //ダメージ量増減計算
                        SearchAndUsePlayerState<IStatePlayerDamagedValue>(a =>
                        {
                            value = a.ChangeDamagedValue(playerBattleManager.GetPlayer(), value);
                        });
                        playerBattleManager.DamageToPlayer(value);
                        break;
                    case SpecialAttackEffectDefine.SpecialEffect.Avoid:
                        //ダメージが回避されているのでダメージなし
                        avoidedFlag = true;
                        break;
                }
                
                break;

            case CardEffectDefine.CardEffect.SelfHeal://自分を回復
                playerBattleManager.DamageToPlayer(-value);
                break;

            //プレイヤーバフデバフ系
            case CardEffectDefine.CardEffect.GetStateBuff://状態異常を付与
            case CardEffectDefine.CardEffect.GetStateDebuff:
                //カード効果の使用する状態効果と同じ効果をインスタンス化して付与
                playerBattleManager.StateToPlayer(StateCopy.CopyInstanceAndSetState(effect.UseState, value));
                break;
            case CardEffectDefine.CardEffect.GetTakan://多感を付与
                useState = new StateTakan();
                useState.value = value;//値を設定
                playerBattleManager.StateToPlayer(useState);
                break;
            case CardEffectDefine.CardEffect.GetBarricade://バリケードを付与
                useState = new StateBlockOnTurnStart();
                useState.value = value;//値を設定
                playerBattleManager.StateToPlayer(useState);
                break;

            //消費した祈りに応じて武器コスト減少を付与
            case CardEffectDefine.CardEffect.GetWeaponCostDownAccordingToConsumedPlay:
                useState = new StateWeaponCostDown();
                int count = 0;
                foreach (IState state in dataPerCard.ConsumedState)
                {
                    if (state is StatePlay)
                    {
                        count += state.value;
                    }
                }
                useState.value = count;
                playerBattleManager.StateToPlayer(useState);
                break;

            //手札操作系
            case CardEffectDefine.CardEffect.DisCard:
                handManager.TrashCardAll();//カードをすべて捨てる
                break;
            case CardEffectDefine.CardEffect.Draw:
                handManager.CardsDraw(value);//カードを引く
                break;
            case CardEffectDefine.CardEffect.DrawRandomCreateCard:
                handManager.DrawRandomCardByAttribute(value,AttributeDefine.Attribute.Create);
                break;
            case CardEffectDefine.CardEffect.FrameDraw:
                //カードを1枚引いて、それが火炎属性のカードならコスト0
                //カードを引く.1枚しか引いていないので、リストの先頭を取得
                Card drawedCard = handManager.CardsDraw(1)[0];
                if (drawedCard.IsItemHasAttribute(AttributeDefine.Attribute.Pyro))
                {//火炎属性のカードなら
                    //コスト0のタグを作成
                    CardTagDefine cardTagDefine = new CardTagDefine();
                    cardTagDefine.cardTag = CardTagDefine.CardTag.Cost0;
                    drawedCard.AddTag(cardTagDefine);
                }
                break;
            case CardEffectDefine.CardEffect.GetShadowBoxingInHand:
                //「シャドーボクシング」を手札に加える
                handManager.AddCardInHand(shadowBoxingSO);
                handManager.AlignCardZones();//手札のカードを整列させる
                handManager.CardsBackToBasePos();//手札のカードを元の位置に戻す
                break;
            case CardEffectDefine.CardEffect.GetDragonSet1:
                //「竜鱗守護」を手札に加える
                handManager.AddCardInHand(dragonShieldSO);
                handManager.AlignCardZones();//手札のカードを整列させる
                handManager.CardsBackToBasePos();//手札のカードを元の位置に戻す
                break;
            case CardEffectDefine.CardEffect.GetDragonSet2:
                //「竜鱗守護」、「火炎爪」を手札に加える
                handManager.AddCardInHand(dragonShieldSO);
                handManager.AddCardInHand(fireNailSO);
                handManager.AlignCardZones();//手札のカードを整列させる
                handManager.CardsBackToBasePos();//手札のカードを元の位置に戻す
                break;
            case CardEffectDefine.CardEffect.GetMysticKnifeInHandXPieces:
                //「幻想ナイフ」を手札に加える
                for(int i = 0; i < value; i++)
                {
                    handManager.AddCardInHand(mysticKnifeSO);
                }
                handManager.AlignCardZones();//手札のカードを整列させる
                handManager.CardsBackToBasePos();//手札のカードを元の位置に戻す
                break;
            case CardEffectDefine.CardEffect.Cost0DrawedCard://このターン中に引いたカードのコストを0
                List<Card> cards = dataPerCard.DrawedCards;
                foreach (Card card in cards)
                {
                    //コスト0のタグを作成
                    CardTagDefine cardTagDefine = new CardTagDefine();
                    cardTagDefine.cardTag = CardTagDefine.CardTag.Cost0;
                    card.AddTag(cardTagDefine);
                }
                break;
            case CardEffectDefine.CardEffect.EnbuTurn:
                fieldManager.GainEnergy(dataPerTurn.GetEnbuUsedCount()-1);//使用した演舞カードの数分エネルギーを獲得(自身はカウントしない)
                break;
            default:
                playerEffectFlag = false;//プレイヤーに対する演出を行わない
                break;
        }

        if (playerEffectFlag)
        {
            //Enemyへのエフェクトを発生させる(Playerに発生するものは既に終了している)
            CardEffectDefine.CardEffectType effectType = CardEffectDefine.CardEffectType.Error;
            //エフェクト種類の選択。回避が発生したなら専用のエフェクト
            if (avoidedFlag)
            {
                effectType = CardEffectDefine.CardEffectType.Avoided;
            }
            else
            {//回避が発生していないのでカード効果に対応したエフェクト
                effectType = CardEffectDefine.GetCardEffectType(effect.effect.cardEffect);
            }

            //Playerへのエフェクトを発生させる
            playerBattleManager.Effect(effectType, attributes[0].attribute, value);
        }
    }

    /// <summary>
    /// 敵に対して発生する様々な効果の処理
    /// </summary>
    /// <param name="effects">処理対象の効果</param>
    /// <param name="enemy">敵</param>
    /// <param name="attributes">カードの属性(エフェクトに使用)</param>
    /// <param name="sequence">演出効果が乗るSequence</param>
    /// <param name="cardPos">使用されたカードの手札の中の位置</param>
    /// <returns>判定を行うカード効果の場合、判定に失敗したらfalse</returns>
    public bool Execute(ActualEffect effect, Enemy enemy,List<AttributeDefine> attributes,Sequence sequence,int cardPos)
    {
        bool b = ExecuteDecision(effect, attributes, sequence, cardPos, enemy);
        if (!b) return false;//判定に失敗したら以降の処理は中断

        //演出は少しの間隔をあけて行われる
        //Playerに対しての効果はEnemyExecuteAction内で発生
        sequence.AppendCallback(() =>
        {
            EnemyExecuteAction(effect, attributes, sequence, cardPos, enemy);
        }).AppendInterval(BattleConstants.CardEffectInterval);

        //ActualEffectがダメージ系効果であった場合、敵の攻撃されたとき効果を発動
        if(CardEffectDefine.GetCardEffectType(effect.effect.cardEffect) == CardEffectDefine.CardEffectType.Damage)
        {
            enemy.SearchAndUseState<IStateEnemyDamaged>(a =>
            {
                a.OnDamagedEffect(this, sequence, enemy);
            });
        }


        return true;
    }

    /// <summary>
    /// 自身に対して発生するカード効果の処理
    /// </summary>
    /// <param name="effect">処理対象のカード効果</param>
    /// <param name="attributes">カード属性(演出に使用)</param>
    /// <param name="sequence">演出が乗るSequence</param>
    /// <param name="cardPos">使用したCardの位置</param>
    /// <returns>判定を行うカード効果の場合、判定に失敗したらfalse</returns>
    public bool Execute(ActualEffect effect, List<AttributeDefine> attributes, Sequence sequence, int cardPos)
    {
        bool b = ExecuteDecision(effect, attributes, sequence, cardPos);
        if (!b) return false;//判定に失敗したら以降の処理は中断

        //演出は少しの間隔をあけて行われる
        sequence.AppendCallback(() =>
        {
            PlayerExecuteAction(effect, attributes, sequence, cardPos);
        }).AppendInterval(BattleConstants.CardEffectInterval);

        return true;
    }

    private void SearchAndUsePlayerState<T>(Action<T> action,IState state=null) where T : class,IState
    {
        playerBattleManager.SearchAndUsePlayerState<T>(action);

        if (state is not null)
        {
            dataPerCard.AddUsedState(state);
        }
    }

    //TODO:Weapon,Fellowのオーバーロードが存在する
    //Playerは最初から取得している

    public DataPerBattle GetDataPerBattle()
    {
        return dataPerBattle;
    }

    public DataPerCard GetDataPerCard()
    {
        return dataPerCard;
    }

    public DataPerTurn GetDataPerTurn()
    {
        return dataPerTurn;
    }
}
