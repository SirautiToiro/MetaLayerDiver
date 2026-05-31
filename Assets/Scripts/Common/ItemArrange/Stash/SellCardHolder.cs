using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 天秤屋など、カードをプレイヤーから売却するときに使用
/// </summary>
public class SellCardHolder : MonoBehaviour,ICardHolder
{
    public ICardHolder.CardHolderType HolderType { get { return ICardHolder.CardHolderType.StackHolder; } }

    private StashPanel stashPanel;

    //店で使用されているなら初期化
    private IShopManager shopManager = null;

    [SerializeField] private StackCardArrangement stackCardArrangement;

    private CardArrangeManager cardArrangeManager;

    public void Init(List<StorageData.CardStack> requiredCards, CardArrangeManager cardArrangeManager, StashPanel stashPanel, IShopManager shopManager = null)
    {
        this.cardArrangeManager = cardArrangeManager;
        this.stashPanel = stashPanel;

        this.shopManager = shopManager;

        stackCardArrangement.Init(requiredCards, this, cardArrangeManager);
    }

    public void EnterCard(int pos)
    {
        if (pos == -1)
        {
            stackCardArrangement.CardsCompleteMove();//移動状況リセット
            stackCardArrangement.CardsBackToBasePos();//位置調整
        }
    }

    public void ExitCard()
    {
        stackCardArrangement.AlignAndSpaceCards(-1);//整列
        stackCardArrangement.CardsCompleteMove();
        stackCardArrangement.SetZoneNum();//Zone数調整
    }

    public GameObject GetScrollViewObject()
    {
        return stackCardArrangement.GetScrollViewObject();
    }

    public void PullCard(int pos)
    {
        Card card = cardArrangeManager.InstantiateAndMoveCard(stackCardArrangement.GetOneCardDataFromList(pos),
            stackCardArrangement.GetPlaceCardZone(pos));

        stashPanel.ChangeDisplayMode(false);//カードを表示するモードに切り替える
        shopManager?.OnBuied(card);
    }

    public void PutCard(Card card, int pos)
    {
        Card newCard = stackCardArrangement.AddAndAlignCard(card);//データに追加し、整列。cardはこの中で破壊される

        //配置の成否はCardArrangeManager側で判断しているため、実際にはnullにはならないはず
        if (newCard is null) return;

        stackCardArrangement.CardsCompleteMove();

        stackCardArrangement.ScrollToCard(newCard);
    }

    /// <summary>
    /// データとして記録されているカードの一覧を返す
    /// </summary>
    /// <returns></returns>
    public List<StorageData.CardStack> GetCards()
    {
        return stackCardArrangement.GetCards();
    }

    public List<Card> GetCardsInstances()
    {
        return stackCardArrangement.GetCardsInstances();
    }
}
