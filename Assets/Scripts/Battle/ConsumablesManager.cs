using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumablesManager : MonoBehaviour
{
    [SerializeField] private GameObject consumablePrefab;
    [SerializeField] private FieldManager fieldManager;
    [SerializeField] private PlaceCardZone[] consumableCardZones; // 消耗品カードゾーンの配列

    /// <summary>
    /// nullのものを除いて詰めながらInventoryDataからのデータを確保する
    /// </summary>
    private List<Consumable> consumables; // 現在の消耗品のリスト

    public void Init()
    {
        consumables = new List<Consumable>();

        List<PhysicalItemDataSO> pConsumablesList = InventoryData.GetEquippingConsumables();

        for(int i=0;i < 3; i++)
        {
            if (pConsumablesList[i] == null)
            {
                continue;
            }

            //消耗品の生成
            Consumable consumable = InstantiateConsumable(pConsumablesList[i], consumableCardZones[i], i);
            consumables.Add(consumable);
        }
    }

    /// <summary>
    /// バトル終了時に呼ばれる
    /// </summary>
    public void EndBattle()
    {
        List<PhysicalItemDataSO> inventoryData = new List<PhysicalItemDataSO>();
        //Consumableのデータをnullで埋める
        inventoryData.Add(null);
        inventoryData.Add(null);
        inventoryData.Add(null);
        foreach (Consumable consumable in consumables)
        {
            //消耗品のデータをInventoryDataにセット
            inventoryData[consumable.GetPos()] = consumable.BasePItemData;
        }

        InventoryData.SetEquippingConsumables(inventoryData);

        //データ削除
        foreach (Consumable consumable in consumables)
        {
            Destroy(consumable.gameObject);
        }
        consumables.Clear();
    }

    /// <summary>
    /// バトル中、アイテムを使用したときに呼ばれる。
    /// アイテムを削除
    /// </summary>
    /// <param name="consumable">使用したアイテム</param>
    public void UseConsumable(Consumable consumable)
    {
        consumables.Remove(consumable);
        Destroy(consumable.gameObject);
    }

    private Consumable InstantiateConsumable(PhysicalItemDataSO consumableData,PlaceCardZone cardZone,int pos)
    {
        GameObject obj = Instantiate(consumablePrefab, cardZone.gameObject.transform.position,
            Quaternion.identity, cardZone.gameObject.transform);
        Consumable consumable = obj.GetComponent<Consumable>();
        consumable.Init(consumableData, cardZone, fieldManager, pos);

        return consumable;
    }
}
