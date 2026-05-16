using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 装備のダンジョン中に発生する効果のタグのためのインターフェース
/// </summary>
public interface IGearTag
{
    int value { get; set; } //タグの値
}

public interface IGearOnEnterDungeon:IGearTag
{
    /// <summary>
    /// ダンジョンに入る時の処理を行う
    /// </summary>
    /// <param name="gear">紐づく装備</param>
    /// <param name="dungeonManager">ダンジョンマネージャ</param>
    void OnEnterDungeon(DungeonManager dungeonManager);
}

public class GearLeadTorch : IGearOnEnterDungeon
{
    public int value { get; set; }

    public void OnEnterDungeon(DungeonManager dungeonManager)
    {
        dungeonManager.SetShowDoorFlag(true);
    }
}
