using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 倉庫のカードを格納するICardHolder
/// </summary>
public class StorageCardsHolder : MonoBehaviour, ICardHolder
{
    public ICardHolder.CardHolderType HolderType { get { return ICardHolder.CardHolderType.StackHolder; } }

    [SerializeField] private CardArrangeManager cardArrangeManager;
    [SerializeField] private InventoryPanel inventoryPanel;

    [SerializeField] private StackCardArrangement stackCardArrangement;

    

    public void Init(List<StorageData.CardStack> cardStacks)
    {
        stackCardArrangement.Init(cardStacks, this);

        SetStorageCards();
    }

    private void SetStorageCards()
    {
        List<StorageData.CardStack> cardList = stackCardArrangement.GetCards();

        inventoryPanel.SetStorageCards(cardList);
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
        cardArrangeManager.InstantiateAndMoveCard(stackCardArrangement.GetOneCardDataFromList(pos),
            stackCardArrangement.GetPlaceCardZone(pos));
        inventoryPanel.ChangeDisplayMode(false);//カードを表示するモードに切り替える

        SetStorageCards();//データ記録
    }

    public void PutCard(Card card, int pos)
    {
        Card newCard = stackCardArrangement.AddAndAlignCard(card);//データに追加し、整列。cardはこの中で破壊される
        stackCardArrangement.CardsCompleteMove();

        stackCardArrangement.ScrollToCard(newCard);

        SetStorageCards();//データ記録
    }

    /// <summary>
    /// 絞り込み条件をセットする
    /// </summary>
    /// <param name="narrowDown"></param>
    public void SetNarrowDown(NarrowDownCardsWindow.CardsNarrowDown narrowDown)
    {
        stackCardArrangement.SetNarrowDown(narrowDown);
    }

    /// <summary>
    /// データとして記録されているカードの一覧を返す
    /// </summary>
    /// <returns></returns>
    public List<StorageData.CardStack> GetCards()
    {
        return stackCardArrangement.GetCards();
    }
}
