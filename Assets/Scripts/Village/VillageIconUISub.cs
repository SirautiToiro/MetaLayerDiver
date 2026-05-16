using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 村が面での選択可能なボタンを管理するクラス
/// メイン。Subもこれに連動して、同じ処理を行う。
/// </summary>
public class VillageIconUISub : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private VillageIconUIMain main;
    public bool enteringFlag = false;

    public void SetMain(VillageIconUIMain main)
    {
        this.main = main;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        enteringFlag = true;
        //付属しているものは、メインを動かす
        main.OnPointerEnter(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        enteringFlag = false;
        //メインに、子からカーソルが離れたことを伝える
        main.OnChildrenPointerExit(eventData);
    }
}
