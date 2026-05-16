using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// アイテムをドラッグや右クリックの詳細画面表示などを用いて扱うことができるインターフェース
/// </summary>
public interface IItemManager
{
    /// <summary>
    /// アイテムのドラッグ開始時処理
    /// </summary>
    /// <param name="item">ドラッグしているアイテム</param>
    public void StartDragging(ItemBase item);

    /// <summary>
    /// アイテムのドラッグ終了時処理
    /// </summary>
    /// <returns>アイテムのドラッグ終了に失敗し、ドラッグ継続</returns>
    public bool EndDragging();

    /// <summary>
    /// アイテムのドラッグ中処理
    /// </summary>
    public void UpdateItemDragging();


    /// <summary>
    /// アイテム右クリックで詳細画面を表示する機能
    /// カードの詳細情報を表示する画面を出す。
    /// </summary>
    /// <param name="serialNum">カードのシリアル番号</param>
    public void ShowDescription(int serialNum);

    /// <summary>
    /// 物理アイテムの詳細情報を表示する
    /// </summary>
    /// <param name="pItemData">物理アイテムデータ</param>
    public void ShowDescription(PhysicalItemDataSO pItemData);

    /// <summary>
    /// アイテムをShift＋Clickで素早く動かすときの動作
    /// </summary>
    /// <param name="item">動かすItem</param>
    public void QuickMove(ItemBase item);
}
