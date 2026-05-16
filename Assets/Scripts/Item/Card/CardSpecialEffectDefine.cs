using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// メタ級などの、特殊なカード演出を行うカードのタグ
/// </summary>
[System.Serializable]
public class CardSpecialEffectDefine
{
    [Header("特殊演出効果")]
    public SpecialEffect specialEffect;

    public enum SpecialEffect
    {
        None,      //なし
        ArcEngine, //アークエンジン
    }
}
