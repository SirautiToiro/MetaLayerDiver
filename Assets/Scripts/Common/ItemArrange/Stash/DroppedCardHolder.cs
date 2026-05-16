using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵からのアイテムドロップの時に出現するICardHolder
/// </summary>
public class DroppedCardHolder : MonoBehaviour, ICardHolder
{
    public ICardHolder.CardHolderType HolderType { get { return ICardHolder.CardHolderType.ScrollHolder; } }


    [SerializeField] private CardArrangeManager cardArrangeManager;
    [SerializeField] private ScrollCardArrangement scrollCardArrangement;
    [SerializeField] private InventoryPanel inventoryPanel;


    public void Init(List<int> droppedCard)
    {
        scrollCardArrangement.Init(droppedCard, this);
    }

    public void EnterCard(int pos)
    {
        if (pos == -1)
        {//カードが離れた場合の動作のみ行う
            //カードが重なった場合は何もしない
            scrollCardArrangement.CardsCompleteMove();//移動状況リセット
            scrollCardArrangement.AlignAndSpaceCards(pos);//カード配列調整
            scrollCardArrangement.CardsBackToBasePos();//位置調整
        }
    }

    public void ExitCard()
    {
        scrollCardArrangement.AlignAndSpaceCards(-1);//カード整列
        scrollCardArrangement.CardsCompleteMove();
        scrollCardArrangement.SetZoneNum();//配置場所数調整
    }

    public GameObject GetScrollViewObject()
    {
        return scrollCardArrangement.GetScrollViewObject();
    }

    public void PullCard(int pos)
    {
        scrollCardArrangement.RemoveCardFromList(pos);//データ調整
        inventoryPanel.ChangeDisplayMode(false);//カードを表示するモードに切り替える
    }

    public void PutCard(Card card, int pos)
    {
        //カードをデータに入れる
        scrollCardArrangement.AddAndAlignCard(card, pos);//データセット、整列
        scrollCardArrangement.CardsCompleteMove();//位置リセット
        scrollCardArrangement.SetZoneNum();//Zone数調整

        scrollCardArrangement.CardsBackToBasePos();//位置調整
        scrollCardArrangement.CardsCompleteMove();//位置リセット

        scrollCardArrangement.ScrollToCard(card);//挿入したCardの位置に移動する
    }

    public List<Card> GetCardInstances()
    {
        return scrollCardArrangement.GetCards();
    }

    /// <summary>
    /// データとして記録されているカードの一覧を返す
    /// </summary>
    /// <returns></returns>
    public List<StorageData.CardStack> GetCards()
    {
        var cards = scrollCardArrangement.GetCards();

        var cardStacks = new List<StorageData.CardStack>();

        foreach (var card in cards)
        {
            cardStacks.Add(new StorageData.CardStack
            {
                cardSerialNum = card.serialNum,
                Stack = 1
            });
        }

        return cardStacks;
    }
}
