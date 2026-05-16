using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 特殊な敵を設定する(ボスなど)
/// </summary>
[System.Serializable]
public class EnemyUniqueDefine
{
    [Header("特殊敵の名前")]
    public UniqueTag uniqueTag;

    public enum UniqueTag
    {
        Normal,             //普通の敵
        BBSwordFront,       //大大剣のスケルトンロード、剣先
        BBSwordMiddle,      //大大剣のスケルトンロード、剣中
        BBSwordSkeletonLoad,//大大剣のスケルトンロード、本体
    }
}
