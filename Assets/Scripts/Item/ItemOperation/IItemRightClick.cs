using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテムの右クリック動作を処理するクラス
/// </summary>
public interface IItemRightClick
{
    /// <summary>
    /// 右クリックされた時の動作
    /// </summary>
    /// <param name="item">右クリックされたアイテム</param>
    public void OnRightClick(ItemBase item);
}

public class ShowDescriptionWhenRightClick : IItemRightClick
{
    /// <summary>
    /// アイテムの右クリック時に説明を表示する
    /// </summary>
    /// <param name="item">右クリックされたアイテム</param>
    public void OnRightClick(ItemBase item)
    {
        // アイテムの説明を表示する処理
        ShowDescription(item);
    }

    /// <summary>
    /// 右クリックで説明を表示する際の処理
    /// </summary>
    /// <param name="item">詳細を説明されるアイテム</param>
    private void ShowDescription(ItemBase item)
    {
        if (item is Card card)
        {//カードならその番号
            item.itemManager.ShowDescription(card.serialNum);
        }
        else if (item is PhysicalItemBase pItem)
        {//物理アイテムなら、そのデータ
            item.itemManager.ShowDescription(pItem.BaseItemData);
        }else if (item is Weapon weapon)
        {//武器なら、それ
            item.itemManager.ShowDescription(weapon.BasePlItemData);
        }else if(item is Gear gear)
        {//装備なら、それ
            item.itemManager.ShowDescription(gear.BasePItemData);
        }else if(item is Consumable consumable)
        {//消耗品なら、それ
            item.itemManager.ShowDescription(consumable.BasePItemData);
        }
    }
}

public class ShowMenuWhenRightClick : IItemRightClick
{
    /// <summary>
    /// アイテムの右クリック時に操作メニューを表示する
    /// 物理アイテムのみ
    /// </summary>
    /// <param name="item">右クリックされたアイテム</param>
    public void OnRightClick(ItemBase item)
    {
        // アイテムのメニューを表示する処理
        ShowMenu(item);
    }
    /// <summary>
    /// 右クリックでメニューを表示する際の処理
    /// </summary>
    /// <param name="item">メニューを表示されるアイテム</param>
    private void ShowMenu(ItemBase item)
    {
        // メニュー表示の処理を実装
        if(item is PhysicalItemBase pItem)
        {//物理アイテムなら
            ((PhysicalItemArrangeManager)pItem.itemManager).ShowMiniMenu(pItem);
        }
    }
}   
