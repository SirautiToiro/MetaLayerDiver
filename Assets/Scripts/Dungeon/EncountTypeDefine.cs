using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// バトル開始時のエンカウントの種類(宝箱、ボスなど)を示す
/// </summary>
[System.Serializable]
public class EncountTypeDefine
{
    [Header("エンカウント種類")]
    public EnecountType enecountType;

    public enum EnecountType
    {
        Normal,     //通常の敵
        Treasure,   //宝箱
        Boss,       //ボス
        LastBoss,   //ラスボス
        Traveler,   //冒険者
    }
}
