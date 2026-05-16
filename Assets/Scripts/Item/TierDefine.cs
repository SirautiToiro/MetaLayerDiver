using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードやアイテムのレア度を定義
/// </summary>
[System.Serializable]
public class TierDefine
{
    //レア度
    [Header("レア度")]
    public Tier tier;

    public enum Tier
    {
        Common = 0, //コモン。一番普通。
        Rare,       //レア。強力。
        Meta,       //メタ。めったなことでは手に入らず、超常的なことが発生する
    }

    readonly public static Dictionary<Tier, string> Dic_TierName = new Dictionary<Tier, string>()
    {
        {Tier.Common,"コモン" },
        {Tier.Rare,"レア" },
        {Tier.Meta,"メタ" },
    };

}
