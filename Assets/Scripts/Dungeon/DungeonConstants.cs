using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 各種の演出で使用する一定の定数を管理するクラス(Dungeon)
/// </summary>
[System.Serializable]
public class DungeonConstants : MonoBehaviour
{
    //ゲーム設定
    public const int StaminaMax = 10;//スタミナの最大値

    //マップ生成
    public const float MapTileDistance = 20.0f;//マップタイルの間隔
    public const float MapTileSize = 0.8f;//マップタイルのサイズ
    public const float MapTileWidth = 80.0f;//マップタイルの横幅
    public const float MapTileHeight = 80.0f;//マップタイルの縦幅
    public const int MapX=9;//マップの縦横のタイル数
    public const int MapY=5;//マップの縦横のタイル数
    //壁あるいは道の形状変化
    public const float WallSizeX = 0.5f;//壁になるときの形状
    public const float WallSizeY = 1.3f;
    public const float RoadSizeX = 0.8f;//道になるときの形状
    public const float RoadSizeY = 0.3f;

    //タイル進入時効果
    public const float TileEnteringImageAppearTime = 0.4f;//タイル進入時、画像の出現時間
    public const float TileEnteringTextAppearTime = 0.6f;//テキストの出現時間.画像よりやや遅らせる
    public const float TileEnteringWaitTime = 0.1f;//演出が出現した後待つ時間
    public const float TileEnteringImageDisappearTime = 0.2f;//画像の消滅時間
    public const float TileEnteringTextDisappearTime = 0.4f;//テキストの消滅時間

    //ボス侵入演出
    public const float BossEnteringPieceMoveTime0 = 0.4f;//ピースの動く時間(フェーズ0)
    public const float BossEnteringPieceMoveTime1 = 0.2f;//ピースの動く時間(フェーズ1)
    public const float BossEnteringPieceWaitTime0 = 0.2f;//ピースの移動の間の間隔(フェーズ0)
    public const float BossEnteringPieceWaitTime1 = 0.1f;//ピースの移動の間の間隔(フェーズ1)
    public const float BossEnteringPhaseTime = 0.5f;//フェーズの間の間隔
    public const float BossEnteringCloseTime = 0.5f;//最後に扉が開閉する時間
    public const float BossEnteringCloseWaitTime = 0.3f;//最後に扉が閉まった後開くまでの時間
    public const float BossEnteringCloseX = 550f;//どれだけ扉が開いて閉じるか
}
