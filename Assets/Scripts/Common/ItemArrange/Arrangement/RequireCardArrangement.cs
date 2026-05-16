using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StorageData;

/// <summary>
/// あるカードを1種類のみ、適切なスタック数で置くことのできる配置場所
/// 2つ以上の配置場所を設定するときはRequireCardArrangementを複数配置する
/// </summary>
public class RequireCardArrangement : MonoBehaviour
{
    [SerializeField] private GameObject requireCardFramePrefab;//カード要求を配置する場所
    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private Transform cardsParentTransform;//StackCardFrameの配置場所
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private RectTransform scrollViewTransform;

    //ScrollViewのコライダーがあり、当たり判定のある部分のオブジェクト
    [SerializeField] private GameObject scrollViewObject;

    //この配置場所が要求しているカード
    //加えて、配置しているカード
    //背後に半透明なCardを生成して配置する
    private List<CardStackAndRequiredFrame> requiredCards = new List<CardStackAndRequiredFrame>();

    //インターフェースなのでInitで取得
    private ICardHolder cardHolder;

    [SerializeField] private CardArrangeManager cardArrangeManager;

    //ある一つのカード、に対応するためのカードデータ(システム上のワイルドカード)
    [SerializeField] private CardDataSO anyoneCardData;

    /// <summary>
    /// 記録されているカードの
    /// 配置されているStackCardFrame,
    /// その数、をひとまとめにしたクラス
    /// </summary>
    private class CardStackAndRequiredFrame
    {
        public RequireCardFlame RequiredCardFrame;
        public Card RealCard;
        public Card VirtualCard;
        public int VirtualStack;
        public int RealStack;
    }

    /// <summary>
    /// カードを追加したときの結果を示す
    /// </summary>
    private enum AddCardResultEnum
    {
        AddedNewCard,//新しいカードを配置した
        IncreasedStack,//既にあるカードのスタックを増やした
        FailedToAdd,//配置できなかった
    }

    private class AddCardResult
    {
        public AddCardResultEnum Result;
        public Card addedCard;

        public AddCardResult(AddCardResultEnum result, Card card = null)
        {
            Result = result;
            addedCard = card;
        }
    }

    /// <summary>
    /// 要求するカードの情報をもとに初期化。最初はカードを配置しない
    /// </summary>
    /// <param name="requiredCards">要求しているカード</param>
    /// <param name="cardHolder">カードを保持する,要求するHolder</param>
    /// <param name="cardArrangeManagerIn">Plefabになっているなどで、紐づける必要があるなら、必要</param>
    public void Init(List<StorageData.CardStack> requiredCards, ICardHolder cardHolder, CardArrangeManager cardArrangeManagerIn = null)
    {
        this.cardHolder = cardHolder;

        //必要なら初期化
        if (cardArrangeManagerIn is not null)
        {
            this.cardArrangeManager = cardArrangeManagerIn;
        }

        SetCardInstances(requiredCards, true);

        //スクロールを最上にする
        scrollRect.verticalNormalizedPosition = 1;
    }

    /// <summary>
    /// カード実体の情報を生成
    /// </summary>
    /// <param name="cardStacks">生成するカード情報</param>
    /// <param name="isVirtual">仮想のカードであるか。Trueの場合半透明になり、接触負荷。</param>
    private void SetCardInstances(List<StorageData.CardStack> cardStacks,bool isVirtual)
    {
        if (isVirtual)
        {//一旦、仮想のもののみプログラムを作成
            if (requiredCards is not null && requiredCards.Count > 0)
            {//既に初期化されている場合は、リストをクリア
                foreach (CardStackAndRequiredFrame cardStackAndRequiredFrame in requiredCards)
                {
                    if (cardStackAndRequiredFrame.VirtualCard != null)
                    {
                        Destroy(cardStackAndRequiredFrame.VirtualCard.gameObject);//CardのGameObjectを破壊
                    }
                    if (cardStackAndRequiredFrame.RealCard != null)
                    {
                        Destroy(cardStackAndRequiredFrame.RealCard.gameObject);//CardのGameObjectを破壊
                    }
                    if (cardStackAndRequiredFrame.RequiredCardFrame != null)
                    {
                        Destroy(cardStackAndRequiredFrame.RequiredCardFrame.gameObject);//StackCardFrameのGameObjectを破壊
                    }
                }
                requiredCards.Clear();
            }

            //カードのスタックデータを初期化,インスタンス化
            this.requiredCards = new List<CardStackAndRequiredFrame>();
            int count = 0;
            foreach (StorageData.CardStack cardStack in cardStacks)
            {
                RequireCardFlame frame = InstantiateRequireCardFrame(count);
                //移動不可な初期化
                Card card = InstantiateCard(PlayerCardData.GetCardDataFromSerialNum(cardStack.cardSerialNum),
                    frame.GetVirtualCardZone(), count,false);
                
                CardStackAndRequiredFrame cardStackAndFrame = new CardStackAndRequiredFrame
                {
                    RequiredCardFrame = frame,
                    VirtualCard = card,
                    VirtualStack = cardStack.Stack,
                    RealStack = 0,
                };
                frame.SetVirtualStack(cardStack.Stack);//個数をUIに反映
                frame.SetRealStack(0);//配置されているのは0個
                requiredCards.Add(cardStackAndFrame);

                count++;
            }

            //サイズを認識させる
            contentSizeFitter.SetLayoutVertical();
            //スクロール位置を上に
            scrollRect.verticalNormalizedPosition = 0;

            //余白は設けないため、必要ない
            //SetZoneNum();//Zoneの数を調整

            SetCardToZone();//対応関係セット

            //カードを対応した位置に移動
            CardsBackToBasePos();
        }
        else
        {
            throw new System.NotImplementedException();
        }
    }

    /// <summary>
    /// cardの情報を読み取り、シリアル番号に応じてスタックさせる。
    /// もとのcardは破壊し、スタックされるのは別オブジェクト
    /// </summary>
    /// <param name="card">追加するCard</param>
    /// <returns>スタックされた先のCard</returns>
    public Card AddAndAlignCard(Card card)
    {
        //発生するパターンは2つ
        //1.同じカードが既に存在する.かつ、表示されている->スタック数を増やす
        //3.同じカードが存在しない.かつ、表示されるべき->新しくインスタンス化して追加

        //CardStackAndFrame cardStackAndFrame = SearchCardStackAndFrame(card.serialNum);

        //データに追加し、その返り値として、追加処理の結果を確認
        AddCardResult result = AddRealToRequiredCards(card);

        Card targetCard = null;//Cardがどの場所に追加されたかを返す必要がある

        switch (result.Result)
        {
            case AddCardResultEnum.AddedNewCard:
                //新しいカードが追加された
                targetCard = result.addedCard;
                break;
            case AddCardResultEnum.IncreasedStack:
                //既にあるカードのスタックが増えた
                targetCard = result.addedCard;
                break;
            case AddCardResultEnum.FailedToAdd:
                //追加に失敗した
                targetCard = null;
                break;
        }

        return targetCard;
    }

    /// <summary>
    /// 配置処理の前に使用。
    /// そのカードが配置可能かを判定する
    /// </summary>
    /// <param name="card">配置したいCard</param>
    /// <returns>配置可能ならTrue</returns>
    public bool IsPuttable(Card card)
    {
        int serialNum = card.serialNum;

        int emptyZone = 0;

        foreach (CardStackAndRequiredFrame cardData in requiredCards)
        {//既にデータが存在するか確認

            if (cardData.RealCard is not null && cardData.RealCard.serialNum == serialNum)
            {
                //既に存在するスタックを増加
                return true;
            }
            else if (cardData.RealCard is null)
            {//配置可能な場所である
                //空いている枠を記録
                emptyZone++;
            }
        }
        //元のデータが存在しない.しかし枠がある以上に追加することは出来ない。
        if (emptyZone == 0) return false;

        int count = 0;
        //もう一回り。配置可能な空きスペースを探す
        foreach (CardStackAndRequiredFrame cardData in requiredCards)
        {
            if (cardData.RealCard is null)
            {//配置可能な場所である
                if (cardData.VirtualCard.serialNum == serialNum ||
                    cardData.VirtualCard.serialNum == anyoneCardData.serialNum)
                {//シリアル番号が一致するか、ワイルドカードであるなら、ここに配置可能
                    //新しいカードを配置可能
                    return true;
                }
            }
            count++;
        }

        //どこにも配置できなかった
        return false;
    }

    /// <summary>
    /// requiredCardsに、Cardのカードがあるかを調べ、
    /// あるならスタックを増やし、
    /// 存在せず配置可能なら配置する
    /// 配置の成否はCardArrangeManager側で判断しているため、実際にはFailedにはならないはず
    /// </summary>
    /// <param name="card">配置を試みるカード</param>
    /// <returns>どのように配置処理が行われたかの結果</returns>
    private AddCardResult AddRealToRequiredCards(Card card)
    {
        int serialNum = card.serialNum;

        int emptyZone = 0;

        foreach (CardStackAndRequiredFrame cardData in requiredCards)
        {//既にデータが存在するか確認

            if (cardData.RealCard is not null && cardData.RealCard.serialNum == serialNum)
            {
                //既に存在するスタックを増加
                cardData.RealStack++;
                cardData.RequiredCardFrame.SetRealStack(cardData.RealStack);

                Destroy(card.gameObject);//運ばれてきたCardを破壊
                return new AddCardResult(AddCardResultEnum.IncreasedStack, cardData.RealCard);
            }
            else if (cardData.RealCard is null)
            {//配置可能な場所である
                //空いている枠を記録
                emptyZone++;
            }
        }
        //元のデータが存在しない.しかし枠がある以上に追加することは出来ない。
        if (emptyZone == 0) return new AddCardResult(AddCardResultEnum.FailedToAdd);

        int count = 0;
        //もう一回り。配置可能な空きスペースを探す
        foreach (CardStackAndRequiredFrame cardData in requiredCards)
        {
            if (cardData.RealCard is null)
            {//配置可能な場所である
                if(cardData.VirtualCard.serialNum == serialNum||
                    cardData.VirtualCard.serialNum== anyoneCardData.serialNum)
                {//シリアル番号が一致するか、ワイルドカードであるなら、ここに配置可能
                    //新しいカードを配置

                    //移動不可な初期化
                    Card newCard = InstantiateCard(PlayerCardData.GetCardDataFromSerialNum(card.serialNum),
                    cardData.RequiredCardFrame.GetRealCardZone(), count,true); 

                    cardData.RealCard = newCard;
                    cardData.RealStack = 1;
                    cardData.RequiredCardFrame.SetRealStack(1);

                    Destroy(card.gameObject);//運ばれてきたCardを破壊

                    return new AddCardResult(AddCardResultEnum.AddedNewCard, cardData.RealCard);
                }
            }
            count++;
        }

        //どこにも配置できなかった
        return new AddCardResult(AddCardResultEnum.FailedToAdd);
    }

    /// <summary>
    /// Cardのオブジェクトの位置をCardZoneに合わせる
    /// 即時完了
    /// </summary>
    public void CardsBackToBasePos()
    {
        //正しい位置に移動させる
        foreach (CardStackAndRequiredFrame cardStackAndFrame in requiredCards)
        {
            if (cardStackAndFrame.RealCard is not null)
            {
                cardStackAndFrame.RealCard.BackToBasePos();
                cardStackAndFrame.RealCard.CompleteDOTween();
            }
            if (cardStackAndFrame.VirtualCard is not null)
            {
                cardStackAndFrame.VirtualCard.BackToBasePos();
                cardStackAndFrame.VirtualCard.CompleteDOTween();
            }
        }
    }

    /// <summary>
    /// Cardのオブジェクトの移動を終了する
    /// </summary>
    public void CardsCompleteMove()
    {
        foreach (CardStackAndRequiredFrame zoneCard in requiredCards)
        {
            if (zoneCard.RealCard != null)
            {
                zoneCard.RealCard.CompleteDOTween();
            }
            if (zoneCard.VirtualCard != null)
            {
                zoneCard.VirtualCard.CompleteDOTween();
            }
        }
    }

    /// <summary>
    /// ScrollRectの表示位置を現在のカードに合わせる
    /// </summary>
    /// <param name="card">合わせるカード</param>
    public void ScrollToCard(Card card)
    {
        if (card is null) return;//nullなら何もしない

        //PlaceCardZoneの親オブジェクト(RectTransform)を取得.これの位置に合わせる
        RectTransform targetRect = (RectTransform)(((PlaceCardZone)card.CurrentZone).gameObject.transform).parent;
        var contentHeight = scrollRect.content.rect.height;//アイテム全体の長さ
        var contentY = scrollRect.content.rect.position.y;//要素全体の位置
        var viewportHeight = scrollRect.viewport.rect.height;//見えている範囲の長さ
        // スクロール不要
        if (contentHeight < viewportHeight) return;

        //目的の座標の計算
        var targetPosY = (targetRect.localPosition.y * (-1) + targetRect.rect.y) + targetRect.rect.height * 0.5f;
        var gap = viewportHeight * 0.5f; // 上端?下端あわせのための調整量

        //全体の中での割合を計算
        var normalizedPos = (targetPosY - gap) / (contentHeight - viewportHeight);
        normalizedPos = Mathf.Clamp01(normalizedPos);

        //適用
        //逆転すると正しくなる
        scrollRect.verticalNormalizedPosition = 1 - normalizedPos;
    }

    /// <summary>
    /// Cardのインスタンス化
    /// </summary>
    /// <param name="cardData">Cardの情報</param>
    /// <param name="cardZone">Cardが配置されるcardZone</param>
    /// <param name="pos">Cardの配置される位置</param>
    /// <param name="isMoveable">移動可能なカードを生成するか</param>
    /// <returns>生成したCard</returns>
    private Card InstantiateCard(CardDataSO cardData, PlaceCardZone cardZone, int pos,bool isMoveable)
    {
        //Cardをインスタンス化
        GameObject cardObj = Instantiate(cardPrefab, cardZone.gameObject.transform.position, Quaternion.identity, cardZone.gameObject.transform);
        Card card = cardObj.GetComponent<Card>();
        if(isMoveable)
        {
            card.Init(cardData, cardZone, cardArrangeManager, pos);
        }
        else
        {
            //移動不可な初期化
            card.Init(cardData);
        }
        return card;
    }

    /// <summary>
    /// ZoneCardSetのリスト、cardListのCardとZoneの対応関係をセットする
    /// </summary>
    private void SetCardToZone()
    {

        foreach (CardStackAndRequiredFrame zoneCardStack in requiredCards)
        {
            if (zoneCardStack.VirtualCard != null)
            {
                //Card.CurrentZoneの設定
                zoneCardStack.VirtualCard.SetCardZone(zoneCardStack.RequiredCardFrame.GetVirtualCardZone());
            }
            if (zoneCardStack.RealCard != null)
            {
                //Card.CurrentZoneの設定
                zoneCardStack.RealCard.SetCardZone(zoneCardStack.RequiredCardFrame.GetRealCardZone());
            }
        }
    }

    /// <summary>
    /// RequireCardFrameを生成.countはその位置
    /// </summary>
    /// <param name="count">生成される位置</param>
    /// <returns>生成されたRequireCardFrame</returns>
    public RequireCardFlame InstantiateRequireCardFrame(int count)
    {
        GameObject frameObject = Instantiate(requireCardFramePrefab, cardsParentTransform.position, Quaternion.identity, cardsParentTransform);
        RequireCardFlame requireCardFrame = frameObject.GetComponent<RequireCardFlame>();
        requireCardFrame.GetRealCardZone().Init(cardHolder, count);
        requireCardFrame.GetVirtualCardZone().Init(cardHolder, count);
        return requireCardFrame;
    }

    /// <summary>
    /// posの位置にスタックされているカードから1つを取り出して、
    /// データを取得する
    /// RealCardを取り出す
    /// </summary>
    /// <param name="pos">取り出す位置</param>
    public CardDataSO GetOneCardDataFromList(int pos)
    {
        if (pos < 0 || pos >= requiredCards.Count)
        {
            Debug.LogError("Invalid position for GetOneCardDataFromList: " + pos);
            return null;
        }

        if (requiredCards[pos].RealCard == null)
        {
            Debug.LogError("No card at position: " + pos);
            return null;
        }

        if (requiredCards[pos].RealStack >= 2)
        {//取り出してもカードが残る場合
            //スタックを減らす
            requiredCards[pos].RealStack--;
            requiredCards[pos].RequiredCardFrame.SetRealStack(requiredCards[pos].RealStack);

            return requiredCards[pos].RealCard.GetBaseCardData();
        }
        else
        {//取り出したらカードがなくなる場合
            //データから取り出す
            requiredCards[pos].RealStack=0;
            CardDataSO cardData = requiredCards[pos].RealCard.GetBaseCardData();
            Destroy(requiredCards[pos].RealCard.gameObject);//CardのGameObjectを破壊
            requiredCards[pos].RealCard = null;//Cardを空白に
            requiredCards[pos].RequiredCardFrame.SetRealStack(0);


            return cardData;
        }
    }

    public PlaceCardZone GetPlaceCardZone(int pos)
    {
        if (pos < 0 || pos >= requiredCards.Count)
        {
            return null;
        }
        return requiredCards[pos].RequiredCardFrame.GetRealCardZone();
    }

    /// <summary>
    /// ScrollViewのGameObjectを返す(当たり判定取得用)
    /// </summary>
    /// <returns>ScrollViewのGameObject</returns>
    public GameObject GetScrollViewObject()
    {
        return scrollViewObject;
    }

    /// <summary>
    /// データとして記録されているカードの一覧を返す
    /// 実際に配置されているもののみ
    /// </summary>
    /// <returns></returns>
    public List<StorageData.CardStack> GetRealCards()
    {
        List<StorageData.CardStack> cardStacks = new List<StorageData.CardStack>();

        foreach(CardStackAndRequiredFrame card in requiredCards)
        {
            if (card.RealCard is not null)
            {
                StorageData.CardStack cardStack = new StorageData.CardStack
                {
                    cardSerialNum = card.RealCard.serialNum,
                    Stack = card.RealStack,
                };
                cardStacks.Add(cardStack);
            }
        }

        return cardStacks;
    }

    /// <summary>
    /// 実際に配置されているカードのインスタンスのリストを返す
    /// </summary>
    /// <returns>実際に配置されているカードのインスタンスのリスト</returns>
    public List<Card> GetCardsInstances()
    {
        List<Card> cards = new List<Card>();

        foreach (CardStackAndRequiredFrame cardStackAndRequiredFrame in requiredCards)
        {
            if (cardStackAndRequiredFrame.RealCard is not null)
            {
                cards.Add(cardStackAndRequiredFrame.RealCard);
            }
        }

        return cards;
    }

    /// <summary>
    /// カードの要求が満たされているならTrueを返す
    /// </summary>
    /// <returns>カードの要求が満たされているならTrueを返す</returns>
    public bool IsRequireCompleted()
    {
        foreach (CardStackAndRequiredFrame cardStackAndRequiredFrame in requiredCards)
        {
            if (cardStackAndRequiredFrame.RealStack != cardStackAndRequiredFrame.VirtualStack)
            {
                //配置されているスタック数が、要求されているスタック数と異なる場合は要求が満たされていない
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 配置されているカードをすべて破壊する
    /// </summary>
    public void DeleteRealCards()
    {
        foreach (CardStackAndRequiredFrame cardStackAndRequiredFrame in requiredCards)
        {
            if (cardStackAndRequiredFrame.RealCard is not null)
            {
                Destroy(cardStackAndRequiredFrame.RealCard.gameObject);
                cardStackAndRequiredFrame.RealCard = null;
                cardStackAndRequiredFrame.RealStack = 0;
                cardStackAndRequiredFrame.RequiredCardFrame.SetRealStack(0);
            }
        }
    }
}
