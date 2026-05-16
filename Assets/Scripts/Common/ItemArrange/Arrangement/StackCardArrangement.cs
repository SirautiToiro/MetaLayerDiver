using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class StackCardArrangement : MonoBehaviour
{
    [SerializeField] private int showCardMin;//この数までPlaceCardZoneは配置する
    //ShowCardMargin + Deck.DECKCARDMAX以上はPlaceCardZoneを配置しない
    [SerializeField] private int showCardMargin;//実際のデッキ内のカード数にこの数を足しただけPlaceCardZoneを配置

    [SerializeField] private GameObject stackCardFramePrefab;
    [SerializeField] private GameObject cardPrefab;

    [SerializeField] private Transform cardsParentTransform;//StackCardFrameの配置場所
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private ContentSizeFitter contentSizeFitter;
    [SerializeField] private RectTransform scrollViewTransform;

    [SerializeField] private CardArrangeManager cardArrangeManager;

    //ScrollViewのコライダーがあり、当たり判定のある部分のオブジェクト
    [SerializeField] private GameObject scrollViewObject;

    //インターフェースなのでInitで取得
    private ICardHolder cardHolder;

    //記録されているカードのデータ.表示されるべきもののみを絞り込んだものが、cardListに入る
    private List<StorageData.CardStack> cardStacksData;

    //後の方にはカード配置用のnullが入る、カードとその配置場所、そのスタック数のリスト
    private List<CardStackAndFrame> cardList = new List<CardStackAndFrame>();

    private NarrowDownCardsWindow.CardsNarrowDown narrowDown = null;

    //絞り込みに使用
    private List<int> buyTypeCardsList;
    private List<int> specialTypeCardsList;


    /// <summary>
    /// 記録されているカードの
    /// 配置されているStackCardFrame,
    /// その数、をひとまとめにしたクラス
    /// </summary>
    private class CardStackAndFrame
    {
        public StackCardFrame StackCardFrame;
        public Card Card;
        public int Stack;
    }

    /// <summary>
    /// カードの情報をもとに初期化
    /// </summary>
    /// <param name="cardStacks">データとして格納するカード情報</param>
    /// <param name="cardHolder">カードを保持するHolder</param>
    /// <param name="cardArrangeManager">Plefabになっているなどで、紐づける必要があるなら、必要</param>
    public void Init(List<StorageData.CardStack> cardStacks,ICardHolder cardHolder,CardArrangeManager cardArrangeManagerIn = null)
    {
        this.cardHolder = cardHolder;

        //必要なら初期化
        if(cardArrangeManagerIn is not null)
        {
            this.cardArrangeManager = cardArrangeManagerIn;
        }

        cardStacksData = new List<StorageData.CardStack>(cardStacks);

        //絞り込みの初期化(既にある場合は実行しない)
        if(narrowDown is null) narrowDown = new NarrowDownCardsWindow.CardsNarrowDown
        {
            Attributes = new List<AttributeDefine.Attribute>(),
            Tiers = new List<TierDefine.Tier>(),
            Costs = new List<int>(),
            EffectTargets = new List<TargetDefine.EffectTarget>(),
            ItemGetTypes = new List<ItemGetTypeDefine.ItemGetType>()
        };

        buyTypeCardsList = PlayerCardData.GetAllSupplyCards().Select(item => item.serialNum).ToList();
        specialTypeCardsList = PlayerCardData.GetAllSpecialOnlyCards().Select(item => item.serialNum).ToList();

        //カード実態を生成と絞り込みの適用
        //SetCardInstances(cardStacksData);
        Refresh();

        //スクロールを最上にする
        scrollRect.verticalNormalizedPosition = 1;
    }

    /// <summary>
    /// データをもとに、再度カードを表示する
    /// 絞り込みなどの情報を適用する
    /// </summary>
    public void Refresh()
    {
        List<StorageData.CardStack> actualData = cardStacksData.Where(
            //カードリストの中のデータそれぞれについて、絞り込み条件に合致するものだけを残す
            item =>
            {return ApplyNarrowDown(item.cardSerialNum);
            }).ToList();

        //カード実態を生成
        SetCardInstances(actualData);
    }

    /// <summary>
    /// serialNumのカードが、絞り込み条件に合致するかを調べる
    /// </summary>
    /// <param name="serialNum"></param>
    /// <returns>trueなら絞り込み後に表示する</returns>
    public bool ApplyNarrowDown(int serialNum)
    {
        CardDataSO data = PlayerCardData.GetCardDataFromSerialNum(serialNum);
        return (data.attributeList.Where(
                //あるカードの属性が絞り込みの属性に含まれている、もしくは絞り込みの属性が空なものを抽出
                attr => narrowDown.Attributes.Contains(attr.attribute)).Any() || narrowDown.Attributes.Count == 0
                ) &&
                //カードのタイプが絞り込みのタイプに含まれている、もしくは絞り込みのタイプが空なものを抽出
                (narrowDown.Tiers.Contains(data.tier.tier) || narrowDown.Tiers.Count == 0
                ) && IsTargetCost(data.cost, narrowDown.Costs) &&
                //カードの効果対象が絞り込みの効果対象に含まれている、もしくは絞り込みの効果対象が空なものを抽出
                (narrowDown.EffectTargets.Contains(data.targetDefine.effectTarget) || narrowDown.EffectTargets.Count == 0
                ) && IsCorrectGetType(data.serialNum, narrowDown.ItemGetTypes, buyTypeCardsList, specialTypeCardsList);

    }

    /// <summary>
    /// targetCostがcostTagListの定義に該当するかを調べる
    /// costTagListの要素に0,1,2が入っているならそのコストは対象。
    /// 3が入っているなら3以上は対象
    /// </summary>
    /// <param name="targetCost">調べるコスト</param>
    /// <param name="costTagList">定義のリスト</param>
    /// <returns>該当するならtrue</returns>
    private bool IsTargetCost(int targetCost,List<int> costTagList)
    {
        if(costTagList.Count == 0)
        {//絞り込みがない場合は全て対象
            return true;
        }

        foreach (int costTag in costTagList)
        {
            if (costTag >= 0 && costTag <= 2)
            {//0から2まではそのままのコスト
                if (targetCost == costTag)
                {
                    return true;
                }
            }
            if(costTag == 3)
            {//3は3以上
                if (targetCost >= 3)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// targetSerialNumのカードが、getTypeListに含まれる取得方法であるかを調べる
    /// </summary>
    /// <param name="targetSerialNum">検索するカード</param>
    /// <param name="getTypeList">このタイプに合った入手方法ならTrue</param>
    /// <param name="buyTypeCardsList">購入による入手のカードのリスト</param>
    /// <param name="specialTypeCardsList">特殊な入手のカードのリスト</param>
    /// <returns></returns>
    private bool IsCorrectGetType(int targetSerialNum, List<ItemGetTypeDefine.ItemGetType> getTypeList,
        List<int> buyTypeCardsList,List<int> specialTypeCardsList)
    {
        if (getTypeList.Count == 0)
        {//絞り込みがない場合は全て対象
            return true;
        }

        ItemGetTypeDefine.ItemGetType targetType = ItemGetTypeDefine.ItemGetType.Battle;
        if(buyTypeCardsList.Contains(targetSerialNum))
        {//ショップ購入のリストに含まれているなら入手タイプはショップ
            targetType = ItemGetTypeDefine.ItemGetType.Shop;
        }
        else if (specialTypeCardsList.Contains(targetSerialNum))
        {//特殊な入手のリストに含まれているなら入手タイプは特殊
            targetType = ItemGetTypeDefine.ItemGetType.Other;
        }

        if (getTypeList.Contains(targetType))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// カード実体の情報を生成
    /// </summary>
    /// <param name="cardStacks">生成するカード情報</param>
    private void SetCardInstances(List<StorageData.CardStack> cardStacks)
    {
        if (cardList is not null && cardList.Count > 0)
        {//既に初期化されている場合は、リストをクリア
            foreach (CardStackAndFrame cardStackAndFrame in cardList)
            {
                if (cardStackAndFrame.Card != null)
                {
                    Destroy(cardStackAndFrame.Card.gameObject);//CardのGameObjectを破壊
                }
                if (cardStackAndFrame.StackCardFrame != null)
                {
                    Destroy(cardStackAndFrame.StackCardFrame.gameObject);//StackCardFrameのGameObjectを破壊
                }
            }
            cardList.Clear();
        }

        //カードのスタックデータを初期化,インスタンス化
        this.cardList = new List<CardStackAndFrame>();
        int count = 0;
        foreach (StorageData.CardStack cardStack in cardStacks)
        {
            StackCardFrame frame = InstantiateStackCardFrame(count);
            Card card = InstantiateCard(PlayerCardData.GetCardDataFromSerialNum(cardStack.cardSerialNum),
                frame.GetPlaceCardZone(), count);

            CardStackAndFrame cardStackAndFrame = new CardStackAndFrame
            {
                StackCardFrame = frame,
                Card = card,
                Stack = cardStack.Stack
            };
            frame.SetStack(cardStack.Stack);//個数をUIに反映
            cardList.Add(cardStackAndFrame);

            count++;
        }

        //サイズを認識させる
        contentSizeFitter.SetLayoutVertical();
        //スクロール位置を上に
        scrollRect.verticalNormalizedPosition = 0;

        SetZoneNum();//Zoneの数を調整

        //カードを対応した位置に移動
        CardsBackToBasePos();
    }


    /// <summary>
    /// cardの情報を読み取り、シリアル番号に応じてスタックさせる。
    /// もとのcardは破壊し、スタックされるのは別オブジェクト
    /// </summary>
    /// <param name="card">追加するCard</param>
    /// <returns>スタックされた先のCard</returns>
    public Card AddAndAlignCard(Card card)
    {
        //発生するパターンは4つ
        //1.同じカードが既に存在する.かつ、表示されている->スタック数を増やす
        //2.同じカードが既に存在する.かつ、表示されていない->データのみスタックを増やす
        //3.同じカードが存在しない.かつ、表示されるべき->新しくインスタンス化して追加
        //4.同じカードが存在しない.かつ、表示されない->データのみ追加

        //CardStackAndFrame cardStackAndFrame = SearchCardStackAndFrame(card.serialNum);

        //データに追加し、その返り値として、既に存在するかを確認
        bool isAlreadyExist = AddToCardStackData(card.serialNum);

        Card targetCard = null;//Cardがどの場所に追加されたかを返す必要がある

        bool isDisplayedCard = ApplyNarrowDown(card.serialNum);//trueなら表示されるカード

        //表示しないカードならインスタンス追加は行わない
        if (isDisplayedCard)
        {
            if (isAlreadyExist)
            {//既に存在するカードなら、それを検索し、スタックを追加する
                CardStackAndFrame cardStackAndFrame = SearchCardStackAndFrame(card.serialNum);
                cardStackAndFrame.Stack++;
                cardStackAndFrame.StackCardFrame.SetStack(cardStackAndFrame.Stack);

                targetCard = cardStackAndFrame.Card;
            }
            else
            {//存在しないカードなら、新しくインスタンス化して追加
                StackCardFrame newFrame = InstantiateStackCardFrame(cardList.Count);
                Card newCard = InstantiateCard(PlayerCardData.GetCardDataFromSerialNum(card.serialNum),
                    newFrame.GetPlaceCardZone(), cardList.Count);
                CardStackAndFrame newCardStackAndFrame = new CardStackAndFrame
                {
                    StackCardFrame = newFrame,
                    Card = newCard,
                    Stack = 1//1個ずつ移動するので、1つ追加
                };
                newFrame.SetStack(1);//個数をUIに反映
                cardList.Add(newCardStackAndFrame);
                AlignAndSpaceCards(-1);//整列
                CardsBackToBasePos();
                targetCard = newCard;
            }
        }

        Destroy(card.gameObject);//運ばれてきたCardを破壊

        return targetCard;
    }

    /// <summary>
    /// posの位置にスタックされているカードから1つを取り出して、
    /// データを取得する
    /// </summary>
    /// <param name="pos">取り出す位置</param>
    public CardDataSO GetOneCardDataFromList(int pos)
    {
        if (pos < 0 || pos >= cardList.Count)
        {
            Debug.LogError("Invalid position for GetOneCardDataFromList: " + pos);
            return null;
        }

        if (cardList[pos].Card == null)
        {
            Debug.LogError("No card at position: " + pos);
            return null;
        }

        if (cardList[pos].Stack >= 2)
        {//取り出してもカードが残る場合
            //データから減らす
            cardStacksData.Find(item => item.cardSerialNum == cardList[pos].Card.serialNum).Stack--;

            //カードのスタック数を減らす
            cardList[pos].Stack--;
            cardList[pos].StackCardFrame.SetStack(cardList[pos].Stack);
            
            return cardList[pos].Card.GetBaseCardData();
        }
        else
        {//取り出したらカードがなくなる場合
            //データから取り出す
            cardStacksData.Remove(cardStacksData.Find(item => item.cardSerialNum == cardList[pos].Card.serialNum));

            CardDataSO cardData = cardList[pos].Card.GetBaseCardData();
            cardList[pos].Stack = 0;
            Destroy(cardList[pos].Card.gameObject);//CardのGameObjectを破壊
            cardList[pos].Card = null;//Cardを空白に

            return cardData;
        }
    }

    public PlaceCardZone GetPlaceCardZone(int pos)
    {
        if (pos < 0 || pos >= cardList.Count)
        {
            return null;
        }
        return cardList[pos].StackCardFrame.GetPlaceCardZone();
    }

    /// <summary>
    /// StackCardFrameを生成.countはその位置
    /// </summary>
    /// <param name="count">生成される位置</param>
    /// <returns>生成されたStackCardFrame</returns>
    public StackCardFrame InstantiateStackCardFrame(int count)
    {
        GameObject frameObject = Instantiate(stackCardFramePrefab, cardsParentTransform.position,Quaternion.identity, cardsParentTransform);
        StackCardFrame stackCardFrame = frameObject.GetComponent<StackCardFrame>();
        stackCardFrame.GetPlaceCardZone().Init(cardHolder, count);
        return stackCardFrame;
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
    /// Cardのオブジェクトの位置をCardZoneに合わせる
    /// 即時完了
    /// </summary>
    public void CardsBackToBasePos()
    {
        //正しい位置に移動させる
        foreach (CardStackAndFrame cardStackAndFrame in cardList)
        {
            if(cardStackAndFrame.Card is not null)
            {
                cardStackAndFrame.Card.BackToBasePos();
                cardStackAndFrame.Card.CompleteDOTween();
            }
        }
    }

    /// <summary>
    /// Cardのオブジェクトの移動を終了する
    /// </summary>
    public void CardsCompleteMove()
    {
        foreach (CardStackAndFrame zoneCard in cardList)
        {
            if (zoneCard.Card != null)
            {
                zoneCard.Card.CompleteDOTween();
            }
        }
    }

    /// <summary>
    /// Cardをposの部分だけが空いた状態で整列させる
    /// pos = -1なら空白部分はない
    /// </summary>
    /// <param name="pos">Cardの空白部分</param>
    public void AlignAndSpaceCards(int pos)
    {
        //TODO:pos = -1は使用されていないので、実装を確認しない。使用していないかを確認

        //Cardをリストに詰めていく.stack = 0なら飛ばす
        List<(Card,int)> tmpCardStack = new List<(Card, int)>();

        foreach (CardStackAndFrame cardStackAndFrame in cardList)
        {
            if (cardStackAndFrame.Stack > 0)
            {
                tmpCardStack.Add((cardStackAndFrame.Card,cardStackAndFrame.Stack));
            }
        }

        //posの部分だけ開けながらCardを再セット
        int count = 0;
        for (int i = 0; i < cardList.Count; i++)
        {
            if (i == pos)
            {//posのいちはnull
                cardList[i].Card = null;
                cardList[i].Stack = 0;//Stackも0にする
                continue;
            }
            if (count >= tmpCardStack.Count)
            {//tmpCardsの方が先になくなる
                //無くなった場合nullを詰める
                cardList[i].Card = null;
                cardList[i].Stack = 0;//Stackも0にする
            }
            else
            {
                cardList[i].Card = tmpCardStack[count].Item1;
                cardList[i].Stack = tmpCardStack[count].Item2;
                count++;
            }
        }

        SetCardToZone();//対応関係セット
        SetCardStack();//StackをUIに反映
    }

    /// <summary>
    /// ZoneCardSetのリスト、cardListのCardとZoneの対応関係をセットする
    /// </summary>
    private void SetCardToZone()
    {

        foreach (CardStackAndFrame zoneCardStack in cardList)
        {
            if (zoneCardStack.Card != null)
            {
                //Card.CurrentZoneの設定
                zoneCardStack.Card.SetCardZone(zoneCardStack.StackCardFrame.GetPlaceCardZone());
            }
        }
    }

    private void SetCardStack()
    {
        //CardのStackをUIに反映
        foreach (CardStackAndFrame cardStackAndFrame in cardList)
        {
            if (cardStackAndFrame.Card != null)
            {
                cardStackAndFrame.StackCardFrame.SetStack(cardStackAndFrame.Stack);
            }
            else
            {//nullならスタック数を表示しない
                cardStackAndFrame.StackCardFrame.SetStack(0);
            }
        }
    }

    /// <summary>
    /// StackCardFrameの数をCardの数に合わせて調整する
    /// Cardの対応などは調整されているものとする
    /// </summary>
    public void SetZoneNum()
    {
        //nullである場所を数える
        int count = 0;
        for (int i = 0; i < cardList.Count; i++)
        {
            if (cardList[i].Card == null) count++;
        }

        //何個消す必要があるかを調べ、それだけ後ろから消す
        //あるいは追加する
        int diff = count - showCardMargin;
        if (diff > 0)
        {//削除の場合
            for (; diff > 0; diff--)
            {
                DeleteCardStackAndFrame(cardList.Count - 1);
            }
        }
        else if (diff < 0)
        {//追加の場合
            for (; diff < 0; diff++)
            {
                StackCardFrame newStackCardFrame = InstantiateStackCardFrame(cardList.Count);

                //最後尾にFrame追加
                cardList.Add(new CardStackAndFrame{
                    Card = null,
                    StackCardFrame = newStackCardFrame,
                    Stack = 0
                });

                newStackCardFrame.SetStack(0);//Stackは0にしておく
            }
        }

        SetCardToZone();//対応関係セット
    }

    /// <summary>
    /// posの位置のZoneとCardをリストから削除し、オブジェクトも削除する。
    /// </summary>
    /// <param name="pos">削除する位置</param>
    public void DeleteCardStackAndFrame(int pos)
    {
        Card targetCard = cardList[pos].Card;
        StackCardFrame targetFrame = cardList[pos].StackCardFrame;

        if (targetCard != null)
        {
            Destroy(targetCard.gameObject);//GameObjectを削除
        }
        if (targetFrame != null)
        {
            Destroy(targetFrame.gameObject);
        }

        cardList.RemoveAt(pos);//リストから削除
    }

    /// <summary>
    /// ScrollRectの表示位置を現在のカードに合わせる
    /// </summary>
    /// <param name="card">合わせるカード</param>
    public void ScrollToCard(Card card)
    {
        if(card is null) return;//nullなら何もしない

        //PlaceCardZoneの親オブジェクト(RectTransform)を取得.これの位置に合わせる
        RectTransform targetRect = (RectTransform)(((PlaceCardZone)card.CurrentZone).gameObject.transform).parent;
        var contentHeight = scrollRect.content.rect.height;//アイテム全体の長さ
        var contentY = scrollRect.content.rect.position.y;//要素全体の位置
        var viewportHeight = scrollRect.viewport.rect.height;//見えている範囲の長さ
        // スクロール不要
        if (contentHeight < viewportHeight) return;

        //目的の座標の計算
        var targetPosY = (targetRect.localPosition.y*(-1) + targetRect.rect.y) + targetRect.rect.height * 0.5f;
        var gap = viewportHeight * 0.5f; // 上端?下端あわせのための調整量

        //全体の中での割合を計算
        var normalizedPos = (targetPosY-gap) / (contentHeight - viewportHeight);
        normalizedPos = Mathf.Clamp01(normalizedPos);

        //適用
        //逆転すると正しくなる
        scrollRect.verticalNormalizedPosition = 1-normalizedPos;
    }

    /// <summary>
    /// cardListからserialNumのカードを検索し、それがセットされている
    /// CardStackAndFrameを返す
    /// </summary>
    /// <param name="serialNum"></param>
    /// <returns>検索されたCardStackAndFrame.ないならnull</returns>
    private CardStackAndFrame SearchCardStackAndFrame(int serialNum)
    {
        foreach (CardStackAndFrame cardStackAndFrame in cardList)
        {
            if(cardStackAndFrame.Card is null)continue;

            if (cardStackAndFrame.Card.serialNum == serialNum)
            {
                return cardStackAndFrame;
            }
        }

        return null;
    }

    /// <summary>
    /// cardStacksDataに、serialNumのカードがあるかを調べ、
    /// あるならデータとして追加する
    /// </summary>
    /// <param name="serialNum">検索するカードのシリアル番号</param>
    /// <returns>既にデータが入っていたならtrueを返す。ないならfalse</returns>
    private bool AddToCardStackData(int serialNum)
    {
        foreach (StorageData.CardStack cardStack in cardStacksData)
        {
            if (cardStack.cardSerialNum == serialNum)
            {
                cardStack.Stack++;
                return true;
            }
        }

        //データが存在しないので新たに追加
        StorageData.CardStack newCardStack = new StorageData.CardStack
        {
            cardSerialNum = serialNum,
            Stack = 1
        };
        cardStacksData.Add(newCardStack);

        return false;
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
    /// </summary>
    /// <returns></returns>
    public List<StorageData.CardStack> GetCards()
    {
        List<StorageData.CardStack> cardStacks = new List<StorageData.CardStack>(cardStacksData);
        
        return cardStacks;
    }


    /// <summary>
    /// 表示絞り込みの設定
    /// </summary>
    /// <param name="narrowDown">絞り込みの情報</param>
    public void SetNarrowDown(NarrowDownCardsWindow.CardsNarrowDown narrowDown)
    {
        this.narrowDown = narrowDown;
        Refresh();
    }

    /// <summary>
    /// 実際に配置されているカードのインスタンスのリストを返す
    /// </summary>
    /// <returns>実際に配置されているカードのインスタンスのリスト</returns>
    public List<Card> GetCardsInstances()
    {
        List<Card> cards = new List<Card>();

        foreach (CardStackAndFrame cardStackAndFrame in cardList)
        {
            if (cardStackAndFrame.Card != null)
            {
                cards.Add(cardStackAndFrame.Card);
            }
        }

        return cards;
    }
}
