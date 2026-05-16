using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleDropsManager : MonoBehaviour
{
    //それぞれのTierの敵のドロップアイテムの割合
    [SerializeField] private DropValue commonEnemyDrops;
    [SerializeField] private DropValue rareEnemyDrops;
    [SerializeField] private DropValue metaEnemyDrops;

    //部屋ごとに追加でドロップする割合
    [SerializeField] private DropValue EnemyRoomDrops;
    [SerializeField] private DropValue TreasureRoomDrops;

    [System.Serializable]
    public class DropValue
    {
        // {Common, Rare, Meta}の順に、ドロップするカードの割合
        //40とある場合、40%の確率でドロップすることを意味する
        public int[] CardDropValue; 
        public int[] ConsumableDropValue;
        public int[] WeaponDropValue;
        public int[] GearDropValue;
        public int[] TreasuresDropValue;
        public int[] DropsDropValue;
    }

    
    /// <summary>
    /// 敵のドロップカードを取得する
    /// </summary>
    /// <param name="enemyNum">{Common,Rare,Meta}の順に、敵を倒した数</param>
    /// <param name="mapTileType">バトルがどのような場所で発生したか。敵、あるいは宝箱</param>
    /// <returns>ドロップカードのリスト(シリアル番号)</returns>
    public List<int> GetDroppedCards(int[] enemyNum, MapTileDefine.MapTile mapTileType)
    {
        List<int> dropCards = new List<int>();


        //敵のTierごとに
        for(int i = 0; i < enemyNum.Length; i++)
        {
            //Tierごとの敵の数ごとに
            for(int j=enemyNum[i]; j > 0; j--)
            {
                //ドロップするカードの乱数を取得
                int dropValue = UnityEngine.Random.Range(0, 100);

                DropValue dropTable = GetDropTable(i);

                //ドロップ表のそれぞれのレア度ごとに
                for(int k = 0; k < 3; k++)
                {
                    dropValue -= dropTable.CardDropValue[k];
                    //ドロップ表のレア度の値が0以下になったら、そのレア度のカードをドロップ
                    if (dropValue <= 0)
                    {
                        dropCards.Add(PlayerCardData.GetRandomCardSerialNumByTier((TierDefine.Tier)k));
                        break; // 一度ドロップしたらループを抜ける
                    }
                }
            }
        }

        return dropCards;
    }

    public List<PhysicalItemDataSO> GetDroppedItems(int[] enemyNum, MapTileDefine.MapTile mapTileType)
    {
        List<PhysicalItemDataSO> dropItems = new List<PhysicalItemDataSO>();
        //敵のTierごとに
        for (int i = 0; i < enemyNum.Length; i++)
        {
            //Tierごとの敵の数ごとに
            for (int j = enemyNum[i]; j > 0; j--)
            {
                //ドロップするアイテムの乱数を取得
                int dropValue = UnityEngine.Random.Range(0, 100);
                DropValue dropTable = GetDropTable(i);
                //消耗品などのドロップを計算
                for (int k = 0; k < 3; k++)
                {
                    dropValue -= dropTable.ConsumableDropValue[k];
                    //ドロップ表のレア度の値が0以下になったら、そのレア度のアイテムをドロップ
                    if (dropValue <= 0)
                    {
                        PhysicalItemDataSO dropItem = PhysicalItemData.GetRandomPhysicalItemDataByTier((TierDefine.Tier)k,
                            PhysicalItemTypeDefine.PhysicalItemType.Consumables);
                        if (dropItem != null)
                        {
                            dropItems.Add(dropItem);
                        }
                        break; // 一度ドロップしたらループを抜ける
                    }
                }
                for (int k = 0; k < 3; k++)
                {
                    dropValue -= dropTable.WeaponDropValue[k];
                    //ドロップ表のレア度の値が0以下になったら、そのレア度のアイテムをドロップ
                    if (dropValue <= 0)
                    {
                        PhysicalItemDataSO dropItem = PhysicalItemData.GetRandomPhysicalItemDataByTier((TierDefine.Tier)k,
                            PhysicalItemTypeDefine.PhysicalItemType.Weapon);
                        if (dropItem != null)
                        {
                            dropItems.Add(dropItem);
                        }
                        break; // 一度ドロップしたらループを抜ける
                    }
                }
                for (int k = 0; k < 3; k++)
                {
                    dropValue -= dropTable.GearDropValue[k];
                    //ドロップ表のレア度の値が0以下になったら、そのレア度のアイテムをドロップ
                    if (dropValue <= 0)
                    {
                        PhysicalItemDataSO dropItem = PhysicalItemData.GetRandomPhysicalItemDataByTier((TierDefine.Tier)k,
                            PhysicalItemTypeDefine.PhysicalItemType.Gear);
                        if (dropItem != null)
                        {
                            dropItems.Add(dropItem);
                        }
                        break; // 一度ドロップしたらループを抜ける
                    }
                }
                for (int k = 0; k < 3; k++)
                {
                    dropValue -= dropTable.TreasuresDropValue[k];
                    //ドロップ表のレア度の値が0以下になったら、そのレア度のアイテムをドロップ
                    if (dropValue <= 0)
                    {
                        PhysicalItemDataSO dropItem = PhysicalItemData.GetRandomPhysicalItemDataByTier((TierDefine.Tier)k,
                            PhysicalItemTypeDefine.PhysicalItemType.Treasures);
                        if (dropItem != null)
                        {
                            dropItems.Add(dropItem);
                        }
                        break; // 一度ドロップしたらループを抜ける
                    }
                }
                for (int k = 0; k < 3; k++)
                {
                    dropValue -= dropTable.DropsDropValue[k];
                    //ドロップ表のレア度の値が0以下になったら、そのレア度のアイテムをドロップ
                    if (dropValue <= 0)
                    {
                        PhysicalItemDataSO dropItem = PhysicalItemData.GetRandomPhysicalItemDataByTier((TierDefine.Tier)k,
                            PhysicalItemTypeDefine.PhysicalItemType.Drops);
                        if (dropItem != null)
                        {
                            dropItems.Add(dropItem);
                        }
                        break; // 一度ドロップしたらループを抜ける
                    }
                }
            }
        }
        return dropItems;
    }

    private DropValue GetDropTable(int tier)
    {
        switch (tier)
        {
            case 0:
                return commonEnemyDrops;
            case 1:
                return rareEnemyDrops;
            case 2:
                return metaEnemyDrops;
            default:
                Debug.LogError("Invalid tier value: " + tier);
                return null;
        }
    }

    public void DropTest()
    {
        List<int> testDropCards = GetDroppedCards(new int[] { 0,3,0}, MapTileDefine.MapTile.Enemy);

        foreach (int serialNum in testDropCards)
        {
            Debug.Log(PlayerCardData.GetCardDataFromSerialNum(serialNum));
        }
    }

    public void TestDropRateDump()
    {
        DropValue[] targets = new DropValue[] { commonEnemyDrops, rareEnemyDrops, metaEnemyDrops, EnemyRoomDrops, TreasureRoomDrops };

        Debug.Log("Common Enemy:\nCard: " + string.Join(", ", targets[0].CardDropValue)+
            "\nConsumable: "+ string.Join(", ", targets[0].ConsumableDropValue)+
            "\nWeapon: "+ string.Join(", ", targets[0].WeaponDropValue) +
            "\nGear: "+ string.Join(", ", targets[0].GearDropValue)+
            "\nTreasures: "+ string.Join(", ", targets[0].TreasuresDropValue) +
            "\nDrops: "+ string.Join(", ", targets[0].DropsDropValue));
        Debug.Log("Rare Enemy:\nCard: " + string.Join(", ", targets[1].CardDropValue) +
            "\nConsumable: "+ string.Join(", ", targets[1].ConsumableDropValue) +
            "\nWeapon: "+ string.Join(", ", targets[1].WeaponDropValue) +
            "\nGear: "+ string.Join(", ", targets[1].GearDropValue) +
            "\nTreasures: "+ string.Join(", ", targets[1].TreasuresDropValue) +
            "\nDrops: "+ string.Join(", ", targets[1].DropsDropValue));
        Debug.Log("Meta Enemy:\nCard: " + string.Join(", ", targets[2].CardDropValue) +
            "\nConsumable: "+ string.Join(", ", targets[2].ConsumableDropValue) +
            "\nWeapon: "+ string.Join(", ", targets[2].WeaponDropValue) +
            "\nGear: "+ string.Join(", ", targets[2].GearDropValue) +
            "\nTreasures: "+ string.Join(", ", targets[2].TreasuresDropValue) +
            "\nDrops: "+ string.Join(", ", targets[2].DropsDropValue));
        Debug.Log("Enemy Room:\nCard: " + string.Join(", ", targets[3].CardDropValue) +
            "\nConsumable: "+ string.Join(", ", targets[3].ConsumableDropValue) +
            "\nWeapon: "+ string.Join(", ", targets[3].WeaponDropValue) +
            "\nGear: "+ string.Join(", ", targets[3].GearDropValue) +
            "\nTreasures: "+ string.Join(", ", targets[3].TreasuresDropValue) +
            "\nDrops: "+ string.Join(", ", targets[3].DropsDropValue));
        Debug.Log("Treasure Room:\nCard: " + string.Join(", ", targets[4].CardDropValue) +
            "\nConsumable: "+ string.Join(", ", targets[4].ConsumableDropValue) +
            "\nWeapon: "+ string.Join(", ", targets[4].WeaponDropValue) +
            "\nGear: "+ string.Join(", ", targets[4].GearDropValue) +
            "\nTreasures: "+string.Join(", ", targets[4].TreasuresDropValue) +
            "\nDrops: "+ string.Join(", ", targets[4].DropsDropValue));
    }
}
