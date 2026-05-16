using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using static TierDefine;

public class Consumable : ItemBase
{
    [SerializeField] private ConsumableUI consumableUI;
    [SerializeField] private ConsumableQuickDescription consumableQuickDescription;


    //消耗品データ
    private ConsumablesDataSO baseConsumableData;
    public ConsumablesDataSO BaseConsumableData { get { return baseConsumableData; } }
    //物理アイテムとしてのデータ
    private PhysicalItemDataSO basePItemData;//物理アイテムとしてのデータ
    public PhysicalItemDataSO BasePItemData { get { return basePItemData; } }

    public List<ActualEffect> Effects;    //アイテム効果のリスト

    public int Cost { get; set; }//武器コスト
    private int pos;//配置される場所。バトル中のみ

    public TargetDefine effectTarget;//武器の効果対象
    public int serialNum;//シリアル番号

    private ItemMover itemMover = null;
    public override ItemMover ItemMover { get { return itemMover; } }

    private ShowDescriptionWhenRightClick itemRightClick;
    public override IItemRightClick ItemRightClick { get { return itemRightClick; } }

    /// <summary>
    /// 初期化処理。バトル中
    /// </summary>
    /// <param name="pConsumableData">データ</param>
    /// <param name="cardZone">配置される場所</param>
    /// <param name="itemManager">生成元</param>
    /// <param name="_pos">Consumableの位置</param>
    public void Init(PhysicalItemDataSO pConsumableData, ICardZone cardZone, IItemManager itemManager, int pos)
    {
        clickableFlag = true;

        this.itemManager = itemManager;

        SetData(pConsumableData);
        this.pos = pos;

        SetCardZone(cardZone);//カードゾーンにセット

        consumableQuickDescription.SetText(this);
        consumableUI.Init(pConsumableData, this);
    }

    /// <summary>
    /// 初期化処理。
    /// clickableFlagをfalseにすることでカードは一切動かないようになる
    /// </summary>
    /// <param name="pConsumableData">消耗品情報のSO</param>
    public void Init(PhysicalItemDataSO pConsumableData)
    {
        clickableFlag = false;

        SetData(pConsumableData);

        consumableQuickDescription.SetText(this);
        //カーソルを重ねると出る説明も表示しない
        consumableQuickDescription.DescriptionOff();
        consumableUI.Init(pConsumableData, this);
    }

    private void SetData(PhysicalItemDataSO pConsumableData)
    {
        itemMover = new ItemMover(this);
        itemRightClick = new ShowDescriptionWhenRightClick();

        ConsumablesDataSO consumableData = pConsumableData.ConsumablesData;
        basePItemData = pConsumableData;

        //効果リストを移動
        foreach (var e in consumableData.EffectList)
        {
            Effects.Add(new ActualEffect(e.Effect, e.Effect.value,e.UseState));
        }

        Cost = consumableData.Cost;
        effectTarget = consumableData.TargetDefine;
        serialNum = pConsumableData.SerialNum;

        baseConsumableData = consumableData;

        tier = pConsumableData.Tier;
    }

    public override bool IsItemHasAttribute(AttributeDefine.Attribute attribute)
    {
        return false;//消費アイテムは属性を持たない
    }

    public override bool IsItemHasTag(CardTagDefine.CardTag cardTag)
    {
        return false;//消費アイテムはタグを持たない
    }

    public int GetPos()
    {
        return pos;
    }

    /// <summary>
    /// 武器にマウスが重なった場合
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (clickableFlag)
        {//操作可能なら
            base.OnPointerEnter(eventData);//元の動作

            //簡易説明オン
            consumableQuickDescription.DescriptionOn();
        }
    }

    /// <summary>
    /// 武器からマウスが離れた場合
    /// </summary>
    /// <param name="eventData"></param>
    public override void OnPointerExit(PointerEventData eventData)
    {
        if (clickableFlag)
        {//操作可能なら

            base.OnPointerExit(eventData);//元の動作

            consumableQuickDescription.DescriptionOff();
        }
    }
}
