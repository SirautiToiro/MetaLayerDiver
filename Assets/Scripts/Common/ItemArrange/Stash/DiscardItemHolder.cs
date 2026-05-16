using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiscardItemHolder : MonoBehaviour,IGridPhysicalItemHolder
{
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;

    //グリッドによるアイテム配置(データの格納もここ)
    [SerializeField] private GridPhysicalItemArrangement gridPhysicalItemArrangement;

    public int GridHeight { get { return gridHeight; } set { gridHeight = value; } }
    public int GridWidth { get { return gridWidth; } set { gridWidth = value; } }

    [SerializeField] private int gridHeight;
    [SerializeField] private int gridWidth;

    public void Init(PhysicalItemBase item)
    {
        gridPhysicalItemArrangement.Init(item, this,0,0);
    }

    public void PullItem(int posX, int posY)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(posX, posY);
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

    public void DicideButton()
    {
        physicalItemArrangeManager.ShowDiscardMenu();
    }

    public void CancelButton()
    {
        physicalItemArrangeManager.CancelDiscard();
    }

    public void SetData()
    {
        //記録しない
    }
    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.SearchSameItems(item);
    }
}
