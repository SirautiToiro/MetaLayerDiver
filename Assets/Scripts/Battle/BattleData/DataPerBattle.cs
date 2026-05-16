using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// バトル一回内の統計データ
/// バトル開始時にリセット
/// </summary>
public class DataPerBattle : MonoBehaviour
{
    public int ShuffleNum{ get; set; }

    public List<EnemyDataSO> DefeatedEnemy;

    public int[] DefeatedEnemyTier { set { defeatedEnemyTier = value; } get { return defeatedEnemyTier; } }
    private int[] defeatedEnemyTier;

    public void ResetData()
    {
        ShuffleNum = 0;
        defeatedEnemyTier = new int[3];
        DefeatedEnemy = new List<EnemyDataSO>();
    }

    /// <summary>
    /// バトル中に倒した敵を記録する
    /// </summary>
    /// <param name="enemyData">倒した敵</param>
    public void SetDefeatedEnemy(EnemyDataSO enemyData)
    {
        DefeatedEnemy.Add(enemyData);

        TierDefine.Tier tier = enemyData.tier.tier;
        switch (tier)
        {
            case TierDefine.Tier.Common:
                DefeatedEnemyTier[0] += 1;
                break;
            case TierDefine.Tier.Rare:
                DefeatedEnemyTier[1] += 1;
                break;
            case TierDefine.Tier.Meta:
                DefeatedEnemyTier[2] += 1;
                break;
        }
    }
}
