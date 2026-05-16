using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// デッキクラスの宣言.デッキという概念
/// </summary>
public class Deck
{
    public static int DECKCARDMAX = 16;//デッキ内のカードの最大値

    //デッキ内のカード群.DECKCARDMAX枚必ず存在する.通し番号で管理
    private List<int> cards;

    //デッキ名
    public string Name { get; set; }

    //デッキ内のカードの数
    private int cardsNum;

    //コンストラクタ
    public Deck(string name)
    {
        cards = new List<int>();
        cardsNum = 0;
        Name = name;
    }

    public List<int> GetCards()
    {
        return cards;
    }

    /// <summary>
    /// n番目のカードを取得
    /// </summary>
    /// <param name="n">カードの番地</param>
    /// <returns></returns>
    public int GetCard(int n)
    {
        return cards[n];
    }

    /// <summary>
    /// n番目にカードを入れる.その場にあったカードは戻ってくる
    /// n番目にカードがないなら、入れるだけ
    /// カードがないところに入れたなら-1を返す
    /// </summary>
    /// <param name="n">カードの番地</param>
    /// <param name="card">入れるカード</param>
    /// <returns>返ってくるカード</returns>
    public int SetCard(int n,int card)
    {
        if (n >= cardsNum)
        {//まだカードが入れられていないところに入れる場合
            //カードリストの最後尾に入れる.0返す
            //cardsNumを一増やす

            cards.Add(card);
            cardsNum++;

            return -1;
        }
        else
        {//カードが入っているところに入れる場合
            int cardTmp = cards[n];
            cards[n] = card;
            return cardTmp;

            //TODO:もともとあったカードが所持品に帰ってくる
        }
    }

    /// <summary>
    /// デッキ内のカードの枚数を返す
    /// </summary>
    /// <returns>デッキ内のカードの枚数</returns>
    public int GetCount()
    {
        return cards.Count;
    }
}
