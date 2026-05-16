using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理的アイテムの位置とスタック数を格納するクラス
/// クラスとしての実体の部分
/// </summary>
public class PhysicalItemGridPosNum
{
    public PhysicalItemBase Item;
    public int X;
    public int Y;

    public PhysicalItemGridPosNum(PhysicalItemBase itemData,int x,int y)
    {
        Item = itemData;
        X = x;
        Y = y;
    }
}
