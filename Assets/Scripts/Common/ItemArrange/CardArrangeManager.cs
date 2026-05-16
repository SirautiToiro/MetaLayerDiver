using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// アイテムの移動画面のカードを制御する部分
/// Cardから必ず呼ばれる
/// </summary>
public class CardArrangeManager : MonoBehaviour, IItemManager
{
    //ドラッグ時に親を最前に操作する機能
    [SerializeField] private ParentManagerInDragging parentManagerInDragging;

    //インベントリ全般の操作クラス
    [SerializeField] private ItemArrangeManager itemArrangeManager;

    private Camera mainCamera;//所属しているシーンのカメラ。Initで取得

    private Card draggingCard;//ドラッグしているカード

    //ドラッグしているカードが元はどこにあったか
    private ICardHolder draggingCardHolder;

    //現在重なっているPlaceCardZone
    private PlaceCardZone enteringCardZone;

    private int startPos;//Cardがどの位置からドラッグを開始したか

    //カード削除に使用するHolder
    [SerializeField] private DiscardCardHolder discardCardHolder;

    //カードの高速移動用
    [SerializeField] private InventoryDeckCardsHolder inventoryDeckCardsHolder;
    [SerializeField] private InventoryBackpackCardsHolder inventoryBackpackCardsHolder;
    [SerializeField] private DroppedCardHolder droppedCardHolder;
    [SerializeField] private StorageCardsHolder storageCardsHolder;
    [SerializeField] private StashPanel stashPanel;
    //これはクエストでの要求カード。店のものではない。
    [SerializeField] private RequireCardHolder requiredCardHolder;

    //接触判定用のコライダーをつけたViewportのGameObjectリスト
    [SerializeField] private List<GameObject> viewportGameObjectList;

    //接触判定クラス
    private OverlappedCardzoneGetter overlappedCardzoneGetter;

    [SerializeField] private Transform popupMessageParent; //ポップアップメッセージの配置場所
    [SerializeField] private GameObject popupMessageWithTogglePrefab; //ポップアップメッセージのプレハブ

    [SerializeField] private GameObject cardPrefab; //カードのプレハブ

    //カード削除画面用
    private bool discardingFlag;
    private List<int> deckBeforeDiscard;
    private List<int> backpackCardsBeforeDiscard;

    [SerializeField] private UIPageManager uiPageManager;

    public void Init(Camera _camera)
    {
        draggingCardHolder = null;
        draggingCard = null;
        enteringCardZone = null;

        discardingFlag = false;

        deckBeforeDiscard = new List<int>();
        backpackCardsBeforeDiscard = new List<int>();

        mainCamera = _camera;

        discardCardHolder.gameObject.SetActive(false);//カード削除Holderを無効化

        //接触判定クラスを初期化
        overlappedCardzoneGetter = new OverlappedCardzoneGetter(mainCamera, parentManagerInDragging, viewportGameObjectList);
    }

    #region ドラッグ関連

    /// <summary>
    /// アイテムのドラッグ開始時処理
    /// </summary>
    /// <param name="item">ドラッグしているアイテム</param>
    public void StartDragging(ItemBase item)
    {
        //不適切なCardの排除
        if (item is not Card) return;
        if (item.CurrentZone is not PlaceCardZone) return;

        PlaceCardZone zone = (PlaceCardZone)item.CurrentZone;

        //Shopからの取り出しだった場合、取り出し可能かを判定
        if (zone.CardHolder is ShopCardHolder shopCardHolder)
        {
            if (shopCardHolder.TryBuyCard((Card)item))
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

        //アイテムのHolderを取得
        draggingCardHolder = zone.CardHolder;
        startPos = zone.Pos;

        if(draggingCardHolder.HolderType != ICardHolder.CardHolderType.StackHolder)
        {//スタックするHolderからの取り出しは、ドラッグ中カードが別ルートで取得される
            //ここはスタックしないHolderからの取り出しを処理。
            draggingCard = (Card)item;
            //ドラッグしているカードの位置を確保
            enteringCardZone = (PlaceCardZone)draggingCard.CurrentZone;
            parentManagerInDragging.SetParent(item);//親操作
        }
        else
        {//スタックするHolderからの取り出しなら、この時動いているitemの移動はキャンセルされる
            item.ItemMover.EndDraggingForcibly();//強制的にドラッグ終了
        }


        if (!discardingFlag)
        {//削除中でないなら
            //元のカード配置を保存(削除機能時に戻すため)
            deckBeforeDiscard = inventoryDeckCardsHolder.GetDeck();
            backpackCardsBeforeDiscard = inventoryBackpackCardsHolder.GetBackpackCards();
        }

        //アイテムのHolderから取り出す処理
        draggingCardHolder.PullCard(startPos);
    }


    /// <summary>
    /// アイテムのドラッグ中処理
    /// </summary>
    public void UpdateItemDragging()
    {

        //重なっているPlaceCardZoneを取得
        PlaceCardZone targetCardZone = overlappedCardzoneGetter.GetOverlappedCardZone<PlaceCardZone>(draggingCard);

        if(enteringCardZone == targetCardZone)
        {//今既に入っているCardZoneと同じところでの処理は行わない
            return;
        }

        if(enteringCardZone == null)
        {//以前のドラッグでどこにも重なっていないなら
            if (targetCardZone == null)
            {//どこにも重なっていないなら 
             //演出なし
            }
            else
            {
                targetCardZone.CardHolder?.EnterCard(targetCardZone.Pos);
            }
        }
        else
        {
            if (targetCardZone == null)
            {//どこにも重なっていないなら 
             //重なる演出(どこにも重ならない)
                enteringCardZone.CardHolder.EnterCard(-1);
            }
            else if (enteringCardZone.CardHolder != targetCardZone.CardHolder)
            {//前回重なっていたところと違う場所に重なった場合
                //元の場所に重なる演出(どこにも重ならない)
                enteringCardZone.CardHolder.EnterCard(-1);

                //新たな場所に重なる演出
                targetCardZone.CardHolder?.EnterCard(targetCardZone.Pos);
            }
            else
            {//カードが前にあった場所とは異なる場所で、だがHolderは同じ
             //カードのスライドが発生
             //その位置に重なる演出
                enteringCardZone.CardHolder.EnterCard(targetCardZone.Pos);
            }
        }

        

        //同じCardZoneに重なった時の処理を行わないため、保存
        enteringCardZone = targetCardZone;
    }

    /// <summary>
    /// アイテムのドラッグ終了時処理
    /// </summary>
    public bool EndDragging()
    {
        parentManagerInDragging.ReturnToBaseParent();//必要ないが、一応元のParentに戻す。

        //重なっているPlaceCardZoneを取得
        PlaceCardZone targetCardZone = overlappedCardzoneGetter.GetOverlappedCardZone<PlaceCardZone>(draggingCard);

        if (targetCardZone is null && !discardingFlag)
        {
            //何もない場所で離した。
            if (itemArrangeManager.GetCaller().DungeonManager is not null)
            {//ダンジョン中ならカード削除画面に移行。(削除中でない場合)
                discardingFlag = true;

                //前の場所から削除
                draggingCardHolder.ExitCard();

                discardCardHolder.gameObject.SetActive(true);//カード削除Holderを有効化
                discardCardHolder.Init(draggingCard);
            }
            else if (draggingCardHolder is ShopCardHolder)
            {//店画面からの取り出しで何もない場所で離した場合、移動を継続(配置しない)
                return false;
            }
            else
            {
                //ダンジョン中でないなら、元の場所に移動
                draggingCardHolder.PutCard(draggingCard, startPos);
            }
        }
        else if (targetCardZone == null && discardingFlag)
        {//削除中に何もない場所で離した場合、元の場所に戻る
            draggingCardHolder.PutCard(draggingCard, startPos);
        }
        else if (targetCardZone.CardHolder == draggingCardHolder)
        {//ドラッグを開始した場所に戻ってきたなら
            if (draggingCardHolder is ShopCardHolder)
            {//店画面からの取り出しで元の場所に戻った場合、移動を継続(配置しない)
                return false;
            }
            else
            {
                //元のHolderに配置処理
                draggingCardHolder.PutCard(draggingCard, targetCardZone.Pos);
            }
        }
        else if (targetCardZone.CardHolder is ShopCardHolder)
        {//店の場所には移動ができず、キャンセルされる
            //元のHolderに配置処理
            draggingCardHolder.PutCard(draggingCard, startPos);
        }
        else if (targetCardZone.CardHolder is RequireCardHolder rHolder)
        {//要求を持つCardHolderなら、配置可能であるかを判定する
            if (rHolder.IsPuutable(draggingCard))
            {
                //配置可能なら配置
                //前の場所から削除
                draggingCardHolder.ExitCard();
                //次の場所に移動
                targetCardZone.CardHolder.PutCard(draggingCard, targetCardZone.Pos);
            }
            else
            {
                //配置不可能なら元の場所に戻す
                draggingCardHolder.PutCard(draggingCard, startPos);
            }
        }
        else
        {//他のHolderの場所に移動した
            //前の場所から削除
            draggingCardHolder.ExitCard();
            //次の場所に移動
            targetCardZone.CardHolder.PutCard(draggingCard, targetCardZone.Pos);
        }

        draggingCard = null;
        return true;
    }

    /// <summary>
    /// Shift+Clickの高速移動
    /// </summary>
    public void QuickMove(ItemBase item)
    {
        if (item is not Card) return;
        if (item.CurrentZone is not PlaceCardZone) return;

        //元あった場所
        PlaceCardZone zone = (PlaceCardZone)item.CurrentZone;
        ICardHolder holder = zone.CardHolder;

        //Shopからの取り出しだった場合、取り出し可能かを判定
        if (zone.CardHolder is ShopCardHolder shopCardHolder)
        {
            if (shopCardHolder.TryBuyCard((Card)item))
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
            {//カード削除中なら
                if (holder is DiscardCardHolder)
                {//削除場所ならBackpackへ
                    QuickMove((Card)item, holder, inventoryBackpackCardsHolder, zone);
                }
                else
                {
                    //削除場所ではないなら削除場所へ
                    QuickMove((Card)item, holder,discardCardHolder, zone);
                }
            }
            else
            {
                //高速移動するCardが元はどこにあったかで移動先を場合分け
                //Backpackから移動した場合はDeckへ
                //Deckから移動した場合はBackpackへ
                if (holder is InventoryDeckCardsHolder)
                {
                    QuickMove((Card)item, holder,inventoryBackpackCardsHolder, zone);
                }
                else if (holder is InventoryBackpackCardsHolder)
                {
                    QuickMove((Card)item, holder,inventoryDeckCardsHolder, zone);
                }
            }
        }
        else if (itemArrangeManager.GetCaller().FieldManager is not null)
        {//バトル中なら(ドロップアイテム取得中)
            if (holder is InventoryBackpackCardsHolder)
            {//鞄のカードならデッキ内に
                QuickMove((Card)item, holder,inventoryDeckCardsHolder, zone);
            }
            else if (holder is DroppedCardHolder||
                holder is InventoryDeckCardsHolder)
            {//ドロップアイテムの場所,デッキ内ならバックパックへ
                QuickMove((Card)item, holder,inventoryBackpackCardsHolder, zone);
            }
        }
        else if (itemArrangeManager.GetCaller().VillageManager is not null)
        {//村なら
            if (uiPageManager.GetTopPage() is InventoryAndShopUIPage)
            {//店を開いているなら
                if (stashPanel.GetCurrentShopManager() is null) return;

                if(holder is InventoryDeckCardsHolder||
                    holder is InventoryBackpackCardsHolder)
                {//インベントリ内なら
                    //高速移動の先になりうるICardHolder
                    ICardHolder targetHolder = stashPanel.GetCurrentShopManager().GetCardHolderForQuickMove();
                    
                    if (targetHolder is null)
                    {//移動先がないので手持ちでの移動
                     //Backpackから移動した場合はDeckへ
                     //Deckから移動した場合はBackpackへ
                        if (holder is InventoryDeckCardsHolder)
                        {
                            QuickMove((Card)item, holder,inventoryBackpackCardsHolder, zone);
                        }
                        else if (holder is InventoryBackpackCardsHolder)
                        {
                            QuickMove((Card)item, holder,inventoryDeckCardsHolder, zone);
                        }
                    }
                    else
                    {//移動先がある場合はそこへ
                        QuickMove((Card)item, holder,targetHolder, zone);
                    }
                }
                else if (holder is ShopCardHolder || holder is RequireCardHolder)
                {//店の場所,カード要求場所ならバックパックへ
                    QuickMove((Card)item, holder,inventoryBackpackCardsHolder, zone);
                }
            }
            else if (uiPageManager.GetTopPage() is InventoryAndStashUIPage)
            {//倉庫中なら
                if (holder is InventoryBackpackCardsHolder ||
                    holder is InventoryDeckCardsHolder)
                {
                    //インベントリ内なら倉庫へ
                    QuickMove((Card)item, holder,storageCardsHolder, zone);
                }
                else if (holder is StorageCardsHolder)
                {//倉庫ならバックパックへ
                    QuickMove((Card)item, holder,inventoryBackpackCardsHolder, zone);
                }
            }
            else if (uiPageManager.GetTopPage() is InventoryAndRewardUIPage)
            {//クエストクリア画面なら
                if (holder is DroppedCardHolder)
                {//クエストクリア報酬ならバックパックへ
                    QuickMove((Card)item, holder,inventoryBackpackCardsHolder, zone);
                }
                else if (holder is InventoryDeckCardsHolder)
                {//デッキ内ならバックパックへ
                    QuickMove((Card)item, holder,inventoryBackpackCardsHolder, zone);
                }
                else if (holder is InventoryBackpackCardsHolder)
                {//バックパック内ならデッキへ
                    QuickMove((Card)item, holder,inventoryDeckCardsHolder, zone);
                }
            }else if(uiPageManager.GetTopPage() is InventoryAndRequireCardsUIPage)
            {//カード要求画面なら
                if (holder is RequireCardHolder)
                {//カード要求場所ならバックパックへ
                    QuickMove((Card)item, holder,inventoryBackpackCardsHolder, zone);
                }
                else if (holder is InventoryBackpackCardsHolder||
                    holder is InventoryDeckCardsHolder)
                {//バックパック内ならカード要求場所へ
                    QuickMove((Card)item, holder,requiredCardHolder, zone);
                }
            }
        }
    }

    /// <summary>
    /// public のQuickMoveの内部で使用。
    /// 動かすCard、動かす先のICardHolder、元あったPlaceCardZone,ICardHolderを指定して、高速移動させる
    /// </summary>
    /// <param name="card">動かすCard</param>
    /// <param name="currentHolder">動かす前のICardHolder</param>
    /// <param name="targetHolder">動かす先のICardHolder</param>
    /// <param name="zone">元あったPlaceCardZone</param>
    private void QuickMove(Card card, ICardHolder currentHolder,ICardHolder targetHolder, PlaceCardZone zone)
    {
        Debug.Log("quickmove");

        //要求を持つHolderなら、配置可能かを判定する
        if (targetHolder is RequireCardHolder rHolder)
        {
            if (!rHolder.IsPuutable(card))
            {
                //配置不可能なら移動しない
                return;
            }
        }

        //元あった場所から移動
        currentHolder.PullCard(zone.Pos);
        //前の場所から削除
        currentHolder.EnterCard(-1);
        currentHolder.ExitCard();

        if (currentHolder.HolderType == ICardHolder.CardHolderType.StackHolder)
        {//スタックするHolderからの取り出しはなら、Cardがドラッグ中になってしまうので、止める
            draggingCard.ItemMover.EndDraggingForcibly();//強制的にドラッグ終了

            //itemが動くわけではない。StackHolder内で生成されたものがdraggingCardにある
            targetHolder.PutCard(draggingCard, 0);
        }
        else
        {
            targetHolder.PutCard(card, 0);
        }
    }

    public void DropsAllMove()
    {
        List<Card> cardList = droppedCardHolder.GetCardInstances();
        foreach (Card card in cardList)
        {//それぞれのカードを高速移動
            QuickMove(card);
        }
    }

    public void CardsAllMoveToStorage()
    {
        List<Card> cardList = inventoryBackpackCardsHolder.GetCardsInstances();
        foreach (Card card in cardList)
        {//それぞれのカードを高速移動
            QuickMove(card);
        }
    }

    public void RequireCardsAllMove()
    {
        //残っているアイテムをすべて移動する
        Card remainedCard = null;
        do
        {
           List<Card> cardList = requiredCardHolder.GetCardsInstances();
            if (cardList.Count > 0)
            {
                remainedCard = cardList[0];
                QuickMove(remainedCard);
            }
            else
            {
                remainedCard = null;
            }
        } while (remainedCard != null);
    }

    #endregion


    /// <summary>
    /// cardDataに記述されたカードをインスタンス化して、ドラッグ状態にする
    /// </summary>
    /// <param name="cardData">生成するカードの情報</param>
    /// <param name="placeCardZone">カードの移動などはどのzoneから始まるか</param>
    /// <returns>生成し、動かし始めたカード</returns>
    public Card InstantiateAndMoveCard(CardDataSO cardData, PlaceCardZone placeCardZone)
    {
        // マウス位置を取得
        Vector2 tapPos = Input.mousePosition;
        Vector3 mouseWorldPoint = Camera.main.ScreenToWorldPoint(tapPos);

        //ドラッグ時の一時場所に生成するので、その場所を取得
        Transform parent = parentManagerInDragging.GetParentGameObject().transform;

        //インスタンス化
        Card newCard = InstantiateCard(cardData, parentManagerInDragging.GetParentCardZone(), mouseWorldPoint);

        //生成したカードをドラッグ状態に
        draggingCard = newCard;
        //配置元は、生成するカードが発生した場所(StorageCardsHolderなど)
        draggingCardHolder = placeCardZone.CardHolder;
        //StackCardArrangementから取り出されているので、posに意味はない
        startPos = 0;
        enteringCardZone = placeCardZone;

        parentManagerInDragging.SetParent(newCard);//親操作

        newCard.ItemMover.StartDraggingForcibly();//強制的にドラッグ開始

        return newCard;
    }

    /// <summary>
    /// カードのインスタンス化
    /// マウスカーソル位置に生成
    /// </summary>
    /// <param name="cardData"></param>
    /// <param name="cardZone"></param>
    /// <param name="pos"></param>
    /// <returns></returns>
    private Card InstantiateCard(CardDataSO cardData, PlaceCardZone cardZone, Vector3 mousePosition)
    {
        //Cardをインスタンス化
        GameObject cardObj = Instantiate(cardPrefab, mousePosition, Quaternion.identity, cardZone.gameObject.transform);
        Card card = cardObj.GetComponent<Card>();
        card.Init(cardData, cardZone, this, 0);
        return card;
    }

    public void CancelDiscard()
    {
        //カード削除をキャンセル
        discardCardHolder.gameObject.SetActive(false);//カード削除Holderを無効化
        discardingFlag = false;
        //元のDeckとBackpackの状態に戻す
        inventoryDeckCardsHolder.Init(deckBeforeDiscard);
        inventoryBackpackCardsHolder.Init(backpackCardsBeforeDiscard);
    }

    public void ShowDiscardMenu()
    {
        if (SettingManager.IsShowCardDiscardPopup)
        {//ポップアップを表示する設定なら
            //削除のポップアップを表示する
            GameObject popupMessageObj = Instantiate(popupMessageWithTogglePrefab, popupMessageParent.position, Quaternion.identity, popupMessageParent);
            PopupMessageWithToggleUI messageUI = popupMessageObj.GetComponent<PopupMessageWithToggleUI>();

            //挙動を定義するクラスを生成してセット
            var controller = new CardDiscardPopupController(this, messageUI, itemArrangeManager.GetInputBlocker());

            //UIの初期化
            messageUI.Init(controller);
        }
        else
        {//メニューを表示せず、いきなり削除
            DiscardCard();
        }
    }

    public void DiscardCard()
    {
        discardCardHolder.gameObject.SetActive(false);//カード削除Holderを無効化
        discardingFlag = false;
    }

    /// <summary>
    /// アイテム右クリックで詳細画面を表示する機能
    /// カードの詳細情報を表示する画面を出す。
    /// </summary>
    /// <param name="serialNum">カードのシリアル番号</param>
    public void ShowDescription(int serialNum)
    {
        itemArrangeManager.ShowDescription(serialNum);
    }

    /// <summary>
    /// 物理アイテムの詳細情報を表示する
    /// </summary>
    /// <param name="pItemData">物理アイテムデータ</param>
    public void ShowDescription(PhysicalItemDataSO pItemData)
    {
        itemArrangeManager.ShowDescription(pItemData);
    }

    public bool IsDiscarding()
    {
        return discardingFlag;
    }   
}
