using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 1*1のアイテム
/// </summary>
public class PhysicalItem1_1 : PhysicalItemBase
{
    //縦横のサイズは1*1
    public override int ItemSizeX { get { return 1; } }

    public override int ItemSizeY { get { return 1; } }

    public override bool IsItemHasAttribute(AttributeDefine.Attribute attribute)
    {
        return false;
    }

    public override bool IsItemHasTag(CardTagDefine.CardTag cardTag)
    {
        return false;
    }

    public override void PullFromHolder()
    {
        //1*1のアイテムは変形が発生しない
    }

    public override void PutToHolder(IPhysicalItemHolder holder)
    {
        //1*1のアイテムは変形が発生しない
    }
}
