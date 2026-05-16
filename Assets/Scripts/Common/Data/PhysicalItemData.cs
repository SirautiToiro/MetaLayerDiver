using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PhysicalItemData : SingletonMonoBehaviour<PhysicalItemData>
{
    //存在する全アイテムの一覧
    [SerializeField] private List<PhysicalItemDataSO> allPhysicalItemsList;

    //支給品に該当するアイテム(ドロップしない)
    [SerializeField] private List<PhysicalItemDataSO> supplyItemsList;

    //全物理アイテムのタイプごとの辞書
    private static Dictionary<PhysicalItemTypeDefine.PhysicalItemType,
        Dictionary<int, PhysicalItemDataSO>> physicalItemData;

    // 各Tierごとのアイテムのシリアル番号のリスト+辞書
    //0:Common,1:Rare,2:Meta
    private static Dictionary<PhysicalItemTypeDefine.PhysicalItemType,
        List<PhysicalItemDataSO>[]> dropItemDataPerTier;

    [SerializeField] private InventoryData inventoryData;
    [SerializeField] private StorageData storageData;

    public void Awake()
    {
        //シングルトンの処理
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        //シングルトン処理終了

        //初期化
        physicalItemData = new Dictionary<PhysicalItemTypeDefine.PhysicalItemType,
            Dictionary<int, PhysicalItemDataSO>>();
        physicalItemData.Add(PhysicalItemTypeDefine.PhysicalItemType.Consumables, new Dictionary<int, PhysicalItemDataSO>());
        physicalItemData.Add(PhysicalItemTypeDefine.PhysicalItemType.Weapon, new Dictionary<int, PhysicalItemDataSO>());
        physicalItemData.Add(PhysicalItemTypeDefine.PhysicalItemType.Gear, new Dictionary<int, PhysicalItemDataSO>());
        physicalItemData.Add(PhysicalItemTypeDefine.PhysicalItemType.Treasures, new Dictionary<int, PhysicalItemDataSO>());
        physicalItemData.Add(PhysicalItemTypeDefine.PhysicalItemType.Drops, new Dictionary<int, PhysicalItemDataSO>());

        dropItemDataPerTier = new Dictionary<PhysicalItemTypeDefine.PhysicalItemType, List<PhysicalItemDataSO>[]>();
        foreach (PhysicalItemTypeDefine.PhysicalItemType itemType in physicalItemData.Keys)
        {
            dropItemDataPerTier.Add(itemType, new List<PhysicalItemDataSO>[3]);
            for (int i = 0; i < 3; i++)
            {
                dropItemDataPerTier[itemType][i] = new List<PhysicalItemDataSO>();
            }
        }

        foreach(PhysicalItemDataSO data in allPhysicalItemsList)
        {
            //アイテムの種類を取得
            PhysicalItemTypeDefine.PhysicalItemType itemType = data.PhysicalItemType.itemType;
            //辞書に追加
            physicalItemData[itemType].Add(data.SerialNum, data);

            //Tierごとに分類
            //ドロップしないアイテムはここで終了
            if(supplyItemsList.Contains(data))
            {
                continue;
            }
            if (data.Tier.tier == TierDefine.Tier.Common)
            {
                dropItemDataPerTier[itemType][0].Add(data);
            }
            else if (data.Tier.tier == TierDefine.Tier.Rare)
            {
                dropItemDataPerTier[itemType][1].Add(data);
            }
            else if (data.Tier.tier == TierDefine.Tier.Meta)
            {
                dropItemDataPerTier[itemType][2].Add(data);
            }
        }

        //アイテムデータが初期化されてから持ち物を初期化
        inventoryData.Init();
        storageData.Init();
    }

    /// <summary>
    /// ドロップするアイテムの辞書から、ランダムなアイテムを一つ、
    /// 指定したTierとタイプから取得する
    /// </summary>
    /// <param name="tier"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static PhysicalItemDataSO GetRandomPhysicalItemDataByTier(TierDefine.Tier tier, PhysicalItemTypeDefine.PhysicalItemType type)
    {
        List<PhysicalItemDataSO> itemList = dropItemDataPerTier[type][(int)tier];

        if (itemList.Count == 0)
        {
            Debug.Log("No items found for the specified tier and type.");
            return null;
        }

        //ランダムにアイテムを選択
        int randomIndex = Random.Range(0, itemList.Count);
        return itemList[randomIndex];
    }

    public static PhysicalItemDataSO GetPhysicalItemDataSO(int serialNum,PhysicalItemTypeDefine.PhysicalItemType type)
    {
        if (physicalItemData.ContainsKey(type) && physicalItemData[type].ContainsKey(serialNum))
        {
            return physicalItemData[type][serialNum];
        }
        else
        {
            Debug.LogError($"PhysicalItemDataSO not found for SerialNum: {serialNum}, Type: {type}");
            return null;
        }
    }
}
