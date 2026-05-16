using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵の戦闘開始時に発生する効果の定義
/// </summary>
[System.Serializable]
public class EnemyInitialEffectDefine
{
    //パラメータ
    [Header("初期行動の種類")]
    public InitialEffect Initial;

    [Header("初期行動で得る状態効果")]
    [SerializeReference, SubclassSelector]
    public IStateEnemy State;

    [Header("状態異常の値,Eternalは0")]
    public int Value;

    public enum InitialEffect
    {
        GetEffect,//状態効果を得る
    }
}
