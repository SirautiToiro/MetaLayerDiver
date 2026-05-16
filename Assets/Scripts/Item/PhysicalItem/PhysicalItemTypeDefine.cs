using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理アイテムの種類を定義する
/// </summary>
[System.Serializable]
public class PhysicalItemTypeDefine
{
    [Header("アイテムの種類")]
    public PhysicalItemType itemType;

    public enum PhysicalItemType
    {
        Weapon,         //武器
        Gear,           //装備
        Consumables,    //消耗品
        //TreasuresとDropsの実装は同じ.表示が異なるのみ.
        Treasures,      //換金アイテム
        Drops,          //ドロップ品
        AnyoneItem,     //システム上のワイルドカード
        None,           //セーブで使用。そこにアイテムがない
    }
}
