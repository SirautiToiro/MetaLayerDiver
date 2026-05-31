using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// それぞれのレア度のアイテムが
/// どの値段で買えるかの定義
/// </summary>
[System.Serializable]
public class ShopPriceRate
{
    public ShopPriceRate(int commonRate, int rareRate, int metaRate)
    {
        CommonRate = commonRate;
        RareRate = rareRate;
        MetaRate = metaRate;
    }

    public int CommonRate;
    public int RareRate;
    public int MetaRate;

    public int GetRate(TierDefine.Tier tier)
    {
        switch(tier)
        {
            case TierDefine.Tier.Common:
                if(CommonRate == -1)
                {
                    return 0;
                }
                else
                {
                    return CommonRate;
                }
            case TierDefine.Tier.Rare:
                if (RareRate == -1)
                {
                    return 0;
                }
                else
                {
                    return RareRate;
                }
            case TierDefine.Tier.Meta:
                if (MetaRate == -1)
                {
                    return 0;
                }
                else
                {
                    return MetaRate;
                }
            default:
                return 0;
            }
    }
}
