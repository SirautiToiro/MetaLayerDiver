using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemGetTypeDefine : MonoBehaviour
{
    //アイテム(カード、物理アイテム)の入手経路の定義
    public enum ItemGetType
    {
        Battle, //戦闘で入手
        Shop,   //店で購入
        Other,  //特殊
    }
}
