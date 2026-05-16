using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードデータのScriptableObject
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "CardData", menuName = " ScriptableObjects/CardData", order = 1)]
public class CardDataSO : ScriptableObject
{
    [Header("カード番号")]
    public int serialNum;

	[Header("カードコスト")]
	public int cost;

	[Header("カード名")]
	public string cardName;

	[Header("アイコン")]
	public Sprite iconSprite;

	//カード効果のリスト
	[Header("効果リスト")]
	public List<CardEffectClass> effectList;

	//カードの効果対象
	[Header("効果対象")]
	public TargetDefine targetDefine;

	//カードの持つ属性のリスト
	[Header("属性リスト")]
	public List<AttributeDefine> attributeList;

	//カードの持つ特殊種類リスト
	[Header("タグリスト")]
	public List<CardTagDefine> tagList;

	//カードの持つレア度
	[Header("レア度")]
	public TierDefine tier;

    //カード特殊演出効果
    [Header("特殊演出効果")]
    public CardSpecialEffectDefine specialEffect;
}
