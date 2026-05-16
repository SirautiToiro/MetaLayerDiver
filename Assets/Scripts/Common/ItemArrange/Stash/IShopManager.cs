using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ショップ管理インターフェース
/// </summary>
public interface IShopManager
{
    public GameObject gameObject { get; }

    /// <summary>
    /// 初期化
    /// </summary>
    /// <param name="stashPanel"></param>
    /// <param name="cardArrangeManager"></param>
    /// <param name="physicalItemArrangeManager"></param>
    /// <param name="physicalItemInstantiateManager"></param>
    public void Init(StashPanel stashPanel, CardArrangeManager cardArrangeManager,
        PhysicalItemArrangeManager physicalItemArrangeManager, PhysicalItemInstantiateManager physicalItemInstantiateManager);

    /// <summary>
    /// インベントリの表示形式を変更する。
    /// </summary>
    /// <param name="isItem">アイテムに変更するか</param>
    public void ChangeDisplayMode(bool isItem);

    public StashPanel GetStashPanel();

    /// <summary>
    /// 購入時に呼び出される
    /// </summary>
    /// <param name="itemBase">購入したアイテム</param>
    public void OnBuied(ItemBase itemBase);

    /// <summary>
    /// 売却時に呼び出される
    /// </summary>
    public void OnSelled();
    public void OnCharacterClicked();

    /// <summary>
    /// インベントリと店が閉じられたときに呼ばれる
    /// </summary>
    public void OnClosed();

    /// <summary>
    /// カード高速移動の移動先となりうるカードホルダーを返す
    /// 存在しないならnullを返す
    /// </summary>
    /// <returns>カード高速移動の移動先となりうるカードホルダー</returns>
    public ICardHolder GetCardHolderForQuickMove();

    /// <summary>
    /// アイテム高速移動の移動先となりうるIPhysicalItemHolderを返す
    /// 存在しないならnullを返す
    /// </summary>
    /// <returns>アイテム高速移動の移動先となりうるIPhysicalItemHolder</returns>
    public IPhysicalItemHolder GetPhysicalItemHolderForQuickMove();
}
