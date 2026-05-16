using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーが入るダンジョンの種類を記す
/// </summary>
[System.Serializable]
public class DungeonTypeDefine
{
    [Header("ダンジョン名")]
    public DungeonType dungeonType;

    public enum DungeonType
    {
        Castle, //城ステージ
        Forest, //森ステージ
        City,   //廃都市ステージ
    }

    readonly public static Dictionary<DungeonType, string> Dic_DungeonName = new Dictionary<DungeonType, string>()
    {
        {DungeonType.Castle,"古城" },
        {DungeonType.Forest,"森林" },
        {DungeonType.City,"廃都市" },
    };
}
