using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ダンジョンに使用するマップタイルの定義
/// </summary>
[System.Serializable]
public class MapTileDefine
{
    public enum MapTile
    {
        Road,       //道(なにもない)
        Enemy,      //敵
        Traveler,   //冒険者
        Treasure,   //宝箱
        Tower,      //塔
        Lake,       //湖
        Stair,      //階段(下層へ行く)
        Door,       //扉(脱出)
        Start,      //スタート(出現場所)
        Boss,       //ボス
    }

    public enum MapWall
    {
        WallHide,//発見されていない壁
        RoadHide,//発見されていない道
        WallShow,//発見されている壁
        RoadShow,//発見されている道
    }
}
