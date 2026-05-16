using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 一連のクエストの流れを管理するScriptableObject
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "QuestLine", menuName = " ScriptableObjects/QuestLine", order = 1)]
public class QuestLine : ScriptableObject
{
    [Header("クエストID")]
    public int ID;

    [Header("一連のクエスト")]
    [SerializeReference, SubclassSelector]
    public List<IQuest> QuestList;
}
