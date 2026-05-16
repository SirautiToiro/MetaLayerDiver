using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemyの1ターンに行う行動を持ったシリアライズされたクラス
/// </summary>
[System.Serializable]
public class ActionInTurn {
    public List<EnemyActionDefine> actionsInTurn;
}
