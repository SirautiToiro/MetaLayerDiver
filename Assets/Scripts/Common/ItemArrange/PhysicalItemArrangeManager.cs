using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

/// <summary>
/// アイテムの移動画面の物理的アイテムを制御する部分
/// PhysicalItemから必ず呼ばれる
/// </summary>
public class PhysicalItemArrangeManager : MonoBehaviour, IItemManager
{
    //ドラッグ時に親を最前に操作する機能
    [SerializeField] private ParentManagerInDragging parentManagerInDragging;

    //インベントリ全般の操作クラス
    [SerializeField] private ItemArrangeManager itemArrangeManager;

    [SerializeField] private StashPanel stashPanel;

    private Camera mainCamera;//所属しているシーンのカメラ。Initで取得

    //ドラッグしているアイテム
    private PhysicalItemBase draggingItem;
    //ドラッグしているアイテムが元はどこにあったか
    private IPhysicalItemHolder draggingItemHolder;

    //ドラッグしているアイテムが元あった位置
    private int startPosX;
    private int startPosY;

    //接触判定クラス
    private OverlappedCardzoneGetter overlappedCardzoneGetter;

    [SerializeField] private DiscardItemHolder discardItemHolder; //アイテム削除用のHolder

    //クイック移動用
    [SerializeField] private InventoryBackpackItemsHolder inventoryBackpackItemsHolder;
    [SerializeField] private InventoryEquipItemsHolder inventoryEquipItemsHolder;
    [SerializeField] private DroppedItemHolder droppedItemHolder;
    [SerializeField] private StorageItemsHolder storageItemsHolder;
    [SerializeField] private RequirePhysicalItemHolder requirePhysicalItemHolder;

    //右クリックメニューのプレハブ
    [SerializeField] private GameObject miniMenuPrefab;
    //右クリックメニューの配置場所
    [SerializeField] private Transform miniMenuParentTransform;

    //アイテム分割メニュー
    [SerializeField] private GameObject itemDivideMenuObject;
    [SerializeField] private ItemDivideMenu itemDivideMenu;

    //アイテムプレハブ生成のためのマネージャー
    [SerializeField] private PhysicalItemInstantiateManager physicalItemInstantiateManager;

    [SerializeField] private Transform popupMessageParent; //ポップアップメッセージの配置場所
    [SerializeField] private GameObject popupMessageWithTogglePrefab; //ポップアップメッセージのプレハブ

    //アイテム削除画面用
    private bool discardingFlag;
    //削除画面に移行する前に持っていたデータ
    private (List<PhysicalItemDataSO> weapons, PhysicalItemDataSO gear, List<PhysicalItemDataSO> consumables) equipsBeforeDiscard;
    private List<PhysicalItemGridPosNumData> backpackItemsBeforeDiscard;

    [SerializeField] private UIPageManager uiPageManager;

    public void Init(Camera camera)
    {
        draggingItemHolder = null;
        draggingItem = null;

        mainCamera = camera;

        discardingFlag = false;
        discardItemHolder.gameObject.SetActive(false); //カード削除Holderを無効化

        itemDivideMenuObject.SetActive(false); //分割メニューを非表示にする

        overlappedCardzoneGetter = new OverlappedCardzoneGetter(mainCamera, parentManagerInDragging);
    }

    #region ドラッグ関連

    public void StartDragging(ItemBase item)
    {
        //不適切なItemの排除
        if (item is not PhysicalItemBase) return;
        if (item.CurrentZone is not IPhysicalItemZone) return;

        //Shopからの取り出しだった場合、取り出し可能かを判定
        if (((IPhysicalItemZone)item.CurrentZone).ItemHolder is ShopItemHolder shopItemHolder)
        {
            if (shopItemHolder.TryBuyItem((PhysicalItemBase)item))
            {
                //購入成功
            }
            else
            {
                //購入失敗
                //ドラッグを開始しない
                return;
            }
        }

        if (!discardingFlag)
        {//削除中でないなら
            //ドラッグ前のデータを保存
            equipsBeforeDiscard = inventoryEquipItemsHolder.GetEquips();
            backpackItemsBeforeDiscard = inventoryBackpackItemsHolder.GetItems();
        }

        draggingItem = (PhysicalItemBase)item;

        //アイテムを移動開始したなら
        //ドラッグ中のHolderを取得
        draggingItemHolder = ((IPhysicalItemZone)item.CurrentZone).ItemHolder;

        (int x, int y)? pos = null;
        if (draggingItemHolder is IGridPhysicalItemHolder gridHolder)
        {//グリッド上のアイテムを移動開始したなら
            //グリッド上の位置でドラッグ前の位置を取得
            pos = gridHolder.GetItemPos(draggingItem);
        }
        else if (item.CurrentZone is EquipCardZone equipZone)
        {//装備されているアイテムなら
            pos = equipZone.GetPos();//装備カードゾーンの位置を取得
        }
        else
        {
            pos = null;//位置が取得できない
        }

        if (pos is null)
        {
            return;//アイテムがなぜかその位置になかった
        }

        //ドラッグ前の位置を取得
        (startPosX, startPosY) = (pos.Value.x, pos.Value.y);

        //GridのHolderから取り出し
        draggingItemHolder.PullItem(startPosX, startPosY);

        parentManagerInDragging.SetParent(item);//親操作

        draggingItem.PullFromHolder();//移動開始時のアイテム変形
    }

    private void StartSwitchDragging(ItemBase item)
    {
        //不適切なItemの排除
        if (item is not PhysicalItemBase) return;

        draggingItem = (PhysicalItemBase)item;

        if (draggingItem.CurrentZone is GridCardZone)
        {//グリッド上のアイテムを移動開始したなら
            //ドラッグ前の位置に意味はないので、枠の外に配置
            (startPosX, startPosY) = (-1, -1);

            parentManagerInDragging.SetParent(item);//親操作
        }
        else if (draggingItem.CurrentZone is EquipCardZone)
        {//TODO:装備アイテムの移動
            //ドラッグ前の位置に意味はないので、枠の外に配置
            (startPosX, startPosY) = (-1, -1);

            parentManagerInDragging.SetParent(item);//親操作
        }
        else
        {//アイテムスタック増加時などの仮配置場所からの移動
            (startPosX, startPosY) = (-1, -1);

            parentManagerInDragging.SetParent(item);//親操作
        }

        draggingItem.PullFromHolder();//移動開始時のアイテム変形
    }

    public void UpdateItemDragging()
    {
        //移動中の演出は特になし
    }
    public bool EndDragging()
    {
        //parentManagerInDragging.ReturnToBaseParent();//必要ないが、一応元のParentに戻す。
        //ドラッグ＝＞交換＝＞配置失敗の時にドラッグ状態を継続したいので戻さない

        //重なっているIPhysicalItemZoneを取得
        IPhysicalItemZone targetCardZone = overlappedCardzoneGetter.GetOverlappedCardZone<IPhysicalItemZone>(draggingItem);



        //配置
        if (targetCardZone == null)
        {//どこにも重なっていない
            if (itemArrangeManager.GetCaller().DungeonManager is not null)
            {//ダンジョン中ならカード削除画面に移行
                if (!discardingFlag)
                {//削除中でないなら
                 //アイテム削除画面へ送る
                    discardingFlag = true;

                    discardItemHolder.gameObject.SetActive(true);//カード削除Holderを有効化
                                                                 //削除位置にセット。左上の場所に配置。
                    discardItemHolder.Init(draggingItem);

                    draggingItem = null;
                }
                else
                {//削除中で画面外に移動した場合、元の場所に戻す
                    SetItemResult result = draggingItemHolder.PutItem(draggingItem, startPosX, startPosY);
                    if (result.IsSuccess == false)
                    {//元に戻すアイテムの配置に失敗している
                     //ドラッグ状態継続
                        return false;
                    }
                    else
                    {
                        //元の場所に戻せたので配置時変形指示
                        draggingItem.PutToHolder(draggingItemHolder);
                    }
                }
            }
            else if (draggingItemHolder is ShopItemHolder)
            {//店から取り出したアイテムの場合、移動を継続(配置しない)
                return false;
            }
            else
            {//それ以外なら元の場所に戻す
                SetItemResult result = draggingItemHolder.PutItem(draggingItem, startPosX, startPosY);
                if (result.IsSuccess == false)
                {//元に戻すアイテムの配置に失敗している
                 //ドラッグ状態継続
                    return false;
                }
                else
                {
                    //元の場所に戻せたので配置時変形指示
                    draggingItem.PutToHolder(draggingItemHolder);
                }
            }
        }
        else
        {//何かのHolderの場所に移動した
            SetItemResult result = new SetItemResult(false);
            if (targetCardZone.ItemHolder is ShopItemHolder)
            {//店には配置ができないので自動で失敗
                result = new SetItemResult(false);
            }
            else
            {
                //通常のアイテム配置
                result = targetCardZone.ItemHolder.PutItem(draggingItem, targetCardZone);
            }


            if (result.IsSuccess == false)
            {//アイテムの配置に失敗している
                //元あった場所がShopだった場合、元の場所には戻さず、ドラッグ続行
                if (draggingItemHolder is ShopItemHolder)
                {
                    return false;
                }


                //元の場所に戻す
                SetItemResult result2 = draggingItemHolder.PutItem(draggingItem, startPosX, startPosY);
                if (result2.IsSuccess == false)
                {//元に戻すアイテムの配置に失敗している
                    //ドラッグ状態継続
                    return false;
                }
                else
                {
                    //元の場所に戻せたので配置時変形指示
                    draggingItem.PutToHolder(draggingItemHolder);
                }
            }
            else if (result.AlternativeItem is not null)
            {//アイテム交換が発生したなら
                //ドラッグしていたアイテムに配置指示(変形が必要なら変形する指示)
                draggingItem.PutToHolder(targetCardZone.ItemHolder);

                if (ReferenceEquals(result.AlternativeItem.Item, draggingItem))
                {//交換したアイテムがドラッグしていたアイテムと同じなら
                    //そのアイテムで交換移動開始
                    StartSwitchDragging(result.AlternativeItem.Item);
                    result.AlternativeItem.Item.ItemMover.StartSwitchMoving();

                    //ドラッグ状態を継続する
                    return false;
                }
                else
                {
                    //交換移動開始
                    StartSwitchDragging(result.AlternativeItem.Item);
                    result.AlternativeItem.Item.ItemMover.StartSwitchMoving();
                }
            }
            else
            {
                //ドラッグしていたアイテムに配置指示(変形が必要なら変形する指示)
                draggingItem.PutToHolder(targetCardZone.ItemHolder);
            }
        }

        //ドラッグ正常終了
        return true;
    }

    /// <summary>
    /// 高速移動メイン処理
    /// 内部でQuickMove(ItemBase item,IPhysicalItemZone beforeZone,IGridPhysicalItemHolder afterHolder)
    /// を使用
    /// Shift+Clickの高速移動
    /// Equip->Backpackと移動
    /// 倉庫など->Backpackの移動
    /// など、itemの元あった場所と開かれている画面によって移動先を判定して、
    /// 内部のQuickMoveに送る
    /// </summary>
    public void QuickMove(ItemBase item)
    {
        if (item is not PhysicalItemBase pItem) return;

        IPhysicalItemZone targetZone = item.CurrentZone as IPhysicalItemZone;
        IPhysicalItemHolder itemHolder = targetZone.ItemHolder;

        //Shopからの取り出しだった場合、取り出し可能かを判定
        //クイック移動は全スタックを移動する
        if (targetZone.ItemHolder is ShopItemHolder shopItemHolder)
        {
            if (shopItemHolder.TryBuyItem(pItem,pItem.Stack))
            {
                //購入成功
            }
            else
            {
                //購入失敗
                //ドラッグを開始しない
                return;
            }
        }

        if (itemArrangeManager.GetCaller().DungeonManager is not null)
        {//ダンジョン中なら
            if (discardingFlag)
            {//削除中なら
                if (itemHolder is DiscardItemHolder)
                {//削除アイテムの移動->バックパックへ移動
                    QuickMove(pItem, targetZone, inventoryBackpackItemsHolder);
                }
                else
                {
                    //その他のアイテム->削除場所へ移動
                    QuickMove(pItem, targetZone, discardItemHolder);
                }
            }
            else
            {//削除中でないなら
                if (itemHolder is InventoryEquipItemsHolder)
                {//装備欄にあるアイテムの移動->バックパックへ移動
                    QuickMove(pItem, targetZone, inventoryBackpackItemsHolder);
                }
            }
        }
        else if (itemArrangeManager.GetCaller().FieldManager is not null)
        {//戦闘中なら
            if (itemHolder is InventoryEquipItemsHolder)
            {//装備欄にあるアイテムの移動->所持欄へ移動
                QuickMove(pItem, targetZone, inventoryBackpackItemsHolder);
            }
            else if (itemHolder is DroppedItemHolder)
            {//ドロップ画面->所持欄へ移動
                QuickMove(pItem, targetZone, inventoryBackpackItemsHolder);
            }
        }
        else if (itemArrangeManager.GetCaller().VillageManager is not null)
        {//村画面なら
            var topPage = uiPageManager.GetTopPage();

            if (topPage is InventoryAndShopUIPage)
            {//店画面なら
                if (stashPanel.GetCurrentShopManager() is null) return;

                if (itemHolder is InventoryEquipItemsHolder ||
                    itemHolder is InventoryBackpackItemsHolder)
                {//インベントリ内なら
                    //高速移動の先になりうるIPhysicalItemHolder
                    IPhysicalItemHolder afterHolder = stashPanel.GetCurrentShopManager().GetPhysicalItemHolderForQuickMove();
                    Debug.Log("testA");
                    if (afterHolder is null)
                    {//移動先がないので手持ちでの移動
                        if (itemHolder is InventoryEquipItemsHolder)
                        {//装備欄にあるアイテムの移動->バックパックへ移動
                            QuickMove(pItem, targetZone, inventoryBackpackItemsHolder);
                        }
                    }
                    else
                    {//移動先がある場合はそこへ
                        Debug.Log("testB");

                        QuickMove(item, targetZone, (IGridPhysicalItemHolder)afterHolder);
                    }
                }
                else if (itemHolder is ShopItemHolder)
                {//店の場所ならバックパックへ
                    Debug.Log("testC");

                    QuickMove(item, targetZone, inventoryBackpackItemsHolder);
                }
            }
            else if (topPage is InventoryAndStashUIPage)
            {//倉庫画面なら
                //カード倉庫画面なら何もしない
                if (stashPanel.GetCurrentPage() == 0)
                {
                    return;
                }

                if (itemHolder is InventoryEquipItemsHolder ||
                    itemHolder is InventoryBackpackItemsHolder)
                {//装備欄にあるアイテムの移動->倉庫へ移動
                 //バックパックにあるアイテム->倉庫へ移動
                    QuickMove(pItem, targetZone, storageItemsHolder);
                }
                else if (itemHolder is StorageItemsHolder || itemHolder is RequirePhysicalItemHolder)
                {//倉庫,アイテム要求->バックパックへ移動
                    QuickMove(pItem, targetZone, inventoryBackpackItemsHolder);
                }
            }
            else if (topPage is InventoryAndRewardUIPage)
            {//報酬画面なら
                if (itemHolder is DroppedItemHolder)
                {//クエスト報酬画面->所持欄へ移動
                    QuickMove(pItem, targetZone, inventoryBackpackItemsHolder);
                }
                else if (itemHolder is InventoryEquipItemsHolder)
                {//装備欄にあるアイテムの移動->所持欄へ移動
                    QuickMove(pItem, targetZone, inventoryBackpackItemsHolder);
                }
            }
            else if (topPage is InventoryAndRequireItemsUIPage)
            {//要求画面なら
                if (itemHolder is InventoryEquipItemsHolder ||
                    itemHolder is InventoryBackpackItemsHolder)
                {//装備欄にあるアイテムの移動->倉庫へ移動
                 //バックパックにあるアイテム->倉庫へ移動
                    QuickMove(pItem, targetZone, requirePhysicalItemHolder);
                }
                else if (itemHolder is RequirePhysicalItemHolder)
                {//アイテム要求->バックパックへ移動
                    QuickMove(pItem, targetZone, inventoryBackpackItemsHolder);
                }
            }
        }
    }

    /// <summary>
    /// 高速移動処理。それまでアイテムがあったZoneとそれ以降に配置されるholderを指定しての移動
    /// アイテムドロップの処理で、外部からも使用。
    /// </summary>
    /// <param name="item">移動するアイテム</param>
    /// <param name="beforeZone">移動前のZone</param>
    /// <param name="afterHolder">移動後のHolder</param>
    public void QuickMove(ItemBase item, IPhysicalItemZone beforeZone, IGridPhysicalItemHolder afterHolder)
    {
        if (item is not PhysicalItemBase pItem) return;

        //要求を持つHolderなら、配置に失敗するかを調べる
        if (afterHolder is RequirePhysicalItemHolder)
        {
            RequirePhysicalItemHolder rHolder = (RequirePhysicalItemHolder)afterHolder;
            if (rHolder.PuttableNum((PhysicalItemBase)item) <= 0)
            {
                return;
            }
        }

        //既に存在する同じアイテム,
        List<(PhysicalItemBase, int)> existItemsAndMaxStack = new List<(PhysicalItemBase, int)>();

        if (afterHolder is RequirePhysicalItemHolder)
        {//要求を持つHolderなら、スタック最大値は配置できる最大値で計算する
            RequirePhysicalItemHolder rHolder = (RequirePhysicalItemHolder)afterHolder;

            existItemsAndMaxStack = rHolder.SearchSameItemsAndStackMax(pItem);
        }
        else
        {
            List<PhysicalItemBase> existItems = afterHolder.SearchSameItems(pItem);

            foreach (PhysicalItemBase existItem in existItems)
            {
                int stackMax = existItem.BaseItemData.StackMax;
                existItemsAndMaxStack.Add((existItem, stackMax));
            }
        }

        if (existItemsAndMaxStack is not null)
        {//バックパックに同じアイテムがあるなら
         //スタックだけ移動する
            foreach ((PhysicalItemBase, int) existItem in existItemsAndMaxStack)
            {//すべての空いている場所を検索
                int stackMax = existItem.Item2;

                if (pItem.Stack + existItem.Item1.Stack > stackMax)
                {//スタック数が上限を超えるなら、上限まで移動
                    int moveStack = stackMax - existItem.Item1.Stack;
                    existItem.Item1.SetStack(stackMax);//スタック数を上限にする
                    pItem.SetStack(pItem.Stack - moveStack);//移動元のアイテムのスタック数を減らす

                    //その後、pItemは残った分が別のスタックに高速移動動作を繰り返す

                    if (afterHolder is RequirePhysicalItemHolder)
                    {//要求を持つHolderなら、要求配置を更新
                        RequirePhysicalItemHolder rHolder = (RequirePhysicalItemHolder)afterHolder;
                        rHolder.RefreshStack();
                    }
                }
                else
                {//スタック数が上限以下なら、全て移動
                    existItem.Item1.SetStack(existItem.Item1.Stack + pItem.Stack);

                    //移動するアイテムを削除する(元の場所があるなら)
                    if (beforeZone is not null) beforeZone.ItemHolder.PullItem(pItem);

                    Destroy(pItem.gameObject);//アイテムを削除する

                    //移動先のデータを記録する
                    ((IPhysicalItemZone)(existItem.Item1.CurrentZone)).ItemHolder.SetData();

                    if (afterHolder is RequirePhysicalItemHolder)
                    {//要求を持つHolderなら、要求配置を更新
                        RequirePhysicalItemHolder rHolder = (RequirePhysicalItemHolder)afterHolder;
                        rHolder.RefreshStack();
                    }

                    return;
                }
            }

        }

        var puttableZone = afterHolder.GetEmptyZone(pItem.ItemSizeX, pItem.ItemSizeY);
        if (puttableZone is not null)
        {//移動先に空きがあるなら
         //アイテムを移動先に移動
            SetItemResult result = afterHolder.PutItem(pItem, puttableZone.Value.x, puttableZone.Value.y);

            if (result.IsSuccess)
            {
                if (result.AlternativeItem is not null)
                {//部分的に移動(削除処理を行わない)

                }
                else
                {
                    //装備アイテムの移動成功
                    //移動元から削除(元の場所があるなら)
                    if (beforeZone is not null) beforeZone.ItemHolder.PullItem(pItem);

                    pItem.PullFromHolder();
                    //移動先に記録
                    pItem.PutToHolder(afterHolder);
                }
            }
        }
        else
        {//移動先に空きがないので、移動失敗

        }
    }

    public void DropsAllMove()
    {
        List<PhysicalItemBase> physicalItems = droppedItemHolder.GetItems();

        foreach (PhysicalItemBase item in physicalItems)
        {
            //ドロップアイテムを全てインベントリに移動する
            QuickMove(item);
        }
    }

    public void ItemsAllMoveToStorage()
    {
        List<PhysicalItemBase> physicalItems = inventoryBackpackItemsHolder.GetItemInstances();
        foreach (PhysicalItemBase item in physicalItems)
        {
            //全て倉庫に移動する
            QuickMove(item);
        }
    }

    public void RequireItemsAllMove()
    {
        //残っているアイテムをすべて移動する
        List<PhysicalItemBase> itemList = requirePhysicalItemHolder.GetItemsInstances();

        foreach (PhysicalItemBase item in itemList)
        {
            //アイテム要求のアイテムを1個ずつ移動する
            //すべてのスタックに対して
            QuickMove(item);
        }
    }

    #endregion

    #region 右クリックメニュー関連

    /// <summary>
    /// 物理アイテム右クリック時にメニュー集合体を表示する
    /// </summary>
    /// <param name="item"></param>
    public void ShowMiniMenu(PhysicalItemBase item)
    {
        if (item.CurrentZone is not IPhysicalItemZone) return;//移動中などは無し。

        //必要なボタンのリストを作成
        List<MiniMenuTipDefine.MiniMenuTipType> buttonTypeList = new List<MiniMenuTipDefine.MiniMenuTipType>();

        IPhysicalItemHolder holder = ((IPhysicalItemZone)item.CurrentZone).ItemHolder;
        if (holder is InventoryBackpackItemsHolder)
        {//(使用)、分割、詳細
            buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Description);

            //使用可能アイテムなら使用を追加
            //村なら使用できない
            if (item.ItemType is PhysicalItemTypeDefine.PhysicalItemType.Consumables&&
                itemArrangeManager.GetCaller().VillageManager is null)
            {//消費アイテムなら
                if (item.BaseItemData.ConsumablesData.UsableOnMap)
                {//マップで使用可能なら
                    buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Use);
                }
            }

            //分割可能アイテムなら分割を追加
            if (item.Stack >= 2)
            {
                buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Divide);
            }
        }
        else if (holder is InventoryEquipItemsHolder)
        {//(使用)、詳細
            buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Description);

            //使用可能アイテムなら使用を追加
            if (item.ItemType is PhysicalItemTypeDefine.PhysicalItemType.Consumables&&
                itemArrangeManager.GetCaller().VillageManager is null)
            {//消費アイテムなら
                if (item.BaseItemData.ConsumablesData.UsableOnMap)
                {//マップで使用可能なら
                    buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Use);
                }
            }

        }
        else if (holder is DroppedItemHolder)
        {//分割,詳細
            buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Description);
            //分割可能アイテムなら分割を追加
            if (item.Stack >= 2)
            {
                buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Divide);
            }
        }
        else if (holder is StorageItemsHolder)
        {//分割,詳細
            buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Description);
            //分割可能アイテムなら分割を追加
            if (item.Stack >= 2)
            {
                buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Divide);
            }
        }
        else if (holder is ShopItemHolder)
        {
            //詳細のみ
            buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Description);
        }
        else if (holder is DiscardItemHolder)
        {
            buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Description);
            //分割可能アイテムなら分割を追加
            if (item.Stack >= 2)
            {
                buttonTypeList.Add(MiniMenuTipDefine.MiniMenuTipType.Divide);
            }
        }
        else
        {
            return;
        }//TODO:他のholderでの定義

        //////////menu作成////////

        //配置するアイテムのtransform
        RectTransform itemTransform = (RectTransform)item.gameObject.transform;

        Vector3 itemPosLocal = item.transform.localPosition;
        float itemLocalWidth = itemTransform.sizeDelta.x;
        float menuLocalWidth = ((RectTransform)(miniMenuPrefab.transform)).sizeDelta.x;

        //右端の場所を計算
        Vector3 rightEndLocal = itemPosLocal + new Vector3(itemLocalWidth + menuLocalWidth, 0, 0);
        Vector3 rightEndWorld = itemTransform.parent.TransformPoint(rightEndLocal);

        MiniMenu menu = null;

        //ビューポート座標を取得
        var viewX = Camera.main.WorldToViewportPoint(rightEndWorld).x;

        //右に表示すると見切れるかどうか
        if (viewX >= 1)
        {//画面の右端に近いので、左に配置
            Vector3 menuPosLocal = itemPosLocal - new Vector3(ItemArrangeConstants.ItemCellSize, 0, 0); //アイテムの左に配置
            Vector3 menuPosWorld = itemTransform.parent.TransformPoint(menuPosLocal);

            //MiniMenu配置場所の配下にインスタンス化
            menu = Instantiate(miniMenuPrefab, menuPosWorld,
                Quaternion.identity,
                miniMenuParentTransform.transform).GetComponent<MiniMenu>();
        }
        else
        {
            //画面の右端に近くないので、右に配置
            Vector3 menuPosLocal = itemPosLocal + new Vector3(ItemArrangeConstants.ItemCellSize, 0, 0); //アイテムの右に配置
            Vector3 menuPosWorld = itemTransform.parent.TransformPoint(menuPosLocal);

            //MiniMenu配置場所の配下にインスタンス化
            menu = Instantiate(miniMenuPrefab, menuPosWorld,
                Quaternion.identity,
                miniMenuParentTransform.transform).GetComponent<MiniMenu>();
        }

        menu?.Init(this, buttonTypeList, item, mainCamera);

        itemArrangeManager.InputBlockingUp(); //入力をブロックする
    }

    public void CloseMiniMenu()
    {
        itemArrangeManager.InputBlockingDown(); //入力ブロック終了
    }

    /// <summary>
    /// アイテム分割メニューを表示
    /// </summary>
    /// <param name="item">分割対象のアイテム</param>
    public void ShowDivideMenu(PhysicalItemBase item)
    {
        itemArrangeManager.InputBlockingUp(); //入力をブロックする

        itemDivideMenuObject.SetActive(true); //分割メニューを表示する


        //分割メニューの配置場所設定
        //配置するアイテムのtransform
        RectTransform itemTransform = (RectTransform)item.gameObject.transform;

        Vector3 itemPosLocal = item.transform.localPosition;
        float itemLocalWidth = itemTransform.sizeDelta.x;
        float menuLocalWidth = ((RectTransform)(itemDivideMenuObject.transform)).sizeDelta.x;

        //lossyScaleをかけることで、ワールド座標での幅に変換できる
        float itemWorldWdth = itemLocalWidth * ((RectTransform)(itemTransform)).lossyScale.x;
        float menuWorldWidth = menuLocalWidth * ((RectTransform)(itemDivideMenuObject.transform)).lossyScale.x;

        //右端の場所を計算
        Vector3 rightEndWorld = item.transform.position + new Vector3(itemWorldWdth + menuWorldWidth, 0, 0);

        //右端のビューポート座標を取得
        var viewX = Camera.main.WorldToViewportPoint(rightEndWorld).x;

        //右に表示すると見切れるかどうか
        if (viewX >= 1)
        {//画面の右端に近いので、左に配置
            Vector3 menuPosLocal = itemPosLocal - new Vector3(ItemArrangeConstants.ItemCellSize + menuLocalWidth, 0, 0); //アイテムの左に配置
            Vector3 menuPosWorld = itemTransform.parent.TransformPoint(menuPosLocal);

            //分割メニューの位置を設定
            itemDivideMenuObject.transform.position = menuPosWorld;
        }
        else
        {
            //画面の右端に近くないので、右に配置
            Vector3 menuPosLocal = itemPosLocal + new Vector3(ItemArrangeConstants.ItemCellSize, 0, 0); //アイテムの右に配置
            Vector3 menuPosWorld = itemTransform.parent.TransformPoint(menuPosLocal);

            //分割メニューの位置を設定
            itemDivideMenuObject.transform.position = menuPosWorld;
        }

        itemDivideMenu.Init(item);
    }

    /// <summary>
    /// アイテム分割メニューを閉じる
    /// </summary>
    public void CloseDivideMenu()
    {
        itemArrangeManager.InputBlockingDown(); //入力ブロック解除

        itemDivideMenuObject.SetActive(false); //分割メニューを非表示にする
    }

    /// <summary>
    /// itemをnumの数分割して所持状態にする
    /// </summary>
    /// <param name="num">分割する個数</param>
    public void DivideItem(PhysicalItemBase item, int num)
    {
        itemArrangeManager.InputBlockingDown(); //入力ブロック解除

        itemDivideMenuObject.SetActive(false); //分割メニューを非表示にする

        //分割処理
        //アイテムのスタック数を減らす
        item.SetStack(item.Stack - num);
        //分割されたスタック数で新しいアイテムを初期化し、所持状態にする
        PhysicalItemDataSO newItemData = item.BaseItemData;

        // マウス位置を取得
        Vector2 tapPos = Input.mousePosition;
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(tapPos);

        //ドラッグ時の一時場所に生成するので、その場所を取得
        Transform parent = parentManagerInDragging.GetParentGameObject().transform;

        //インスタンス化
        PhysicalItemBase newItem = physicalItemInstantiateManager.InstantiatePhysicalItem(newItemData, mouseWorldPoint, parent);

        newItem.Init(newItemData, parentManagerInDragging.GetParentCardZone(), this, num); //初期化

        //生成したアイテムをドラッグ状態に
        draggingItem = (PhysicalItemBase)newItem;
        //ドラッグしているアイテムの配置元を、仮に分割前のアイテムと同じにする
        draggingItemHolder = ((IPhysicalItemZone)item.CurrentZone).ItemHolder;

        newItem.PullFromHolder();//移動開始時のアイテム変形

        //ドラッグ前の位置に意味はないので、枠の外に配置
        (startPosX, startPosY) = (-1, -1);

        parentManagerInDragging.SetParent(newItem);//親操作

        newItem.ItemMover.StartDraggingForcibly(); //強制的にドラッグ状態にする
    }


    public void UseItem(PhysicalItemBase item)
    {
        if (item.CurrentZone is not IPhysicalItemZone pZone) return;

        itemArrangeManager.UseItem(item);

        //アイテムのスタックを減らす。無くなったら削除
        if (item.Stack == 1)
        {//スタックが1なら削除
            //アイテムを削除する
            pZone.ItemHolder.PullItem(item);//アイテムをHolderから取り除く
            Destroy(item.gameObject);
        }
        else
        {//スタックを減らす
            item.SetStack(item.Stack - 1);
        }
    }

    public void ShowDescription(int serialNum)
    {
        //これは物理アイテムしか操作しない
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 物理アイテムの詳細情報を表示する
    /// </summary>
    /// <param name="pItemData">物理アイテムデータ</param>
    public void ShowDescription(PhysicalItemDataSO pItemData)
    {
        itemArrangeManager.ShowDescription(pItemData);
    }
    #endregion

    #region 削除関連
    public void CancelDiscard()
    {
        //カード削除をキャンセル
        discardItemHolder.gameObject.SetActive(false);//カード削除Holderを無効化
        discardingFlag = false;
        //元のEquipとBackpackの状態に戻す
        inventoryEquipItemsHolder.Init(equipsBeforeDiscard.weapons, equipsBeforeDiscard.gear, equipsBeforeDiscard.consumables);
        inventoryBackpackItemsHolder.Init(backpackItemsBeforeDiscard);
    }

    public void ShowDiscardMenu()
    {
        if (SettingManager.IsShowCardDiscardPopup)
        {//ポップアップを表示する設定なら
            //削除のポップアップを表示する
            GameObject popupMessageObj = Instantiate(popupMessageWithTogglePrefab, popupMessageParent.position, Quaternion.identity, popupMessageParent);
            PopupMessageWithToggleUI messageUI = popupMessageObj.GetComponent<PopupMessageWithToggleUI>();

            //挙動を定義するクラスを生成してセット
            var controller = new ItemDiscardPopupController(this, messageUI, itemArrangeManager.GetInputBlocker());

            //UIの初期化
            messageUI.Init(controller);
        }
        else
        {//メニューを表示せず、いきなり削除
            DiscardItem();
        }
    }

    public void DiscardItem()
    {
        discardItemHolder.gameObject.SetActive(false);//カード削除Holderを無効化
        discardingFlag = false;
    }

    public bool IsDiscarding()
    {
        return discardingFlag;
    }
    #endregion
}
