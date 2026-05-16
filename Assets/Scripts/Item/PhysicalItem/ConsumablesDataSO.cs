using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 消耗品データのScriptableObject
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "ConsumablesData", menuName = " ScriptableObjects/ConsumablesData", order = 1)]
public class ConsumablesDataSO : ScriptableObject
{
    [Header("使用コスト")]
    public int Cost;

    [Header("効果リスト")]
    public List<CardEffectClass> EffectList;

    //アイテムの効果対象
    [Header("効果対象")]
    public TargetDefine TargetDefine;

    [Header("マップで使用可能か")]
    public bool UsableOnMap;
}
