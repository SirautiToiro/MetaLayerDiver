using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードを廃棄する画面で一時的に出てくるICardHolder
/// 廃棄用なので、ここに配置されたカードはデータに記録されない
/// </summary>
public class DiscardCardHolder : MonoBehaviour, ICardHolder
{
    public ICardHolder.CardHolderType HolderType { get { return ICardHolder.CardHolderType.ScrollHolder; } }

    //CardはCardArrangeManagerが操作する
    [SerializeField] private CardArrangeManager cardArrangeManager;

    //スクロールするカード保存
    [SerializeField] private ScrollCardArrangement scrollCardArrangement;

    /// <summary>
    /// 最初に画面外にドラッグされたCardを、捨てるアイテムとして最初に登録する
    /// </summary>
    /// <param name="card">最初に捨てるアイテム</param>
    public void Init(Card card)
    {
        List<Card> cardList = new List<Card>();
        cardList.Add(card);//最初に捨てるカードを登録

        scrollCardArrangement.Init(cardList, this);//スクロールカードの初期化
        scrollCardArrangement.CardsCompleteMove(); //カードの移動を完了させる
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

    public void PullCard(int pos)
    {
        scrollCardArrangement.RemoveCardFromList(pos);//データ調整
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
    public GameObject GetScrollViewObject()
    {
        return scrollCardArrangement.GetScrollViewObject();
    }

    public void DicideButton()
    {//廃棄を決定
        cardArrangeManager.ShowDiscardMenu();
    }

    public void CancelButton()
    {//廃棄をキャンセル
        cardArrangeManager.CancelDiscard();
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
