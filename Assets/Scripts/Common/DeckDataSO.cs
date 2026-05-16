using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 初期のデッキデータを格納するスクリプタブルオブジェクト
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "DeckData", menuName = " ScriptableObjects/DeckData", order = 1)]
public class DeckDataSO : ScriptableObject
{
    [Header("名前")]
    public string deckName;

    [Header("デッキ内カードSO")]
    public List<CardDataSO> cardList;
}
