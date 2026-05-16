using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵の行動の効果種類を定義
/// </summary>
[System.Serializable]
public class EnemyActionDefine
{
    //パラメータ
    [Header("効果の種類")]
    public EnemyAction enemyAction;
    [Header("効果値")]
    public int value;

    [Header("対応する状態異常")]
    [SerializeReference, SubclassSelector] public IState UseState;//敵の行動が使用する状態異常

    #region 効果種類の定義
    public enum EnemyAction
    {
        //---------//
        //追加するときは、GetActionTypeも触ること
        //---------//

        DamagePhysics,      //ダメージ(物理)
        DamagePsycho,       //ダメージ(念動)
        DamageFaith,        //ダメージ(信仰)
        DamageEnergy,       //ダメージ(エネルギー)
        DamagePyro,         //ダメージ(発火)
        DamageCreate,       //ダメージ(生成)
        DamageMind,         //ダメージ(精神)
        Block,              //ブロック付与

        GetBuff,      //バフ
        GetDebuff,
        GetBuffAll,
        GetDebuffAll,
        CauseDebuff,
        CauseBuff,

        NotMove,            //何も動かない

        _MAX,//最後
    }
    #endregion

    //効果タイプの定義
    public enum EnemyActionType
    {
        Damage,     //プレイヤーにダメージ
        Block,      //敵にシールド
        Debuff,     //プレイヤーにデバフ
        BuffSelf,   //敵にバフ
        BuffAll,    //敵にバフ
        PlayerHeal, //プレイヤーを回復する行為
        EnemyHeal,  //敵を回復する行為
        Other,      //その他
        Avoided,    //攻撃が回避された場合
        DebuffSelf, //自身にデバフ
        DebuffAll,  //味方全体にデバフ
        Error,
    }

    /// <summary>
    /// 敵の攻撃の効果タイプを取得する
    /// </summary>
    /// <param name="action">敵の攻撃</param>
    /// <returns>対応する効果タイプ</returns>
    public static EnemyActionType GetActionType(EnemyAction action)
    {
        switch (action)
        {
            //ダメージ行為
            case EnemyAction.DamagePhysics:
            case EnemyAction.DamagePsycho:
            case EnemyAction.DamageFaith:
            case EnemyAction.DamageEnergy:
            case EnemyAction.DamagePyro:
            case EnemyAction.DamageCreate:
            case EnemyAction.DamageMind:
                return EnemyActionType.Damage;

            //ブロック行為
            case EnemyAction.Block:
                return EnemyActionType.Block;

            //デバフ行為
            case EnemyAction.CauseDebuff:
                return EnemyActionType.Debuff;

            //バフ行為(自身)
            case EnemyAction.GetBuff:
                return EnemyActionType.BuffSelf;
            //バフ行為(全体)
            case EnemyAction.GetBuffAll:
                return EnemyActionType.BuffAll;

            //デバフ行為(自身)
            case EnemyAction.GetDebuff:
                return EnemyActionType.Debuff;
            //デバフ行為(全体)
            case EnemyAction.GetDebuffAll:
                return EnemyActionType.DebuffAll;

            //プレイヤー回復
            //敵回復

            //その他(演出なし)
            case EnemyAction.NotMove:
                return EnemyActionType.Other;

            default: 
                return EnemyActionType.Error;
        }
    }

    readonly public static Dictionary<EnemyActionType, string> Dic_ActionDescription = new Dictionary<EnemyActionType, string>()
    {
        {EnemyActionType.Damage, "ダメージを与える"},
        {EnemyActionType.Block, "ブロックを得る"},
        {EnemyActionType.Debuff, "弱体行動"},
        {EnemyActionType.DebuffAll, "弱体行動"},
        {EnemyActionType.BuffSelf, "強化行動"},
        {EnemyActionType.BuffAll, "強化行動"},
        {EnemyActionType.PlayerHeal, "回復を行う"},
        {EnemyActionType.EnemyHeal, "回復を行う"},
        {EnemyActionType.Other, "その他の行動"},
    };
}
