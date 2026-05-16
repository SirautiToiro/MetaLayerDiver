using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopTypeDefine
{
    /// <summary>
    /// 店の種類を定義
    /// </summary>
    [System.Serializable]
    public enum ShopType
    {
        SupplyShop = 0, //支給品
        CardShop,//村のカード商店
        ClownSan,//ピエロさん
        TenbinYa,//天秤屋
    }
}
