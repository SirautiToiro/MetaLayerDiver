using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 物理アイテム操作の右クリックで出現するMiniMenuで現れる各ボタンの定義クラス
/// </summary>
public class MiniMenuTipDefine
{
    public enum MiniMenuTipType
    {
        Description, //説明
        Divide, //分割
        Use, //使用
        //Discard, //捨てるは画面外ドラッグで
    }
}
