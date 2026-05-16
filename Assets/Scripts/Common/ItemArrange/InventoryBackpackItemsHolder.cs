using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryBackpackItemsHolder : MonoBehaviour, IGridPhysicalItemHolder
{
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;
    [SerializeField] private InventoryPanel inventoryPanel;

    //グリッドによるアイテム配置(データの格納もここ)
    [SerializeField] private GridPhysicalItemArrangement gridPhysicalItemArrangement;

    public int GridHeight { get { return gridHeight; } set { gridHeight = value; } }
    public int GridWidth { get { return gridWidth; } set { gridWidth = value; } }

    [SerializeField] private int gridHeight;
    [SerializeField] private int gridWidth;

    public void Init(List<PhysicalItemGridPosNumData> backpackItems)
    {
        gridPhysicalItemArrangement.Init(backpackItems,this);
        SetBackpackItems();//データを記録。(アイテム削除でキャンセルされた時に戻る処理でも使用するため)
    }

    /// <summary>
    /// アイテムのドラッグ終了時、変化したアイテム配置を格納する
    /// </summary>
    /// <returns>trueなら格納成功</returns>
    private bool SetBackpackItems()
    {
        List<PhysicalItemGridPosNumData> items = gridPhysicalItemArrangement.GetItems();

        return inventoryPanel.SetBackpackItems(items);
    }

    /// <summary>
    /// アイテム情報と配置のデータを返す
    /// </summary>
    /// <returns>アイテム情報と配置のデータ</returns>
    public List<PhysicalItemGridPosNumData> GetItems()
    {
        return gridPhysicalItemArrangement.GetItems();
    }

    /// <summary>
    /// Holderが持っているアイテムのインスタンスの一覧を返す
    /// </summary>
    /// <returns>生成されているアイテムの一覧</returns>
    public List<PhysicalItemBase> GetItemInstances()
    {
        return gridPhysicalItemArrangement.GetItemInstances();
    }

    /// <summary>
    /// Hodlerが持っているアイテムの一覧から
    /// その中のItemを検索し、
    /// その位置を返す
    /// 参照一致
    /// 存在しないならnullを返す
    /// </summary>
    /// <param name="item">検索するItem</param>
    /// <returns>存在しないならnull,存在するならその座標</returns>
    public (int x, int y)? GetItemPos(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.GetItemPos(item);
    }

    public void PullItem(int posX, int posY)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(posX, posY);

        //データ更新
        SetBackpackItems();

        //倉庫画面なら、倉庫画面の開き方を適切にする
        if(inventoryPanel.GetCaller().VillageManager != null)
        {
            inventoryPanel.ChangeStashMode(true);//アイテムモードにする
        }
    }

    public void PullItem(PhysicalItemBase item)
    {
        gridPhysicalItemArrangement.RemoveItemFromList(item);

        //データ更新
        SetBackpackItems();
    }

    public SetItemResult PutItem(PhysicalItemBase item, IPhysicalItemZone itemZone)
    {
        var result = gridPhysicalItemArrangement.AddAndSetItem(item);
        //データ更新
        SetBackpackItems();

        return result;
    }

    public SetItemResult PutItem(PhysicalItemBase item, int posX, int posY)
    {
        var result = gridPhysicalItemArrangement.AddAndSetItem(item, posX, posY);
        //データ更新
        SetBackpackItems();
        return result;
    }

    public (int x, int y)? GetEmptyZone(int x, int y)
    {
        return gridPhysicalItemArrangement.GetEmptyZone(x,y);
    }


    public void TestItemDump()
    {
        //テスト用にアイテムをダンプする
        gridPhysicalItemArrangement.TestItemDump();
    }

    public void SetData()
    {
        SetBackpackItems();
    }
    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item)
    {
        return gridPhysicalItemArrangement.SearchSameItems(item);
    }
}
