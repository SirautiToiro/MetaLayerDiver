using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 物理アイテム操作画面で右クリックすると表示されるメニューのボタンの一つ
/// </summary>
public class MiniMenuTip : MonoBehaviour
{
    [SerializeField] Button button; //このボタンのButtonコンポーネント
    [SerializeField] TextMeshProUGUI text; //このボタンのButtonコンポーネント

    private MiniMenu miniMenu; //このメニューの親となるMiniMenu

    public void Init(MiniMenu miniMenu, MiniMenuTipDefine.MiniMenuTipType type)
    {
        this.miniMenu = miniMenu;

        switch (type)
        {
            case MiniMenuTipDefine.MiniMenuTipType.Description:
                button.onClick.AddListener(miniMenu.DescribeItem);
                text.text = "詳細";
                break;
            case MiniMenuTipDefine.MiniMenuTipType.Divide:
                button.onClick.AddListener(miniMenu.DivideItem);
                text.text = "分割";
                break;
            case MiniMenuTipDefine.MiniMenuTipType.Use:
                button.onClick.AddListener(miniMenu.UseItem);
                text.text = "使用";
                break;
        }
    }
}
