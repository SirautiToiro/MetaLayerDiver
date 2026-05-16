using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理アイテムの配置中に配置の結果を格納するクラス
/// </summary>
public class SetItemResult
{
    //アイテムを配置できたかどうか
    public bool IsSuccess { get; }
    //配置の際に交換が発生した場合のアイテム(と、その配置情報)
    public PhysicalItemGridPosNum AlternativeItem { get; }

    public SetItemResult(bool isSuccess, PhysicalItemGridPosNum alternativeItem = null)
    {
        this.IsSuccess = isSuccess;
        this.AlternativeItem = alternativeItem;
    }
}
