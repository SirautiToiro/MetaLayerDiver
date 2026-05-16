using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


/// <summary>
/// インベントリとスタッシュ(倉庫)が同時に開いた状態。
/// </summary>
public class InventoryAndStashUIPage : MonoBehaviour,IUIPage
{
    [SerializeField] UIPageManager uiPageManager;

    [SerializeField] private GameObject closeButton;
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private StashPanel stashPanel;

    [SerializeField] private ItemArrangeManager itemArrangeManager;

    public UIPageManager UIPageManager { get { return uiPageManager; }}

    public void OnPushed()
    {
        closeButton.SetActive(true);
        inventoryPanel.Open();
        stashPanel.Open(true);
    }

    public void OnPopped()
    {
        /*TODO:不正チェックはダンジョン入場時のみ

        if (!inventoryDeckCardsHolder.IsValidDeck())
        {//不適切なデッキなら閉じない
            //カード不適切のポップアップを表示する
            GameObject popupMessageObj = Instantiate(popupMessagePrefab, popupMessageParent.position, Quaternion.identity, popupMessageParent);
            PopupMessageUI messageUI = popupMessageObj.GetComponent<PopupMessageUI>();

            //挙動を定義するクラスを生成してセット
            var controller = new InvalidCardPopupController(this, messageUI, itemArrangeManager.GetInputBlocker(),true);

            //UIの初期化
            messageUI.Init(controller);

            return;
        }

        if(!backpackCardsHolder.IsValidBackpack())
        {//不適切なバックパックなら閉じない
            //カード不適切のポップアップを表示する
            GameObject popupMessageObj = Instantiate(popupMessagePrefab, popupMessageParent.position, Quaternion.identity, popupMessageParent);
            PopupMessageUI messageUI = popupMessageObj.GetComponent<PopupMessageUI>();
            //挙動を定義するクラスを生成してセット
            var controller = new InvalidCardPopupController(this, messageUI, itemArrangeManager.GetInputBlocker(), false);
            //UIの初期化
            messageUI.Init(controller);
            return;
        }
        */

        itemArrangeManager.CloseInventory();
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
}
