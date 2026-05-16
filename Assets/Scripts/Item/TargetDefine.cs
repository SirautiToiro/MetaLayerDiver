using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カード,武器の効果範囲を定義
/// 武器は近、遠、中のみ
/// </summary>
[System.Serializable]
public class TargetDefine
{
    //パラメータ
    [Header("効果対象")]
    public EffectTarget effectTarget;

    #region 効果対象の定義
    public enum EffectTarget
    {
        Weapon,//武器枠
        Self,//自身
        ShortRange,//近距離
        MediumRange,//中距離
        LongRange,//遠距離
        EnemyAll,//敵全体
        Fellow,//仲間枠
    }
    #endregion

    #region 対象説明
    readonly public static Dictionary<EffectTarget,string> Dic_EffectTarget= new Dictionary<EffectTarget, string>()
    {
        {EffectTarget.Weapon,"武器" },
        {EffectTarget.Self,"自身" },
        {EffectTarget.ShortRange,"近距離" },
        {EffectTarget.MediumRange,"中距離" },
        {EffectTarget.LongRange,"遠距離" },
        {EffectTarget.EnemyAll,"敵全体" },
        {EffectTarget.Fellow,"仲間" },
    };
    #endregion

}
