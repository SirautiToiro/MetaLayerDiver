using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//物理アイテムの生成処理を行う。それぞれのサイズに対応するプレハブを持っている
public class PhysicalItemInstantiateManager : MonoBehaviour
{
    //アイテムサイズごとのプレハブ
    [SerializeField] private GameObject physicalItem1_1Prefab;
    [SerializeField] private GameObject physicalItem1_2Prefab;
    [SerializeField] private GameObject physicalItem2_1Prefab;
    [SerializeField] private GameObject physicalItem4_1Prefab;

    /// <summary>
    /// 物理アイテムの生成処理を行う
    /// </summary>
    /// <param name="itemData">生成するデータ</param>
    /// <param name="pos">生成する位置</param>
    /// <param name="parent">生成されたものの配置される親</param>
    /// <returns>生成されたデータ</returns>
    public PhysicalItemBase InstantiatePhysicalItem(PhysicalItemDataSO itemData,Vector3 pos,
        Transform parent)
    {
        PhysicalItemBase item = null;
        if (itemData.SizeX == 1 && itemData.SizeY == 1)
        {
            item = Instantiate(physicalItem1_1Prefab, pos, Quaternion.identity,
            parent).GetComponent<PhysicalItemBase>();
        }
        else if (itemData.SizeX == 1 && itemData.SizeY == 2)
        {
            item = Instantiate(physicalItem1_2Prefab, pos, Quaternion.identity,
                        parent).GetComponent<PhysicalItemBase>();
        }
        else if (itemData.SizeX == 2 && itemData.SizeY == 1)
        {
            item = Instantiate(physicalItem2_1Prefab, pos, Quaternion.identity,
                        parent).GetComponent<PhysicalItemBase>();
        }
        else if (itemData.SizeX == 4 && itemData.SizeY == 1)
        {
            item = Instantiate(physicalItem4_1Prefab, pos, Quaternion.identity,
                        parent).GetComponent<PhysicalItemBase>();
        }

        return item;
    }
}
