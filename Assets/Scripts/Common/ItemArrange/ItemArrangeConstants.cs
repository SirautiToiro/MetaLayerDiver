using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテム配置時に使用する定数を管理するクラス
/// </summary>
[System.Serializable]
public class ItemArrangeConstants : MonoBehaviour
{
    public const int ItemCellSize = 80;//アイテムを表示するときの一つのマスの縦横

    public const int BackpackCardMax = 16;//鞄に入るカードの最大数
}
