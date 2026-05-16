using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequirePhysicalItemHolder : MonoBehaviour, IGridPhysicalItemHolder
{
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;
    [SerializeField] private InventoryPanel inventoryPanel;

    [SerializeField] private RequirePhysicalItemArrangement requirePhysicalItemArrangement;

    public int GridHeight { get { return gridHeight; } set { gridHeight = value; } }
    public int GridWidth { get { return gridWidth; } set { gridWidth = value; } }

    [SerializeField] private int gridHeight;
    [SerializeField] private int gridWidth;

    public void Init(List<PhysicalItemGridPosNumData> requiredItems)
    {
        requirePhysicalItemArrangement.Init(requiredItems,this);
    }

    public (int x, int y)? GetEmptyZone(int x, int y)
    {
        return requirePhysicalItemArrangement.GetEmptyZone(x, y);
    }

    public (int x, int y)? GetItemPos(PhysicalItemBase item)
    {
        return requirePhysicalItemArrangement.GetItemPos(item);
    }

    public void PullItem(int posX, int posY)
    {
        requirePhysicalItemArrangement.RemoveItemFromList(posX, posY);

        inventoryPanel.ChangeDisplayMode(true);
    }

    public void PullItem(PhysicalItemBase item)
    {
        requirePhysicalItemArrangement.RemoveItemFromList(item);
    }

    public SetItemResult PutItem(PhysicalItemBase item, IPhysicalItemZone itemZone)
    {
        var result = requirePhysicalItemArrangement.AddAndSetItem(item);

        return result;
    }

    public SetItemResult PutItem(PhysicalItemBase item, int posX, int posY)
    {
        var result = requirePhysicalItemArrangement.AddAndSetItem(item, posX, posY);

        return result;
    }

    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item)
    {
        return requirePhysicalItemArrangement.SearchSameItems(item);
    }

    public List<(PhysicalItemBase,int)> SearchSameItemsAndStackMax(PhysicalItemBase item)
    {
        return requirePhysicalItemArrangement.SearchSameItemsAndStackMax(item);
    }

    public List<PhysicalItemBase> GetItemsInstances()
    {
        return requirePhysicalItemArrangement.GetItemsInstances();
    }

    public void RefreshStack()
    {
        requirePhysicalItemArrangement.RefreshStack();
    }

    public void SetData()
    {
        //ここではデータを記録しない
    }

    public int PuttableNum(PhysicalItemBase item)
    {
        return requirePhysicalItemArrangement.PuttableNum(item);
    }

    /// <summary>
    /// アイテムの要求が満たされているならTrueを返す。
    /// </summary>
    /// <returns>アイテムの要求が満たされているならTrue</returns>
    public bool IsRequireCompleted()
    {
        return requirePhysicalItemArrangement.IsRequireCompleted();
    }

    /// <summary>
    /// 配置されているアイテムをすべて破壊する
    /// </summary>
    public void DeleteRealItems()
    {
        requirePhysicalItemArrangement.DeleteRealItems();
    }

    public void TestItemDump()
    {
        //テスト用にアイテムをダンプする
        requirePhysicalItemArrangement.TestItemDump();
    }

}
