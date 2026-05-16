using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySummonManager : MonoBehaviour
{
    [SerializeField] private List<SummonedEnemyData> summonedEnemyData;

    [Serializable] public class SummonedEnemyData
    {
        public EnemyDataSO enemyData;
        public SummonedEnemyType enemyType;
    }

    //召喚される敵の種類
    public enum SummonedEnemyType
    {
        StellaBit,  //ステラビット
    }

    //召喚のやり方
    public enum SummonPolicy
    {
        Front,//最前列に
        Back,//最後尾に
    }

    /// <summary>
    /// 召喚される敵のタイプを指定すると、そのデータを返す
    /// </summary>
    /// <param name="type">召喚される敵のタイプ</param>
    /// <returns>対応する敵のタイプ</returns>
    public EnemyDataSO GetSummonedEnemyData(SummonedEnemyType type)
    {
        foreach(SummonedEnemyData data in summonedEnemyData)
        {
            if(type == data.enemyType)
            {
                return data.enemyData;
            }
        }

        return null;
    }
}
