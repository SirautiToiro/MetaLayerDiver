using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static DungeonTypeDefine;

/// <summary>
/// 全てのEnemyのデータ
/// </summary>
public class EnemyData : SingletonMonoBehaviour<EnemyData>
{
    //未整理の全ての敵データ
    [SerializeField] private List<EnemyDataSO> unorganizedEnemyList;

    [SerializeField] private List<EnemySet> castleBossList;

    //冒険者のリスト(冒険者はどこでも出現)
    [SerializeField] private List<TravelerSet> travelerListSerialized;

    //[ダンジョン、レア度]に応じてEnemyDataSOを分別した先
    private static List<EnemyDataSO>[,] enemyList;

    //[ダンジョン、階層]に応じてボスのリストを整理したもの
    private static EnemySet[,] allBossList;

    private static List<TravelerSet> travelerList;

    /// <summary>
    /// 一群となって敵が出てくる場合のひとつのセットの表現
    /// </summary>
    [System.Serializable]
    private class EnemySet
    {
        public EnemyDataSO[] enemies;//敵の並び
    }

    /// <summary>
    /// 冒険者のセット。冒険者定義も含まれる
    /// </summary>
    [System.Serializable]
    private class TravelerSet:EnemySet 
    {
        [Header("冒険者の名前")]
        public TravelerType type;
    }

    /// <summary>
    /// 冒険者の名前
    /// </summary>
    [System.Serializable]
    public enum TravelerType
    {
        Medousa,    //命道差
        Sasame,     //雪原ささめ
        Terasu,     //雨宮照
        Sutera,     //雨宮星
    }

    public void Awake()
    {
        //シングルトンの処理
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        //シングルトン処理終了

        //ダンジョン数だけ確保,レア度数だけ確保
        enemyList = new List<EnemyDataSO>[System.Enum.GetValues(typeof(DungeonTypeDefine.DungeonType)).Length,
            System.Enum.GetValues(typeof(TierDefine.Tier)).Length];

        //敵リスト領域作成
        for (int i = 0; i < System.Enum.GetValues(typeof(DungeonTypeDefine.DungeonType)).Length; i++)
        {
            for (int j = 0; j < System.Enum.GetValues(typeof(TierDefine.Tier)).Length; j++)
            {
                enemyList[i, j] = new List<EnemyDataSO>();
            }
        }
        //ボスリスト領域作成
        allBossList = new EnemySet[System.Enum.GetValues(typeof(DungeonTypeDefine.DungeonType)).Length,2];
        for (int i = 0; i < System.Enum.GetValues(typeof(DungeonTypeDefine.DungeonType)).Length; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                allBossList[i, j] = new EnemySet();
            }
        }

        //分類して確保
        foreach (EnemyDataSO enemy in unorganizedEnemyList)
        {
            enemyList[(int)enemy.spawn.dungeonType, (int)enemy.tier.tier].Add(enemy);
        }
        //ボス記録
        for (int i=0;i<castleBossList.Count;i++)
        {//古城記録
            allBossList[0, i]=castleBossList[i];//古城は0に対応
        }

        //冒険者記録
        travelerList = new List<TravelerSet>();
        for (int i = 0; i < travelerListSerialized.Count; i++)
        {
            travelerList.Add(travelerListSerialized[i]);
        }
    }

    /// <summary>
    /// 該当地帯の敵のデータリストを取得する
    /// </summary>
    /// <param name="dungeonType">敵の出現するダンジョン</param>
    /// <param name="tier">敵のレアリティ</param>
    /// <returns>敵データリスト</returns>
    public static List<EnemyDataSO> GetEnemyData(DungeonTypeDefine.DungeonType dungeonType,TierDefine.Tier tier)
    {
        return enemyList[(int)dungeonType, (int)tier];
    }

    /// <summary>
    /// 該当地帯の敵のデータを取得する(1つだけランダム)
    /// </summary>
    /// <param name="dungeonType">敵の出現するダンジョン</param>
    /// <param name="tier">敵のレアリティ</param>
    /// <returns>該当する中からランダムに敵データ</returns>
    public static EnemyDataSO GetEnemyDataRandomOne(DungeonTypeDefine.DungeonType dungeonType, TierDefine.Tier tier)
    {
        int len = enemyList[(int)dungeonType, (int)tier].Count;
        int ran = UnityEngine.Random.Range(0, len);
        return enemyList[(int)dungeonType, (int)tier][ran];
    }


    /// <summary>
    /// 該当地帯のボスデータを取得する
    /// </summary>
    /// <param name="dungeonType">敵の出現するダンジョン</param>
    /// <param name="floor">階層</param>
    /// <param name="pos">敵の配置位置</param>
    /// <returns>ボスデータ</returns>
    public static EnemyDataSO GetBossData(DungeonTypeDefine.DungeonType dungeonType,int floor,int pos)
    {
        //配置されている以上の場所はnull
        if (pos > allBossList[(int)dungeonType, floor].enemies.Length-1) return null;
        return allBossList[(int)dungeonType, floor].enemies[pos];
    }

    /// <summary>
    /// 冒険者を生成する時に使用。ランダムな冒険者のタイプを返す
    /// </summary>
    /// <returns>ランダムな冒険者のタイプを返す</returns>
    public static TravelerType GetRandomTravelerType()
    {
        return travelerList[UnityEngine.Random.Range(0, travelerList.Count)].type;
    }

    /// <summary>
    /// ある冒険者のある位置の敵データを返す
    /// </summary>
    /// <param name="type">冒険者のタイプ</param>
    /// <param name="pos">返す敵の位置</param>
    /// <returns>冒険者のデータ</returns>
    public static EnemyDataSO GetTravelerData(TravelerType type,int pos)
    {
        foreach(TravelerSet t in travelerList)
        {
            if (t.type == type)
            {
                //配置されている以上の場所はnull
                if (pos > t.enemies.Length-1) return null;
                return t.enemies[pos];
            }
        }

        return null;
    }

    //forTest
    private void TestEnemyListDump()
    {
        for(int i=0;i< System.Enum.GetValues(typeof(DungeonTypeDefine.DungeonType)).Length; i++)
        {
            for(int j=0;j< System.Enum.GetValues(typeof(TierDefine.Tier)).Length; j++)
            {
                if (enemyList[i, j] == null) continue;
                foreach (EnemyDataSO enemy in enemyList[i,j])
                {
                    Debug.Log(enemy.spawn.dungeonType.ToString() + ":" +enemy.tier.tier.ToString()+":"+enemy.enemyName);
                    Debug.Log((DungeonTypeDefine.DungeonType)i+":"+(TierDefine.Tier)j);
                }
            }
        }
    }
}
