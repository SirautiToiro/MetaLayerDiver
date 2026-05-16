using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

/// <summary>
/// バトル中の装備の管理
/// </summary>
public class GearManager : MonoBehaviour
{
    [SerializeField] private Transform gearParent; //装備の親オブジェクト
    [SerializeField] private GameObject gearPrefab;
    [SerializeField] private FieldManager fieldManager;
    [SerializeField] private PlayerBattleManager playerBattleManager; 

    //生成している装備
    private Gear gear;

    public void Init()
    {
        PhysicalItemDataSO pGear = InventoryData.GetEquippingGear();
        gear = InstantiateGear(pGear, gearParent);
    }

    private Gear InstantiateGear(PhysicalItemDataSO gearData, Transform gearParent)
    {
        //インスタンス化
        //親はPlaceCardZone
        GameObject gearObj = Instantiate(gearPrefab,gearParent.position, Quaternion.identity, gearParent);
        Gear gear = gearObj.GetComponent<Gear>();
        gear.Init(gearData, fieldManager,this);//バトル中の生成

        return gear;
    }

    public IStateInGear GetGearState()
    {
        return gear.GetState();
    }

    public void DecreaseStateInGear()
    {
        gear.DecreaseState();
    }

    public void ConsumeGearState(IStateInGear state, int value)
    {
        gear.ConsumeState(state, value);
    }

    public void ResetIStateInGearCountPerTurn()
    {
        gear.ResetIStateCountPerTurn();
    }

    public void DecreaseCountOfCountStateInGear(IStateHasCount cState)
    {
        gear.DecreaseCountOfCountState(cState);
    }

    public void Refresh()
    {
        //状態異常が更新されたなどで、
        playerBattleManager.RefreshCard();
        playerBattleManager.RefreshWeapon();
    }
}
