using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryAndRequireItemsUIPage : MonoBehaviour, IUIPage
{
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private StashPanel stashPanel;

    [SerializeField] private ItemArrangeManager itemArrangeManager;
    [SerializeField] private PhysicalItemArrangeManager physicalItemArrangeManager;

    [SerializeField] private UIPageManager uiPageManager;

    [SerializeField] private RequirePhysicalItemHolder requireItemHolder;

    public UIPageManager UIPageManager { get { return uiPageManager; } }

    private List<PhysicalItemGridPosNumData> requiredItems = null;

    private Action onCompletion = null;

    public void Init(List<PhysicalItemGridPosNumData> requiredItems,Action onCompletion)
    {
        this.requiredItems = requiredItems;
        this.onCompletion = onCompletion;
    }

    public void OnPopped()
    {
        physicalItemArrangeManager.RequireItemsAllMove();

        itemArrangeManager.CloseInventory();
        this.requiredItems = null;
        this.onCompletion = null;
    }

    /// <summary>
    /// ボタンからの呼び出し。提出。
    /// 完了が受理されるなら、OnCompletionを呼び出して、UIを閉じる。
    /// </summary>
    public void CompleteRequireButton()
    {
        if (requireItemHolder.IsRequireCompleted())
        {
            onCompletion?.Invoke();

            //配置されているアイテムをすべて消す
            requireItemHolder.DeleteRealItems();
            UIPageManager.PopUIPage();
        }
        else
        {
            return;
        }
    }

    public void OnPushed()
    {
        itemArrangeManager.OpenItemRequirePanel(requiredItems);
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
