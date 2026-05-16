using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 様々な大きさのアイテムの抽象クラス
/// 必ずPhysicalItemArrangeManagerによって操作される
/// </summary>
public abstract class PhysicalItemBase : ItemBase
{
    private PhysicalItemDataSO _baseItemData;

    public PhysicalItemDataSO BaseItemData { get { return _baseItemData; }}

    [SerializeField] private PhysicalItemUI physicalItemUI;

    public PhysicalItemUI PhysicalItemUI { get { return physicalItemUI; } }

    //このアイテムの縦横の大きさ
    public abstract int ItemSizeX { get; }
    public abstract int ItemSizeY { get; }

    //このアイテムの種類
    private PhysicalItemTypeDefine.PhysicalItemType itemType;
    public PhysicalItemTypeDefine.PhysicalItemType ItemType { get { return itemType; }}

    /// <summary>
    /// アイテムのスタックする上限
    /// </summary>
    private int stackMax;
    public int StackMax { get { return stackMax; } }

    //スタックしている数
    private int stack;
    public int Stack { get { return stack; }}

    //管理番号
    private int serialNum;
    public int SerialNum {  get { return serialNum; } }

    private ItemMover _itemMover;
    public override ItemMover ItemMover { get { return _itemMover; } }

    private ShowMenuWhenRightClick _itemRightClick;
    public override IItemRightClick ItemRightClick { get { return _itemRightClick; } }

    //cardZoneがICardZoneなのは、IPhysicalItemZoneだと、アイテム分割時に一時的に配置するPlaceCardZoneに対応できないため。
    public void Init(PhysicalItemDataSO itemData, ICardZone cardZone, PhysicalItemArrangeManager _itemManager,int stack)
    {
        _baseItemData = itemData;

        clickableFlag = true;

        //移動機能、右クリックメニュー機能の初期化
        _itemRightClick = new ShowMenuWhenRightClick();
        _itemMover = new ItemMover(this);

        //スタック最大値確保
        stackMax = itemData.StackMax;

        itemManager = _itemManager;

        //カードゾーンに設定
        SetCardZone(cardZone);

        //管理番号セット
        serialNum = itemData.SerialNum;

        physicalItemUI.Init(itemData, stack,this);

        this.stack = stack;

        this.tier = itemData.Tier;

        itemType = itemData.PhysicalItemType.itemType;
    }

    /// <summary>
    /// 初期化処理
    /// clickableFlagをfalseにすることでアイテムは一切動かないようになる
    /// </summary>
    /// <param name="itemData">アイテム情報の元となるSO</param>
    public void Init(PhysicalItemDataSO itemData)
    {
        clickableFlag = false;

        _itemRightClick = new ShowMenuWhenRightClick();
        _itemMover = new ItemMover(this);

        //スタック最大値確保
        stackMax = itemData.StackMax;

        //表示用なのでスタックは0
        stack = 0;

        this.tier = itemData.Tier;

        //管理番号セット
        serialNum = itemData.SerialNum;

        physicalItemUI.Init(itemData,0,this);

        itemType = itemData.PhysicalItemType.itemType;
    }

    public PhysicalItemDataSO GetItemData()
    {
        return _baseItemData;
    }

    /// <summary>
    /// アイテムが配置された際の、アイテムサイズの変形などを行う
    /// </summary>
    /// <param name="holder">アイテムが配置された場所</param>
    public abstract void PutToHolder(IPhysicalItemHolder holder);

    /// <summary>
    /// アイテムが動かされた際の、アイテムサイズの変形などを行う
    /// </summary>
    public abstract void PullFromHolder();

    public void SetStack(int num)
    {
        stack = num;
        physicalItemUI.SetStack(num);
    }
}
