using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// ダンジョンマップのデータ構造格納,
/// マップやプレイヤー駒の操作
/// </summary>
public class Dungeon : MonoBehaviour
{
    [SerializeField] private DungeonUI dungeonUI;
    [SerializeField] private DungeonManager dungeonManager;
    [SerializeField] private DungeonSceneManager dungeonSceneManager;

    //ダンジョンのマップ格納場所
    //[X,Y]
    //[DungeonConstants.MapX,DungeonConstants.MapY]で初期化される
    private MapTileDefine.MapTile[,] dungeonMap;
    //ダンジョンの壁があるかどうかを格納する
    //MapTileDefine.MapWallを使用する
    //横行はdungeonWallsHorizontal,縦列はdungeonWallsVertical
    //それぞれ、[DungeonConstants.MapX,DungeonConstants.MapY-1],
    //[DungeonConstants.MapX-1,DungeonConstants.MapY]で初期化される
    //dungeonMap[2,2]とdungeonMap[2,3]の間は
    //dungeonWallsHorizontal[2,2]
    private MapTileDefine.MapWall[,] dungeonWallsHorizontal;
    private MapTileDefine.MapWall[,] dungeonWallsVertical;

    //ダンジョンマップが表示されているかのフラグ
    //falseなら非表示(humeiの画像を使用)
    private bool[,] dungeonMapDisplayFlags;

    //プレイヤーの駒の配置されている場所の座標
    private int playerX;
    private int playerY;

    //タイルの生成のされやすさをインスペクターに格納
    [SerializeField] TileGenerateValue[] tileGenerateValues;

    //タイルの生成される個数をインスペクターに格納
    [SerializeField] TileGenerateValue[] tileGenerateNums;

    private int genereateValueSum;//タイルの生成のされやすさの合計(最初に計算)

    [System.Serializable]
    /// <summary>
    /// タイルの生成のされやすさを格納するクラス
    /// あるいは、ダンジョン内タイルの絶対的な個数を格納するクラス
    /// </summary>
    public class TileGenerateValue
    {
        public MapTileDefine.MapTile tile;
        public int value;
    }

    /// <summary>
    /// ダンジョンの生成
    /// </summary>
    /// <param name="showDoorFlag">ドアの位置を最初から見せるか</param>
    public void Init(bool showDoorFlag)
    {
        //tileGenerateValuesの値をチェックすると同時に、合計値を格納(test)
        genereateValueSum = CheckTileGenerateValue(false);
        GenerateDungeon();
        dungeonUI.Init(dungeonMap,dungeonMapDisplayFlags,dungeonWallsHorizontal,dungeonWallsVertical,playerX,playerY);
        ClearFog(playerX, playerY, false);//プレイヤー座標と周囲の霧を晴らす
        if (showDoorFlag) {
            ClearDoor();//ドアの位置を見せる指示があるなら、見せる
        }
        //DisplayDungeonMap();//ForTestダンジョンデータ出力
    }

    #region マップ生成
    private void GenerateDungeon()
    {
        //マップ初期化
        dungeonMap = new MapTileDefine.MapTile[DungeonConstants.MapX, DungeonConstants.MapY];
        //壁初期化
        dungeonWallsHorizontal = new MapTileDefine.MapWall[DungeonConstants.MapX, DungeonConstants.MapY-1];
        dungeonWallsVertical = new MapTileDefine.MapWall[DungeonConstants.MapX-1, DungeonConstants.MapY];

        dungeonMapDisplayFlags = new bool[DungeonConstants.MapX, DungeonConstants.MapY];

        //ダンジョン生成時に使用
        //trueになっている場合、ダンジョンの道が繋がっている
        //まず、ダンジョンの全てのタイルを道につなげて、
        //その周囲を壁にした後、壁をランダムに取り払う。
         bool[,] dungeonTileConnected;

        //全てのタイルを非表示にする
        for (int i = 0; i < dungeonMapDisplayFlags.GetLength(0); i++)
        {
            for (int j = 0; j < dungeonMapDisplayFlags.GetLength(1); j++)
            {
                dungeonMapDisplayFlags[i, j] = false;
            }
        }

        //個数がランダムのタイルを配置する
        for (int i = 0; i < dungeonMap.GetLength(0); i++)
        {
            for(int j = 0; j < dungeonMap.GetLength(1); j++)
            {
                dungeonMap[i, j] = MapTileDefine.MapTile.Road;//最初に入れるが、必ず置き換えられる

                //0からgenereateValueSumまでの範囲をtileGenerateValues.value
                //で区切っていき、生成された乱数がその区切りのどこにあるかで
                //生成するタイルを判定する
                int ran =UnityEngine.Random.Range(0, genereateValueSum);
                foreach(TileGenerateValue tValue in tileGenerateValues)
                {
                    ran-=tValue.value;
                    if (ran < 0)
                    {
                        dungeonMap[i, j] = tValue.tile;//対応するタイルを格納
                        break;
                    }
                }
                
            }
        }

        int doorNum = GetTileGenerateNums(MapTileDefine.MapTile.Door);
        //固定の個数存在するdoor,stairについて、それまでに作った分を置き換えて生成する
        for (int i = 0; i < doorNum; i++)
        {
            int ranX=UnityEngine.Random.Range(0, dungeonMap.GetLength(0));
            int ranY=UnityEngine.Random.Range(0, dungeonMap.GetLength(1));
            if(dungeonMap[ranX,ranY]== MapTileDefine.MapTile.Door)
            {//既に扉が生成されている位置なら
                i--;//何もしないで逃げ帰る.カウントも進めない
                continue;
            }
            else
            {//新しく扉を生成する
                dungeonMap[ranX, ranY] = MapTileDefine.MapTile.Door;
                
            }
        }

        int stairNum = GetTileGenerateNums(MapTileDefine.MapTile.Stair);
        //固定の個数存在するdoor,stairについて、それまでに作った分を置き換えて生成する
        for (int i = 0; i < stairNum; i++)
        {
            int ranX = UnityEngine.Random.Range(0, dungeonMap.GetLength(0));
            int ranY = UnityEngine.Random.Range(0, dungeonMap.GetLength(1));
            if (dungeonMap[ranX, ranY] == MapTileDefine.MapTile.Door||
                dungeonMap[ranX, ranY] == MapTileDefine.MapTile.Stair)
            {//既に扉か階段が生成されている位置なら
                i--;//何もしないで逃げ帰る.カウントも進めない
                continue;
            }
            else
            {//新しく階段を生成する
                dungeonMap[ranX, ranY] = MapTileDefine.MapTile.Stair;
                
            }
        }
        int startNum = GetTileGenerateNums(MapTileDefine.MapTile.Start);
        //固定の個数存在するStartについて、それまでに作った分を置き換えて生成する
        for (int i = 0; i < startNum; i++)
        {
            int ranX = UnityEngine.Random.Range(0, dungeonMap.GetLength(0));
            int ranY = UnityEngine.Random.Range(0, dungeonMap.GetLength(1));
            if (dungeonMap[ranX, ranY] == MapTileDefine.MapTile.Door ||
                dungeonMap[ranX, ranY] == MapTileDefine.MapTile.Stair||
                dungeonMap[ranX, ranY] == MapTileDefine.MapTile.Start)
            {//既に扉か階段かスタートが生成されている位置なら
                i--;//何もしないで逃げ帰る.カウントも進めない
                continue;
            }
            else
            {//新しくスタートを生成する
                dungeonMap[ranX, ranY] = MapTileDefine.MapTile.Start;
                //スタート地点をプレイヤーの座標にする
                playerX = ranX;
                playerY = ranY;
                
            }
        }
        int bossNum = GetTileGenerateNums(MapTileDefine.MapTile.Boss);
        //固定の個数存在するBossについて、それまでに作った分を置き換えて生成する
        for (int i = 0; i < bossNum; i++)
        {
            int ranX = UnityEngine.Random.Range(0, dungeonMap.GetLength(0));
            int ranY = UnityEngine.Random.Range(0, dungeonMap.GetLength(1));
            if (dungeonMap[ranX, ranY] == MapTileDefine.MapTile.Door ||
                dungeonMap[ranX, ranY] == MapTileDefine.MapTile.Stair ||
                dungeonMap[ranX, ranY] == MapTileDefine.MapTile.Start||
                dungeonMap[ranX, ranY] == MapTileDefine.MapTile.Boss)
            {//既に扉か階段かスタートかボスが生成されている位置なら
                i--;//何もしないで逃げ帰る.カウントも進めない
                continue;
            }
            else
            {//新しくスタートを生成する
                dungeonMap[ranX, ranY] = MapTileDefine.MapTile.Boss;
            }
        }

        //ダンジョンの連結フラグを全てfalseにする。
        dungeonTileConnected = new bool[DungeonConstants.MapX, DungeonConstants.MapY];
        for(int i = 0; i < dungeonTileConnected.GetLength(0); i++)
        {
            for(int j = 0; j < dungeonTileConnected.GetLength(1); j++)
            {
                dungeonTileConnected[i,j] = false;
            }
        }
        //壁を全て壁にする
        for (int i = 0; i < dungeonWallsHorizontal.GetLength(0); i++)
        {
            for (int j = 0; j < dungeonWallsHorizontal.GetLength(1); j++)
            {
                dungeonWallsHorizontal[i, j] = MapTileDefine.MapWall.WallHide;
            }
        }
        for (int i = 0; i < dungeonWallsVertical.GetLength(0); i++)
        {
            for (int j = 0; j < dungeonWallsVertical.GetLength(1); j++)
            {
                dungeonWallsVertical[i, j] = MapTileDefine.MapWall.WallHide;
            }
        }

        //ダンジョンに連結できるタイルがまだ存在する間、一筆書きで
        //タイルを連結していく。
        //https://qiita.com/kyooooooooma/items/a8ee1157b89b7f744098

        //つなげていくタイルの現在位置
        int connectTileStartX = 0;
        int connectTileStartY = 0;
        //つなげていくタイルの次の位置
        int? connectTileNextX = 0;
        int? connectTileNextY = 0;

        //最初の位置に繋がったフラグを付ける
        dungeonTileConnected[connectTileStartX, connectTileStartY]=true;

        //一筆書きで描ける限りつなげていく
        while (connectTileNextX != null)
        {
            (connectTileNextX, connectTileNextY) = SearchUnconnectedTile(connectTileStartX, connectTileStartY,dungeonTileConnected);
            if (connectTileNextX == null||connectTileNextY==null)
            {//もう繋がらないなら終了
                break;
            }

            //道を繋げる
            SetRoad(connectTileStartX, connectTileStartY, connectTileNextX.Value, connectTileNextY.Value);

            //繋がったタイルに繋がったフラグを付ける
            dungeonTileConnected[connectTileNextX.Value, connectTileNextY.Value] = true;
            //次にタイルの連結を計算するときの始点を変更
            connectTileStartX = connectTileNextX.Value;
            connectTileStartY = connectTileNextY.Value;
        }

        //一筆書きで繋げられなかったマスを、全て繋ぐ。
        //繋げられなかったマスの数を計算
        int unconnectedTiles = 0;
        for (int i = 0; i < dungeonTileConnected.GetLength(0); i++)
        {
            for (int j = 0; j < dungeonTileConnected.GetLength(1); j++)
            {
                if(dungeonTileConnected[i, j] == false)
                {
                    unconnectedTiles++;
                }
            }
        }

        //つなげられていないタイルがある間
        while(unconnectedTiles > 0)
        {
            for (int i = 0; i < dungeonTileConnected.GetLength(0); i++)
            {
                for (int j = 0; j < dungeonTileConnected.GetLength(1); j++)
                {
                    //つなげられていないタイルなら
                    if (dungeonTileConnected[i, j] == false)
                    {
                        //次につなげるタイルの座標を取得。繋げられないならnull
                        //既につながっているタイルから選ぶ
                        (connectTileNextX, connectTileNextY) = SearchConnectedTile(i,j,dungeonTileConnected);
                        if (connectTileNextX == null || connectTileNextY == null)
                        {//繋がらないならスキップ
                            continue;
                        }
                        else
                        {//繋げられるなら繋げる。
                            SetRoad(i,j, connectTileNextX.Value, connectTileNextY.Value);
                            //繋がったタイルに繋がったフラグを付ける
                            dungeonTileConnected[i,j] = true;
                            unconnectedTiles--;//つなげられたタイルを1つ減らす
                        }
                    }
                }
            }
        }

        //ランダムで壁を道にする
        for (int i = 0; i < dungeonWallsHorizontal.GetLength(0); i++)
        {
            for (int j = 0; j < dungeonWallsHorizontal.GetLength(1); j++)
            {
                int ran = UnityEngine.Random.Range(0, 3);
                if (ran <= 1)
                {
                    //道に
                    dungeonWallsHorizontal[i, j] = MapTileDefine.MapWall.RoadHide;
                }
            }
        }
        for (int i = 0; i < dungeonWallsVertical.GetLength(0); i++)
        {
            for (int j = 0; j < dungeonWallsVertical.GetLength(1); j++)
            {
                int ran = UnityEngine.Random.Range(0, 3);
                if (ran <= 1)
                {
                    //道に
                    dungeonWallsVertical[i, j] = MapTileDefine.MapWall.RoadHide;
                }
            }
        }
    }

    /// <summary>
    /// dungeonTileConnectedのx,yの位置から上下左右を見て、
    /// 連結されていないタイルを発見すると、その中からランダムで一つ選び、
    /// 座標を返す。
    /// </summary>
    /// <param name="x">マップ内の接続を行うスタート位置x座標</param>
    /// <param name="y">マップ内の接続を行うスタート位置y座標</param>
    /// <returns>次に連結するタイルの座標.接続できないならNull</returns>
    private (int? x, int? y) SearchUnconnectedTile(int x, int y,bool[,] dungeonTileConnected)
    {
        //次に接続することが可能な座標のリスト
        List<(int nextX, int nextY)> nextPos = new List<(int nextX, int nextY)>();
        List<(int dirX, int dirY)> direcitonList = new List<(int nextX, int nextY)>()
        {
            (-1,0),(1,0),(0,-1),(0,1)
        };
        for (int i = 0; i < direcitonList.Count; i++)
        {
            //次に確認する場所のx,y座標
            int xPos = x + direcitonList[i].dirX;
            int yPos = y + direcitonList[i].dirY;
            if (!IsInsideDungeon(xPos,yPos))
            {//マップの外側に行こうとしていたなら
                continue;
            }
            else
            {//マップの内側なら
                if (!dungeonTileConnected[xPos, yPos])
                {//繋がっていないなら
                    nextPos.Add((xPos, yPos));
                }
            }
        }

        int nextPosLength = nextPos.Count;
        if (nextPosLength == 0)
        {
            return (null, null);
        }
        else
        {
            int rand = UnityEngine.Random.Range(0, nextPosLength);
            return nextPos[rand];
        }
    }

    /// <summary>
    /// dungeonTileConnectedのx,yの位置から上下左右を見て、
    /// 連結されてるタイルを発見すると、その中からランダムで一つ選び、
    /// 座標を返す。
    /// </summary>
    /// <param name="x">マップ内の接続を行うスタート位置x座標</param>
    /// <param name="y">マップ内の接続を行うスタート位置y座標</param>
    /// <returns>次に連結するタイルの座標.接続できないならNull</returns>
    private (int? x, int? y) SearchConnectedTile(int x, int y, bool[,] dungeonTileConnected)
    {
        //次に接続することが可能な座標のリスト
        List<(int nextX, int nextY)> nextPos = new List<(int nextX, int nextY)>();
        List<(int dirX, int dirY)> direcitonList = new List<(int nextX, int nextY)>()
        {
            (-1,0),(1,0),(0,-1),(0,1)
        };
        for (int i = 0; i < direcitonList.Count; i++)
        {
            //次に確認する場所のx,y座標
            int xPos = x + direcitonList[i].dirX;
            int yPos = y + direcitonList[i].dirY;
            if (!IsInsideDungeon(xPos, yPos))
            {//マップの外側に行こうとしていたなら
                continue;
            }
            else
            {//マップの内側なら
                if (dungeonTileConnected[xPos, yPos])
                {//連結されているタイルなら
                    nextPos.Add((xPos, yPos));
                }
            }
        }

        int nextPosLength = nextPos.Count;
        if (nextPosLength == 0)
        {
            return (null, null);
        }
        else
        {
            int rand = UnityEngine.Random.Range(0, nextPosLength);
            return nextPos[rand];
        }
    }

    /// <summary>
    /// dungeonWallsHorizontal,dungeonWallsVerticalの壁情報を、
    /// startとendの座標を利用して道に変更する
    /// </summary>
    /// <param name="startX">道の開始地点のX座標</param>
    /// <param name="startY">道の開始地点のY座標</param>
    /// <param name="endX">道の終了地点のX座標</param>
    /// <param name="endY">道の終了地点のY座標</param>
    private void SetRoad(int startX,int startY,int endX,int endY)
    {
        //繋がる方向を取得
        int connectDirectionX = endX - startX;
        int connectDirectionY = endY - startY;

        //繋がっている方向の壁を道にする
        if (connectDirectionX == 0)
        {
            //横方向の壁
            if (connectDirectionY == -1)
            {
                dungeonWallsHorizontal[startX, startY - 1] = MapTileDefine.MapWall.RoadHide;
            }
            else if (connectDirectionY == 1)
            {
                dungeonWallsHorizontal[startX, startY] = MapTileDefine.MapWall.RoadHide;
            }
        }
        else if (connectDirectionY == 0)
        {
            //縦方向の壁
            if (connectDirectionX == -1)
            {
                dungeonWallsVertical[startX - 1, startY] = MapTileDefine.MapWall.RoadHide;
            }
            else if (connectDirectionX == 1)
            {
                dungeonWallsVertical[startX, startY] = MapTileDefine.MapWall.RoadHide;
            }
        }
    }
    #endregion

    #region マップ状態操作
    /// <summary>
    /// マップの不明地点を表示する
    /// (x,y)の座標の周囲に対して行う
    /// bigFlag=falseなら隣接1マス,壁に阻まれる
    /// bigFlag=trueなら周囲2マス.晴らしたタイルの間にある壁も可視化する
    /// UIにも反映
    /// </summary>
    /// <param name="x">不明地点を晴らす中心のx座標</param>
    /// <param name="y">不明地点を晴らす中心のy座標</param>
    /// <param name="bigFlag">晴らし方を大きくするか</param>
    private void ClearFog(int x,int y,bool bigFlag)
    {
        //真下の座標の霧を晴らす
        if(dungeonMapDisplayFlags[x, y] == false)
        {
            dungeonMapDisplayFlags[x, y] = true;
            dungeonUI.UpdateFog(x,y, true);
        }
        if (bigFlag)
        {//晴らし方が大きいなら
            //晴らしたい先のリスト
            List<(int dirX, int dirY)> direcitonList = new List<(int nextX, int nextY)>()
            {
                (-2,0),(-1,-1),(-1,0),(-1,1),(0,-2),(0,-1),(0,1),(0,2),(1,0),(1,1),(1,-1),(2,0)
            };
            //縦横の壁の晴らす先のリスト
            //横
            List<(int dirX, int dirY)> direcitonListHorizontalWalls = new List<(int nextX, int nextY)>()
            {
                (-1,-1),(-1,0),(0,-2),(0,-1),(0,0),(0,1),(1,-1),(1,0)
            };
            //縦
            List<(int dirX, int dirY)> direcitonListVerticalWalls = new List<(int nextX, int nextY)>()
            {
                (-1,-1),(0,-1),(-2,0),(-1,0),(0,0),(1,0),(-1,1),(0,1)
            };

            for (int i = 0; i < direcitonList.Count; i++)
            {
                int nextX = x + direcitonList[i].dirX;
                int nextY = y + direcitonList[i].dirY;
                if (IsInsideDungeon(nextX, nextY)&& dungeonMapDisplayFlags[nextX, nextY] == false)
                {//マップ内に次の座標があるなら,霧の状態の場所なら
                    //霧を晴らす
                    dungeonMapDisplayFlags[nextX, nextY] = true;
                    dungeonUI.UpdateFog(nextX, nextY, true);
                }
            }

            //それぞれの壁の霧を晴らす
            for(int i = 0; i < direcitonListHorizontalWalls.Count; i++)
            {
                int nextX = x + direcitonListHorizontalWalls[i].dirX;
                int nextY = y + direcitonListHorizontalWalls[i].dirY;
                if (IsInsideDungeonWallsHorizontal(nextX, nextY))
                {
                    DisplayWall(nextX, nextY, true, true);
                    dungeonUI.UpdateWall(nextX, nextY, true,true);
                }
            }
            for (int i = 0; i < direcitonListVerticalWalls.Count; i++)
            {
                int nextX = x + direcitonListVerticalWalls[i].dirX;
                int nextY = y + direcitonListVerticalWalls[i].dirY;
                if (IsInsideDungeonWallsVertical(nextX, nextY))
                {
                    DisplayWall(nextX, nextY, true, false);
                    dungeonUI.UpdateWall(nextX, nextY, true, false);
                }
            }
        }
        else
        {//晴らし方が小さいなら
            //晴らしたい先のリスト
            List<(int dirX, int dirY)> direcitonList = new List<(int nextX, int nextY)>()
            {
                (-1,0),(1,0),(0,-1),(0,1)
            };

            for(int i = 0; i < direcitonList.Count; i++)
            {
                int nextX=x+direcitonList[i].dirX;
                int nextY=y+direcitonList[i].dirY;
                
                if (IsInsideDungeon(nextX, nextY))
                {//マップ内に次の座標があるなら
                    //壁を表示状態にする
                    DisplayWall(x, y, nextX, nextY, true);
                    dungeonUI.UpdateWall(x, y, nextX, nextY, true);
                    //霧の状態の場所なら
                    if (dungeonMapDisplayFlags[nextX, nextY] == false)
                    {
                        //壁があるかを確認,通れるなら
                        if (IsRoad(x, y, nextX, nextY))
                        {
                            //霧を晴らす
                            dungeonMapDisplayFlags[nextX, nextY] = true;
                            dungeonUI.UpdateFog(nextX, nextY, true);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// ドアの位置をすべて表示する
    /// </summary>
    private void ClearDoor()
    {
        for(int i = 0; i < dungeonMap.GetLength(0); i++)
        {
            for(int j= 0; j < dungeonMap.GetLength(1); j++)
            {
                if (dungeonMap[i,j] == MapTileDefine.MapTile.Door)
                {//ドアの位置のマップタイルに
                    dungeonMapDisplayFlags[i,j] = true;
                    dungeonUI.UpdateFog(i,j, true);

                    //晴らしたい先のリスト
                    List<(int dirX, int dirY)> direcitonList = new List<(int nextX, int nextY)>()
                    {
                        (-1,0),(1,0),(0,-1),(0,1)
                    };

                    //壁を晴らす
                    for (int k = 0; k < direcitonList.Count; k++)
                    {
                        int nextX = i + direcitonList[k].dirX;
                        int nextY = j + direcitonList[k].dirY;
                        if (IsInsideDungeon(nextX, nextY))
                        {//マップ内に次の座標があるなら
                         //壁を表示状態にする
                            DisplayWall(i,j, nextX, nextY, true);
                            dungeonUI.UpdateWall(i,j, nextX, nextY, true);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 道・壁の表示状態を更新する
    /// </summary>
    /// <param name="x">道の始点のx座標</param>
    /// <param name="y">道の始点のy座標</param>
    /// <param name="nextX">道の終点のx座標</param>
    /// <param name="nextY">道の終点のy座標</param>
    /// <param name="isDisplay">trueなら表示状態にする</param>
    private void DisplayWall(int x, int y, int nextX, int nextY,bool isDisplay)
    {
        //方向
        int dirX = nextX - x;
        int dirY = nextY - y;
        int targetX, targetY;
        
        //方向を確認
        if (dirX == 0)
        {//横方向
            //更新する道の座標
            if (dirY == 1)
            {
                targetX = x;
                targetY = y;
            }
            else if (dirY == -1)
            {
                targetX = x;
                targetY = y-1;
            }
            else
            {
                return;
            }

            //以前の状態に応じて更新する
            if(dungeonWallsHorizontal[targetX, targetY]==MapTileDefine.MapWall.RoadHide||
                dungeonWallsHorizontal[targetX, targetY] == MapTileDefine.MapWall.RoadShow)
            {
                if (isDisplay)
                {
                    dungeonWallsHorizontal[targetX, targetY] = MapTileDefine.MapWall.RoadShow;
                }
                else
                {
                    dungeonWallsHorizontal[targetX, targetY] = MapTileDefine.MapWall.RoadHide;
                }
            }
            else
            {
                if (isDisplay)
                {
                    dungeonWallsHorizontal[targetX, targetY] = MapTileDefine.MapWall.WallShow;
                }
                else
                {
                    dungeonWallsHorizontal[targetX, targetY] = MapTileDefine.MapWall.WallHide;
                }
            }
            
        }
        else if (dirY == 0)
        {//縦方向
            //更新する道の座標
            if (dirX == 1)
            {
                targetX = x;
                targetY = y;
            }
            else if (dirX == -1)
            {
                targetX = x-1;
                targetY = y;
            }
            else
            {
                return;
            }

            //以前の状態に応じて更新する
            if (dungeonWallsVertical[targetX, targetY] == MapTileDefine.MapWall.RoadHide ||
                dungeonWallsVertical[targetX, targetY] == MapTileDefine.MapWall.RoadShow)
            {
                if (isDisplay)
                {
                    dungeonWallsVertical[targetX, targetY] = MapTileDefine.MapWall.RoadShow;
                }
                else
                {
                    dungeonWallsVertical[targetX, targetY] = MapTileDefine.MapWall.RoadHide;
                }
            }
            else
            {
                if (isDisplay)
                {
                    dungeonWallsVertical[targetX, targetY] = MapTileDefine.MapWall.WallShow;
                }
                else
                {
                    dungeonWallsVertical[targetX, targetY] = MapTileDefine.MapWall.WallHide;
                }
            }
        }
    }

    /// <summary>
    /// 道・壁の表示状態を更新する
    /// 座標に直接更新
    /// 配列の範囲内に存在することは確認されているものとする
    /// </summary>
    /// <param name="x">変更先のx座標</param>
    /// <param name="y">変更先のy座標</param>
    /// <param name="isDisplay">trueなら表示状態にする</param>
    /// <param name="isHorizontal">横方向の壁を変更するか。trueならdungeonWallsHorizontalを更新。</param>
    private void DisplayWall(int x,int y,bool isDisplay,bool isHorizontal)
    {
        if (isHorizontal)
        {
            if(dungeonWallsHorizontal[x,y]==MapTileDefine.MapWall.RoadShow||
                dungeonWallsHorizontal[x, y] == MapTileDefine.MapWall.RoadHide)
            {//壁の状態なら
                if (isDisplay)
                {//可視化するなら
                    dungeonWallsHorizontal[x, y] = MapTileDefine.MapWall.RoadShow;
                }
                else
                {
                    dungeonWallsHorizontal[x, y] = MapTileDefine.MapWall.RoadHide;
                }
            }
            else
            {
                if (isDisplay)
                {//可視化するなら
                    dungeonWallsHorizontal[x, y] = MapTileDefine.MapWall.WallShow;
                }
                else
                {
                    dungeonWallsHorizontal[x, y] = MapTileDefine.MapWall.WallHide;
                }
            }

        }
        else
        {
            if (dungeonWallsVertical[x, y] == MapTileDefine.MapWall.RoadShow ||
                dungeonWallsVertical[x, y] == MapTileDefine.MapWall.RoadHide)
            {//壁の状態なら
                if (isDisplay)
                {//可視化するなら
                    dungeonWallsVertical[x, y] = MapTileDefine.MapWall.RoadShow;
                }
                else
                {
                    dungeonWallsVertical[x, y] = MapTileDefine.MapWall.RoadHide;
                }
            }
            else
            {
                if (isDisplay)
                {//可視化するなら
                    dungeonWallsVertical[x, y] = MapTileDefine.MapWall.WallShow;
                }
                else
                {
                    dungeonWallsVertical[x, y] = MapTileDefine.MapWall.WallHide;
                }
            }
        }
    }

    

    #endregion

    #region タイル情報チェック
    /// <summary>
    /// タイル生成確率を確認する
    /// </summary>
    /// <param name="b">trueなら、確率をログに表示する</param>
    /// <returns>タイル生成確率の合計値</returns>
    private int CheckTileGenerateValue(bool b)
    {
        int sum = 0;
        Array array=Enum.GetValues(typeof(MapTileDefine.MapTile));
        foreach(MapTileDefine.MapTile mapTile in array)
        {
            //タイルがtileGenerateNumsかtileGenerateValuesで定義されているかを確認する
            int ans = -1;
            if(mapTile== MapTileDefine.MapTile.Door|| mapTile == MapTileDefine.MapTile.Stair||mapTile==MapTileDefine.MapTile.Start || mapTile == MapTileDefine.MapTile.Boss)
            {
                //Door,Stair,StartはtileGenerateNumsで定義している
                ans = GetTileGenerateNums(mapTile);

                if (ans < 0)
                {
                    Debug.Log(String.Format("{0}がtileGenerateNumsに用意されていません", mapTile.ToString()));
                }
            }
            else
            {
                //その他はtileGenerateValuesで定義している
                ans = GetTileGenerateValues(mapTile);

                if (ans < 0)
                {
                    Debug.Log(String.Format("{0}がtileGenerateValuesに用意されていません", mapTile.ToString()));
                }
                else
                {
                    sum += ans;
                }
            }
        }

        if (b)
        {
            //合計からそれぞれのタイルの確率を求める
            //test表示
            foreach (TileGenerateValue generateValue in tileGenerateValues)
            {
                Debug.Log(String.Format("{0}:{1}%", generateValue.tile.ToString(), (float)generateValue.value * 100 / (float)sum));
            }
        }
        
        return sum;
    }

    /// <summary>
    /// tileGenerateValuesのUnknown以外の値について、MapTileを受け取り、そのvalueを返す
    /// 存在しないなら-1を返す
    /// </summary>
    /// <param name="mapTile">検索するMapTile</param>
    /// <returns>対応するvalue,存在しないなら-1</returns>
    private int GetTileGenerateValues(MapTileDefine.MapTile mapTile)
    {
        foreach(TileGenerateValue generateValue in tileGenerateValues)
        {
            if(generateValue.tile == mapTile)
            {//一致するものがあれば
                return generateValue.value;
            }
        }

        return -1;
    }

    /// <summary>
    /// tileGenerateNumsのUnknown以外の値について、MapTileを受け取り、そのvalueを返す
    /// 存在しないなら-1を返す
    /// </summary>
    /// <param name="mapTile">検索するMapTile</param>
    /// <returns>対応するvalue,存在しないなら-1</returns>
    private int GetTileGenerateNums(MapTileDefine.MapTile mapTile)
    {
        foreach (TileGenerateValue generateValue in tileGenerateNums)
        {
            if (generateValue.tile == mapTile)
            {//一致するものがあれば
                return generateValue.value;
            }
        }

        return -1;
    }

    /// <summary>
    /// 座標(x,y)がマップの内側であるかを判定する
    /// </summary>
    /// <param name="x">x座標</param>
    /// <param name="y">y座標</param>
    /// <returns>trueならマップの内側</returns>
    private bool IsInsideDungeon(int x, int y)
    {
        if (x < 0 || y < 0 || x >= dungeonMap.GetLength(0) || y >= dungeonMap.GetLength(1))
        {//マップの外側に行こうとしていたなら
            return false;
        }
        else
        {//マップの内側なら
            return true;
        }
    }

    /// <summary>
    /// 横方向の壁の配列範囲内にあるかを判定
    /// </summary>
    /// <param name="x">x座標</param>
    /// <param name="y">y座標</param>
    /// <returns>trueなら内側</returns>
    private bool IsInsideDungeonWallsHorizontal(int x,int y)
    {
        if (x < 0 || y < 0 || x >= dungeonWallsHorizontal.GetLength(0) || y >= dungeonWallsHorizontal.GetLength(1))
        {//マップの外側に行こうとしていたなら
            return false;
        }
        else
        {//マップの内側なら
            return true;
        }
    }
    /// <summary>
    /// 縦方向の壁の配列範囲内にあるかを判定
    /// </summary>
    /// <param name="x">x座標</param>
    /// <param name="y">y座標</param>
    /// <returns>trueなら内側</returns>
    private bool IsInsideDungeonWallsVertical(int x, int y)
    {
        if (x < 0 || y < 0 || x >= dungeonWallsVertical.GetLength(0) || y >= dungeonWallsVertical.GetLength(1))
        {//マップの外側に行こうとしていたなら
            return false;
        }
        else
        {//マップの内側なら
            return true;
        }
    }

    /// <summary>
    /// dungeonMapの(x,y)の位置にあるタイル情報を取得する
    /// </summary>
    /// <param name="x">x座標</param>
    /// <param name="y">y座標</param>
    /// <returns>指定された場所のタイル情報</returns>
    public MapTileDefine.MapTile GetMapTile(int x, int y)
    {
        return dungeonMap[x, y];
    }

    /// <summary>
    /// (x,y)と(nextX,nextY)の間が、壁によって塞がれているなら
    /// falseを返す。壁によって塞がれていないならtrue
    /// </summary>
    /// <param name="x">移動前のx座標</param>
    /// <param name="y">移動前のy座標</param>
    /// <param name="nextX">移動後のx座標</param>
    /// <param name="nextY">移動後のy座標</param>
    /// <returns>壁によって塞がれていないならtrue</returns>
    private bool IsRoad(int x, int y, int nextX, int nextY)
    {
        //方向
        int dirX = nextX - x;
        int dirY = nextY - y;
        //壁があるかを確認
        if (dirX == 0)
        {//横方向の壁を確認
            if (dirY == 1)
            {
                //壁があるなら
                if (dungeonWallsHorizontal[x, y] == MapTileDefine.MapWall.WallShow ||
                    dungeonWallsHorizontal[x, y] == MapTileDefine.MapWall.WallHide)
                {
                    return false;
                }
            }
            else if (dirY == -1)
            {
                //壁があるなら
                if (dungeonWallsHorizontal[x, y - 1] == MapTileDefine.MapWall.WallShow ||
                    dungeonWallsHorizontal[x, y - 1] == MapTileDefine.MapWall.WallHide)
                {
                    return false;
                }
            }
            else
            {//離れた場所なら
                return false;
            }
        }
        else if (dirY == 0)
        {//縦方向の壁を確認
            if (dirX == 1)
            {
                //壁があるなら
                if (dungeonWallsVertical[x, y] == MapTileDefine.MapWall.WallShow ||
                    dungeonWallsVertical[x, y] == MapTileDefine.MapWall.WallHide)
                {
                    return false;
                }
            }
            else if (dirX == -1)
            {
                //壁があるなら
                if (dungeonWallsVertical[x - 1, y] == MapTileDefine.MapWall.WallShow ||
                    dungeonWallsVertical[x - 1, y] == MapTileDefine.MapWall.WallHide)
                {
                    return false;
                }
            }
            else
            {//離れた場所なら
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// プレイヤーの駒の座標と指定した先の座標が
    /// 霧によって遮られていないかつ、道タイルによって繋がっている時、
    /// trueを返す.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool IsConnectedByRoad(int x, int y)
    {
        //道によって繋がれているかの判定、trueなら繋がっている
        bool[,] connectedByRoad = new bool[dungeonMap.GetLength(0), dungeonMap.GetLength(1)];
        for (int i = 0; i < connectedByRoad.GetLength(0); i++)
        {
            for (int j = 0; j < connectedByRoad.GetLength(1); j++)
            {
                connectedByRoad[i, j] = false;
            }
        }
        //目的地自身は繋がっている。
        //目的地からさかのぼって検索する。
        connectedByRoad[x, y] = true;

        //検索しているもっとも外側の場所のリスト
        //順次、外側を追加していく処理を行う。
        List<(int posX, int posY)> searchingList = new List<(int posX, int posY)>();

        searchingList.Add((x, y));//最初の場所を追加

        //上下左右のリスト
        List<(int dirX, int dirY)> direcitonList = new List<(int nextX, int nextY)>()
            {
                (-1,0),(1,0),(0,-1),(0,1)
            };

        //ループのその回でリストに追加された個数。0なら終了
        int addedCount = 1;//ひとつ追加しているので1

        //リストへの追加処理
        while (addedCount > 0)
        {
            int loopNum = addedCount;//周囲のタイル検索をループする回数
            addedCount = 0;
            //リストのそれぞれのマスについて、周囲の繋がっているタイルを検索し、
            //繋がっているならリストに追加
            for (int i = 0; i < loopNum; i++)
            {
                //それぞれの方向について
                for (int j = 0; j < direcitonList.Count; j++)
                {
                    //調べる座標
                    int searchX = searchingList[i].posX + direcitonList[j].dirX;
                    int searchY = searchingList[i].posY + direcitonList[j].dirY;

                    if (IsInsideDungeon(searchX, searchY)
                        && IsRoad(searchingList[i].posX, searchingList[i].posY, searchX, searchY))
                    {//壁に阻まれていないなら,マップ内なら
                        if (searchX == playerX && searchY == playerY)
                        {//プレイヤーの場所を探し当てたなら
                            return true;
                        }
                        if (dungeonMapDisplayFlags[searchX, searchY]
                            && (dungeonMap[searchX, searchY] == MapTileDefine.MapTile.Road
                            || dungeonMap[searchX, searchY] == MapTileDefine.MapTile.Start))
                        {//霧がなく見える場所かつ、道のタイルであるなら
                            if (connectedByRoad[searchX, searchY] == false)
                            {//まだ検索を行っていない場所なら
                                //検索する場所のリストの末尾に追加
                                searchingList.Add((searchX, searchY));
                                addedCount++;//個数を増加
                                connectedByRoad[searchX, searchY] = true;//その場所は繋がっている
                            }
                        }
                    }
                }
            }

            //前回の分のリストを削除する
            for (int i = 0; i < loopNum; i++)
            {
                searchingList.RemoveAt(0);
            }
        }
        //検索で発見されなかった
        return false;
    }
    #endregion

#region マップ移動

    /// <summary>
    /// 動くことが可能な位置なら、
    /// x,yの座標にプレイヤーの駒を進める
    /// 周囲の霧を晴らす
    /// 隣接していないと進まない(test)
    /// 今後は、経路検索をして進めるようにしたい(TODO)
    /// </summary>
    /// <param name="x">移動先のX座標</param>
    /// <param name="y">移動先のY座標</param>
    public void Move(int x, int y)
    {
        if (IsConnectedByRoad(x, y))
        {//道で繋がっている場所なら
            playerX = x;
            playerY = y;
            dungeonUI.MovePiece(x, y);

            EnterTile(x, y);//タイル進入時演出
            ClearFog(x, y, false);
        }
    }

    /// <summary>
    /// マップ移動の際にタイルに侵入した際の処理を行う。
    /// タイルごとの画像を一瞬表示し、マップタイルごとの処理に移行。
    /// </summary>
    /// <param name="x">x座標</param>
    /// <param name="y">y座標</param>
    private void EnterTile(int x,int y)
    {
        MapTileDefine.MapTile tile=dungeonMap[x,y];//移動した先のタイル
        if (tile == MapTileDefine.MapTile.Road || tile == MapTileDefine.MapTile.Start)
        {
            //道なので何もしない
        }
        else
        {
            //道でない場合、元気を消費する
            dungeonManager.ConsumeStamina(1);
            switch (tile)
            {
                case MapTileDefine.MapTile.Enemy:
                    //敵との戦闘が開始
                    //即時
                    dungeonUI.EnteringTileEffectInstant(tile, () => { dungeonManager.StartBattle(EncountTypeDefine.EnecountType.Normal); });
                    //現在の場所を道タイルに変更
                    dungeonMap[x, y] = MapTileDefine.MapTile.Road;
                    dungeonUI.UpdateTile(x, y);//UIに反映
                    break;
                case MapTileDefine.MapTile.Traveler:
                    //TODO:冒険者との会話が開始。会話次第で戦闘
                    //イベント

                    //今は即座に戦闘
                    dungeonUI.EnteringTileEffectInstant(tile, () => { dungeonManager.StartBattle(EncountTypeDefine.EnecountType.Traveler); }); 
                    dungeonMap[x, y]= MapTileDefine.MapTile.Road;
                    dungeonUI.UpdateTile(x, y);
                    break;
                case MapTileDefine.MapTile.Treasure:
                    //宝箱の敵との戦闘が開始
                    //即時
                    dungeonUI.EnteringTileEffectInstant(tile, () => { dungeonManager.StartBattle(EncountTypeDefine.EnecountType.Treasure); });
                    //現在の場所を道タイルに変更
                    dungeonMap[x, y] = MapTileDefine.MapTile.Road;
                    dungeonUI.UpdateTile(x, y);//UIに反映
                    break;
                case MapTileDefine.MapTile.Tower:
                    //塔エフェクトが発生し、霧を大きくはらす
                    //即時
                    dungeonUI.EnteringTileEffectInstant(tile, () => { ClearFog(x, y, true); });//霧を大きくはらす
                    //現在の場所を道タイルに変更
                    dungeonMap[x, y] = MapTileDefine.MapTile.Road;
                    dungeonUI.UpdateTile(x, y);//UIに反映
                    break;
                case MapTileDefine.MapTile.Lake:
                    //湖イベントが発生
                    //イベント
                    break;
                case MapTileDefine.MapTile.Stair:
                    //下の階に移動するかを聞かれる
                    //イベント
                    break;
                case MapTileDefine.MapTile.Door:
                    //TODO:脱出するかを聞かれる

                    //今は瞬時に移動
                    dungeonUI.EnteringTileEffectInstant(tile, () => {
                        dungeonSceneManager.GoVillage();
                    });

                    //イベント
                    break;
                case MapTileDefine.MapTile.Boss:
                    //TODO:ボス戦に挑むかを聞かれ、NOにするなら戻される
                    //TODO:イベント

                    //ボス戦を開始し、演出の後ボスバトル画面
                    dungeonUI.EnteringBossEffect(() => dungeonManager.StartBattle(EncountTypeDefine.EnecountType.Boss));

                    //現在の場所を道タイルに変更
                    dungeonMap[x, y] = MapTileDefine.MapTile.Road;
                    dungeonUI.UpdateTile(x, y);//UIに反映
                    break;
            }
        }
    }

    #endregion

    #region test
    /// <summary>
    /// マップタイル情報をログテキストにダンプ
    /// </summary>
    private void DisplayDungeonMap()
    {
        string logText = "";
        for (int i = 0; i < dungeonMap.GetLength(0); i++)
        {
            for (int j = 0; j < dungeonMap.GetLength(1); j++)
            {
                logText+=dungeonMap[i,j].ToString();
                logText += ":";
            }
            logText += "\n";
        }
        Debug.Log(logText);
    }
    #endregion
}
