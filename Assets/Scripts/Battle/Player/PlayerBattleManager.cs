using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

/// <summary>
/// バトル中のプレイヤーデータの管理、生成
/// Enemyと同じ扱いで生成するように作成
/// </summary>
public class PlayerBattleManager : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private PlayerCardZone playerCardZone;//playerが配置されるカードゾーン

    [SerializeField] private FieldManager fieldManager;
    [SerializeField] private CardEffectExecute cardEffectExecute;
    [SerializeField] private HandManager handManager;
    [SerializeField] private GearManager gearManager;
    [SerializeField] private PlayerAnimation playerAnimation;

    private GameObject playerObj;

    private List<Action<CardEffectExecute, Sequence, Player>> turnStartEvents;

    /// <summary>
    /// 初期化処理。プレイヤー立ち絵Prefabの生成
    /// </summary>
    public void Init()
    {
        PlayerUI playerUI= InstantiatePlayer();
        playerAnimation.PlayerAppeearAnimation(playerUI);
        turnStartEvents = new List<Action<CardEffectExecute, Sequence, Player>>();
    }

    /// <summary>
    /// playerのPrefabをインスタンス化
    /// </summary>
    /// <returns>生成したPlayerUI</returns>
    private PlayerUI InstantiatePlayer()
    {
        //インスタンス化
        //親はPlayerCardZone
        playerObj = Instantiate(playerPrefab, playerCardZone.gameObject.transform.position, Quaternion.identity, playerCardZone.gameObject.transform);
        PlayerUI playerUI = playerObj.GetComponent<PlayerUI>();

        playerCardZone.Init(fieldManager, player, cardEffectExecute, handManager);

        player.SetBattle(playerUI,this);//playerとplayerUIを繋げる

        return playerUI;
    }

    //プレイヤーのオブジェクトを削除
    public void DeletePlayerObject()
    {
        GameObject.Destroy(playerObj);
    }

    #region ターン開始終了時処理

    /// <summary>
    /// プレイヤーの盾を初期化する
    /// </summary>
    public void ResetPlayerShield()
    {
        player.ResetShield();
    }

    /// <summary>
    /// ターン終了時に発生するStateの減少を発生させる。
    /// 実際にはPlayerが実行
    /// </summary>
    public void DecreaseState()
    {
        player.DecreaseState();
    }

    /// <summary>
    /// 装備アイテムのターン終了時減少を発生させる
    /// </summary>
    public void DecreaseStateInGear()
    {
        gearManager.DecreaseStateInGear();
    }

    /// <summary>
    /// ターン開始時に発生すべきイベントを貯めておいて、一つずつ実行する
    /// </summary>
    /// <param name="callback">イベント発生後に行うコールバック</param>
    public void OnTurnStart(Action callback)
    {
        Sequence sequence = DOTween.Sequence();

        foreach (var action in turnStartEvents)
        {
            action.Invoke(cardEffectExecute,sequence,player);
        }

        sequence.AppendCallback(() =>
        {
            //ターン開始イベントが全て終わった後にコールバックを実行
            callback?.Invoke();
        });

        //クリア
        turnStartEvents = new List<Action<CardEffectExecute, Sequence, Player>>();
    }

    public void SetTurnStartEvent(Action<CardEffectExecute,Sequence,Player> action)
    {
        if (action != null)
        {
            turnStartEvents.Add(action);
        }
    }
    #endregion

    #region プレイヤーに対してのエフェクト
    /// <summary>
    /// プレイヤーが攻撃された際のエフェクト効果を発生させる
    /// </summary>
    /// <param name="actionType">エフェクトの種類</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void PlayerEffect(EnemyActionDefine.EnemyActionType actionType, EnemyActionDefine.EnemyAction enemyAction, int value)
    {
        player.Effect(actionType, enemyAction, value);
    }

    /// <summary>
    /// Card効果によるエフェクトを発生させる
    /// </summary>
    /// <param name="effectType">カード効果の種類</param>
    /// <param name="attribute">エフェクトの属性</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Effect(CardEffectDefine.CardEffectType effectType, AttributeDefine.Attribute attribute, int value)
    {
        player.Effect(effectType, attribute, value);
    }

    /// <summary>
    /// プレイヤーに発生しているエフェクト全てに削除演出を行う
    /// </summary>
    public void DestroyEffects()
    {
        player.DestroyEffects();
    }

    #endregion

    #region Playerに発生する効果

    /// <summary>
    /// プレイヤーにダメージを与える
    /// </summary>
    /// <param name="value">ダメージの値</param>
    public void DamageToPlayer(int value)
    {
        //ダメージ
        player.Damage(value);

        if (player.GetHp()<=0)
        {//Hpが0以下なら
            fieldManager.DefeatBattle();
        }
    }

    /// <summary>
    /// プレイヤーに状態異常を与える
    /// </summary>
    /// <param name="state">状態異常</param>
    public void StateToPlayer(IState state)
    {
        player.GetState(state);
    }

    /// <summary>
    /// プレイヤーの状態異常の値をvalueだけ減少させる
    /// stateは、既にPlayerについている状態異常
    /// </summary>
    /// <param name="state">既にPlayerについている状態異常</param>
    /// <param name="value">減少させる値</param>
    public void ConsumeState(IState state ,int value)
    {
        player.ConsumeState(state, value);
    }

    public void DecreaseCountOfCountState(IStateHasCount cState)
    {
        player.DecreaseCountOfCountState(cState);
    }

    /// <summary>
    /// ターンの開始時に、ターンごとにカウントのある状態異常の値をリセットする
    /// </summary>
    public void ResetIStateCountPerTurn()
    {
        player.ResetIStateCountPerTurn();
    }

    /// <summary>
    /// プレイヤーの状態異常を削除する
    /// </summary>
    /// <param name="state">削除する状態異常</param>
    public void DeleteState(IState state)
    {
        player.DeleteState(state);
    }

    /// <summary>
    /// プレイヤーにシールドを与える
    /// </summary>
    /// <param name="value">シールドの値</param>
    public void ShieldToPlayer(int value)
    {
        player.GetShield(value);
    }

    /// <summary>
    /// プレイヤーの所有する状態異常のリストからTに該当するものを検索し、
    /// デリゲートで指示したものを実行させる
    /// </summary>
    /// <typeparam name="T">検索する状態異常のタイプ</typeparam>
    /// <param name="useAction">a => a.UseA()のように、Tのメソッドを使用する指示</param>
    public void SearchAndUsePlayerState<T>(Action<T> useAction) where T : class, IState
    {
        player.SearchAndUseState<T>(useAction);
    }

    

    public void DeletePlayerState()
    {
        player.DeleteAllState();
    }

    public Player GetPlayer()
    {
        return player;
    }

    public void EndBattle()
    {
        DeletePlayerState();
        DeletePlayerObject();
        player.EndBattle();
    }

    #endregion

    #region 装備アイテムに対して発生する効果

    public void ConsumeGearState(IStateInGear state,int value)
    {
        gearManager.ConsumeGearState(state, value);
    }

    public void ResetIStateInGearCountPerTurn()
    {
        gearManager.ResetIStateInGearCountPerTurn();
    }

    public void DecreaseCountOfCountStateInGear(IStateHasCount cState)
    {
        gearManager.DecreaseCountOfCountStateInGear(cState);
    }

    public IStateInGear GetGearState()
    {
        return gearManager.GetGearState();
    }

    #endregion


    #region プレイヤーから発生する効果


    /// <summary>
    /// カードUI情報を再読み込みする
    /// </summary>
    public void RefreshCard()
    {
        fieldManager.RefreshCard();
    }

    /// <summary>
    /// 武器UI情報を再読み込みする
    /// </summary>
    public void RefreshWeapon()
    {
        fieldManager.RefreshWeapon();
    }

    #endregion

    public FieldManager GetFieldManager()
    {
        return fieldManager;
    }
}
