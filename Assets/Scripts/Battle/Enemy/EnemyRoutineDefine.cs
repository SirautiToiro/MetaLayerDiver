using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemyの行動がどのような順番で発生するか
/// </summary>
[System.Serializable]
public class EnemyRoutineDefine
{
    [Header("行動ルーチン")]
    public EnemyRoutine enemyRoutine;

    public enum EnemyRoutine
    {
        Periodic,//周期的
        Random,//ランダム
    }
}
