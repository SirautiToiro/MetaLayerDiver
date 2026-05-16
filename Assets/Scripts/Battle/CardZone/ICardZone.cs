using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// カードが配置されうる場所を示すインターフェース
/// カードが配置された時の反応,可視化時の反応を定める。
/// </summary>
public interface ICardZone
{
    /// <summary>
    /// カードゾーンの座標を取得する
    /// </summary>
    /// <returns>カードゾーンの座標</returns>
    public Vector3 GetPosition();

    /// <summary>
    /// カードゾーンのGameObjectのTransformを取得する
    /// </summary>
    /// <returns>Transform</returns>
    public Transform GetTransform();
}
