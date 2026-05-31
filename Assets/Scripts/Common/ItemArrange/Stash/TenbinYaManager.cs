using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ShopTypeDefine;
using UnityEngine.UI;

public class TenbinYaManager : MonoBehaviour, IShopManager, IButtonWithHighlightManager
{
    private StashPanel stashPanel;
    private CardArrangeManager cardArrangeManager;
    private PhysicalItemArrangeManager physicalItemArrangeManager;
    private PhysicalItemInstantiateManager physicalItemInstantiateManager;

    [SerializeField] private SellCardHolder sellCardHolder;
    [SerializeField] private SellItemHolder sellItemHolder;
    [SerializeField] private ShopItemHolder returnGoldHolder;

    [SerializeField] private ShopMessage shopMessage;

    //現在何を表示しているか
    private ShopType currentShopType;

    //各種ハイライト付きボタン。一つのボタンが点灯するときそれ以外は消灯
    [SerializeField] private List<ButtonWithHighlight> buttonWithHighlightList;

    //店の料金表示
    [SerializeField] private PriceListPanel priceListPanel;
    [SerializeField] private ShopPriceRate cardPriceRate;
    [SerializeField] private ShopPriceRate itemPriceRate;

    //ボタン(表示を出し入れ)
    [SerializeField] private Button button;

    [SerializeField] private PhysicalItemDataSO goldData;

    private enum ShopType
    {
        SellCards = 0,//カード販売画面
        SellItems,  //アイテム販売画面
        ReturnGolds//販売結果画面
    }

    public void ChangeDisplayMode(bool isItem)
    {
        stashPanel.ChangeDisplayMode(isItem);
    }

    public StashPanel GetStashPanel()
    {
        return stashPanel;
    }

    public void Init(StashPanel stashPanel, CardArrangeManager cardArrangeManager, PhysicalItemArrangeManager physicalItemArrangeManager, PhysicalItemInstantiateManager physicalItemInstantiateManager)
    {
        this.stashPanel = stashPanel;
        this.cardArrangeManager = cardArrangeManager;
        this.physicalItemArrangeManager = physicalItemArrangeManager;
        this.physicalItemInstantiateManager = physicalItemInstantiateManager;

        shopMessage.ShowMessage("Selling");//メッセージ

        currentShopType = ShopType.SellCards;//現在はカード販売画面
        ShowShop(currentShopType);

        //最初のもの(カード販売)を除いて、全てのボタンを消灯
        for (int i = 0; i < buttonWithHighlightList.Count; i++)
        {
            buttonWithHighlightList[i].Init(this, false);
        }
        buttonWithHighlightList[0].Init(this, true);
    }

    /// <summary>
    /// ボタンから呼出。int型でShopTypeを指定する
    /// </summary>
    /// <param name="shopType"></param>
    public void ShowShop(int shopType)
    {
        ShowShop((ShopType)shopType);
    }

    private void ShowShop(ShopType shopType,int soldPrice=0)
    {
        //それまでに配置されていたカード類を戻す
        switch (currentShopType)
        {
            case ShopType.SellCards:
                //残っているアイテムをすべて移動する
                //カード販売部分
                Card remainedCard = null;
                do
                {
                    List<Card> cardList = sellCardHolder.GetCardsInstances();
                    if (cardList.Count > 0)
                    {
                        remainedCard = cardList[0];
                        cardArrangeManager.QuickMove(remainedCard);
                    }
                    else
                    {
                        remainedCard = null;
                    }
                } while (remainedCard != null);
                break;
            case ShopType.SellItems:
                //アイテム販売部分
                //残っているアイテムをすべて移動する
                List<PhysicalItemBase> itemList = sellItemHolder.GetItems();

                foreach (PhysicalItemBase item in itemList)
                {
                    //アイテム要求のアイテムを1個ずつ移動する
                    //すべてのスタックに対して
                    physicalItemArrangeManager.QuickMove(item);
                }
                break;
            case ShopType.ReturnGolds:
                //アイテム販売部分
                //残っているアイテムをすべて移動する
                List<PhysicalItemBase> itemList2 = returnGoldHolder.GetItems();

                foreach (PhysicalItemBase item in itemList2)
                {
                    //アイテム要求のアイテムを1個ずつ移動する
                    //すべてのスタックに対して
                    physicalItemArrangeManager.QuickMove(item);
                }
                break;
        }


        //対応するボタンを除いて、全てのボタンを消灯
        for (int i = 0; i < buttonWithHighlightList.Count; i++)
        {
            buttonWithHighlightList[i].Init(this, false);
        }
        buttonWithHighlightList[(int)shopType].Init(this, true);

        currentShopType = shopType;

        switch (shopType)
        {
            case ShopType.SellCards:
                sellCardHolder.gameObject.SetActive(true);
                sellItemHolder.gameObject.SetActive(false);
                returnGoldHolder.gameObject.SetActive(false);
                button.gameObject.SetActive(true);

                sellCardHolder.Init(new List<StorageData.CardStack>() {}, cardArrangeManager, stashPanel, this);


                //料金表示初期化
                priceListPanel.Init(cardPriceRate);
                break;
            case ShopType.SellItems:
                sellCardHolder.gameObject.SetActive(false);
                sellItemHolder.gameObject.SetActive(true);
                returnGoldHolder.gameObject.SetActive(false);
                button.gameObject.SetActive(true);

                sellItemHolder.Init(new List<PhysicalItemGridPosNumData>() { }, physicalItemArrangeManager, stashPanel, this);


                //料金表示初期化
                priceListPanel.Init(itemPriceRate);
                break;
            case ShopType.ReturnGolds:
                if(soldPrice>returnGoldHolder.GridHeight* returnGoldHolder.GridWidth* goldData.StackMax)
                {
                    shopMessage.ShowMessage("TooManyCards");//カードが多すぎる
                }

                sellCardHolder.gameObject.SetActive(false);
                sellItemHolder.gameObject.SetActive(false);
                returnGoldHolder.gameObject.SetActive(true);
                button.gameObject.SetActive(false);

                List<PhysicalItemDataSO> goldDataList = new List<PhysicalItemDataSO>() { };
                int stackMax= goldData.StackMax;
                for(int i = 0; i<soldPrice; i++)
                {
                    goldDataList.Add(goldData);
                }

                returnGoldHolder.Init(goldDataList, physicalItemArrangeManager,this,physicalItemInstantiateManager,new ShopPriceRate(0,0,0),true);
                break;
        }
    }

    //配置されているカード、アイテムを販売
    public void SellButton()
    {
        int soldPrice = 0;

        switch (currentShopType)
        {
            case ShopType.SellCards:
                List<StorageData.CardStack> cardList = sellCardHolder.GetCards();
                foreach (StorageData.CardStack card in cardList)
                {
                    switch(PlayerCardData.GetCardDataFromSerialNum(card.cardSerialNum).tier.tier)
                    {
                        case TierDefine.Tier.Common:
                            soldPrice += cardPriceRate.CommonRate * card.Stack;
                            break;
                        case TierDefine.Tier.Rare:
                            soldPrice += cardPriceRate.RareRate * card.Stack;
                            break;
                        case TierDefine.Tier.Meta:
                            soldPrice += cardPriceRate.MetaRate * card.Stack;
                            break;
                    }
                }

                //中身を消す
                sellCardHolder.Init(new List<StorageData.CardStack>() { }, cardArrangeManager, stashPanel, this);

                break;
            case ShopType.SellItems:
                var items = sellItemHolder.GetItems();

                foreach(PhysicalItemBase item in items)
                {
                    if(item.BaseItemData.PhysicalItemType==goldData.PhysicalItemType&&item.BaseItemData.SerialNum==goldData.SerialNum)
                    {//ゴールドは同じ値段
                        soldPrice += item.Stack;
                        continue;
                    }
                    switch (item.GetItemData().Tier.tier)
                    {
                        case TierDefine.Tier.Common:
                            soldPrice += itemPriceRate.CommonRate * item.Stack;
                            break;
                        case TierDefine.Tier.Rare:
                            soldPrice += itemPriceRate.RareRate * item.Stack;
                            break;
                        case TierDefine.Tier.Meta:
                            soldPrice += itemPriceRate.MetaRate * item.Stack;
                            break;
                    }
                }

                //中身を消す
                sellItemHolder.Init(new List<PhysicalItemGridPosNumData>() { }, physicalItemArrangeManager, stashPanel, this);

                break;
            default:
                return;
        }
        ShowShop(ShopType.ReturnGolds,soldPrice);
    }

    /// <summary>
    /// キャラクターがクリックされたときに呼出
    /// </summary>
    public void OnCharacterClicked()
    {
        shopMessage.ShowMessage("CharacterClicked");
    }

    public void OnBuied(ItemBase itemBase)
    {
        //使用しない;
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

    public void OnSelled()
    {
        shopMessage.ShowMessage("Sold");
    }

    public void OnClosed()
    {
        switch (currentShopType)
        {
            case ShopType.SellCards:
                //残っているアイテムをすべて移動する
                //カード販売部分
                Card remainedCard = null;
                do
                {
                    List<Card> cardList = sellCardHolder.GetCardsInstances();
                    if (cardList.Count > 0)
                    {
                        remainedCard = cardList[0];
                        cardArrangeManager.QuickMove(remainedCard);
                    }
                    else
                    {
                        remainedCard = null;
                    }
                } while (remainedCard != null);
                break;
            case ShopType.SellItems:
                //アイテム販売部分
                //残っているアイテムをすべて移動する
                List<PhysicalItemBase> itemList = sellItemHolder.GetItems();

                foreach (PhysicalItemBase item in itemList)
                {
                    //アイテム要求のアイテムを1個ずつ移動する
                    //すべてのスタックに対して
                    physicalItemArrangeManager.QuickMove(item);
                }
                break;
            case ShopType.ReturnGolds:
                //アイテム販売部分
                //残っているアイテムをすべて移動する
                List<PhysicalItemBase> itemList2 = returnGoldHolder.GetItems();

                foreach (PhysicalItemBase item in itemList2)
                {
                    //アイテム要求のアイテムを1個ずつ移動する
                    //すべてのスタックに対して
                    physicalItemArrangeManager.QuickMove(item);
                }
                break;
        }
        Debug.Log("Closed");
    }

    public ICardHolder GetCardHolderForQuickMove()
    {
        //カード要求に配置可能な場合
        switch (currentShopType)
        {
            case ShopType.SellCards:
                return sellCardHolder;
            default:
                return null;
        }
    }

    public IPhysicalItemHolder GetPhysicalItemHolderForQuickMove()
    {
        //アイテム要求に配置可能な場合
        switch (currentShopType)
        {
            case ShopType.SellItems:
                return sellItemHolder;
            default:
                return null;
        }
    }
}
