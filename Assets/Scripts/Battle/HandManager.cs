using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// 手札管理をするクラス
/// </summary>
public class HandManager : MonoBehaviour
{
    //handCardZonesとcardsの同じ場所の物は対応している
    List<HandCardZone> handCardZones;//カードの配置場所のリスト//オブジェクト
    List<Card> cards;//カードのリスト//オブジェクト
    int cardsNum;//カードの数

    [SerializeField] GameObject cardPrefab;
    [SerializeField] GameObject handCardZonePrefab;
    [SerializeField] FieldManager fieldManager;

    [SerializeField] RectTransform rectTransform;//この手札のrectTransform

    [SerializeField] DeckDataInBattle deckData;

    //バトル中の効果に影響するデータ
    [SerializeField] private DataPerCard dataPerCard;
    [SerializeField] private DataPerBattle dataPerBattle;
    [SerializeField] private DataPerTurn dataPerTurn;

    /// <summary>
    /// 初期化処理。FieldManagerによって呼び出される
    /// </summary>
    public void Init()
    {
        handCardZones = new List<HandCardZone>();
        cards = new List<Card>();
        cardsNum = 0;
    }

    #region カードドロー処理
    /****************
     * 
     * カードを引くまでの流れ
     * 
     * (デッキにカードがない場合、シャッフルする。それでもなければキャンセル)
     * 
     * CardZoneSetを引くカードの枚数分行い、カードの配置場所を決定する
     * AlignCardZonesを行い、CardZoneを整える
     * 既存のカードにBackToBasePosを行う
     * MakeCardForDrawを行い、カードを適切にインスタンス化
     * ↑cardsの要素数がcardsNumと等しくなるまで行う
     * CardにBackToBasePosを行い、ドロー演出
     * 
     ***************/

    /// <summary>
    /// デッキからnum枚のカードを引く
    /// 引けないならシャッフルする
    /// それでも引けないなら引けるだけ引く
    /// </summary>
    /// <param name="num">引くカードの枚数</param>
    /// <returns>この時引いたカードのリスト</returns>
    public List<Card> CardsDraw(int num)
    {
        int drawableNum = 0;//最終的にドロー可能な枚数

        int preCardsNum = cardsNum;//カードを引く前のカードの数

        drawableNum=deckData.GetCardNum()+deckData.GetTrashNum();//デッキと捨て札の合計

        //TODO:いらない？
        dataPerCard.DrawedCards = new List<Card>();//ドローしたカードの情報をリセット

        if (drawableNum > num)
        {
            //TODO:手札上限の処理
            drawableNum = num;//numと等しいなら問題ない。numより小さい場合はその数だけ引く
        }

        //カードゾーンを作成
        for(int i = 0; i < drawableNum; i++)
        {
            CardZoneSet();
            cardsNum++;
        }
        //カードゾーンを揃える
        AlignCardZones();

        //カードゾーンに既存のカードを揃える
        for (int i = 0; i < preCardsNum; i++)
        {
            cards[i].BackToBasePos();
        }

        //返り値のために作成したCardのリストを作成
        List<Card> createdCards = new List<Card>();

        //カードを作成
        for (int i = 0; i < drawableNum; i++)
        {
            //デッキにカードがない場合シャッフルする
            if (deckData.GetCardNum() == 0)
            {
                ShuffleCard();
                if (deckData.GetCardNum() == 0)
                {//デッキ内にシャッフルしてもカードが残っていないなら
                    break;//何もしない
                }
            }

            //上から一枚カードを得る
            int cardData = deckData.GetCard();
            CardDataSO cardDataSO = PlayerCardData.GetCardDataFromSerialNum(cardData);
            Card c = MakeCardForDraw(cardDataSO, preCardsNum+i);
            createdCards.Add(c);//作成したCardのリスト
        }

        //カードゾーンに追加のカードを揃える
        for (int i = preCardsNum; i < preCardsNum + drawableNum; i++)
        {
            cards[i].BackToBasePos();
        }

        for(int i = 0; i < createdCards.Count; i++)
        {
            dataPerCard.DrawedCards.Add(createdCards[i]);//ドローしたカードの情報を追加
        }

        return createdCards;
    }

    /// <summary>
    /// 属性を持つカードをデッキからランダムにnum枚引く
    /// </summary>
    /// <param name="num">引く枚数</param>
    /// <param name="attribute">検索する属性</param>
    /// <returns>作成したカードのリスト</returns>
    public List<Card> DrawRandomCardByAttribute(int num,AttributeDefine.Attribute attribute)
    {
        int preCardsNum = cardsNum;//カードを引く前のカードの数

        //TODO:手札上限の管理

        List<int> drawedCards = new List<int>();

        int count = 0;
        for(int i = 0; i < num; i++)
        {
            int cardSerialNum = deckData.GetRandomCardByAttribute(attribute);
            if(cardSerialNum == -1)
            {
                break;//引けるカードがない
            }
            drawedCards.Add(cardSerialNum);
            count++;
        }

        //count枚手札に加わる
        //カードゾーンを作成
        for (int i = 0; i < count; i++)
        {
            CardZoneSet();
            cardsNum++;
        }
        //カードゾーンを揃える
        AlignCardZones();

        //カードゾーンに既存のカードを揃える
        for (int i = 0; i < preCardsNum; i++)
        {
            cards[i].BackToBasePos();
        }

        //返り値のために作成したCardのリストを作成
        List<Card> createdCards = new List<Card>();

        //カードを作成
        for (int i = 0; i < count; i++)
        {
            CardDataSO cardDataSO = PlayerCardData.GetCardDataFromSerialNum(drawedCards[i]);
            Card c = MakeCardForDraw(cardDataSO, preCardsNum + i);
            createdCards.Add(c);//作成したCardのリスト
        }

        //カードゾーンに追加のカードを揃える
        for (int i = preCardsNum; i < preCardsNum + count; i++)
        {
            cards[i].BackToBasePos();
        }

        for (int i = 0; i < createdCards.Count; i++)
        {
            dataPerCard.DrawedCards.Add(createdCards[i]);//ドローしたカードの情報を追加
        }

        return createdCards;
    }

    /// <summary>
    /// カードゾーンを一つインスタンス化し、handCardZonesリストに追加
    /// カードはまだ追加されていないので、別で必ず作成すること
    /// </summary>
    private void CardZoneSet()
    {
        //HandCardZoneをインスタンス化してリストに追加
        //親を手札オブジェクトに
        GameObject handCardZoneObj = Instantiate(handCardZonePrefab, rectTransform.position, Quaternion.identity, rectTransform);
        HandCardZone handCardZone = handCardZoneObj.GetComponent<HandCardZone>();
        handCardZones.Add(handCardZone);//リストに追加
    }

    /// <summary>
    /// ドロー処理の前にカードを作成する
    /// </summary>
    /// <param name="cardSO">引くカード</param>
    /// <param name="handPos">引くカードの手札での位置</param>
    /// <returns>作成したCard</returns>

    private Card MakeCardForDraw(CardDataSO cardSO,int handPos)
    {
        //Cardをインスタンス化してリストに追加
        //親を手札オブジェクトに
        //カードの最初に置かれる位置
        //ドローの時は、HandManagerの下の方
        Vector3 zonePos=handCardZones[handPos].GetLocalPos();
        Vector3 cardStartPosRocal = zonePos+new Vector3( 0, -rectTransform.sizeDelta.y, 0);

        //ローカル座標をワールド座標に変換
        Vector3 cardStartPosWorld = rectTransform.TransformPoint(cardStartPosRocal);
        //ワールド座標変換終わり

        //Instantiateは引数でPositionを設定するとワールド座標に設置する
        GameObject cardObj = Instantiate(cardPrefab, cardStartPosWorld, Quaternion.identity, rectTransform);
        Card card = cardObj.GetComponent<Card>();
        card.Init(cardSO, handCardZones[handPos], fieldManager, handPos,this);//Card初期化,現在の手札での位置を渡す
        cards.Add(card);//リストに追加
        return card;
    }

    /// <summary>
    /// カードオブジェクトの指定した場所を削除する.リストからも削除
    /// CardZoneも削除
    /// 使用した後はcardsNumを引くこと
    /// </summary>
    private void CardKill(int handPos)
    {
        GameObject targetObj = cards[handPos].gameObject;
        cards.RemoveAt(handPos);//リストから削除
        Destroy(targetObj);//ゲームオブジェクトを削除
        targetObj = handCardZones[handPos].gameObject;
        handCardZones.RemoveAt(handPos);//リストから削除
        Destroy(targetObj);//ゲームオブジェクトを削除
    }

    /// <summary>
    /// 1つのカードを捨て札に配置し、手札からは消す
    /// </summary>
    public void TrashCard(int handPos)
    {
        deckData.TrashCard(cards[handPos]);
        CardKill(handPos);
        cardsNum--;//カード数を1引く
        SetHandPos();//カード番号再調整
        AlignCardZones();//カードゾーンを並べなおす
        for(int i = 0; i < cardsNum; i++)
        {
            cards[i].BackToBasePos();
        }
    }

    /// <summary>
    /// 手札のカード全てを捨て札に配置し、手札から消す
    /// </summary>
    public void TrashCardAll()
    {
        int beforeCardsNum = cardsNum;
        //カードを捨札に送る
        for (int i = 0; i < beforeCardsNum; i++)
        {
            deckData.TrashCard(cards[i]);
            
        }

        //カードオブジェクト破壊
        //末尾から消す
        for (int i = beforeCardsNum - 1; i >= 0; i--)
        {
            CardKill(i);
            cardsNum--;//カード数を1引く
        }
    }

    /// <summary>
    /// 手札のカードを廃棄し、バトル中は使用できないようにする
    /// </summary>
    /// <param name="handPos">廃棄するカードの場所</param>
    public void DiscardCard(int handPos)
    {
        CardKill(handPos);
        cardsNum--;//カード数を1引く
        SetHandPos();//カード番号再調整
        AlignCardZones();//カードゾーンを並べなおす
        for (int i = 0; i < cardsNum; i++)
        {
            cards[i].BackToBasePos();
        }
    }

    /// <summary>
    /// cardSOのカードを生成して手札に加える
    /// </summary>
    /// <param name="cardSO">生成するCardの情報</param>
    public void AddCardInHand(CardDataSO cardSO)
    {
        //カードゾーン生成
        CardZoneSet();
        cardsNum++;//手札枚数の更新
        //TODO:手札上限の処理
        MakeCardForDraw(cardSO, cardsNum-1);//リストの先頭にカードを生成

        //BackToBasePosは呼出元の処理でしよう
    }

    /*
    /// <summary>
    /// この場所以外のカードをすべて捨てる
    /// CardEffectExecuteからの呼び出しではタイミングの問題でうまく機能しない
    /// </summary>
    /// <param name="handPos">捨てないカードの場所</param>
    public void TrashCardExceptThis(int handPos)
    {
        int beforeCardsNum = cardsNum;
        //カードを捨札に送る
        for (int i = 0; i < beforeCardsNum; i++)
        {
            if (i == handPos) continue;//目的の位置のカードはスキップ

            deckData.TrashCard(cards[i]);
        }

        //カードオブジェクト破壊
        //末尾から削除していく
        for (int i = beforeCardsNum-1; i >=0; i--)
        {
            if (i == handPos) continue;//目的の位置のカードはスキップ

            CardKill(i);
            cardsNum--;//カード数を1引く
        }

        //cardsNumは1になっているはず

        SetHandPos();//カード番号再調整
        AlignCardZones();//カードゾーンを並べなおす
        for (int i = 0; i < cardsNum; i++)
        {
            cards[i].BackToBasePos();
        }
    }
    */
    

    /// <summary>
    /// Cardの手札内での位置の情報を再調整する
    /// </summary>
    private void SetHandPos()
    {
        for(int i = 0; i < cardsNum; i++)
        {
            cards[i].pos = i;
        }
    }

    /// <summary>
    /// CardZoneを整列させる。カードを従わせるのは
    /// カードにBackToBasePosを実行させるのは別の場所
    /// 必要なら重ね合わせる
    /// </summary>
    public void AlignCardZones()
    {
        //自身の横幅を取得
        //これはローカル座標の幅
        float width = rectTransform.rect.size.x;

        if (cardsNum < 0)
        {//手札にカードがないとき
            return;
        }else if(cardsNum == 1)
        {//一枚の時
            //中央に表示
            float posx = rectTransform.position.x;
            float posy = rectTransform.position.y;
            handCardZones[0].SetPos(posx, posy);
        }
        else
        {//二枚以上の時

            //カード間のスペース       
            float space = width / (cardsNum - 1);

            //最初のカードは左端

            float posx = width / 2;
            float posy = 0;

            for (int i = 0; i < cardsNum; i++)
            {
                
                handCardZones[i].SetPos(posx, posy);

                posx -= space;//位置を増やす
            }
        }    
    }

    private void ShuffleCard()
    {
        dataPerBattle.ShuffleNum++;
        //シャッフルした回数の記録
        deckData.ShuffleCard();
    }
    #endregion

    /// <summary>
    /// 手札のカード全ての表示を更新する
    /// </summary>
    public void RefreshCard()
    {
        foreach(Card c in cards)
        {
            c.RefreshCard();
        }
    }

    /// <summary>
    /// シャッフルした回数を取得する
    /// </summary>
    /// <returns>シャッフルした回数</returns>
    public int GetShuffleNum()
    {
        return dataPerBattle.ShuffleNum;
    }

    /// <summary>
    /// コストがcost残っている状態で、
    /// 使用可能なカードが残っているかを判定する
    /// </summary>
    /// <param name="cost">残っているコスト</param>
    /// <param name="isHaveBlindness">盲目の効果を持っているか</param>
    /// <returns>使用可能なカードが残っているならtrue</returns>
    public bool IsPlayableCardRemained(int cost,bool isHaveBlindness)
    {
        foreach (Card c in cards)
        {
            //使用可能コスト以上のカードならスキップ
            if (c.actualCost > cost) continue;

            //使用可能なカードでも、盲目かつ攻撃カードならスキップ
            if (isHaveBlindness)
            {
                if (c.actualEffectTarget.effectTarget == TargetDefine.EffectTarget.ShortRange||
                    c.actualEffectTarget.effectTarget == TargetDefine.EffectTarget.MediumRange||
                    c.actualEffectTarget.effectTarget == TargetDefine.EffectTarget.LongRange ||
                    c.actualEffectTarget.effectTarget == TargetDefine.EffectTarget.EnemyAll)
                {
                    continue;
                }
            }

            //使用可能なカードが残っている
            return true;
        }

        //使用可能なカードが残っていない
        return false;
    }

    public void CardsBackToBasePos()
    {
        //手札のカードを全て元の位置に戻す
        foreach (Card c in cards)
        {
            c.BackToBasePos();
        }
    }

    public DataPerTurn GetDataPerTurn()
    {
        return dataPerTurn;
    }

    public DataPerBattle GetDataPerBattle()
    {
        return dataPerBattle;
    }

    public DataPerCard GetDataPerCard()
    {
        return dataPerCard;
    }

    #region ForTest
    public void TestCardsShuffle()
    {
        deckData.ShuffleCard();
    }
    #endregion
}
