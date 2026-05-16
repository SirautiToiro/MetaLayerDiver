using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryPanel : MonoBehaviour
{
    [SerializeField] private Canvas canvas;

    [SerializeField] private GameObject itemFrame;
    [SerializeField] private GameObject equipFrame;
    [SerializeField] private GameObject deckCardFrame;
    [SerializeField] private GameObject bagCardFrame;

    [SerializeField] private ItemArrangeManager itemArrangeManager;
    [SerializeField] private InventoryDeckCardsHolder inventoryDeckCardsHolder;
    [SerializeField] private InventoryBackpackCardsHolder backpackCardsHolder;
    [SerializeField] private InventoryBackpackItemsHolder inventoryBackpackItemsHolder;
    [SerializeField] private InventoryEquipItemsHolder inventoryEquipItemsHolder;

    [SerializeField] private StashPanel stashPanel;

    [SerializeField] private Transform popupMessageParent; //ポップアップメッセージの配置場所
    [SerializeField] private GameObject popupMessagePrefab; //ポップアップメッセージのプレハブ

    [SerializeField] private UIPageManager uiPageManager;
    [SerializeField] private InputBlocker inputBlocker;//アイテム画面の入力制限

    /// <summary>
    /// インベントリ画面を初期化する
    /// </summary>
    public void Init()
    {
        canvas.enabled = false;

        //アイテム表示初期化
        inventoryBackpackItemsHolder.Init(InventoryData.GetBackpackItems());
        inventoryEquipItemsHolder.Init(InventoryData.GetEquippingPhysicalWeapons(),
            InventoryData.GetEquippingGear(), InventoryData.GetEquippingConsumables());

        //デッキ表示初期化
        inventoryDeckCardsHolder.Init(InventoryData.GetPlayerDeck());
        backpackCardsHolder.Init(InventoryData.GetBackpackCards());
    }

    /// <summary>
    /// インベントリの画面を開く処理
    /// </summary>
    public void Open()
    {
        SetDisplayMode(true);//最初はアイテムモード
        canvas.enabled = true;
    }

    /// <summary>
    /// ボタンから操作。
    /// アイテム表示モードに切り替え
    /// </summary>
    public void ShowItems()
    {
        SetDisplayMode(true);
    }

    /// <summary>
    /// ボタンから操作。
    /// カード表示モードに切り替え
    /// </summary>
    public void ShowCards()
    {
        SetDisplayMode(false);
    }

    public void HideInventory()
    {
        canvas.enabled = false;

    }

    /// <summary>
    /// インベントリの表示形式を変更する。
    /// 表示形式は、アイテムと装備を表示するモードと
    /// カードを表示するモード
    /// </summary>
    /// <param name="isItem">trueならアイテムを表示するモード</param>
    private void SetDisplayMode(bool isItem)
    {
        itemFrame.SetActive(isItem);
        equipFrame.SetActive(isItem);
        deckCardFrame.SetActive(!isItem);
        bagCardFrame.SetActive(!isItem);

        if (isItem)
        {
            //アイテム表示初期化
            inventoryBackpackItemsHolder.Init(InventoryData.GetBackpackItems());
            inventoryEquipItemsHolder.Init(InventoryData.GetEquippingPhysicalWeapons(),
                InventoryData.GetEquippingGear(),InventoryData.GetEquippingConsumables());
        }
        else
        {
            //デッキ表示初期化
            inventoryDeckCardsHolder.Init(InventoryData.GetPlayerDeck());
            backpackCardsHolder.Init(InventoryData.GetBackpackCards());
        }
    }

    /// <summary>
    /// 初期化を行わない表示モード変更
    /// </summary>
    /// <param name="isItem">trueならアイテムを表示する</param>
    public void ChangeDisplayMode(bool isItem)
    {
        itemFrame.SetActive(isItem);
        equipFrame.SetActive(isItem);
        deckCardFrame.SetActive(!isItem);
        bagCardFrame.SetActive(!isItem);
    }

    /// <summary>
    /// アイテムの移動を開始したときなどに、倉庫の開き方が不適切なら、対応した場所を開く
    /// </summary>
    /// <param name="isItem"></param>
    public void ChangeStashMode(bool isItem)
    {
        if(isItem)
        {
            if(stashPanel.GetCurrentPage() == 0)//カードページが開かれているなら
            {
                stashPanel.OpenStorageItem(1);//アイテム倉庫の1ページ目を開く
            }
        }
        else
        {
            if (stashPanel.GetCurrentPage() != 0)//アイテムページが開かれているなら
            {
                stashPanel.OpenStorageCard();
            }
        }
    }

    /// <summary>
    /// デッキ編集画面でデッキが編集完了した。
    /// デッキとして成立しているかをboolで返す
    /// </summary>
    /// <param name="deck">編集したデッキ</param>
    /// <returns>trueなら編集成功</returns>
    public bool SetDeck(Deck deck)
    {
        return InventoryData.SetPlayerDeck(deck);
    }

    /// <summary>
    /// バックパック内カードの編集が完了した。
    /// 鞄に入るかをboolで返す。
    /// </summary>
    /// <param name="cardSerials">編集が終了したカードたち</param>
    /// <returns>trueなら編集成功</returns>
    public bool SetBackpackCards(List<int> cardSerials)
    {
        return InventoryData.SetBackpackCards(cardSerials);
    }

    public bool SetBackpackItems(List<PhysicalItemGridPosNumData> items)
    {
        return InventoryData.SetBackpackItems(items);
    }

    public bool SetEquippingItems(List<PhysicalItemDataSO> weapons, PhysicalItemDataSO gear, List<PhysicalItemDataSO> consumables)
    {
        return InventoryData.SetEquippingItems(weapons, gear, consumables);
    }

    public void SetStorageCards(List<StorageData.CardStack> cardStacks)
    {
        StorageData.SetStorageCards(cardStacks);
    }

    public void SetStorageItems(List<PhysicalItemGridPosNumData> items)
    {
        StorageData.SetStorageItems(items,stashPanel.GetCurrentPage()-1);//アイテムのページは1始まり
    }

    public ItemArrangeManager.Caller GetCaller()
    {
        return itemArrangeManager.GetCaller();
    }

    public void InputBlockingUp()
    {
        inputBlocker.InputBlockingUp();
    }

    public void InputBlockingDown()
    {
        inputBlocker.InputBlockingDown();
    }
}
