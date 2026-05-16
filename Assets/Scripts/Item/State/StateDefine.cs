using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// カードや敵の効果などにより発生する状態効果を記す
/// </summary>
[System.Serializable]
public class StateDefine
{
    //状態異常を意味するクラス(IStateに対応するString、StateTypeを持つ。)
    [System.Serializable]
    public class StateNew
    {
        //対応する状態異常のクラスの型の名前
        public string stateType;

        //属性ごとの状態異常である場合に使用
        public AttributeDefine attribute;

        //状態異常の名前
        [Header("表示名")]
        public string Name;

        //状態異常の説明文
        [TextArea]
        [Header("説明文")]
        public string Description;

        [Header("アイコン")]
        public Sprite IconSprite;
    }
}
