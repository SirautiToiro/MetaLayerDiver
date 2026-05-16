using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 店画面で使用するIPhysicalItemHolder。IShopManagerから与えられたアイテムを、与えられた値段で販売する
/// ShopItemHolderにはドラッグで配置ができない。PhysicalItemArrangeManager.EndDragging()に記述
/// </summary>
public class ShopItemHolder : MonoBehaviour, IGridPhysicalItemHolder
{
    private IShopManager shopManager;
    private PhysicalItemArrangeManager physicalItemArrangeManager;

    //グリッドによるアイテム配置(データの格納もここ)
    [SerializeField] private GridPhysicalItemArrangement gridPhysicalItemArrangement;

    public int GridHeight { get { return gridHeight; } set { gridHeight = value; } }
    public int GridWidth { get { return gridWidth; } set { gridWidth = value; } }

    [SerializeField] private int gridHeight;
    [SerializeField] private int gridWidth;

    private ShopPriceRate shopPriceRate;

    public void Init(List<PhysicalItemGridPosNumData> shopDataItems,PhysicalItemArrangeManager physicalItemArrangeManager,
        IShopManager shopManager,PhysicalItemInstantiateManager instantiateManager, ShopPriceRate rate)
    {
        this.physicalItemArrangeManager = physicalItemArrangeManager;
        this.shopManager = shopManager;
        this.shopPriceRate = rate;
        gridPhysicalItemArrangement.Init(shopDataItems, this,physicalItemArrangeManager,instantiateManager);
    }

    public void PullItem(int posX, int posY)
    {
        //PhysicalItemBase b = gridPhysicalItemArrangement.RemoveItemFromList(posX, posY);
        //1スタックだけを取り出すようにする
        PhysicalItemBase b = gridPhysicalItemArrangement.RemoveOneItemFromList(posX, posY);
        shopManager.ChangeDisplayMode(true);//アイテムを表示するモードに切り替える

        shopManager.OnBuied(b);//購入時メッセージ
    }

    public void PullItem(PhysicalItemBase item)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(item);
        //全スタックを取り出すようにする
    }

    public SetItemResult PutItem(PhysicalItemBase item, IPhysicalItemZone itemZone)
    {
        var result = gridPhysicalItemArrangement.AddAndSetItem(item);

        return result;
    }

    public SetItemResult PutItem(PhysicalItemBase item, int posX, int posY)
    {
        var result = gridPhysicalItemArrangement.AddAndSetItem(item, posX, posY);

        return result;
    }

    public void TestItemDump()
    {
        gridPhysicalItemArrangement.TestItemDump();
    }

    public (int x, int y)? GetItemPos(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.GetItemPos(item);
    }

    public (int x, int y)? GetEmptyZone(int x, int y)
    {
        return gridPhysicalItemArrangement.GetEmptyZone(x, y);
    }

    public void SetData()
    {
        //記録しない
    }

    public bool TryBuyItem(PhysicalItemBase item,int num=1)
    {
        return shopManager.GetStashPanel().ReduceCoin(shopPriceRate.GetRate(item.tier.tier) * num);
    }

    public List<PhysicalItemBase> GetItems()
    {
        return gridPhysicalItemArrangement.GetItemInstances();
    }
    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.SearchSameItems(item);
    }
}
