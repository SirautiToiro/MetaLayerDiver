using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 換金アイテムもしくはドロップ品のScriptableObject
/// 内部的な実装は同じ
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "TreasuresOrDropsData", menuName = " ScriptableObjects/TreasuresOrDropsData", order = 1)]
public class TreasuresOrDropsDataSO : ScriptableObject
{
    [Header("フレーバーテキスト")]
    [Multiline(7)]
    public string FlavorText; //フレーバーテキスト
}
