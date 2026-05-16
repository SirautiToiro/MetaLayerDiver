using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "GearData", menuName = " ScriptableObjects/GearData", order = 1)]
public class GearDataSO : ScriptableObject
{
    [Header("‘Î‰‚·‚éó‘ÔˆÙí")]
    [SerializeReference, SubclassSelector]public IStateInGear state;

    [Header("ó‘ÔˆÙí‚Ì’l")]
    [SerializeField] public int stateValue;

    [Header("‘•”õà–¾")]
    [Multiline(7)]
    public string description;

    [Header("‘•”õ‚Ìƒ_ƒ“ƒWƒ‡ƒ“Œø‰Ê")]
    [SerializeReference, SubclassSelector] public IGearTag tag;
}
