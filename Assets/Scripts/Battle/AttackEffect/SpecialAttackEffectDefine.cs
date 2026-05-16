using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 攻撃エフェクトが特殊な事情により変更された時、その変更のタイプ
/// </summary>
[System.Serializable]
public class SpecialAttackEffectDefine
{
    public SpecialEffect specialEffect;

    public enum SpecialEffect
    {
        NoChange,   //変化なし
        Avoid,      //回避
    }
}
