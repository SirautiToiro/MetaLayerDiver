using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryEquipItemsHolder : MonoBehaviour, IEquipPhysicalItemHolder
{
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;
    [SerializeField] private InventoryPanel inventoryPanel;

    [SerializeField] private EquipPhysicalItemArrangement equipPhysicalItemArrangement;

    public void Init(List<PhysicalItemDataSO> weapons, PhysicalItemDataSO gear, List<PhysicalItemDataSO> consumables)
    {
        equipPhysicalItemArrangement.Init(weapons, gear, consumables, this);

        SetEquippingItems();//データを記録。(アイテム削除でキャンセルされた時に戻る処理でも使用するため)
    }

    private bool SetEquippingItems()
    {
        var equips = equipPhysicalItemArrangement.GetEquips();
        
        return inventoryPanel.SetEquippingItems(
            equips.weapons,equips.gear,equips.consumables);
    }

    public (List<PhysicalItemDataSO> weapons, PhysicalItemDataSO gear, List<PhysicalItemDataSO> consumables) GetEquips()
    {
        return equipPhysicalItemArrangement.GetEquips();
    }

    public void PullItem(int posX, int posY)
    {
        equipPhysicalItemArrangement.RemoveItemFromList(posX, posY);
        SetEquippingItems();

        //倉庫画面なら、倉庫画面の開き方を適切にする
        if (inventoryPanel.GetCaller().VillageManager != null)
        {
            inventoryPanel.ChangeStashMode(true);//アイテムモードにする
        }
    }

    public void PullItem(PhysicalItemBase item)
    {
        equipPhysicalItemArrangement.RemoveItemFromList(item);

        //データ更新
        SetEquippingItems();
    }

    public SetItemResult PutItem(PhysicalItemBase item, IPhysicalItemZone itemZone)
    {
        //装備CardZoneにセットする。
        if(itemZone is EquipCardZone eZone)
        {//一応、対応するZoneかをチェック
            var result = equipPhysicalItemArrangement.AddAndSetItem(item, eZone);
            SetEquippingItems();
            return result;
        }
        else
        {
            SetEquippingItems();
        }

        return new SetItemResult(false);
    }

    public SetItemResult PutItem(PhysicalItemBase item, int posX, int posY)
    {
        var result = equipPhysicalItemArrangement.AddAndSetItem(item, posX, posY);
        SetEquippingItems();
        return result;
    }

    public void TestItemDump()
    {

    }

    public void SetData()
    {
        SetEquippingItems();
    }
}
