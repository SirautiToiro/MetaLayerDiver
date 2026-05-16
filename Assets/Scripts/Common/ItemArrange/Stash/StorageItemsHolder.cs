using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageItemsHolder : MonoBehaviour, IGridPhysicalItemHolder
{
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;
    [SerializeField] private InventoryPanel inventoryPanel;

    //グリッドによるアイテム配置(データの格納もここ)
    [SerializeField] private GridPhysicalItemArrangement gridPhysicalItemArrangement;

    public int GridHeight { get { return gridHeight; } set { gridHeight = value; } }
    public int GridWidth { get { return gridWidth; } set { gridWidth = value; } }

    [SerializeField] private int gridHeight;
    [SerializeField] private int gridWidth;

    /// <summary>
    /// 初期化。
    /// </summary>
    /// <param name="storageDataItems">アイテムがどのように配置されているかのデータ.
    /// 本来ページごとに分かれているが、StashPanelがその部分の整理を行う</param>
    public void Init(List<PhysicalItemGridPosNumData> storageDataItems)
    {
        gridPhysicalItemArrangement.Init(storageDataItems, this);
        SetStorageItems();//データを記録。(アイテム削除でキャンセルされた時に戻る処理でも使用するため)(使わないかもしれないが、一応)
    }

    private void SetStorageItems()
    {
        List<PhysicalItemGridPosNumData> items = gridPhysicalItemArrangement.GetItems();

        inventoryPanel.SetStorageItems(items);
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

        SetStorageItems();

        inventoryPanel.ChangeDisplayMode(true);//アイテムを表示するモードに切り替える
    }

    public void PullItem(PhysicalItemBase item)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(item);

        SetStorageItems();
    }

    public SetItemResult PutItem(PhysicalItemBase item, IPhysicalItemZone itemZone)
    {
        var result = gridPhysicalItemArrangement.AddAndSetItem(item);

        SetStorageItems();

        return result;
    }

    public SetItemResult PutItem(PhysicalItemBase item, int posX, int posY)
    {
        var result = gridPhysicalItemArrangement.AddAndSetItem(item, posX, posY);
        SetStorageItems();
        return result;
    }

    public void TestItemDump()
    {
        //テスト用にアイテムをダンプする
        gridPhysicalItemArrangement.TestItemDump();
    }
    public void SetData()
    {
        SetStorageItems();
    }
    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.SearchSameItems(item);
    }
}
