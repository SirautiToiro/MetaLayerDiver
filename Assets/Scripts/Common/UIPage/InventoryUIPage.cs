using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIPage : MonoBehaviour,IUIPage
{
    [SerializeField] UIPageManager uiPageManager;

    [SerializeField] private GameObject closeButton;
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private StashPanel stashPanel;

    [SerializeField] private ItemArrangeManager itemArrangeManager;

    public UIPageManager UIPageManager { get { return uiPageManager; } }

    public void OnBecomeTopPage()
    {
        inventoryPanel.InputBlockingDown();
    }

    public void OnCovered()
    {
        inventoryPanel.InputBlockingUp();
    }

    public void OnPopped()
    {
        itemArrangeManager.CloseInventory();
    }

    public void OnPushed()
    {
        closeButton.SetActive(true);
        inventoryPanel.Open();
        stashPanel.Open(false);//‘qŒÉ•”•ª‚Í•Â‚¶‚é
    }

    public void PushSelf()
    {
        UIPageManager.PushUIPage(this);
    }
}
