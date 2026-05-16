using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Gear : ItemBase
{
    [SerializeField] private GearUI gearUI;

    //消耗品データ
    private GearDataSO baseGearData;
    public GearDataSO BaseGearData { get { return baseGearData; } }
    //物理アイテムとしてのデータ
    private PhysicalItemDataSO basePItemData;//物理アイテムとしてのデータ
    public PhysicalItemDataSO BasePItemData { get { return basePItemData; } }

    public int serialNum;//シリアル番号

    private ItemMover itemMover = null;
    public override ItemMover ItemMover { get { return itemMover; } }

    private ShowDescriptionWhenRightClick itemRightClick;
    public override IItemRightClick ItemRightClick { get { return itemRightClick; } }

    private IStateInGear state;

    private GearManager gearManager;

    /// <summary>
    /// バトル中の初期化。
    /// ItemMoverの第2引数をtrueにすることで、カードの右クリックのみを許可
    /// ここでもCardZoneにはセットしない(必要ない)
    /// </summary>
    /// <param name="pGearData"></param>
    /// <param name="itemManager"></param>
    /// <param name="gearManager"></param>
    public void Init(PhysicalItemDataSO pGearData,IItemManager itemManager,GearManager gearManager)
    {
        this.itemManager = itemManager;
        basePItemData = pGearData;
        baseGearData = pGearData.GearData;

        itemMover = new ItemMover(this,true);
        itemRightClick = new ShowDescriptionWhenRightClick();

        clickableFlag = true;

        state = baseGearData.state.Clone(this);//コピーを取得
        state.value = BaseGearData.stateValue;//初期値を設定
        serialNum = pGearData.SerialNum;

        tier = pGearData.Tier;

        this.gearManager = gearManager;

        gearUI.Init(pGearData, state,this,true);
    }

    /// <summary>
    /// 初期化処理。
    /// clickableFlagをfalseにすることでカードは一切動かないようになる
    /// </summary>
    /// <param name="pGearData">装備情報のSO</param>
    public void Init(PhysicalItemDataSO pGearData)
    {
        basePItemData = pGearData;
        baseGearData = pGearData.GearData;

        clickableFlag = false;

        itemMover = new ItemMover(this);
        itemRightClick = new ShowDescriptionWhenRightClick();

        state = baseGearData.state.Clone(this);//コピーを取得
        state.value = BaseGearData.stateValue;//初期値を設定
        serialNum = pGearData.SerialNum;

        tier = pGearData.Tier;

        gearUI.Init(pGearData,state, this,false);
    }

    public override bool IsItemHasAttribute(AttributeDefine.Attribute attribute)
    {
        return false;
    }

    public override bool IsItemHasTag(CardTagDefine.CardTag cardTag)
    {
        return false;
    }

    public IStateInGear GetState()
    {
        return state;
    }

    public void ConsumeState(IStateInGear state, int value)
    {
        state.value -= value;
        gearUI.SetValue(state.value);//UIの値を更新

        //装備の状態異常は削除されない

        //バトル表示更新
        gearManager.Refresh();
    }

    public void ResetIStateCountPerTurn()
    {
        if(state is IStateCountPerTurn cState)
        {
            //ターンごとにカウントするステータスなら
            //カウントをリセット
            cState.IStateCounter.ResetCount();
            //UIに反映
            gearUI.SetValue(cState.IStateCounter.GetCount());
        }
    }

    public void DecreaseCountOfCountState(IStateHasCount cState)
    {
        //ターンごとにカウントするステータスなら
        //カウントを減少させる
        cState.IStateCounter.CountNum();
        //UIに反映
        gearUI.SetValue(cState.IStateCounter.GetCount());

        //表示リセット
        gearManager.Refresh();
    }

    public void DecreaseState()
    {
        if (state.TurnEndAdjust())
        {//値を変更したなら
            gearUI.SetValue(state.value);
        }
    }
}
