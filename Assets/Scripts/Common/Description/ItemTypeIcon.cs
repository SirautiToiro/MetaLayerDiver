using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// アイテム詳細を表示する際にアイテムタイプを表示する
/// </summary>
public class ItemTypeIcon : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    [SerializeField] private Sprite weaponSprite;
    [SerializeField] private Sprite gearSprite;
    [SerializeField] private Sprite consumablesSprite;
    [SerializeField] private Sprite treasuresSprite;
    [SerializeField] private Sprite dropsSprite;
    [SerializeField] private Sprite cardSprite;

    /// <summary>
    /// アイテムタイプに応じたspriteをセットする
    /// </summary>
    /// <param name="type">セットするアイテムタイプ</param>
    public void SetSprite(PhysicalItemTypeDefine.PhysicalItemType type)
    {
        switch (type)
        {
            case PhysicalItemTypeDefine.PhysicalItemType.Weapon:
                iconImage.sprite = weaponSprite;
                break;
            case PhysicalItemTypeDefine.PhysicalItemType.Gear:
                iconImage.sprite = gearSprite;
                break;
            case PhysicalItemTypeDefine.PhysicalItemType.Consumables:
                iconImage.sprite = consumablesSprite;
                break;
            case PhysicalItemTypeDefine.PhysicalItemType.Treasures:
                iconImage.sprite = treasuresSprite;
                break;
            case PhysicalItemTypeDefine.PhysicalItemType.Drops:
                iconImage.sprite = dropsSprite;
                break;
        }
    }

    /// <summary>
    /// カードのアイテムタイプを持っている
    /// </summary>
    /// <param name="card"></param>
    public void SetSprite(Card card)
    {
        iconImage.sprite = cardSprite;
    }
}
