using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasuresOrDrops : ItemBase
{
    [SerializeField] private TreasuresOrDropsUI treasuresOrDropsUI;

    private TreasuresOrDropsDataSO baseTreasuresOrDropsData;
    public TreasuresOrDropsDataSO BaseTreasuresOrDropsData { get { return baseTreasuresOrDropsData; } }

    //物理アイテムとしてのデータ
    private PhysicalItemDataSO basePlItemData; //物理アイテムとしてのデータ
    public PhysicalItemDataSO BasePlItemData { get { return basePlItemData; } }

    public int serialNum;//シリアル番号

    private ItemMover itemMover = null;
    public override ItemMover ItemMover { get { return itemMover; } }

    private ShowDescriptionWhenRightClick itemRightClick;
    public override IItemRightClick ItemRightClick { get { return itemRightClick; } }

    /// <summary>
    /// 初期化処理。
    /// clickableFlagをfalseにすることでカードは一切動かないようになる
    /// </summary>
    public void Init(PhysicalItemDataSO pTDData)
    {
        baseTreasuresOrDropsData = pTDData.TreasuresOrDropsData;
        basePlItemData = pTDData;

        clickableFlag = false;

        itemMover = new ItemMover(this);
        itemRightClick = new ShowDescriptionWhenRightClick();

        serialNum = pTDData.SerialNum;

        tier = pTDData.Tier;

        treasuresOrDropsUI.Init(pTDData, this);
    }


    public override bool IsItemHasAttribute(AttributeDefine.Attribute attribute)
    {
        return false;//このアイテムは属性を持たない
    }

    public override bool IsItemHasTag(CardTagDefine.CardTag cardTag)
    {
        return false;//このアイテムはタグを持たない
    }
}
