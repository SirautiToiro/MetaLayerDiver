using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// インベントリ画面全体
/// アイテムの移動を各種の画面で行う
/// カードとアイテムの移動機能の設定
/// </summary>
public class ItemArrangeManager : MonoBehaviour
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private StashPanel stashPanel;
    [SerializeField] private QuestPanel questPanel;
    [SerializeField] private CardArrangeManager cardArrangeManager;
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;
    [SerializeField] private InputBlocker inputBlocker;//アイテム画面の入力制限

    [SerializeField] private GameObject closeButton;//閉じるボタン(バトル終了時のドロップ画面では出現しない)

    private Camera mainCamera;//所属しているシーンのカメラ。Initで取得

    //カード説明表示用
    [SerializeField] private Transform descriptionParent;//説明パネルをセットする親

    //右クリックで出る説明パネルのプレハブ
    [SerializeField] private GameObject descriptionPrefab;

    private DescriptionPanel descriptionPanel = null;//インスタンス化したときにセット

    [SerializeField]private UIPageManager uiPageManager;

    // このインスタンスが何によって呼び出されたか
    private Caller instanceCaller;

    [SerializeField] private InventoryUIPage inventoryUIPage;
    [SerializeField]private InventoryAndStashUIPage inventoryAndStashUIPage;
    [SerializeField] private InventoryAndShopUIPage inventoryAndShopUIPage;

    /// <summary>
    /// このインスタンスが何によって呼び出されたか
    /// どちらかのみしか値を取らない
    /// </summary>
    public class Caller
    {
        private FieldManager fieldManager;
        private DungeonManager dungeonManager;
        private VillageManager villageManager;

        public FieldManager FieldManager
        {
            get { return fieldManager; }
            set { fieldManager = value;
                dungeonManager = null;
                villageManager = null;
            }
        }

        public DungeonManager DungeonManager
        {
            get { return dungeonManager; }
            set { dungeonManager = value;
                fieldManager = null;
                villageManager = null;
            }
        }

        public VillageManager VillageManager
        {
            get { return villageManager; }
            set
            {
                villageManager = value;
                dungeonManager = null;
                fieldManager = null;
            }
        }
    }

    public void Update()
    {
        //Escが押されたなら
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (instanceCaller.VillageManager != null)
            {
                uiPageManager.PopUIPage();
            }
        }
    }

    public void Init(FieldManager fieldManager,Camera camera)
    {
        instanceCaller = new Caller();
        InstantiateDescriptions();//説明画面初期化
        instanceCaller.FieldManager = fieldManager;
        mainCamera = camera;
        inputBlocker.Init();
        inventoryPanel.Init();
        stashPanel.Init(false);
        questPanel.Init(fieldManager.GetScenarioManager());
        cardArrangeManager.Init(mainCamera);
        physicalItemArrangeManager.Init(mainCamera);

        uiPageManager.Init(fieldManager);

        closeButton.SetActive(false);
    }

    public void Init(DungeonManager dungeonManager,Camera _camera)
    {
        instanceCaller = new Caller();

        InstantiateDescriptions();//説明画面初期化

        instanceCaller.DungeonManager = dungeonManager;
        mainCamera = _camera;
        inputBlocker.Init();
        inventoryPanel.Init();
        stashPanel.Init(false);
        questPanel.Init(dungeonManager.GetScenarioManager());
        cardArrangeManager.Init(mainCamera);
        physicalItemArrangeManager.Init(mainCamera);

        uiPageManager.Init(dungeonManager);

        closeButton.SetActive(true);
    }

    public void Init(VillageManager villageManager, Camera _camera)
    {
        instanceCaller = new Caller();

        InstantiateDescriptions();//説明画面初期化

        instanceCaller.VillageManager = villageManager;
        mainCamera = _camera;
        inputBlocker.Init();
        inventoryPanel.Init();
        stashPanel.Init(true);
        questPanel.Init(villageManager.GetScenarioManager());
        cardArrangeManager.Init(mainCamera);
        physicalItemArrangeManager.Init(mainCamera);

        uiPageManager.Init(villageManager);

        closeButton.SetActive(true);
    }

    /*
    /// <summary>
    /// ダンジョン内からインベントリを開く
    /// </summary>
    /// <param name="dungeonManager"></param>
    public void OpenInventoryInDungeon(DungeonManager dungeonManager)
    {
        instanceCaller.DungeonManager = dungeonManager;
 
        closeButton.SetActive(true);
        inventoryPanel.Open();
        stashPanel.Open(false);//倉庫部分は閉じる
        
    }
    */

    public void OpenInventoryInDungeon()
    {
        //UIPageManagerの実装により、インベントリとスタッシュを同時に表示するためのUIPageを作成して表示するように変更
        inventoryUIPage.PushSelf();
    }

    /// <summary>
    /// 村画面からの初期化
    /// </summary>
    public void OpenInventoryInVillage()
    {
        //UIPageManagerの実装により、インベントリとスタッシュを同時に表示するためのUIPageを作成して表示するように変更
        inventoryAndStashUIPage.PushSelf();

        /*
        closeButton.SetActive(true);
        inventoryPanel.Open();
        stashPanel.Open(true);//インベントリではないが、倉庫部分は常に同時に開いている
        */
    }

    /// <summary>
    /// 敵を倒した時にアイテムがドロップするときのインベントリを開く
    /// (FieldManagerから)
    /// </summary>
    /// <param name="droppedCards">ドロップしたカード</param>
    /// <param name="droppedItems">ドロップしたアイテム</param>
    public void OpenItemDropInventory(List<int> droppedCards, List<PhysicalItemDataSO> droppedItems, StashPanel.DropFrameType type)
    {
        inventoryPanel.Open();
        stashPanel.Open(false);//倉庫部分は閉じる
        stashPanel.OpenDropFrame(droppedCards, droppedItems, type);//ドロップしたカードをスタッシュに表示する
        closeButton.SetActive(false);
    }


    /// <summary>
    /// 店画面を開く
    /// </summary>
    /// <param name="shopType"></param>
    public void OpenShop(ShopTypeDefine.ShopType shopType)
    {
        //UIPageManagerの実装により、インベントリとスタッシュを同時に表示するためのUIPageを作成して表示するように変更
        inventoryAndShopUIPage.Init(shopType);
        inventoryAndShopUIPage.PushSelf();
        closeButton.SetActive(true);
        /*
        closeButton.SetActive(true);
        inventoryPanel.Open();
        stashPanel.OpenShop(shopType);
        */
    }

    /// <summary>
    /// クエストパネルを開く
    /// </summary>
    public void OpenStoryQuest()
    {
        //closeButton.SetActive(false);
        //UIPageManagerの実装により、インベントリとスタッシュを同時に表示するためのUIPageを作成して表示するように変更
        questPanel.PushSelf();
    }

    /// <summary>
    /// クエストパネルを閉じる
    /// </summary>
    public void CloseStoryQuest()
    {
        //UIPageManagerの実装により閉じる動作が変更
        /*
        questPanel.Close();
        inventoryPanel.CloseButton();
        */
        uiPageManager.PopUIPage();
    }

    /// <summary>
    /// クエストのカード要求パネルを開く
    /// </summary>
    /// <param name="requiredCards">パネルが要求するべきカード</param>
    public void OpenCardRequirePanel(List<StorageData.CardStack> requiredCards)
    {
        inventoryPanel.Open();
        stashPanel.Open(false);//倉庫部分は閉じる
        stashPanel.OpenCardRequirePanel(requiredCards);
        closeButton.SetActive(false);
    }

    public void OpenItemRequirePanel(List<PhysicalItemGridPosNumData> requiredItems)
    {
        inventoryPanel.Open();
        stashPanel.Open(false);//倉庫部分は閉じる
        stashPanel.OpenItemRequirePanel(requiredItems);
        closeButton.SetActive(false);
    }

    /// <summary>
    /// カード要求パネルを閉じ、クエスト画面に戻る
    /// </summary>
    public void CloseCardRequirePanel()
    {
        //UIPageManagerの実装により閉じる動作が変更
        /*
        inventoryPanel.CloseButton();
        questPanel.CloseRequestPanel();
        */
        uiPageManager.PopUIPage();
    }

    public void CloseInventory()
    {
        if (cardArrangeManager.IsDiscarding())
        {//カード削除中なら、キャンセル
            cardArrangeManager.CancelDiscard();
        }
        if(physicalItemArrangeManager.IsDiscarding())
        {//物理アイテム削除中なら、キャンセル
            physicalItemArrangeManager.CancelDiscard();
        }

        stashPanel.CloseStash();//スタッシュパネルを閉じる
        inventoryPanel.HideInventory();//インベントリを閉じる

        //インベントリ終了を通知
        if (instanceCaller.DungeonManager is not null)
        {
            instanceCaller.DungeonManager.InventoryClosed();
        }else if(instanceCaller.VillageManager is not null)
        {
            instanceCaller.VillageManager.InventoryClosed();
        }
    }

    public void UseItem(PhysicalItemBase item)
    {
        if(instanceCaller.DungeonManager is not null)
        {//ダンジョン内でしかアイテムは使用できない
            instanceCaller.DungeonManager.UseItem(item);
        }
    }

    public void DropsAllMoveButton()
    {//ドロップアイテムを全てインベントリに移動する
        cardArrangeManager.DropsAllMove();
        physicalItemArrangeManager.DropsAllMove();
    }

    public void AllMoveToStorageButton()
    {//装備していないカードとアイテムを全て倉庫に移動する
        if(stashPanel.GetCurrentPage() == 0)
        {//カード倉庫を見ているなら
            //カード全移動
            cardArrangeManager.CardsAllMoveToStorage();
        }
        else
        {//アイテム倉庫を見ているなら
            physicalItemArrangeManager.ItemsAllMoveToStorage();
        }
    }

    #region 詳細説明機能

    /// <summary>
    /// アイテム右クリックで詳細画面を表示する機能
    /// カードの詳細情報を表示する画面を出す。
    /// </summary>
    /// <param name="serialNum">カードのシリアル番号</param>
    public void ShowDescription(int serialNum)
    {
        inputBlocker.InputBlockingUp();
        descriptionPanel.OpenPanel(serialNum);
    }

    /// <summary>
    /// 物理アイテムの詳細情報を表示する
    /// </summary>
    /// <param name="pItemData">物理アイテムデータ</param>
    public void ShowDescription(PhysicalItemDataSO pItemData)
    {
        inputBlocker.InputBlockingUp();
        descriptionPanel.OpenPanel(pItemData);
    }

    /// <summary>
    /// カードなどの説明文をインスタンス化する
    /// 既に存在していた場合は何もしない
    /// </summary>
    private void InstantiateDescriptions()
    {
        if (descriptionPanel == null)
        {//カード説明のインスタンス化
            GameObject cardDesObj = Instantiate(descriptionPrefab, descriptionParent.position, Quaternion.identity, descriptionParent);
            descriptionPanel = cardDesObj.GetComponent<DescriptionPanel>();
            descriptionPanel.Init(this);
        }
    }

    public void CloseDescription()
    {
        inputBlocker.InputBlockingDown();//UIブロック解除
    }

    #endregion

    /*
     * UIPageManagerの実装により、バトル終了時とクエスト報酬画面終了時のポップアップ表示はUIPageとして実装することになったため、以下のメソッドは使用されなくなった。
    private void ShowEndBattleMenu()
    {
        if (SettingManager.IsShowEndBattlePopup)
        {//ポップアップを表示する設定なら
            //削除のポップアップを表示する
            GameObject popupMessageObj = Instantiate(popupMessageWithTogglePrefab, popupMessageParent.position, Quaternion.identity, popupMessageParent);
            PopupMessageWithToggleUI messageUI = popupMessageObj.GetComponent<PopupMessageWithToggleUI>();

            //挙動を定義するクラスを生成してセット
            var controller = new EndBattlePopupController(this, messageUI, inputBlocker);

            //UIの初期化
            messageUI.Init(controller);
        }
        else
        {//メニューを表示せず、いきなり終了
            EndBattle();
        }
    }

    private void ShowEndQusetRewardMenu()
    {
        if (SettingManager.IsShowQuestRewardPopup)
        {//ポップアップを表示する設定なら
            GameObject popupMessageObj = Instantiate(popupMessageWithTogglePrefab, popupMessageParent.position, Quaternion.identity, popupMessageParent);
            PopupMessageWithToggleUI messageUI = popupMessageObj.GetComponent<PopupMessageWithToggleUI>();
            //挙動を定義するクラスを生成してセット
            var controller = new EndQuestRewardPopupController(this, messageUI, inputBlocker);
            //UIの初期化
            messageUI.Init(controller);
        }
        else
        {//メニューを表示せず、いきなり終了
            EndQuestReward();
        }
    }
    */

    /// <summary>
    /// バトルからの呼び出しで、ドロップ取得を閉じてバトルを終了
    /// </summary>
    public void EndBattle()
    {
        //UIPageManagerの実装により閉じる動作が変更
        /*
        stashPanel.CloseStash();//スタッシュパネルを閉じる
        inventoryPanel.CloseButton();
        */
        uiPageManager.PopUIPage();

        /*
        if (instanceCaller.FieldManager != null)
        {
            instanceCaller.FieldManager.EndBattle();
        }
        */
    }

    /// <summary>
    /// クエスト画面からの呼び出しで、報酬画面を閉じる
    /// </summary>
    public void EndQuestReward()
    {
        //UIPageManagerの実装により閉じる動作が変更
        /*
        stashPanel.CloseStash();//スタッシュパネルを閉じる
        inventoryPanel.CloseButton();
        questPanel.EndQuestReward();
        */
        uiPageManager.PopUIPage();
    }

    /// <summary>
    /// 入力を停止する処理
    /// </summary>

    public void InputBlockingUp()
    {
        inputBlocker.InputBlockingUp();
    }

    /// <summary>
    /// 入力を受け付ける処理
    /// </summary>
    public void InputBlockingDown()
    {
        inputBlocker.InputBlockingDown();
    }

    public InputBlocker GetInputBlocker()
    {
        return inputBlocker;
    }

    public Caller GetCaller()
    {
        return instanceCaller;
    }
}
