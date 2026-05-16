using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 武器データのScriptableObject
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "WeaponData", menuName = " ScriptableObjects/WeaponData", order = 1)]
public class WeaponDataSO : ScriptableObject
{
	[Header("武器使用コスト")]
	public int cost;

    //武器の持つ効果のリスト
    //効果リストの先頭が武器の主要な能力で、その値がUIに表示される
    [Header("効果リスト")]
    public List<CardEffectClass> effectList;

    //武器の効果対象
    [Header("効果対象")]
	public TargetDefine targetDefine;

	//武器の持つ属性のリスト
	[Header("属性リスト")]
	public List<AttributeDefine> attributeList;

	//武器の持つ特殊種類リスト
	[Header("タグリスト")]
	public List<CardTagDefine> tagList;
}
