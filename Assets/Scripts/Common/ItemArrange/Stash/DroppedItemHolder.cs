using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemHolder : MonoBehaviour, IGridPhysicalItemHolder
{
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;
    [SerializeField] private InventoryPanel inventoryPanel;

    //グリッドによるアイテム配置(データの格納もここ)
    [SerializeField] private GridPhysicalItemArrangement gridPhysicalItemArrangement;

    public int GridHeight { get { return gridHeight; } set { gridHeight = value; } }
    public int GridWidth { get { return gridWidth; } set { gridWidth = value; } }

    [SerializeField] private int gridHeight;
    [SerializeField] private int gridWidth;

    public void Init(List<PhysicalItemDataSO> itemDataList)
    {
        gridPhysicalItemArrangement.Init(new List<PhysicalItemGridPosNumData>(), this);

        // アイテムデータをグリッドにセット(一つずつ高速移動を用いる)
        foreach (var itemData in itemDataList)
        {
            (int x, int y)? enptyZone = gridPhysicalItemArrangement.GetEmptyZone(itemData.SizeX, itemData.SizeY);
            if (enptyZone.HasValue)
            {
                //新しく生成
                PhysicalItemBase item = gridPhysicalItemArrangement.InstantiatePhysicalItem(
                    new PhysicalItemGridPosNumData(itemData, enptyZone.Value.x, enptyZone.Value.y, 1));

                //QuickMoveでセット
                physicalItemArrangeManager.QuickMove(item, null, this);
            }
        }
    }

    public (int x, int y)? GetItemPos(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.GetItemPos(item);
    }

    public (int x, int y)? GetEmptyZone(int x, int y)
    {
        return gridPhysicalItemArrangement.GetEmptyZone(x, y);
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

    public void PullItem(int posX, int posY)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(posX, posY);
        inventoryPanel.ChangeDisplayMode(true);//アイテムを表示するモードに切り替える

    }

    public void PullItem(PhysicalItemBase item)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(item);
        inventoryPanel.ChangeDisplayMode(true);//アイテムを表示するモードに切り替える

    }

    public void TestItemDump()
    {
        gridPhysicalItemArrangement.TestItemDump();
    }

    public List<PhysicalItemBase> GetItems()
    {
        return gridPhysicalItemArrangement.GetItemInstances();
    }

    public void SetData()
    {
        //ドロップアイテムはデータを記録しない
    }

    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.SearchSameItems(item);
    }
}