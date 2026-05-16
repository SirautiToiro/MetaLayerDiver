using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryAndRequireCardsUIPage : MonoBehaviour, IUIPage
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private StashPanel stashPanel;

    [SerializeField] private ItemArrangeManager itemArrangeManager;

    [SerializeField] private UIPageManager uiPageManager;

    [SerializeField] private CardArrangeManager cardArrangeManager;
    [SerializeField] private RequireCardHolder requireCardHolder;

    public UIPageManager UIPageManager { get { return uiPageManager; } }

    private List<StorageData.CardStack> requiredCards = null;

    private Action onCompletion = null;

    public void Init(List<StorageData.CardStack> requiredCards,Action onCompletion)
    {
        this.onCompletion = onCompletion;
        this.requiredCards = requiredCards;
    }
    
    public void OnPushed()
    {
        itemArrangeManager.OpenCardRequirePanel(requiredCards);
    }

    public void OnPopped()
    {
        //要求カード配置場所に配置されているカードをすべて戻す
        cardArrangeManager.RequireCardsAllMove();

        itemArrangeManager.CloseInventory();
        this.requiredCards = null;
        this.onCompletion = null;
    }

    /// <summary>
    /// 提出完了をするときのボタンからの呼び出し。完了が受理されるなら、
    /// onCompletionを呼び出して、UIを閉じる。
    /// </summary>
    public void CompleteRequireButton()
    {
        if (requireCardHolder.IsRequireCompleted())
        {//要求が満たされているなら、完了処理
            //完了時処理
            onCompletion?.Invoke();

            //要求されたカードをすべて消す
            requireCardHolder.DeleteRealCards();
            UIPageManager.PopUIPage();
        }
        else
        {//要求が満たされていないなら、何もしない
            return;
        }
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
