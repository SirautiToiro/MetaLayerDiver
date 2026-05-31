using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DungeonTile : MonoBehaviour
{
    [SerializeField] Image tileImage;

    private Dungeon dungeon;

    //マップタイルの座標
    private int x;
    private int y;

    /// <summary>
    /// マップタイルの初期化
    /// </summary>
    /// <param name="sprite">マップタイルにセットされる画像</param>
    /// <param name="_x">マップタイルのx座標</param>
    /// <param name="_y">マップタイルのy座標</param>
    /// <param name="_dungeon">ダンジョン本体のスクリプト</param>
    public void Init(Sprite sprite,int _x,int _y,Dungeon _dungeon)
    {
        x = _x;
        y = _y;
        tileImage.sprite = sprite;
        dungeon = _dungeon;
    }

    /// <summary>
    /// 表示する画像を変更する
    /// </summary>
    /// <param name="sprite">変更先の画像</param>
    public void ChangeSprite(Sprite sprite)
    {
        tileImage.sprite = sprite;
    }

    /// <summary>
    /// クリックされた時の動作
    /// ダンジョンにクリックされた座標を送信
    /// </summary>
    public void OnClick()
    {
        dungeon.Move(x, y);
    }
}
