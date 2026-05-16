using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StashPanel : MonoBehaviour, IButtonWithHighlightManager
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject stashDropFrame;
    [SerializeField] private GameObject storageFrame;
    [SerializeField] private DroppedCardHolder droppedCardHolder;
    [SerializeField] private DroppedItemHolder droppedItemHolder;
    [SerializeField] private StorageCardsHolder storageCardsHolder;
    [SerializeField] private StorageItemsHolder storageItemsHolder;
    [SerializeField] private ItemArrangeManager itemArrangeManager;

    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private CardArrangeManager cardArrangeManager;
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;
    [SerializeField] private PhysicalItemInstantiateManager physicalItemInstantiateManager;
    [SerializeField] private InventoryBackpackItemsHolder inventoryBackpackItemsHolder;
    [SerializeField] private GameObject requiredCardObject;
    [SerializeField] private RequireCardHolder requiredCardHolder;
    [SerializeField] private GameObject requiredItemObject;
    [SerializeField] private RequirePhysicalItemHolder requiredItemHolder;

    [SerializeField] private InputBlocker inputBlocker;

    [SerializeField] private NarrowDownCardsWindow narrowDownCardsWindow;

    //倉庫の各種ハイライト付きボタン。一つのボタンが点灯するときそれ以外は消灯
    [SerializeField] private List<ButtonWithHighlight> buttonWithHighlightList;

    private int currentItemPage = 0;//現在の倉庫アイテムページ.カードを表示している時、0

    //////ショップインスタンス化用のPrefab
    [SerializeField] private GameObject supplyShopPrefab;
    [SerializeField] private GameObject cardShopPrefab;
    [SerializeField] private GameObject clownSanPrefab;
    [SerializeField] private GameObject tenbinYaPrefab;

    //////

    //現在表示している店。表示していないならnull
    private IShopManager currentShopManager;

    [SerializeField] private Transform shopParent;//ショップの親オブジェクト

    [SerializeField] private PhysicalItemDataSO coinSO;//コインのSO(コイン枚数カウント用)

    ///////ドロップ関係処理

    [SerializeField] TextMeshProUGUI dropTitleText;//ドロップ画面のタイトルテキスト
    [SerializeField] Button dropEndButton;//ドロップ画面の終了ボタン
    public DropFrameType CurrentDropFrameType;//現在のドロップフレームタイプ

    private bool testAllCards = true;//テスト用。全てのカードを表示するかどうか

    /// <summary>
    /// アイテム取得画面を開く際に、ドロップ元を示すタイプ
    /// </summary>
    public enum DropFrameType
    {
        Monster,    //モンスターによるドロップ
        QuestReward,//クエスト報酬
    }

    /// <summary>
    /// スタッシュの初期化処理
    /// </summary>
    /// <param name="isOpenStorage">倉庫画面を開くか.ダンジョンでは開かない。</param>
    public void Init(bool isOpenStorage)
    {
        canvas.enabled = false;

        currentShopManager = null;

        stashDropFrame.SetActive(false);
        if(isOpenStorage)
        {
            storageFrame.SetActive(true);//倉庫画面を開く
            OpenStorageItem(1);//最初はアイテム画面
        }
        else
        {
            storageFrame.SetActive(false);
        }

        //倉庫タブボタンを最初のもの(アイテム画面)を除いて、全てのボタンを消灯
        for (int i = 0; i < buttonWithHighlightList.Count; i++)
        {
            buttonWithHighlightList[i].Init(this,false);
        }
        buttonWithHighlightList[1].Init(this, true);

        requiredCardObject.SetActive(false);//要求カードパネルを非表示
        requiredItemObject.SetActive(false);//要求カードパネルを非表示

        //絞り込みウィンドウを非表示
        narrowDownCardsWindow.Init();

        CurrentDropFrameType = DropFrameType.Monster;
    }

    /// <summary>
    /// スタッシュの画面を開く処理
    /// </summary>
    public void Open(bool isOpenStorage)
    {
        storageFrame.SetActive(isOpenStorage);

        Init(isOpenStorage);//画面の初期化

        canvas.enabled = true;
    }

    /// <summary>
    /// スタッシュの画面を閉じる処理
    /// </summary>
    public void CloseStash()
    {
        //店を削除
        if (currentShopManager is not null)
        {
            currentShopManager.OnClosed();
            Destroy(currentShopManager?.gameObject);
            currentShopManager = null;
        }

        storageFrame.SetActive(false);
        canvas.enabled = false;
        stashDropFrame.SetActive(false);
    }

    /// <summary>
    /// スタッシュのドロップフレームを開く処理
    /// </summary>
    public void OpenDropFrame(List<int> droppedCards, List<PhysicalItemDataSO> droppedItems,DropFrameType type)
    {
        switch (type)
        {
            case DropFrameType.Monster:
                dropTitleText.text = "戦利品";
                CurrentDropFrameType = DropFrameType.Monster;
                break;
            case DropFrameType.QuestReward:
                dropTitleText.text = "クエスト報酬";
                CurrentDropFrameType = DropFrameType.QuestReward;
                break;
        }

        storageFrame.SetActive(false);
        stashDropFrame.SetActive(true);
        droppedCardHolder.Init(droppedCards);
        droppedItemHolder.Init(droppedItems);
    }

    /// <summary>
    /// クエストのカード要求パネルを開く
    /// </summary>
    /// <param name="requiredCards">パネルが要求するべきカード</param>
    public void OpenCardRequirePanel(List<StorageData.CardStack> requiredCards)
    {
        requiredCardObject.SetActive(true);//要求カードパネルを表示
        requiredCardHolder.Init(requiredCards,cardArrangeManager,this);//初期化
    }

    public void OpenItemRequirePanel(List<PhysicalItemGridPosNumData> requiredItems)
    {
        requiredItemObject.SetActive(true);
        requiredItemHolder.Init(requiredItems);
    }

    /// <summary>
    /// ボタンからも呼出。倉庫カード画面を開く
    /// </summary>
    public void OpenStorageCard()
    {
        currentItemPage = 0;//カードを表示している時、0

        storageCardsHolder.gameObject.SetActive(true);
        storageItemsHolder.gameObject.SetActive(false);//アイテムは閉じる

        if (TestFlags.Instance.showAllCardFlag&&testAllCards)
        {//全てのカードを表示する
            testAllCards = false;//一回だけ
            List<StorageData.CardStack> cardStacks = new List<StorageData.CardStack>();
            foreach (CardDataSO data in PlayerCardData.GetAllCards())
            {
                cardStacks.Add(new StorageData.CardStack { cardSerialNum = data.serialNum, Stack = 3 });
            }
            storageCardsHolder.Init(cardStacks);
        }
        else
        {//プレイヤーの倉庫カードを表示する（通常）
            storageCardsHolder.Init(StorageData.GetPlayerStorageCards());
        }

        //ハイライトされたボタンをCardにする。
        //ボタンが押される場合はその時に処理しているが、それ以外の呼び出しの時に必要
        foreach (ButtonWithHighlight button in buttonWithHighlightList)
        {
            button.SetHighlight(false);
        }
        buttonWithHighlightList[0].SetHighlight(true);
    }

    /// <summary>
    /// ボタンからも呼出。倉庫アイテム画面を開く
    /// </summary>
    /// <param name="storageNum">倉庫番号</param>
    public void OpenStorageItem(int storageNum)
    {
        currentItemPage = storageNum;//現在の倉庫アイテムページ

        storageItemsHolder.gameObject.SetActive(true);
        storageCardsHolder.gameObject.SetActive(false);//カードは閉じる
        storageItemsHolder.Init(StorageData.GetPlayerStorageItems(storageNum-1));//ボタンは1始まり

        //ハイライトされたボタンをItemにする。
        //ボタンが押される場合はその時に処理しているが、それ以外の呼び出しの時に必要
        foreach (ButtonWithHighlight button in buttonWithHighlightList)
        {
            button.SetHighlight(false);
        }
        buttonWithHighlightList[storageNum].SetHighlight(true);
    }

    /// <summary>
    /// 店画面を開く
    /// </summary>
    /// <param name="shopType"></param>
    public void OpenShop(ShopTypeDefine.ShopType shopType)
    {
        canvas.enabled = true;
        storageFrame.SetActive(false);//倉庫画面を閉じる

        //店を開く
        switch (shopType)
        {
            case ShopTypeDefine.ShopType.SupplyShop:
                currentShopManager = InstantiateShop(supplyShopPrefab);
                break;
            case ShopTypeDefine.ShopType.CardShop:
                currentShopManager = InstantiateShop(cardShopPrefab);
                break;
            case ShopTypeDefine.ShopType.ClownSan:
                currentShopManager = InstantiateShop(clownSanPrefab);
                break;
            case ShopTypeDefine.ShopType.TenbinYa:
                currentShopManager = InstantiateShop(tenbinYaPrefab);
                break;
        }
        currentShopManager?.Init(this, cardArrangeManager,physicalItemArrangeManager,physicalItemInstantiateManager);
    }

    /// <summary>
    /// ハイライト付きボタンがクリックされた。
    /// それのみを点灯し、それ以外を消灯
    /// </summary>
    public void OnButtonWithHighlightClicked(ButtonWithHighlight buttonWithHighlight)
    {
        foreach (ButtonWithHighlight button in buttonWithHighlightList)
        {
            if (ReferenceEquals(button, buttonWithHighlight))
            {
                button.SetHighlight(true);
            }
            else
            {
                button.SetHighlight(false);
            }
        }
    }

    /// <summary>
    /// ボタンから呼出。絞り込みウィンドウを開く
    /// </summary>
    public void OpenNarrowDownWindow()
    {
        if (currentItemPage == 0)
        {
            //カードの絞り込みウィンドウを開く
            narrowDownCardsWindow.PushSelf();
        }
    }

    /// <summary>
    /// 絞り込みウィンドウが閉じられた
    /// </summary>
    public void CloseNarrowDownCardsWindow(NarrowDownCardsWindow.CardsNarrowDown result)
    {
        //適用
        storageCardsHolder.SetNarrowDown(result);
    }

    /// <summary>
    /// ショップをインスタンス化
    /// </summary>
    /// <param name="shopPrefab">インスタンス化するプレハブ</param>
    /// <returns>インスタンス化されたIShopManager</returns>
    private IShopManager InstantiateShop(GameObject shopPrefab)
    {
        IShopManager shopManager = Instantiate(shopPrefab, shopParent.position, Quaternion.identity,
            shopParent).GetComponent<IShopManager>();

        return shopManager;
    }

    public void ChangeDisplayMode(bool isItem)
    {
        inventoryPanel.ChangeDisplayMode(isItem);
    }


    public int GetCurrentPage()
    {
        return currentItemPage;
    }

    public IShopManager GetCurrentShopManager()
    {
        return currentShopManager;
    }

    /// <summary>
    /// 所持品に入っている、あるいは倉庫に入っている金貨を全て確認して、
    /// numで与えられた分だけ減らす。(配置データの削除含む)
    /// numが所持金より多い場合はfalseを返す
    /// </summary>
    /// <param name="num">コインを消費する枚数</param>
    /// <returns>コインの消費に成功したならtrue</returns>
    public bool ReduceCoin(int num)
    {
        //インベントリにあるコインの位置情報を全て取得
        int coinsInInventoryNum = 0;//コインの枚数
        List<PhysicalItemGridPosNumData> coinsInInventory = InventoryData.GetBackpackItems().FindAll(item =>
        {
            if(item.ItemData == coinSO)
            {
                coinsInInventoryNum += item.Stack;
                return true;
            }
            else
            {
                return false;
            }
        });

        //倉庫にあるコインの位置情報を全て取得
        int coinsInStorageNum = 0;//コインの枚数
        //データの存在するページと、そのデータ
        List<(int page, PhysicalItemGridPosNumData data)> coinsInStorage = new List<(int page, PhysicalItemGridPosNumData data)>();
        for(int i=0;i<StorageData.GetPageNum() ; i++)
        {
            List<PhysicalItemGridPosNumData> itemsInPage = StorageData.GetPlayerStorageItems(i).FindAll(item =>
            {
                if (item.ItemData == coinSO)
                {
                    coinsInStorageNum += item.Stack;
                    return true;
                }
                else
                {
                    return false;
                }
            });
            foreach (var item in itemsInPage)
            {
                coinsInStorage.Add((i, item));
            }
        }

        //コインを消費できない場合はfalse
        if (coinsInInventoryNum + coinsInStorageNum < num)
        {
            return false;
        }

        //インベントリからコインを消費していく
        int remain = num;//残りの消費すべきコイン枚数
        foreach (var item in coinsInInventory)
        {
            if (remain <= 0) break;

            if (item.Stack<=remain)
            {//itemの破壊を伴う消費
                remain -= item.Stack;
                InventoryData.ReduceBackpackItem(item, item.Stack);
            }
            else
            {//itemが残っている。remainのぶん消費
                InventoryData.ReduceBackpackItem(item, remain);
                remain = 0;
                break;
            }
        }

        if (remain <= 0){
            //インベントリの表示を更新する
            inventoryBackpackItemsHolder.Init(InventoryData.GetBackpackItems());
            return true;
        }

        //倉庫からコインを消費
        foreach (var item in coinsInStorage)
        {
            if (remain <= 0) break;

            if (item.data.Stack <= remain)
            {//itemの破壊を伴う消費
                remain -= item.data.Stack;
                StorageData.ReduceStorageItem(item.data, item.page, item.data.Stack);
            }
            else
            {//itemが残っている。remainのぶん消費
                StorageData.ReduceStorageItem(item.data, item.page, remain);
                remain = 0;
                break;
            }
        }

        if (remain <= 0)
        {
            //インベントリの表示を更新する
            inventoryBackpackItemsHolder.Init(InventoryData.GetBackpackItems());
            return true;
        }

        return false;
    }

    public ItemArrangeManager GetItemArrangeManager()
    {
        return itemArrangeManager;
    }
}
