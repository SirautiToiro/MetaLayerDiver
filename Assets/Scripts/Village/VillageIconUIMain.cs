using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

/// <summary>
/// 村が面での選択可能なボタンを管理するクラス
/// メイン。Subもこれに連動して、同じ処理を行う。
/// </summary>
public class VillageIconUIMain : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] List<VillageIconUISub> includedIcons;//同時に動くオブジェクト

    public bool enteringFlag;

    public void OnPointerEnter(PointerEventData eventData)
    {
        enteringFlag = true;

        SetSize();
    }

    /// <summary>
    /// 自身からカーソルが離れた場合
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        enteringFlag = false;

        SetSize();
    }

    /// <summary>
    /// 子オブジェクトから、カーソルが離れたことを伝えられた場合
    /// </summary>
    /// <param name="eventData"></param>
    public void OnChildrenPointerExit(PointerEventData eventData)
    {
        SetSize();
    }

    private void SetSize()
    {
        //カーソルが重なっているかどうかで大きさを変える
        if (IsEntering())
        {
            Vector3 scale = new Vector3(1, 1, 1);
            scale *= VillageConstants.IconUIEnlarge;
            this.gameObject.transform.localScale = scale;

            foreach (var icon in includedIcons)
            {
                //iconがこのオブジェクトの子であるかを判定
                //直接の親子関係しか判定できていない
                if(icon.transform.parent != this.transform)
                {//直接の子でないなら、その大きさも変える
                   icon.transform.localScale = scale;
                }
            }
        }
        else
        {
            this.gameObject.transform.localScale = new Vector3(1, 1, 1);

            foreach (var icon in includedIcons)
            {
                //iconがこのオブジェクトの子であるかを判定
                //直接の親子関係しか判定できていない
                if (icon.transform.parent != this.transform)
                {//直接の子でないなら、その大きさも変える
                    icon.transform.localScale = new Vector3(1, 1, 1);
                }
            }
        }
    }

    private bool IsEntering()
    {
        if(enteringFlag)
        {
            return true;
        }

        foreach (var icon in includedIcons)
        {
            if (icon.enteringFlag)
            {
                return true;
            }
        }

        return false;
    }

    // Start is called before the first frame update
    void Start()
    {
        enteringFlag = false;
        foreach (var obj in includedIcons)
        {
            obj.SetMain(this);
        }
    }
}
