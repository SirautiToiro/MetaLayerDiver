using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SupplyShopManager : MonoBehaviour, IShopManager,IButtonWithHighlightManager
{
    private StashPanel stashPanel;
    private CardArrangeManager cardArrangeManager;
    private PhysicalItemArrangeManager physicalItemArrangeManager;
    private PhysicalItemInstantiateManager physicalItemInstantiateManager;

    [SerializeField] private ShopCardHolder cardHolder;
    [SerializeField] private ShopItemHolder itemHolder;
    [SerializeField] private ShopMessage shopMessage;

    //販売するもののデータ
    [SerializeField] private List<StorageData.CardStackData> supplyCardsData;
    [SerializeField] private List<PhysicalItemGridPosNumData> supplyItemsData;
    [SerializeField] private List<PhysicalItemGridPosNumData> buyingItemsData;

    //現在何を表示しているか
    private ShopType currentShopType;

    //各種ハイライト付きボタン。一つのボタンが点灯するときそれ以外は消灯
    [SerializeField] private List<ButtonWithHighlight> buttonWithHighlightList;

    //店の料金表示
    [SerializeField]private PriceListPanel priceListPanel;
    [SerializeField]private ShopPriceRate shopPriceRateSupplyCards;
    [SerializeField]private ShopPriceRate shopPriceRateSupplyItems;
    [SerializeField]private ShopPriceRate shopPriceRateBuyingItems;


    private enum ShopType
    {
        SupplyCards = 0,
        SupplyItems,
        BuyingItems,
    }

    public void Init(StashPanel stashPanel,CardArrangeManager cardArrangeManager,
        PhysicalItemArrangeManager physicalItemArrangeManager,PhysicalItemInstantiateManager physicalItemInstantiateManager)
    {
        this.stashPanel = stashPanel;
        this.cardArrangeManager = cardArrangeManager;
        this.physicalItemArrangeManager = physicalItemArrangeManager;
        this.physicalItemInstantiateManager = physicalItemInstantiateManager;

        ShowShop(ShopType.SupplyCards);

        //最初のもの(支給カード)を除いて、全てのボタンを消灯
        for (int i = 0; i < buttonWithHighlightList.Count; i++)
        {
            buttonWithHighlightList[i].Init(this, false);
        }
        buttonWithHighlightList[0].Init(this, true);
    }

    private void ShowShop(ShopType shopType)
    {
        currentShopType = shopType;
        switch (shopType)
        {
            case ShopType.SupplyCards:
                cardHolder.gameObject.SetActive(true);
                itemHolder.gameObject.SetActive(false);
                cardHolder.Init(supplyCardsData,cardArrangeManager,this,shopPriceRateSupplyCards);
                priceListPanel.Init(shopPriceRateSupplyCards);

                shopMessage.ShowMessage("Sikyu");//メッセージ
                break;
            case ShopType.SupplyItems:
                cardHolder.gameObject.SetActive(false);
                itemHolder.gameObject.SetActive(true);
                itemHolder.Init(supplyItemsData,physicalItemArrangeManager,this,physicalItemInstantiateManager,shopPriceRateSupplyItems);
                priceListPanel.Init(shopPriceRateSupplyItems);

                shopMessage.ShowMessage("Sikyu");//メッセージ
                break;
            case ShopType.BuyingItems:
                cardHolder.gameObject.SetActive(false);
                itemHolder.gameObject.SetActive(true);
                itemHolder.Init(buyingItemsData, physicalItemArrangeManager, this, physicalItemInstantiateManager, shopPriceRateBuyingItems);
                priceListPanel.Init(shopPriceRateBuyingItems);

                shopMessage.ShowMessage("Selling");//メッセージ
                break;
        }
    }

    public void OnBuied(ItemBase item)
    {
        switch (currentShopType)
        {
            case ShopType.SupplyCards:
            case ShopType.SupplyItems:
                shopMessage.ShowMessage("BuySikyu");
                //支給カード購入時の処理
                break;
            case ShopType.BuyingItems:
                shopMessage.ShowMessage("BuySelling");
                //売却アイテム購入時の処理
                break;
        }
    }

    public void OnSelled()
    {
        shopMessage.ShowMessage("error");//支給品は売却されない
    }

    /// <summary>
    /// キャラクターがクリックされたときに呼出
    /// </summary>
    public void OnCharacterClicked()
    {
        shopMessage.ShowMessage("CharacterClicked");
    }

    /// <summary>
    /// ボタンから呼出。int型でShopTypeを指定する
    /// </summary>
    /// <param name="shopType"></param>
    public void ShowShop(int shopType)
    {
        ShowShop((ShopType)shopType);
    }

    public void ChangeDisplayMode(bool isItem)
    {
        stashPanel.ChangeDisplayMode(isItem);
    }

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

    public StashPanel GetStashPanel()
    {
        return stashPanel;
    }

    public void OnClosed()
    {
    }

    public ICardHolder GetCardHolderForQuickMove()
    {
        return null;
    }

    public IPhysicalItemHolder GetPhysicalItemHolderForQuickMove()
    {
        return null;
    }
}
