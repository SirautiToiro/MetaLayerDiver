using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryAndRewardUIPage : MonoBehaviour, IUIPageNeedsPopCheck
{
    [SerializeField] private GameObject closeButton;
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private StashPanel stashPanel;

    [SerializeField] private ItemArrangeManager itemArrangeManager;

    [SerializeField] private UIPageManager uiPageManager;

    public UIPageManager UIPageManager { get { return uiPageManager; } }

    private bool popCheckFlag = false;

    public bool PopCheckFlag { get { return popCheckFlag; } set { popCheckFlag = value; } }

    private List<int> droppedCards;
    private List<PhysicalItemDataSO> droppedItems;
    private StashPanel.DropFrameType? dropFrameType;

    private Action onCompletion;

    [SerializeField] private Transform popupMessageParent; //ポップアップメッセージの配置場所
    [SerializeField] private GameObject popupMessageWithTogglePrefab; //ポップアップメッセージのプレハブ

    public void Init(List<int> droppedCards, List<PhysicalItemDataSO> droppedItems, StashPanel.DropFrameType type
        , Action onCompletion)
    {
        this.droppedCards = droppedCards;
        this.droppedItems = droppedItems;
        this.dropFrameType = type;
        this.onCompletion = onCompletion;
    }

    public void OnPushed()
    {
        if (droppedCards is null || droppedItems is null || dropFrameType is null) return;

        popCheckFlag = false;

        itemArrangeManager.OpenItemDropInventory(droppedCards, droppedItems, dropFrameType.Value);
    }

    public void OnPopped()
    {
        //完了時処理
        onCompletion?.Invoke();

        itemArrangeManager.CloseInventory();

        droppedCards = null;
        droppedItems = null;
        dropFrameType = null;
        onCompletion = null;
    }

    public void FinishRewardButton()
    {

    }

    public void PushSelf()
    {
        UIPageManager.PushUIPage(this);
    }

    public void OnCovered()
    {
        inventoryPanel.InputBlockingUp();
    }

    public void OnBecomeTopPage()
    {
        inventoryPanel.InputBlockingDown();
    }

    /// <summary>
    /// Popする前にポップアップウィンドウを表示するなどの処理をする。ポップしていいならtrueを返す。
    /// </summary>
    /// <returns>popしていいか</returns>
    public bool CheckCanPop()
    {
        switch (stashPanel.CurrentDropFrameType)
        {
            case StashPanel.DropFrameType.Monster:
                return ShowEndBattleMenu();
            case StashPanel.DropFrameType.QuestReward:
                return ShowEndQusetRewardMenu();
        }
        return true;
    }

    private bool ShowEndBattleMenu()
    {
        if (SettingManager.IsShowEndBattlePopup)
        {//ポップアップを表示する設定なら
            //削除のポップアップを表示する
            GameObject popupMessageObj = Instantiate(popupMessageWithTogglePrefab, popupMessageParent.position, Quaternion.identity, popupMessageParent);
            PopupMessageWithToggleUI messageUI = popupMessageObj.GetComponent<PopupMessageWithToggleUI>();

            //挙動を定義するクラスを生成してセット
            var controller = new EndBattlePopupController(this, messageUI, itemArrangeManager.GetInputBlocker());

            //UIの初期化
            messageUI.Init(controller);
            return false;
        }
        else
        {//メニューを表示せず、いきなり終了
            return true;
        }
    }

    private bool ShowEndQusetRewardMenu()
    {
        if (SettingManager.IsShowQuestRewardPopup)
        {//ポップアップを表示する設定なら
            GameObject popupMessageObj = Instantiate(popupMessageWithTogglePrefab, popupMessageParent.position, Quaternion.identity, popupMessageParent);
            PopupMessageWithToggleUI messageUI = popupMessageObj.GetComponent<PopupMessageWithToggleUI>();
            //挙動を定義するクラスを生成してセット
            var controller = new EndQuestRewardPopupController(this, messageUI, itemArrangeManager.GetInputBlocker());
            //UIの初期化
            messageUI.Init(controller);

            return false;
        }
        else
        {//メニューを表示せず、いきなり終了
            return true;
        }
    }

    public void PopUIPage()
    {
        UIPageManager.PopUIPage();
    }
}
