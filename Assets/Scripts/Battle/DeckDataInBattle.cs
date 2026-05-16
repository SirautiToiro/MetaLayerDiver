using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// バトル中のデッキデータ処理
/// </summary>
public class DeckDataInBattle : MonoBehaviour
{
    //バトル中のデッキ
    [SerializeField] private List<int> cardsInDeck;//デッキに残っているカード
    [SerializeField] private List<int> cardsInTrash;//捨札に入っているカード
    [SerializeField] private List<int> cardsInVoid;//バトル中に消費されたカード
    //TODO:SerializeFieldはTest用

    [SerializeField] private HandManager handManager;

    //初期化
    public void Init()
    {
        cardsInDeck = new List<int>();
        cardsInTrash = new List<int>();
        cardsInVoid = new List<int>();
        //所持しているデッキの内部のカードを全て戦闘中デッキの中に
        //TrashとVoidは空
        Deck deck= InventoryData.GetPlayerDeck();
        for (int i = 0; i < Deck.DECKCARDMAX; i++)
        {
            cardsInDeck.Add(deck.GetCard(i));
        }
    }

    /// <summary>
    /// cardsInDeckの上から一枚を返す(引く)
    /// それはcardsInDeckから無くなる
    /// デッキにカードがない場合-1を返す
    /// </summary>
    /// <returns>引くカードのシリアル番号</returns>
    public int GetCard()
    {
        if (cardsInDeck.Count == 0)
        {
            return -1;
        }

        int lastPos = cardsInDeck.Count - 1;//リストの最後の場所
        int card=cardsInDeck[lastPos];//出力するカード
        cardsInDeck.RemoveAt(lastPos);//リストの最後のカードを削除

        return card;//引かれたカードを返す
    }

    /// <summary>
    /// デッキから属性を持つカードをランダムに一枚返す
    /// 存在しない場合は-1
    /// </summary>
    /// <param name="attribute"></param>
    /// <returns></returns>
    public int GetRandomCardByAttribute(AttributeDefine.Attribute attribute)
    {
        List<int> targetCards = new List<int>();
        foreach (int card in cardsInDeck)
        {
            CardDataSO cardData = PlayerCardData.GetCardDataFromSerialNum(card);
            if (cardData != null && cardData.attributeList.Exists(attr => attr.attribute == attribute))
            {
                targetCards.Add(card);
            }
        }

        //ランダムに一つ返す
        if (targetCards.Count == 0)
        {//存在しないなら-1
            return -1;
        }
        int rand = UnityEngine.Random.Range(0, targetCards.Count);
        return targetCards[rand];
    }

    /// <summary>
    /// カードを捨札に入れる
    /// </summary>
    /// <param name="card">捨札に行くカード</param>
    public void TrashCard(Card card)
    {
        if (card.IsItemHasTag(CardTagDefine.CardTag.Temporary))
        {//一時的なカードは捨て札に送らず、破棄

        }
        else
        {//捨札に記録
            cardsInTrash.Add(card.serialNum);
        }
        
    }

    /// <summary>
    /// 捨て札を山札に戻し、シャッフルする
    /// </summary>
    public void ShuffleCard()
    {
        //捨て札を全て山札に入れる
        while (cardsInTrash.Count > 0)
        {
            cardsInDeck.Add(cardsInTrash[0]);
            cardsInTrash.RemoveAt(0);
        }

        List<int> listTmp = new List<int>();

        //listTmpにランダムに移動(シャッフル)
        while(cardsInDeck.Count>0)
        {
            int rand = UnityEngine.Random.Range(0, cardsInDeck.Count);
            listTmp.Add(cardsInDeck[rand]);
            cardsInDeck.RemoveAt(rand);
        }

        //listTmpから戻す
        for(int i=0;i<listTmp.Count;i++){
            cardsInDeck.Add(listTmp[i]);
        }
    }

    /// <summary>
    /// デッキ内のカード枚数を返す
    /// </summary>
    /// <returns>デッキ内のカード枚数</returns>
    public int GetCardNum()
    {
        return cardsInDeck.Count;
    }

    /// <summary>
    /// 捨て札内のカード枚数を返す
    /// </summary>
    /// <returns>捨て札のカード枚数</returns>
    public int GetTrashNum()
    {
        return cardsInTrash.Count;
    }
}
