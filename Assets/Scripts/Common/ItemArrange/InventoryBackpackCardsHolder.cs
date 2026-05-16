using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;


public class InventoryBackpackCardsHolder : MonoBehaviour, ICardHolder
{
    public ICardHolder.CardHolderType HolderType { get { return ICardHolder.CardHolderType.ScrollHolder; } }


    //CardはCardArrangeManagerが操作する
    [SerializeField] private CardArrangeManager cardArrangeManager;
    [SerializeField] private InventoryPanel inventoryPanel;

    //スクロールするカード保存
    [SerializeField] private ScrollCardArrangement scrollCardArrangement;

    [SerializeField] private TextMeshProUGUI cardMaxText;
    [SerializeField] private TextMeshProUGUI cardNumText;

    /// <summary>
    /// 鞄に入っているカードで初期化
    /// </summary>
    /// <param name="cardSerials">鞄に入っているカード</param>
    public void Init(List<int> cardSerials)
    {
        scrollCardArrangement.Init(cardSerials, this);
        cardMaxText.text = ItemArrangeConstants.BackpackCardMax.ToString();//TEST
        bool isCorrectCard = SetBackpackCards();//データを記録。(アイテム削除でキャンセルされた時に戻る処理でも使用するため)

        SetCardNum(isCorrectCard);
    }

    private bool SetBackpackCards()
    {
        List<Card> cardList = scrollCardArrangement.GetCards();
        List<int> cardSerials = new List<int>();

        for(int i = 0; i < cardList.Count; i++)
        {
            cardSerials.Add(cardList[i].serialNum);
        }

        return inventoryPanel.SetBackpackCards(cardSerials);
    }

    /// <summary>
    /// 適正な鞄内カードであるかを判定する
    /// </summary>
    /// <returns></returns>
    public bool IsValidCards()
    {
        return SetBackpackCards();
    }

    public List<int> GetBackpackCards()
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
    /// カードの枚数の表示を更新する
    /// </summary>
    /// <param name="isCorrectCard">正しいカード枚数か</param>
    private void SetCardNum(bool isCorrectCard)
    {
        int num = scrollCardArrangement.GetCardsNum();
        cardNumText.text = num.ToString();
        if (!isCorrectCard)
        {
            cardNumText.color = Color.red;//不正なカード枚数なら色を赤くする
        }
        else
        {
            cardNumText.color = Color.white;//正しいカード枚数なら白く
        }
    }

    /// <summary>
    /// numの位置にカードを置く行為
    /// </summary>
    /// <param name="card">置かれるカード</param>
    /// <param name="pos">カードが取られる場所</param>
    public void PutCard(Card card, int pos)
    {
        //カードをデータに入れる
        scrollCardArrangement.AddAndAlignCard(card, pos);//データセット、整列
        scrollCardArrangement.CardsCompleteMove();
        scrollCardArrangement.SetZoneNum();//Zone数調整
        scrollCardArrangement.CardsBackToBasePos();//位置調整
        scrollCardArrangement.CardsCompleteMove();//位置リセット

        scrollCardArrangement.ScrollToCard(card);//挿入したCardの位置に移動する

        bool isCorrectCard = SetBackpackCards();//鞄セット
        SetCardNum(isCorrectCard);
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
    /// 現在、自動ソートされるため、新しいときに重なった時の動作は行わない
    /// </summary>
    /// <param name="pos">カードが重なっている場所重なっていないときはpos = -1</param>
    public void EnterCard(int pos)
    {
        if (pos == -1)
        {
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

        bool isCorrectCard = SetBackpackCards();//鞄セット
        SetCardNum(isCorrectCard);
    }
    public GameObject GetScrollViewObject()
    {
        return scrollCardArrangement.GetScrollViewObject();
    }

    public List<Card> GetCardsInstances()
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

        foreach(var card in cards)
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
