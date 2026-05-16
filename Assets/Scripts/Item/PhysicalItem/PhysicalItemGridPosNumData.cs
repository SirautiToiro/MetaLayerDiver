using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理的アイテムの位置とスタック数を格納するクラス
/// データの部分
/// </summary>
[System.Serializable]
public class PhysicalItemGridPosNumData
{
    public PhysicalItemDataSO ItemData;
    public int X;
    public int Y;
    public int Stack;

    public PhysicalItemGridPosNumData(PhysicalItemDataSO itemData, int x, int y, int stack)
    {
        ItemData = itemData;
        X = x;
        Y = y;
        Stack = stack;
    }
}
