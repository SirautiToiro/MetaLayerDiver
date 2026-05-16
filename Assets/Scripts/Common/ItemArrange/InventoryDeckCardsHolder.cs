using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

/// <summary>
/// インベントリ操作のデッキのカードを管理するクラス
/// </summary>
public class InventoryDeckCardsHolder : MonoBehaviour,ICardHolder
{

    //CardはCardArrangeManagerが操作する
    [SerializeField] private CardArrangeManager cardArrangeManager;
    [SerializeField] private InventoryPanel inventoryPanel;

    //スクロールするカード保存
    [SerializeField] private ScrollCardArrangement scrollCardArrangement;

    //DeckCardFrame
    [SerializeField] private TextMeshProUGUI deckMaxText;
    [SerializeField] private TextMeshProUGUI deckNumText;

    private string deckName;

    public ICardHolder.CardHolderType HolderType { get { return ICardHolder.CardHolderType.ScrollHolder; } }

    /// <summary>
    /// 現在のデッキを参照してカードを並べる
    /// </summary>
    /// <param name="deck"></param>
    public void Init(Deck deck)
    {
        deckName = deck.Name;

        List<int> serialNums = new List<int>();
        for(int i = 0; i < deck.GetCount(); i++)
        {
            serialNums.Add(deck.GetCard(i));
        }

        scrollCardArrangement.Init(serialNums,this);

        deckMaxText.text = Deck.DECKCARDMAX.ToString();//デッキ最大数を設定

        bool isCorrectDeck = SetDeck();//デッキセット
        SetDeckNum(isCorrectDeck);//枚数更新
    }

    /// <summary>
    /// シリアル番号を参照してカードセット
    /// </summary>
    /// <param name="serialNums">デッキに配置するシリアル番号</param>
    public void Init(List<int> serialNums)
    {
        scrollCardArrangement.Init(serialNums, this);
        deckMaxText.text = Deck.DECKCARDMAX.ToString();//デッキ最大数を設定

        bool isCorrectDeck = SetDeck();//デッキセット
        SetDeckNum(isCorrectDeck);//枚数更新

        SetDeck();//デッキセット
    }

    /// <summary>
    /// デッキ編集のドラッグ終了時、変化したデッキを格納する判定を行う
    /// </summary>
    /// <returns></returns>
    private bool SetDeck()
    {
        //デッキ名は最初に取得している
        Deck deck = new Deck(deckName);

        List<Card> list = scrollCardArrangement.GetCards();

        //デッキにカードを設定
        int count = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] != null)
            {
                deck.SetCard(count, list[i].serialNum);
                count++;
            }
        }

        //作成したデッキをInventoryPanelに処理してもらう
        return inventoryPanel.SetDeck(deck);
    }

    /// <summary>
    /// 適正なデッキであるかを判定する
    /// </summary>
    /// <returns></returns>
    public bool IsValidDeck()
    {
        return SetDeck();
    }

    public List<int> GetDeck()
    {
        List<Card> cardList = scrollCardArrangement.GetCards();
        List<int> cardSerials = new List<int>();
        for (int i = 0; i < cardList.Count; i++)
        {
            cardSerials.Add(cardList[i].serialNum);
        }
        return cardSerials;
    }

    /// <summary>
    /// デッキ内のカードの枚数の表示を更新する
    /// </summary>
    /// <param name="isCorrectDeck">正しいデッキであるか</param>
    private void SetDeckNum(bool isCorrectDeck)
    {
        deckNumText.text = scrollCardArrangement.GetCardsNum().ToString();
        if (!isCorrectDeck)
        {
            deckNumText.color = Color.red;//不正なデッキ枚数なら色を赤くする
        }
        else
        {
            deckNumText.color = Color.white;//正しいデッキ枚数なら白く
        }
    }

    /// <summary>
    /// numの位置にカードを置く行為
    /// </summary>
    /// <param name="card">置かれるカード</param>
    /// <param name="pos">カードが取られる場所</param>
    public void PutCard(Card card,int pos)
    {
        //カードをデータに入れる
        scrollCardArrangement.AddAndAlignCard(card, pos);//データセット、整列
        scrollCardArrangement.CardsCompleteMove();//位置リセット
        scrollCardArrangement.SetZoneNum();//Zone数調整

        scrollCardArrangement.CardsBackToBasePos();//位置調整
        scrollCardArrangement.CardsCompleteMove();//位置リセット

        scrollCardArrangement.ScrollToCard(card);//挿入したCardの位置に移動する

        //UI表示系
        bool isCorrectDeck = SetDeck();//デッキセット
        SetDeckNum(isCorrectDeck);//枚数更新
    }

    /// <summary>
    /// numの位置からカードを動かす行為
    /// リスト中のCardを削除する。整列は、まだ重なっているのでしない
    /// </summary>
    /// <param name="pos">カードが取られる場所</param>
    public void PullCard(int pos)
    {
        scrollCardArrangement.RemoveCardFromList(pos);//データ調整

        //倉庫画面なら、倉庫画面の開き方を適切にする
        if (inventoryPanel.GetCaller().VillageManager != null)
        {
            inventoryPanel.ChangeStashMode(false);//アイテムモードにする
        }
    }

    /// <summary>
    /// posの位置のCardZoneにドラッグ中のカードが重なった時の動作
    /// 重ならない状態になった時はpos=-1(カードを順に整列させる動作)
    /// </summary>
    /// <param name="pos">カードが重なっている場所重なっていないときはpos = -1</param>
    public void EnterCard(int pos)
    {
        if(pos == -1)
        {//カードが離れた場合の動作のみ行う
            //カードが重なった場合は何もしない
            scrollCardArrangement.CardsCompleteMove();//移動状況リセット
            scrollCardArrangement.AlignAndSpaceCards(pos);//カード配列調整
            scrollCardArrangement.CardsBackToBasePos();//位置調整
        }
    }

    /// <summary>
    /// CardZoneからドラッグ中のカードが離れた時のドラッグ終了時動作
    /// </summary>
    public void ExitCard()
    {
        scrollCardArrangement.AlignAndSpaceCards(-1);//カード整列
        scrollCardArrangement.CardsCompleteMove();
        scrollCardArrangement.SetZoneNum();//配置場所数調整

        bool isCorrectDeck = SetDeck();//デッキセット
        SetDeckNum(isCorrectDeck);//枚数更新
    }

    public GameObject GetScrollViewObject()
    {
        return scrollCardArrangement.GetScrollViewObject();
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
