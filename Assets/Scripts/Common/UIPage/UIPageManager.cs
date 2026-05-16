using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 重なり合うアイテム操作UIのページを管理するクラス
/// </summary>
public class UIPageManager : MonoBehaviour
{
    List<IUIPage> uiPageStack = new List<IUIPage>();

    /// <summary>
    /// 初期化。ItemArrangeManagerから呼び出される。最初のUIPageを積む。
    /// </summary>
    /// <param name="baseUIPage"></param>
    public void Init(IBaseUIPage baseUIPage)
    {
        uiPageStack = new List<IUIPage>();
        PushUIPage(baseUIPage);
    }

    /// <summary>
    /// UIPageを積む。積まれたUIPageはOnPusedを実行。
    /// </summary>
    /// <param name="uiPage">積まれるUIPage</param>
    public void PushUIPage(IUIPage uiPage)
    {
        uiPageStack.Add(uiPage);
        uiPage.OnPushed();
        if (uiPageStack.Count > 1)
        {
            uiPageStack[uiPageStack.Count - 2].OnCovered();
        }
    }

    /// <summary>
    /// Removes the top UI page from the stack and triggers its OnPopped event.
    /// </summary>
    public void PopUIPage()
    {
        if (uiPageStack.Count == 0)
        {
            return;
        }
        IUIPage topPage = uiPageStack[uiPageStack.Count - 1];

        //Popしていいかのチェックが必要な場合、確認する
        if (topPage is IUIPageNeedsPopCheck needsPopCheck)
        {
            if (!needsPopCheck.PopCheckFlag&&!needsPopCheck.CheckCanPop())
            {
                return;
            }
        }

        try
        {
            //topPageが消えているときがあるので、例外処理を入れる
            topPage.OnPopped();

            uiPageStack.RemoveAt(uiPageStack.Count - 1);

            if (uiPageStack.Count > 0)
            {
                uiPageStack[uiPageStack.Count - 1].OnBecomeTopPage();
            }
        }
        catch (UnityEngine.MissingReferenceException e)
        {
            //topPageが消えているときがある
            Debug.Log(e);
        }
    }

    public IUIPage GetTopPage()
    {
        return uiPageStack[uiPageStack.Count - 1];
    }
}
