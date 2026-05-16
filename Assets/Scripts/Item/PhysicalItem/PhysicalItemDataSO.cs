using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理アイテムのScriptableObject
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "PItemData", menuName = " ScriptableObjects/PItemData", order = 1)]
public class PhysicalItemDataSO : ScriptableObject
{
    [Header("アイテム番号(種類別)")]
    public int SerialNum;

    [Header("アイテム名")]
    public string ItemName;

    [Header("アイコン")]
    public Sprite IconSprite;

    [Header("(大きなサイズのアイテムで、装備可能なら)小さいアイコン")]
    public Sprite MiniIconSprite;

    [Header("スタック最大数")]
    public int StackMax;

    [Header("縦横の大きさ")]
    public int SizeX;
    public int SizeY;

    [Header("アイテムタイプ")]
    public PhysicalItemTypeDefine PhysicalItemType;

    [Header("レア度")]
    public TierDefine Tier;

    [Header("対応するWeaponDataSO(あるなら)")]
    public WeaponDataSO WeaponData;

    [Header("対応するConsumablesDataSO")]
    public ConsumablesDataSO ConsumablesData;

    [Header("対応するTreasuresOrDropsDataSO")]
    public TreasuresOrDropsDataSO TreasuresOrDropsData;

    [Header("対応するGearDataSO(あるなら)")]
    public GearDataSO GearData;
}
