using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardShopManager : MonoBehaviour, IShopManager, IButtonWithHighlightManager
{
    private StashPanel stashPanel;
    private CardArrangeManager cardArrangeManager;
    private PhysicalItemArrangeManager physicalItemArrangeManager;
    private PhysicalItemInstantiateManager physicalItemInstantiateManager;

    [SerializeField] private ShopCardHolder cardHolder;

    [SerializeField] private ShopMessage shopMessage;

    //現在何を表示しているか
    private ShopType currentShopType;

    //各種ハイライト付きボタン。一つのボタンが点灯するときそれ以外は消灯
    [SerializeField] private List<ButtonWithHighlight> buttonWithHighlightList;

    //店の料金表示
    [SerializeField] private PriceListPanel priceListPanel;
    [SerializeField] private ShopPriceRate shopPriceRateSpecialPriceCards;
    [SerializeField] private ShopPriceRate shopPriceRateNormalCards;

    //販売物データ
    private VillageShopData villageShopData;

    private enum ShopType
    {
        SpecialPriceCards = 0,
        NormalCards
    }

    public void Init(StashPanel stashPanel, CardArrangeManager cardArrangeManager,
        PhysicalItemArrangeManager physicalItemArrangeManager, PhysicalItemInstantiateManager physicalItemInstantiateManager)
    {
        this.stashPanel = stashPanel;
        this.cardArrangeManager = cardArrangeManager;
        this.physicalItemArrangeManager = physicalItemArrangeManager;
        this.physicalItemInstantiateManager = physicalItemInstantiateManager;

        this.villageShopData = stashPanel.GetItemArrangeManager().GetCaller().VillageManager?.GetVillageShopData();

        ShowShop(ShopType.SpecialPriceCards);

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
            case ShopType.SpecialPriceCards:
                cardHolder.gameObject.SetActive(true);
                cardHolder.Init(villageShopData.specialPriceCardsData, cardArrangeManager, this, shopPriceRateSpecialPriceCards);
                priceListPanel.Init(shopPriceRateSpecialPriceCards);

                shopMessage.ShowMessage("SpecialPrice");//メッセージ
                break;
            case ShopType.NormalCards:
                cardHolder.gameObject.SetActive(true);
                cardHolder.Init(villageShopData.normalCardsData, cardArrangeManager, this, shopPriceRateNormalCards);
                priceListPanel.Init(shopPriceRateNormalCards);

                shopMessage.ShowMessage("Selling");//メッセージ
                break;
        }
    }

    public void OnBuied(ItemBase itemBase)
    {
        switch (currentShopType)
        {
            case ShopType.SpecialPriceCards:
                shopMessage.ShowMessage("BuySpecialPrice");
                //特価カード購入時の処理
                villageShopData.SpecialPriceCardsOnBuied(((Card)itemBase).serialNum);
                break;
            case ShopType.NormalCards:
                shopMessage.ShowMessage("BuySelling");
                //売却カード購入時の処理
                villageShopData.NormalCardsOnBuied(((Card)itemBase).serialNum);
                break;
        }
    }

    public void OnSelled()
    {
        shopMessage.ShowMessage("error");//売却されない
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
