using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Clown_SanManager : MonoBehaviour, IShopManager, IButtonWithHighlightManager
{
    private StashPanel stashPanel;
    private CardArrangeManager cardArrangeManager;
    private PhysicalItemArrangeManager physicalItemArrangeManager;
    private PhysicalItemInstantiateManager physicalItemInstantiateManager;

    [SerializeField] private RequireCardHolder cardHolder;
    [SerializeField] private ShopCardHolder returnHolder;

    [SerializeField] private ShopMessage shopMessage;

    //現在何を表示しているか
    private ShopType currentShopType;

    //各種ハイライト付きボタン。一つのボタンが点灯するときそれ以外は消灯
    [SerializeField] private List<ButtonWithHighlight> buttonWithHighlightList;

    //店の料金表示
    [SerializeField] private PriceListPanel priceListPanel;
    [SerializeField] private ShopPriceRate shopPriceRate;

    //ある一つのカード、に対応するためのカードデータ(システム上のワイルドカード)
    [SerializeField] private CardDataSO anyoneCardData;

    //ボタンの表示テキスト
    [SerializeField] private TextMeshProUGUI buttonText;

    private enum ShopType
    {
        DuplicateCards = 0,//複製画面
        ReturnCards//複製結果画面
    }

    //For test
    //[SerializeField] private CardDataSO testCardData;
    //[SerializeField] private CardDataSO testCardData2;

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

        currentShopType = ShopType.DuplicateCards;//現在は複製画面
        ShowShop();
    }

    private void ShowShop()
    {
        cardHolder.gameObject.SetActive(true);
        returnHolder.gameObject.SetActive(false);
        buttonText.text = "複製";

        StorageData.CardStack oneAnyoneCard = new StorageData.CardStack();
        oneAnyoneCard.cardSerialNum = anyoneCardData.serialNum;
        oneAnyoneCard.Stack = 1;

        /*
        //テスト用
        StorageData.CardStack testCard = new StorageData.CardStack();
        testCard.cardSerialNum = testCardData.serialNum;
        testCard.Stack = 1;
        StorageData.CardStack testCard2 = new StorageData.CardStack();
        testCard2.cardSerialNum = testCardData2.serialNum;
        testCard2.Stack = 2;
        cardHolder.Init( new List<StorageData.CardStack>() { testCard , testCard2 },cardArrangeManager,this);
        */

        cardHolder.Init(new List<StorageData.CardStack>() { oneAnyoneCard }, cardArrangeManager, stashPanel, this);


        //料金表示初期化
        priceListPanel.Init(shopPriceRate);
    }

    /// <summary>
    /// UI上の複製ボタンが押されたときに呼ばれる
    /// あるいは、複製結果画面では「もう一度」ボタンが押されたときに呼ばれる
    /// </summary>
    public void DuplicateButton()
    {
        var placedCards = cardHolder.GetCards();

        switch (currentShopType)
        {
            case ShopType.DuplicateCards:
                //複製ボタン
                if (placedCards.Count > 0 && placedCards[0].Stack >= 2)
                {//2枚以上のカードは複製できない
                    shopMessage.ShowMessage("TooMuchCard");//メッセージ
                    return;
                }
                else if (!cardHolder.IsRequireCompleted())
                {//要求が満たされているかのチェック
                    return;
                }
                else
                {//複製可能
                 //コインの消費を試みる
                    CardDataSO cardDataSO = PlayerCardData.GetCardDataFromSerialNum(placedCards[0].cardSerialNum);
                    if (stashPanel.ReduceCoin(shopPriceRate.GetRate(cardDataSO.tier.tier)))
                    {//複製成功
                        cardHolder.DeleteRealCards();
                        //複製されたカードを表示する
                        //複製結果画面
                        shopMessage.ShowMessage("Duplicate");//メッセージ

                        //ボタンの表記を変える
                        currentShopType = ShopType.ReturnCards;
                        buttonText.text = "もう一度";

                        //表示するHolderを変更
                        cardHolder.gameObject.SetActive(false);
                        returnHolder.gameObject.SetActive(true);

                        var duplicatedCards = new List<StorageData.CardStackData>();
                        foreach (var placedCard in placedCards)
                        {
                            duplicatedCards.Add(new StorageData.CardStackData()
                            {
                                CardData = PlayerCardData.GetCardDataFromSerialNum(placedCard.cardSerialNum),
                                Stack = placedCard.Stack * 2
                            });
                        }

                        //複製されたカードは既に料金を払っているので無料で受け取られる
                        var cost0PriceRate = new ShopPriceRate();
                        cost0PriceRate.CommonRate = 0;
                        cost0PriceRate.RareRate = 0;
                        cost0PriceRate.MetaRate = 0;

                        returnHolder.Init(duplicatedCards, cardArrangeManager, this, cost0PriceRate);
                    }
                    else
                    {//コインが足りない
                        shopMessage.ShowMessage("NoMoney");//メッセージ
                    }
                }
                break;
            case ShopType.ReturnCards:
                //もう一度ボタン
                if (returnHolder.GetCards().Count > 0)
                {//複製結果のカードを全て受け取ってからでないともう一度は押せない
                    shopMessage.ShowMessage("AlreadyCard");//メッセージ
                    return;
                }
                else
                {
                    shopMessage.ShowMessage("Selling");//メッセージ
                    currentShopType = ShopType.DuplicateCards;
                    ShowShop();
                    break;
                }
        }

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

    }

    public void OnSelled()
    {
        //使用しない
    }

    public void OnClosed()
    {
        //残っているアイテムをすべて移動する
        //カード要求部分
        Card remainedCard = null;
        do
        {
            List<Card> cardList = cardHolder.GetCardsInstances();
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

        //カード返却部分
        remainedCard = null;
        do
        {
            List<Card> cardList = returnHolder.GetCardsInstances();
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
    }

    public ICardHolder GetCardHolderForQuickMove()
    {
        //カード要求には配置できる
        switch (currentShopType)
        {
            case ShopType.DuplicateCards:
                return cardHolder;
            case ShopType.ReturnCards:
                return null;
            default:
                return null;
        }
    }

    public IPhysicalItemHolder GetPhysicalItemHolderForQuickMove()
    {
        return null;
    }
}
