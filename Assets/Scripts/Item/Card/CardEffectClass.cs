using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CardEffectClass
{
    public CardEffectDefine Effect; // カード効果の種類と値
    [Header("対応する状態異常")]
    [SerializeReference, SubclassSelector] public IState UseState;//カード効果が使用する状態異常
}
