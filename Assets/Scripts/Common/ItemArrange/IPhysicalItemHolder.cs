using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カード以外のアイテムをドラッグなどによって配置するもの。鞄内など。
/// インターフェース
/// </summary>
public interface IPhysicalItemHolder
{
    /// <summary>
    /// マウスカーソルの位置にアイテムを置く行為
    /// </summary>
    /// <param name="item">配置するアイテム</param>
    /// /// <param name="itemZone">アイテムをセットするCardZone</param>
    /// <returns>アイテムセット結果のクラス</returns>
    public SetItemResult PutItem(PhysicalItemBase item,IPhysicalItemZone itemZone);

    /// <summary>
    /// posX, posYの位置にアイテムを置く行為
    /// 座標から指定
    /// 置けない場合false
    /// </summary>
    /// <param name="item">配置するアイテム</param>
    /// <param name="posX">配置するX座標</param>
    /// <param name="posY">配置するY座標</param>
    /// <returns>アイテムセット結果のクラス</returns>
    public SetItemResult PutItem(PhysicalItemBase item, int posX, int posY);

    /// <summary>
    /// posX, posYの位置からアイテムを取り出す行為
    /// その位置にアイテムがなければスキップ
    /// </summary>
    /// <param name="posX">取り出す場所</param>
    /// <param name="posY">取り出す場所</param>
    public void PullItem(int posX, int posY);

    /// <summary>
    /// 与えられたitemを検索して取り出す行為
    /// </summary>
    /// <param name="item">取り出すitem</param>
    public void PullItem(PhysicalItemBase item);

    //for Test
    public void TestItemDump();
    
    /// <summary>
    /// 保管されているアイテムのデータを記録する
    /// </summary>
    public void SetData();
}

/// <summary>
/// グリッド上に配置されるアイテムを管理するインターフェース
/// </summary>
public interface IGridPhysicalItemHolder : IPhysicalItemHolder
{
    //グリッドの縦横のマスの数
    public int GridHeight { get; set; }
    public int GridWidth { get; set; }

    /// <summary>
    /// アイテムのクラスに対して、それが配置されている場所を検索して取得する。
    /// </summary>
    /// <param name="item">検索する物理アイテム</param>
    /// <returns>場所。グリッド上の位置</returns>
    public (int x, int y)? GetItemPos(PhysicalItemBase item);

    /// <summary>
    /// 現在配置可能なマスの中で、(x,y)の大きさのものが配置できる場所を検索し、
    /// その場所を返す。存在しないならnull
    /// </summary>
    /// <param name="x">配置したいアイテムサイズX</param>
    /// <param name="y">配置したいアイテムサイズY</param>
    /// <returns>配置可能な座標の右上.nullなら配置不可能。</returns>
    public (int x, int y)? GetEmptyZone(int x, int y);

    /// <summary>
    /// Holderが持っているアイテムの一覧から、
    /// 与えられたitemと同じアイテムを検索し、
    /// その一覧を返す
    /// 存在しないならnull
    /// </summary>
    /// <param name="item">同じアイテムを検索するアイテム</param>
    /// <returns>検索されたアイテムのリスト</returns>
    public List<PhysicalItemBase> SearchSameItems(PhysicalItemBase item);
}

/// <summary>
/// 装備アイテムを配置する場所のインターフェース
/// </summary>
public interface IEquipPhysicalItemHolder : IPhysicalItemHolder
{
    
}
