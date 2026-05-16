using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestRequest
{
    //倒されるべき敵のデータと数のセット
    [System.Serializable]
    public class EnemyNumData
    {
        public bool IsAnyEnemy;//trueのとき、どんな敵でもいい。falseのとき、EnemyDataの敵を倒す必要がある
        public EnemyDataSO EnemyData;
        public int Num;
    }


    public enum RequestType
    {
        CollectCards,   //指定したカードを集める
        DefeatEnemies,  //指定した敵を倒す
        GatherItems,    //指定したアイテムを集める
        None,           //要求なし。クリア後のストーリーにも派生しない。読むだけ。
    }

    public RequestType requestType;

    public List<StorageData.CardStackData> RequiredCards;

    public List<PhysicalItemGridPosNumData> RequiredItems;

    public EnemyNumData RequiredEnemies;
}
