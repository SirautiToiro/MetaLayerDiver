using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "TestData", menuName = " ScriptableObjects/TestData", order = 1)]
public class TestMainSO : ScriptableObject
{
    [Header("テストクラス")]
    [SerializeReference, SubclassSelector] public CardEffectClass effectClass;
}
