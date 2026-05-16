using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ScrollView上でカードのオブジェクトとデータを操作する
/// </summary>
public class ScrollCardArrangement : MonoBehaviour
{
    [SerializeField] private int showCardMin;//この数までPlaceCardZoneは配置する
    //ShowCardMargin + Deck.DECKCARDMAX以上はPlaceCardZoneを配置しない
    [SerializeField] private int showCardMargin;//実際のデッキ内のカード数にこの数を足しただけPlaceCardZoneを配置

    [SerializeField] private GameObject placeCardZonePrefab;
    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private Transform cardsParentTransform;
    [SerializeField] private ScrollRect deckScrollRect;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private RectTransform scrollViewTransform;

    [SerializeField] private CardArrangeManager cardArrangeManager;

    //ScrollViewのコライダーがあり、当たり判定のある部分のオブジェクト
    [SerializeField] private GameObject scrollViewObject;

    //インターフェースなのでInitで取得
    private ICardHolder cardHolder;

    private List<ZoneCardSet> zoneCards;

    /// <summary>
    /// カードとカードの配置されているCardZoneの組のクラス
    /// </summary>
    private class ZoneCardSet
    {
        public Card Card { get; set; }
        public PlaceCardZone CardZone { get; set; }

        public ZoneCardSet(Card card, PlaceCardZone cardZone)
        {
            Card = card;
            CardZone = cardZone;
        }
    }

    /// <summary>
    /// カードのシリアル番号からなるリストを受け取り、生成し、ScrollViewにセット
    /// </summary>
    /// <param name="serialNums">カードのシリアル番号のリスト</param>
    /// <param name="_cardHolder">呼び出しているCardHolder</param>
    public void Init(List<int> serialNums,ICardHolder _cardHolder)
    {
        cardHolder = _cardHolder;

        if (zoneCards != null && zoneCards.Count != 0)
        {//カード情報をリセット(オブジェクトを消す)
            for (int i = zoneCards.Count - 1; i >= 0; i--)
            {
                DeleteZoneCardSet(i);
            }
        }

        zoneCards = new List<ZoneCardSet>();//初期化

        //リストは参照型なので、元への変更を極力避けている。
        //バグ自体は起きていないが……
        List<int> tmpIntList = new List<int>();
        for(int i = 0; i < serialNums.Count; i++)
        {
            tmpIntList.Add(serialNums[i]);
        }

        //ソート
        tmpIntList.Sort((a, b) => ( a - b));

        int zoneLen = 0;
        int cardLen = tmpIntList.Count;

        //PlaceCardZoneの数を調整する
        for (; zoneLen < cardLen + showCardMargin; zoneLen++)
        {
            //必要なだけ生成し、リストに追加
            zoneCards.Add(new ZoneCardSet(null, InstantiatePlaceCardZone(zoneLen)));
        }

        //カードを生成する
        for (int i = 0; i < cardLen; i++)
        {//deckから一つずつ取り出して生成
            int serialNum = tmpIntList[i];
            CardDataSO data = PlayerCardData.GetCardDataFromSerialNum(serialNum);
            Card card = InstantiateCard(data, zoneCards[i].CardZone, i);
            //リストに格納
            zoneCards[i].Card = card;
        }

        //サイズを認識させる
        contentSizeFitter.SetLayoutHorizontal();
        //スクロール位置を左に
        deckScrollRect.horizontalNormalizedPosition = 0;
    }

    /// <summary>
    /// カードのインスタンスのリストを受け取り、ScrollViewにセット。
    /// </summary>
    /// <param name="cardList">カードのインスタンスのリスト</param>
    /// <param name="cardHolder">呼び出しているCardHolder</param>
    public void Init(List<Card> cardList,ICardHolder cardHolder)
    {
        this.cardHolder = cardHolder;

        if (zoneCards != null && zoneCards.Count != 0)
        {//カード情報をリセット(オブジェクトを消す)
            for (int i = zoneCards.Count - 1; i >= 0; i--)
            {
                DeleteZoneCardSet(i);
            }
        }

        zoneCards = new List<ZoneCardSet>();//初期化

        //リストは参照型なので、元への変更を極力避けている。
        //バグ自体は起きていないが……
        List<Card> tmpCardList = new List<Card>();
        for (int i = 0; i < cardList.Count; i++)
        {
            tmpCardList.Add(cardList[i]);
        }

        //シリアル番号順にソート
        tmpCardList.Sort((a, b) => (a.serialNum - b.serialNum));

        int zoneLen = 0;
        int cardLen = tmpCardList.Count;

        //PlaceCardZoneの数を調整する
        for (; zoneLen < cardLen + showCardMargin; zoneLen++)
        {
            //必要なだけ生成し、リストに追加
            zoneCards.Add(new ZoneCardSet(null, InstantiatePlaceCardZone(zoneLen)));
        }

        //カードを対応付ける
        for (int i = 0; i < cardLen; i++)
        {
            //リストに格納
            zoneCards[i].Card = tmpCardList[i];
        }

        SetCardToZone();//カード側の対応関係セット

        //サイズを認識させる
        contentSizeFitter.SetLayoutHorizontal();
        //スクロール位置を左に
        deckScrollRect.horizontalNormalizedPosition = 0;

        //カードを対応した位置に移動
        CardsBackToBasePos();
    }

    /// <summary>
    /// placeCardZoneのインスタンス化
    /// </summary>
    /// <param name="pos">生成するplaceCardZoneの位置</param>
    /// <returns>生成したplaceCardZone</returns>
    private PlaceCardZone InstantiatePlaceCardZone(int pos)
    {
        //PlaceCardZoneをインスタンス化
        GameObject placeCardZoneObj = Instantiate(placeCardZonePrefab, cardsParentTransform.position, Quaternion.identity, cardsParentTransform);
        PlaceCardZone placeCardZone = placeCardZoneObj.GetComponent<PlaceCardZone>();
        placeCardZone.Init(cardHolder, pos);
        return placeCardZone;
    }

    /// <summary>
    /// Cardのインスタンス化
    /// </summary>
    /// <param name="cardData">Cardの情報</param>
    /// <param name="cardZone">Cardが配置されるcardZone</param>
    /// <param name="pos">Cardの配置される位置</param>
    /// <returns>生成したCard</returns>
    private Card InstantiateCard(CardDataSO cardData, PlaceCardZone cardZone, int pos)
    {
        //Cardをインスタンス化
        GameObject cardObj = Instantiate(cardPrefab, cardZone.gameObject.transform.position, Quaternion.identity, cardZone.gameObject.transform);
        Card card = cardObj.GetComponent<Card>();
        card.Init(cardData, cardZone, cardArrangeManager, pos);
        return card;
    }

    /// <summary>
    /// posの位置のカードをzoneCards内でnullにする
    /// </summary>
    /// <param name="pos">Cardをnullにする位置</param>
    public void RemoveCardFromList(int pos)
    {
        zoneCards[pos].Card = null;
    }

    /// <summary>
    /// posの位置のZoneとCardをリストから削除し、オブジェクトも削除する。
    /// </summary>
    /// <param name="pos">削除する位置</param>
    private void DeleteZoneCardSet(int pos)
    {
        Card targetCard = zoneCards[pos].Card;
        PlaceCardZone targetZone = zoneCards[pos].CardZone;
        if (targetCard != null)
        {
            Destroy(targetCard.gameObject);//GameObjectを削除
        }
        if (targetZone != null)
        {
            Destroy(targetZone.gameObject);
            //サイズを認識させる
            //contentSizeFitter.SetLayoutHorizontal();
        }

        zoneCards.RemoveAt(pos);//リストから削除
    }

    /// <summary>
    /// Cardをposの部分だけが空いた状態で整列させる
    /// pos = -1なら空白部分はない
    /// </summary>
    /// <param name="pos">Cardの空白部分</param>
    public void AlignAndSpaceCards(int pos)
    {
        //Cardをリストに詰めていく.nullなら飛ばす
        List<Card> tmpCards = new List<Card>();
        for (int i = 0; i < zoneCards.Count; i++)
        {
            if (zoneCards[i].Card != null)
            {
                tmpCards.Add(zoneCards[i].Card);
            }
        }

        //posの部分だけ開けながらCardを再セット
        int count = 0;
        for (int i = 0; i < zoneCards.Count; i++)
        {
            if (i == pos)
            {//posのいちはnull
                zoneCards[i].Card = null;
                continue;
            }
            if (count >= tmpCards.Count)
            {//tmpCardsの方が先になくなる
                //無くなった場合nullを詰める
                zoneCards[i].Card = null;
            }
            else
            {
                zoneCards[i].Card = tmpCards[count];
                count++;
            }
        }

        SetCardToZone();//対応関係セット
    }

    /// <summary>
    /// posの位置にCardのデータを追加し、
    /// 整列する
    /// </summary>
    /// <param name="card">追加するCard</param>
    /// <param name="pos">追加する場所</param>
    public void AddAndAlignCard(Card card, int pos)
    {
        //Cardをリストに詰めていく.nullなら飛ばす
        List<Card> tmpCards = new List<Card>();
        for (int i = 0; i < zoneCards.Count; i++)
        {
            if (zoneCards[i].Card != null)
            {
                tmpCards.Add(zoneCards[i].Card);
            }
        }
        
        if(tmpCards.Count == 0)
        {//カードが一枚もない状態ならそのまま追加
            tmpCards.Add(card);
        }
        else
        {
            //入るべき場所を検索して挿入
            for (int i = 0; i < tmpCards.Count; i++)
            {
                if (card.serialNum < tmpCards[i].serialNum)
                {
                    tmpCards.Insert(i, card);
                    break;
                }

                if (i == tmpCards.Count - 1)
                {
                    tmpCards.Insert(i, card);
                    break;
                }
            }
        }

        //zoneCardsに詰める。最後はnull
        for (int i = 0; i < zoneCards.Count; i++)
        {
            if (i >= tmpCards.Count)
            {
                zoneCards[i].Card = null;
            }
            else
            {
                zoneCards[i].Card = tmpCards[i];
            }
        }

        SetCardToZone();//対応関係セット
    }

    /// <summary>
    /// PlaceCardZoneの数をCardの数に合わせて調整する
    /// Cardの対応などは調整されているものとする
    /// </summary>
    public void SetZoneNum()
    {
        //nullである場所を数える
        int count = 0;
        for(int i = 0; i < zoneCards.Count; i++)
        {
            if(zoneCards[i].Card == null)count++;
        }

        //何個消す必要があるかを調べ、それだけ後ろから消す
        //あるいは追加する
        int diff = count - showCardMargin;
        if(diff > 0)
        {
            for (; diff > 0; diff--)
            {
                DeleteZoneCardSet(zoneCards.Count - 1);
            }
        }else if(diff < 0)
        {
            for (; diff < 0; diff++)
            {
                //最後尾にZone追加
                zoneCards.Add(new ZoneCardSet(null, InstantiatePlaceCardZone(zoneCards.Count)));
            }
        }

        SetCardToZone();//対応関係セット
    }

    /// <summary>
    /// ZoneCardSetのリスト、zoneCardsのCardとZoneの対応関係をセットする
    /// </summary>
    private void SetCardToZone()
    {

        foreach (ZoneCardSet zoneCard in zoneCards)
        {
            if (zoneCard.Card != null)
            {
                //Card.CurrentZoneの設定
                zoneCard.Card.SetCardZone(zoneCard.CardZone);
            }
        }
    }

    /// <summary>
    /// Cardのオブジェクトの位置をCardZoneに合わせる
    /// </summary>
    public void CardsBackToBasePos()
    {
        //正しい位置に移動させる
        foreach (ZoneCardSet zoneCard in zoneCards)
        {
            if (zoneCard.Card != null)
            {
                zoneCard.Card.BackToBasePos();
            }
        }
    }

    /// <summary>
    /// Cardのオブジェクトの移動を終了する
    /// </summary>
    public void CardsCompleteMove()
    {
        foreach (ZoneCardSet zoneCard in zoneCards)
        {
            if (zoneCard.Card != null)
            {
                zoneCard.Card.CompleteDOTween();
            }
        }
    }

    /// <summary>
    /// リスト内のカードのリストを返す
    /// </summary>
    /// <returns>カードのリスト</returns>
    public List<Card> GetCards()
    {
        List<Card> list = new List<Card>();

        //デッキにカードを設定
        int count = 0;
        for (int i = 0; i < zoneCards.Count; i++)
        {
            if (zoneCards[i].Card != null)
            {
                list.Add(zoneCards[i].Card);
                count++;
            }
        }

        return list;
    }

    /// <summary>
    /// リスト内のカードの総数を返す
    /// </summary>
    /// <returns>カード枚数</returns>
    public int GetCardsNum()
    {
        int count = 0;

        for (int i = 0; i < zoneCards.Count; i++)
        {
            if (zoneCards[i].Card != null)
            {
                count++;
            }
        }

        return count;
    }

    public void ScrollToCard(Card card)
    {
        RectTransform targetRect = (RectTransform)((PlaceCardZone)card.CurrentZone).gameObject.transform;
        var contentWidth = deckScrollRect.content.rect.width;//アイテム全体の長さ
        var contentX = deckScrollRect.content.rect.position.x;//要素全体の位置
        var viewportWidth = deckScrollRect.viewport.rect.width;//見えている範囲の長さ
        // スクロール不要
        if (contentWidth < viewportWidth) return;

        //目的の座標の計算
        var targetPosX = (targetRect.localPosition.x + targetRect.rect.x) + targetRect.rect.width * 0.5f;
        var gap = viewportWidth * 0.5f; // 上端?下端あわせのための調整量

        //全体の中での割合を計算
        var normalizedPos = (targetPosX - gap) / (contentWidth - viewportWidth);
        normalizedPos = Mathf.Clamp01(normalizedPos);

        //適用
        deckScrollRect.horizontalNormalizedPosition = normalizedPos;
    }

    /// <summary>
    /// ScrollViewのGameObjectを返す(当たり判定取得用)
    /// </summary>
    /// <returns>ScrollViewのGameObject</returns>
    public GameObject GetScrollViewObject()
    {
        return scrollViewObject;
    }
}
