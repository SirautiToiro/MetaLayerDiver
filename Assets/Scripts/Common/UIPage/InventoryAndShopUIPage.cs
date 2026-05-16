using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryAndShopUIPage : MonoBehaviour, IUIPage
{
    [SerializeField] private GameObject closeButton;
    [SerializeField] private InventoryPanel inventoryPanel;
    [SerializeField] private StashPanel stashPanel;

    [SerializeField] private ItemArrangeManager itemArrangeManager;

    [SerializeField] private UIPageManager uiPageManager;

    public UIPageManager UIPageManager { get { return uiPageManager; } }

    private ShopTypeDefine.ShopType? shopType = null;

    public void Init(ShopTypeDefine.ShopType shopType)
    {
        this.shopType = shopType;
    }

    public void OnPushed()
    {
        if(shopType is not null)
        {
            closeButton.SetActive(true);
            inventoryPanel.Open();
            stashPanel.OpenShop(shopType.Value);
        }
    }

    public void OnPopped()
    {
        itemArrangeManager.CloseInventory();

        this.shopType = null;
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
