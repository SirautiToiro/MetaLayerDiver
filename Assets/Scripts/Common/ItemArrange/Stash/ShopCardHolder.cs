using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 店画面で使用するCardHolder。IShopManagerから与えられたアイテムを、与えられた値段で販売する
/// ShopCardHolderにはドラッグで配置ができない。CardArrangeManager.EndDragging()に記述
/// </summary>
public class ShopCardHolder : MonoBehaviour, ICardHolder
{
    public ICardHolder.CardHolderType HolderType { get { return ICardHolder.CardHolderType.StackHolder; } }

    private IShopManager shopManager;

    [SerializeField] private StackCardArrangement stackCardArrangement;

    private CardArrangeManager cardArrangeManager;

    private ShopPriceRate shopPriceRate;

    public void Init(List<StorageData.CardStackData> cardStackData,CardArrangeManager cardArrangeManager,
        IShopManager shopManager,ShopPriceRate rate)
    {
        this.cardArrangeManager = cardArrangeManager;
        this.shopManager = shopManager;
        shopPriceRate = rate;

        List<StorageData.CardStack> cardStacks= cardStackData.Select(x=> new StorageData.CardStack{
            cardSerialNum = x.CardData.serialNum,
            Stack = x.Stack
        }).ToList();
        stackCardArrangement.Init(cardStacks, this, cardArrangeManager);
    }

    public void EnterCard(int pos)
    {
        if (pos == -1)
        {
            stackCardArrangement.CardsCompleteMove();//移動状況リセット
            stackCardArrangement.AlignAndSpaceCards(pos);//カード配列調整
            stackCardArrangement.CardsBackToBasePos();//位置調整
        }
    }

    public void ExitCard()
    {
        stackCardArrangement.AlignAndSpaceCards(-1);//整列
        stackCardArrangement.CardsCompleteMove();
        stackCardArrangement.SetZoneNum();//Zone数調整
    }

    public GameObject GetScrollViewObject()
    {
        return stackCardArrangement.GetScrollViewObject();
    }

    public void PullCard(int pos)
    {
        Card card = cardArrangeManager.InstantiateAndMoveCard(stackCardArrangement.GetOneCardDataFromList(pos),
            stackCardArrangement.GetPlaceCardZone(pos));
        shopManager.ChangeDisplayMode(false);//カードを表示するモードに切り替える

        shopManager.OnBuied(card);
    }

    public void PutCard(Card card, int pos)
    {
        Card newCard = stackCardArrangement.AddAndAlignCard(card);//データに追加し、整列。cardはこの中で破壊される
        stackCardArrangement.CardsCompleteMove();

        stackCardArrangement.ScrollToCard(newCard);
    }

    public bool TryBuyCard(Card item)
    {
        return shopManager.GetStashPanel().ReduceCoin(shopPriceRate.GetRate(item.tier.tier));
    }

    /// <summary>
    /// データとして記録されているカードの一覧を返す
    /// </summary>
    /// <returns></returns>
    public List<StorageData.CardStack> GetCards()
    {
        return stackCardArrangement.GetCards();
    }

    public List<Card> GetCardsInstances()
    {
        return stackCardArrangement.GetCardsInstances();
    }
}
