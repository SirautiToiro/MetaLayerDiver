using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PhysicalItem4_1 : PhysicalItemBase
{
    //縦横のサイズは1*1
    public override int ItemSizeX { get { return 4; } }

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
        //アイテムサイズを通常に戻す
        PhysicalItemUI.ChangeItemSize(ItemSizeX, ItemSizeY);
        //アイコンを通常のものに戻す
        PhysicalItemUI.ChangeIcon(BaseItemData.IconSprite);
    }

    public override void PutToHolder(IPhysicalItemHolder holder)
    {
        if (holder is IEquipPhysicalItemHolder)
        {
            //小アイコンが用意されていないことを検知する方法はよくわからない

            //装備場所に設置された時のみ変形が発生する
            //1*1に一時的に変形し、アイコンを変化させる
            PhysicalItemUI.ChangeItemSize(1, 1);
            PhysicalItemUI.ChangeIcon(BaseItemData.MiniIconSprite);
        }
    }
}
