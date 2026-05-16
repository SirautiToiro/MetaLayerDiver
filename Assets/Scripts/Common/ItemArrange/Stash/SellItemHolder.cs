using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 天秤屋など、アイテムをプレイヤーから売却するときに使用
/// </summary>
public class SellItemHolder : MonoBehaviour,IGridPhysicalItemHolder
{
    private PhysicalItemArrangeManager physicalItemArrangeManager;
    private StashPanel stashPanel;

    private IShopManager shopManager;

    //グリッドによるアイテム配置(データの格納もここ)
    [SerializeField] private GridPhysicalItemArrangement gridPhysicalItemArrangement;

    public int GridHeight { get { return gridHeight; } set { gridHeight = value; } }
    public int GridWidth { get { return gridWidth; } set { gridWidth = value; } }

    [SerializeField] private int gridHeight;
    [SerializeField] private int gridWidth;

    /// <summary>
    /// 初期化。
    /// </summary>
    /// <param name="storageDataList">アイテムがどのように配置されているかのデータ.</param>
    public void Init(List<PhysicalItemGridPosNumData> storageDataItems
        , PhysicalItemArrangeManager physicalItemArrangeManager, StashPanel stashPanel, IShopManager shopManager = null)
    {
        this.physicalItemArrangeManager = physicalItemArrangeManager;
        this.stashPanel = stashPanel;
        this.shopManager = shopManager;

        gridPhysicalItemArrangement.Init(storageDataItems,this);
    }

    public (int x, int y)? GetEmptyZone(int x, int y)
    {
        return gridPhysicalItemArrangement.GetEmptyZone(x, y);
    }

    public (int x, int y)? GetItemPos(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.GetItemPos(item);
    }
    public void PullItem(int posX, int posY)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(posX, posY);

        stashPanel.ChangeDisplayMode(true);//アイテムを表示するモードに切り替える
    }

    public void PullItem(PhysicalItemBase item)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(item);
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
        //テスト用にアイテムをダンプする
        gridPhysicalItemArrangement.TestItemDump();
    }

    public void SetData()
    {
        //データ記録なし
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
