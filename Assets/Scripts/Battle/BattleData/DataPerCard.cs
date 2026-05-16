using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

/// <summary>
/// カード一つのプレイ時の統計データ
/// カードプレイ終了でリセット
/// </summary>
public class DataPerCard : MonoBehaviour
{
    //そのカードによって引かれたカード
    public List<Card> DrawedCards { get; set; }

    //そのカードによって消費された状態の種類と量
    //同じ種類の状態が複数列に含まれる可能性あり
    public List<IState> ConsumedState { get; set; }

    public void ResetData()
    {
        DrawedCards = new List<Card>();
        ConsumedState = new List<IState>();
    }

    public void AddUsedState(IState state)
    {
        ConsumedState.Add(state);
    }
}
