using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードをドラッグなどによって配置するもの。鞄内など。
/// PlaceCardZoneを内部で使用する。インターフェース
/// </summary>
public interface ICardHolder
{
    //Holder内でどのような操作をするか。
    //スクロールし、1つのみスタックするものか
    //1つ以上スタックが可能な倉庫用のものか。
    public CardHolderType HolderType { get; }

    public enum CardHolderType
    {
        ScrollHolder,
        StackHolder,
    }

    /// <summary>
    /// numの位置にカードを置く行為
    /// </summary>
    /// <param name="card">置かれるカード</param>
    /// <param name="pos">カードが置かれる場所</param>
    public void PutCard(Card card,int pos);

    /// <summary>
    /// numの位置からカードを動かす行為
    /// </summary>
    /// <param name="pos">カードが取られる場所</param>
    public void PullCard(int pos);

    /// <summary>
    /// posの位置のCardZoneにドラッグ中のカードが重なった時の動作
    /// 重ならない状態になった時はpos=-1(カードを順に整列させる動作)
    /// </summary>
    /// <param name="pos">カードが重なっている場所重なっていないときはpos = -1</param>
    public void EnterCard(int pos);

    
    /// <summary>
    /// CardZoneからドラッグ中のカードが離れた時のドラッグ終了時動作
    /// </summary>
    public void ExitCard();

    /// <summary>
    /// Holderが保持しているScrollViewのGameObjectを取得する
    /// </summary>
    /// <returns>Holderが保持しているScrollViewのGameObject</returns>
    public GameObject GetScrollViewObject();

    /// <summary>
    /// データとして記録されているカードの一覧を返す
    /// </summary>
    /// <returns>Holderが保持しているカードのリストs</returns>
    public List<StorageData.CardStack> GetCards();
}
