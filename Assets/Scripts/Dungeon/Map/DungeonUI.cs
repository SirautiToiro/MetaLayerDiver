using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;

public class DungeonUI : MonoBehaviour
{
    [SerializeField] private GameObject mapTilePrefab;
    [SerializeField] private GameObject wallPrefab;//壁用
    [SerializeField] private GameObject playerPiecePrefab;//プレイヤーの駒

    [SerializeField] private Transform mapTileParent;//マップタイルがインスタンス化されるときの親
    [SerializeField] private Transform mapWallParent;//マップの壁の親
    [SerializeField] private Transform pieceParent;//プレイヤーの駒の親

    //各種マップタイルの画像
    [SerializeField] private Sprite roadIconSprite;
    [SerializeField] private Sprite unknownIconSprite;
    [SerializeField] private Sprite enemyIconSprite;
    [SerializeField] private Sprite travelerIconSprite;
    [SerializeField] private Sprite treasureIconSprite;
    [SerializeField] private Sprite towerIconSprite;
    [SerializeField] private Sprite lakeIconSprite;
    [SerializeField] private Sprite doorIconSprite;
    [SerializeField] private Sprite stairIconSprite;
    [SerializeField] private Sprite bossIconSprite;

    //各種マップ進入時演出の画像
    [SerializeField] private Sprite enemyImageSprite;
    [SerializeField] private Sprite travelerImageSprite;
    [SerializeField] private Sprite treasureImageSprite;
    [SerializeField] private Sprite towerImageSprite;
    [SerializeField] private Sprite lakeImageSprite;
    [SerializeField] private Sprite doorImageSprite;
    [SerializeField] private Sprite stairImageSprite;
    [SerializeField] private Sprite bossImageSprite;

    //マップ進入時演出に使用するGameObject
    [SerializeField] private Image tileEnteringImage;
    [SerializeField] private TextMeshProUGUI tileEnteringText;

    [SerializeField] private Dungeon dungeon;//ダンジョン本体のスクリプト
    [SerializeField] private InputBlocker inputBlocker;
    [SerializeField] BossEntering bossEntering;

    //表示しているマップタイルを格納
    private DungeonTile[,] dungeonTiles;
    //表示している壁を格納
    private GameObject[,] wallObjectsHorizontal;
    private GameObject[,] wallObjectsVertical;

    //プレイヤーの位置を表示するコマを格納
    private GameObject playerPiece;

    //マップ生成に使用する値
    //マップ幅
    float mapWidth;
    float mapHeight;
    //マップの左上ローカル座標(ここを原点とする)
    float mapTileOrginX;
    float mapTileOrginY;
    //ダンジョンのマス数
    int dungeonLengthX;
    int dungeonLengthY;

    /// <summary>
    /// マップの初期化処理を行う(表示)
    /// </summary>
    /// <param name="dungeonMap">表示するダンジョンのマップ情報</param>
    /// <param name="dungeonMapDisplayFlags">ダンジョンマップの表示状態情報</param>
    /// <param name="wallsHorizontal">表示するダンジョンの横壁</param>
    /// <param name="wallsVertical">表示するダンジョンの縦壁</param>
    /// <param name="playerX">プレイヤーのX座標</param>
    /// <param name="playerY">プレイヤーのY座標</param>
    public void Init(MapTileDefine.MapTile[,] dungeonMap, bool[,] dungeonMapDisplayFlags, MapTileDefine.MapWall[,] wallsHorizontal, MapTileDefine.MapWall[,] wallsVertical,int playerX,int playerY)
    {
        //マップ進入時効果を非表示
        tileEnteringImage.gameObject.SetActive(false);
        tileEnteringText.gameObject.SetActive(false);

        ShowMapInit(dungeonMap, dungeonMapDisplayFlags, wallsHorizontal, wallsVertical);
        SetPiece(playerX, playerY);

        //ボス演出の初期化
        bossEntering.SetActive(false);
        bossEntering.PosReset();
    }

    #region マップ表示の生成
    /// <summary>
    /// ダンジョンのマップ情報を受け取ってそれのUIを表示する
    /// タイルの生成を行っているので最初の一回のみ
    /// </summary>
    /// <param name="dungeonMap">表示するダンジョンのマップ情報</param>
    /// <param name="dungeonMapDisplayFlags">ダンジョンマップの表示状態情報</param>
    /// /// <param name="wallsHorizontal">表示するダンジョンの横壁</param>
    /// <param name="wallsVertical">表示するダンジョンの縦壁</param>
    private void ShowMapInit(MapTileDefine.MapTile[,] dungeonMap, bool[,] dungeonMapDisplayFlags, MapTileDefine.MapWall[,] wallsHorizontal, MapTileDefine.MapWall[,] wallsVertical)
    {
        int x = dungeonMap.GetLength(0);
        int y = dungeonMap.GetLength(1);

        dungeonLengthX = x;
        dungeonLengthY = y;

        dungeonTiles = new DungeonTile[x, y];

        mapWidth = DungeonConstants.MapTileWidth * x + DungeonConstants.MapTileDistance * (x - 1);
        mapHeight = DungeonConstants.MapTileHeight * y + DungeonConstants.MapTileDistance * (y - 1);

        //マップの左上ローカル座標(ここを原点とする)
        mapTileOrginX = -0.5f * (mapWidth - DungeonConstants.MapTileWidth);
        mapTileOrginY = -0.5f * (mapHeight + DungeonConstants.MapTileHeight) - DungeonConstants.MapTileDistance;



        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                //それぞれのタイルの位置
                Vector3 mapTilePos=GetMaptilePos(i, j);
                GameObject mapTile = Instantiate(mapTilePrefab,
                    mapTileParent.position,
                    Quaternion.identity, mapTileParent);

                RectTransform rectT = mapTile.transform as RectTransform;
                //Scaleを変更
                Vector3 scale = rectT.localScale;
                scale.x = DungeonConstants.MapTileSize;
                scale.y = DungeonConstants.MapTileSize;
                rectT.localScale = scale;

                //位置を変更
                rectT.localPosition = mapTilePos;

                //タイル用のスクリプトを取得
                DungeonTile dungeonTile=mapTile.GetComponent<DungeonTile>();
                dungeonTiles[i, j] = dungeonTile;//配列に生成したタイルを格納

                Sprite tileSprite=unknownIconSprite;//ここに、DungeonTileにセットする画像を用意
                if (dungeonMapDisplayFlags[i, j])
                {//表示するタイルなら
                 //対応するタイルを表示
                    tileSprite = MaptileToSprite(dungeonMap[i, j]);
                }
                else
                {//非表示のタイルなら
                    tileSprite = unknownIconSprite;
                }

                dungeonTile.Init(tileSprite, i, j,dungeon);
            }
        }

        //壁の生成(横壁)
        x = wallsHorizontal.GetLength(0);
        y = wallsHorizontal.GetLength(1);
        wallObjectsHorizontal = new GameObject[x, y];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                //それぞれの壁の位置
                Vector3 mapTilePos = new Vector3(mapTileOrginX + (DungeonConstants.MapTileWidth + DungeonConstants.MapTileDistance) * i,
                    mapTileOrginY + (DungeonConstants.MapTileHeight + DungeonConstants.MapTileDistance) * (y - j+0.5f)
                    , 0);
                GameObject wall = Instantiate(wallPrefab,
                    mapWallParent.position,
                    Quaternion.identity, mapWallParent);

                RectTransform rectT = wall.transform as RectTransform;
                if (wallsHorizontal[i, j]== MapTileDefine.MapWall.WallShow||
                    wallsHorizontal[i, j] == MapTileDefine.MapWall.WallHide)
                {//壁なら
                 //Scaleを変更
                    Vector3 scale = rectT.localScale;
                    scale.x = DungeonConstants.WallSizeX;
                    scale.y = DungeonConstants.WallSizeY;
                    rectT.localScale = scale;

                    //回転
                    rectT.Rotate(0, 0, 90);
                }
                else
                {//道なら
                    Vector3 scale = rectT.localScale;
                    scale.x = DungeonConstants.RoadSizeX;
                    scale.y = DungeonConstants.RoadSizeY;
                    rectT.localScale = scale;
                }

                //位置を変更
                Vector3 pos = rectT.localPosition;
                Vector3 hoseidPos = pos + mapTilePos;
                rectT.localPosition = hoseidPos;

                if(wallsHorizontal[i, j] == MapTileDefine.MapWall.RoadHide ||
                    wallsHorizontal[i, j] == MapTileDefine.MapWall.WallHide)
                {//隠すものなら非表示
                    wall.SetActive(false);
                }

                //配列に格納
                wallObjectsHorizontal[i, j] = wall;
            }
        }
        //壁の生成(縦壁)
        x = wallsVertical.GetLength(0);
        y = wallsVertical.GetLength(1);
        wallObjectsVertical = new GameObject[x, y];
        for (int i = 0; i < x; i++)
        {
            for (int j = 0; j < y; j++)
            {
                //それぞれの壁の位置
                Vector3 mapTilePos = new Vector3(mapTileOrginX + (DungeonConstants.MapTileWidth + DungeonConstants.MapTileDistance) * (i+0.5f),
                    mapTileOrginY + (DungeonConstants.MapTileHeight + DungeonConstants.MapTileDistance) * (y - j)
                    , 0);
                GameObject wall = Instantiate(wallPrefab,
                    mapWallParent.position,
                    Quaternion.identity, mapWallParent);

                RectTransform rectT = wall.transform as RectTransform;
                if (wallsVertical[i, j] == MapTileDefine.MapWall.WallShow ||
                    wallsVertical[i, j] == MapTileDefine.MapWall.WallHide)
                {//壁なら
                 //Scaleを変更
                    Vector3 scale = rectT.localScale;
                    scale.x = DungeonConstants.WallSizeX;
                    scale.y = DungeonConstants.WallSizeY;
                    rectT.localScale = scale;
                }
                else
                {//道なら
                    Vector3 scale = rectT.localScale;
                    scale.x = DungeonConstants.RoadSizeX;
                    scale.y = DungeonConstants.RoadSizeY;
                    rectT.localScale = scale;

                    //回転
                    rectT.Rotate(0, 0, 90);
                }

                //位置を変更
                Vector3 pos = rectT.localPosition;
                Vector3 hoseidPos = pos + mapTilePos;
                rectT.localPosition = hoseidPos;

                if (wallsVertical[i, j] == MapTileDefine.MapWall.RoadHide ||
                    wallsVertical[i, j] == MapTileDefine.MapWall.WallHide)
                {//隠すものなら非表示
                    wall.SetActive(false);
                }

                //配列に格納
                wallObjectsVertical[i, j] = wall;
            }
        }
    }

    /// <summary>
    /// プレイヤーの駒を配置する
    /// </summary>
    /// <param name="x">x座標</param>
    /// <param name="y">y座標</param>
    private void SetPiece(int x,int y)
    {
        Vector3 mapTilePos = GetMaptilePos(x,y);
        Vector3 scale = Vector3.zero;
        scale.x = DungeonConstants.MapTileSize;
        scale.y = DungeonConstants.MapTileSize;
        //プレイヤーの駒を配置
        playerPiece = Instantiate(playerPiecePrefab, pieceParent.position,
                Quaternion.identity, pieceParent);
        RectTransform pieceRectT = playerPiece.transform as RectTransform;
        //サイズ変更
        pieceRectT.localScale = scale;
        //位置変更
        pieceRectT.localPosition = mapTilePos;
    }
    #endregion

    #region マップ表示変更
    /// <summary>
    /// 座標の位置にプレイヤーの駒を移動させる
    /// </summary>
    /// <param name="x">移動先x座標</param>
    /// <param name="y">移動先y座標</param>
    public void MovePiece(int x,int y)
    {
        Vector3 mapTilePos = GetMaptilePos(x, y);//移動先の位置
        //駒のrectTransform
        RectTransform pieceRectT = playerPiece.transform as RectTransform;
        //位置変更
        pieceRectT.localPosition = mapTilePos;
    }

    /// <summary>
    /// (x,y)の位置にある霧をbの状態にする
    /// b==tureならtileを表示する
    /// </summary>
    /// <param name="x">霧を変更するx座標</param>
    /// <param name="y">霧を変更するy座標</param>
    /// <param name="b">falseなら霧の状態、trueなら本来のタイルが表示されている</param>
    public void UpdateFog(int x,int y,bool b)
    {
        if (b)
        {//霧を晴らすなら
            //対応する画像を表示
            MapTileDefine.MapTile mapTile=dungeon.GetMapTile(x,y);
            dungeonTiles[x, y].ChangeSprite(MaptileToSprite(mapTile));
        }
        else
        {//霧を作るなら
            dungeonTiles[x, y].ChangeSprite(unknownIconSprite);
        }
    }

    /// <summary>
    /// (x,y)の位置のタイル情報を取得して、UIを更新
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void UpdateTile(int x,int y)
    {
        //タイルの情報を取得
        MapTileDefine.MapTile mapTile = dungeon.GetMapTile(x, y);
        //対応する画像を表示
        dungeonTiles[x, y].ChangeSprite(MaptileToSprite(mapTile));
    }

    /// <summary>
    /// 道・壁の状態を更新する
    /// </summary>
    /// <param name="x">道の始点のx座標</param>
    /// <param name="y">道の始点のy座標</param>
    /// <param name="nextX">道の終点のx座標</param>
    /// <param name="nextY">道の終点のy座標</param>
    /// <param name="isDisplay">trueなら表示状態にする</param>
    public void UpdateWall(int x, int y, int nextX, int nextY, bool isDisplay)
    {
        //方向
        int dirX = nextX - x;
        int dirY = nextY - y;

        GameObject targetObject = null;

        //方向を確認
        if (dirX == 0)
        {//縦方向
            if (dirY == 1)
            {
                targetObject = wallObjectsHorizontal[x, y];
            }
            else if (dirY == -1)
            {
                targetObject = wallObjectsHorizontal[x, y - 1];
            }
        }
        else if (dirY == 0)
        {//横方向
            if (dirX == 1)
            {
                targetObject = wallObjectsVertical[x, y] ;
            }
            else if (dirX == -1)
            {
                targetObject = wallObjectsVertical[x - 1, y];
            }
        }

        //表示を更新
        targetObject.SetActive(isDisplay);
    }

    /// <summary>
    /// 壁、道の状態を更新する
    /// 座標に直接更新
    /// 配列の範囲内に存在することは確認されているものとする
    /// </summary>
    /// <param name="x">変更先のx座標</param>
    /// <param name="y">変更先のy座標<</param>
    /// <param name="isDisplay">trueなら表示状態にする</param>
    /// <param name="isHorizontal">横方向の壁を変更するか。trueならwallObjectsHorizontalを更新.</param>
    public void UpdateWall(int x,int y,bool isDisplay,bool isHorizontal)
    {
        if (isHorizontal)
        {
            wallObjectsHorizontal[x,y].SetActive(isDisplay);
        }
        else
        {
            wallObjectsVertical[x, y].SetActive(isDisplay);
        }
    }
    #endregion

    #region マップ演出
    /// <summary>
    /// 即時発生して消えるタイル進入時のエフェクトを発生させる
    /// 演出が終わると、与えられた引数のコールバックを発生させる
    /// 演出中はその他の入力を受け付けないようにする(DungeonInputを操作)
    /// </summary>
    /// <param name="tile">侵入先のマップタイル</param>
    /// <param name="callback">マップ演出が終わった時に呼び出されるコールバック関数</param>
    public void EnteringTileEffectInstant(MapTileDefine.MapTile tile,Action callback)
    {
        inputBlocker.InputBlockingUp();//入力を一時的に止める

        Transform ImageTransform = tileEnteringImage.gameObject.transform;
        Transform TextTransform = tileEnteringText.gameObject.transform;
        //マップ進入時効果を表示
        tileEnteringImage.gameObject.SetActive(true);
        tileEnteringText.gameObject.SetActive(true);

        //画像をセット
        tileEnteringImage.sprite = MaptileToEnteringImage(tile);
        //テキストをセット
        tileEnteringText.text = MaptileToEnteringString(tile);

        Sequence sequence=DOTween.Sequence();

        //中央から広がって消える演出
        //スケールを0に
        ImageTransform.localScale = Vector3.zero;
        TextTransform.localScale = Vector3.zero;
        //透明度を0に
        Color color = tileEnteringImage.color;
        color.a = 0.0f;
        tileEnteringImage.color = color;
        color = tileEnteringText.color;
        color.a = 0.0f;
        tileEnteringText.color = color;

        sequence.Append(ImageTransform.DOScale(Vector3.one, DungeonConstants.TileEnteringImageAppearTime))
            .Join(TextTransform.DOScale(Vector3.one, DungeonConstants.TileEnteringTextAppearTime))
            .Join(tileEnteringImage.DOFade(1.0f, DungeonConstants.TileEnteringImageAppearTime))
            .Join(tileEnteringText.DOFade(1.0f, DungeonConstants.TileEnteringTextAppearTime))

            .AppendInterval(DungeonConstants.TileEnteringWaitTime)//一瞬待つ

            //サイズを上げて消滅
            .Append(ImageTransform.DOScale(new Vector3(2.0f, 2.0f, 2.0f), DungeonConstants.TileEnteringImageDisappearTime))
            .Join(TextTransform.DOScale(new Vector3(2.0f, 2.0f, 2.0f), DungeonConstants.TileEnteringTextDisappearTime))
            .Join(tileEnteringImage.DOFade(0.0f, DungeonConstants.TileEnteringImageDisappearTime))
            .Join(tileEnteringText.DOFade(0.0f, DungeonConstants.TileEnteringTextDisappearTime))
            .SetLink(this.gameObject)
            .AppendCallback(() =>
            {
                //非表示に
                tileEnteringImage.gameObject.SetActive(false);
                tileEnteringText.gameObject.SetActive(false);

                //コールバック処理を実行
                callback.Invoke();

                inputBlocker.InputBlockingDown();//入力の差し止め終了
            });
    }

    public void EnteringBossEffect(Action callback)
    {
        inputBlocker.InputBlockingUp();//入力を一時的に止める
        //演出を表示
        bossEntering.SetActive(true);

        //ボス侵入演出を実行させ、終了時のコールバックに
        //与えられたコールバックと、加えてボス侵入演出のリセットを加える。
        bossEntering.PlayEntering(() =>
        {
            callback.Invoke();

        }, () =>
        {
            bossEntering.SetActive(false);
            bossEntering.PosReset();
            inputBlocker.InputBlockingDown();//入力の差し止め終了
        });
    }
    #endregion

    #region 情報取得
    /// <summary>
    /// 座標から実際に表示するときの画面座標を取得する
    /// </summary>
    /// <param name="x">x座標</param>
    /// <param name="y">y座標</param>
    /// <returns>画面上での座標</returns>
    private Vector3 GetMaptilePos(int x,int y)
    {
        return new Vector3(mapTileOrginX + (DungeonConstants.MapTileWidth + DungeonConstants.MapTileDistance) * x,
                    mapTileOrginY + (DungeonConstants.MapTileHeight + DungeonConstants.MapTileDistance) * (dungeonLengthY - y)
                    , 0);
    }

    /// <summary>
    /// MapTileを受け取り、それに対応するタイル画像を返す
    /// </summary>
    /// <param name="mapTile">MapTileDefine.MapTile</param>
    /// <returns>対応する画像</returns>
    private Sprite MaptileToSprite(MapTileDefine.MapTile mapTile)
    {
        Sprite tileSprite = unknownIconSprite;//初期化.ここに格納
        switch (mapTile)
        {
            case MapTileDefine.MapTile.Road:
                tileSprite = roadIconSprite;
                break;
            case MapTileDefine.MapTile.Enemy:
                tileSprite = enemyIconSprite;
                break;
            case MapTileDefine.MapTile.Traveler:
                tileSprite = travelerIconSprite;
                break;
            case MapTileDefine.MapTile.Treasure:
                tileSprite = treasureIconSprite;
                break;
            case MapTileDefine.MapTile.Tower:
                tileSprite = towerIconSprite;
                break;
            case MapTileDefine.MapTile.Lake:
                tileSprite = lakeIconSprite;
                break;
            case MapTileDefine.MapTile.Door:
                tileSprite = doorIconSprite;
                break;
            case MapTileDefine.MapTile.Stair:
                tileSprite = stairIconSprite;
                break;
            case MapTileDefine.MapTile.Start:
                tileSprite = roadIconSprite;
                break;
            case MapTileDefine.MapTile.Boss:
                tileSprite = bossIconSprite;
                break;
        }

        return tileSprite;
    }

    /// <summary>
    /// MapTileを受け取り、それに対応するタイル進入時の画像を返す
    /// </summary>
    /// <param name="mapTile">MapTileDefine.MapTile</param>
    /// <returns>対応するタイル進入時画像</returns>
    private Sprite MaptileToEnteringImage(MapTileDefine.MapTile mapTile)
    {
        Sprite imageSprite = unknownIconSprite;//初期化.ここに格納
        switch (mapTile)
        {
            case MapTileDefine.MapTile.Enemy:
                imageSprite = enemyImageSprite;
                break;
            case MapTileDefine.MapTile.Traveler:
                imageSprite = travelerImageSprite;
                break;
            case MapTileDefine.MapTile.Treasure:
                imageSprite = treasureImageSprite;
                break;
            case MapTileDefine.MapTile.Tower:
                imageSprite = towerImageSprite;
                break;
            case MapTileDefine.MapTile.Lake:
                imageSprite = lakeImageSprite;
                break;
            case MapTileDefine.MapTile.Door:
                imageSprite = doorImageSprite;
                break;
            case MapTileDefine.MapTile.Stair:
                imageSprite = stairImageSprite;
                break;
            case MapTileDefine.MapTile.Boss:
                imageSprite = bossImageSprite;
                break;
        }

        return imageSprite;
    }

    /// <summary>
    /// MapTileを受け取り、それに対応するタイル進入時のテキストを返す
    /// </summary>
    /// <param name="mapTile">MapTileDefine.MapTile</param>
    /// <returns>対応するタイル進入時テキスト</returns>
    private string MaptileToEnteringString(MapTileDefine.MapTile mapTile)
    {
        string str = "ERROR";//初期化.ここに格納
        switch (mapTile)
        {
            case MapTileDefine.MapTile.Enemy:
                str = "Enemy";
                break;
            case MapTileDefine.MapTile.Traveler:
                str = "冒険者";
                break;
            case MapTileDefine.MapTile.Treasure:
                str = "Treasure";
                break;
            case MapTileDefine.MapTile.Tower:
                str = "Tower";
                break;
            case MapTileDefine.MapTile.Lake:
                str = "湖";
                break;
            case MapTileDefine.MapTile.Door:
                str = "脱出";
                break;
            case MapTileDefine.MapTile.Stair:
                str = "地下への階段";
                break;
            case MapTileDefine.MapTile.Boss:
                str = "Boss";
                break;
        }
        return str;
    }
    #endregion
}
