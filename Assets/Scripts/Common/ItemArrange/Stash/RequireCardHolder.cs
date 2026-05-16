using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ある種類のカードを要求し、それを配置することのできるHolder
/// 配置した後の購入処理がある場合、店で行う(ShopManager)
/// </summary>
public class RequireCardHolder : MonoBehaviour, ICardHolder
{
    public ICardHolder.CardHolderType HolderType { get { return ICardHolder.CardHolderType.StackHolder; } }

    private StashPanel stashPanel;

    //店で使用されているなら初期化
    private IShopManager shopManager = null;

    [SerializeField] private RequireCardArrangement requireCardArrangement;

    private CardArrangeManager cardArrangeManager;


    public void Init(List<StorageData.CardStack> requiredCards, CardArrangeManager cardArrangeManager,StashPanel stashPanel,IShopManager shopManager = null)
    {
        this.cardArrangeManager = cardArrangeManager;
        this.stashPanel = stashPanel;

        this.shopManager = shopManager;

        requireCardArrangement.Init(requiredCards, this, cardArrangeManager);
    }

    public void EnterCard(int pos)
    {
        if (pos == -1)
        {
            requireCardArrangement.CardsCompleteMove();//移動状況リセット
            requireCardArrangement.CardsBackToBasePos();//位置調整
        }
    }

    public void ExitCard()
    {
        requireCardArrangement.CardsCompleteMove();
    }

    public GameObject GetScrollViewObject()
    {
        return requireCardArrangement.GetScrollViewObject();
    }

    public void PullCard(int pos)
    {
        Card card = cardArrangeManager.InstantiateAndMoveCard(requireCardArrangement.GetOneCardDataFromList(pos),
            requireCardArrangement.GetPlaceCardZone(pos));

        stashPanel.ChangeDisplayMode(false);//カードを表示するモードに切り替える
        shopManager?.OnBuied(card);
    }

    public void PutCard(Card card, int pos)
    {
        Card newCard = requireCardArrangement.AddAndAlignCard(card);//データに追加し、整列。cardはこの中で破壊される

        //配置の成否はCardArrangeManager側で判断しているため、実際にはnullにはならないはず
        if (newCard is null) return;

        requireCardArrangement.CardsCompleteMove();

        requireCardArrangement.ScrollToCard(newCard);
    }

    /// <summary>
    /// 配置処理の前に使用。
    /// そのカードが配置可能かを判定する
    /// </summary>
    /// <param name="card">配置したいCard</param>
    /// <returns>配置可能ならTrue</returns>
    public bool IsPuutable(Card card)
    {
        return requireCardArrangement.IsPuttable(card);
    }

    /// <summary>
    /// データとして記録されているカードの一覧を返す
    /// 実際に配置されているもののみ
    /// ここで、要求されているカードが配置されているかを確認
    /// </summary>
    /// <returns>配置されているカード</returns>
    public List<StorageData.CardStack> GetCards()
    {
        return requireCardArrangement.GetRealCards();
    }

    public List<Card> GetCardsInstances()
    {
        return requireCardArrangement.GetCardsInstances();
    }

    /// <summary>
    /// カードの要求が満たされているならTrueを返す
    /// </summary>
    /// <returns>カードの要求が満たされているならTrueを返す</returns>
    public bool IsRequireCompleted()
    {
        return requireCardArrangement.IsRequireCompleted();
    }

    /// <summary>
    /// 配置されているカードをすべて削除する
    /// </summary>
    public void DeleteRealCards()
    {
        requireCardArrangement.DeleteRealCards();
    }
}
