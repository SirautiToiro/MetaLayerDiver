using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 重なり合う画面UIのインターフェース
/// </summary>
public interface IUIPage
{
    public UIPageManager UIPageManager { get;}

    /// <summary>
    /// 有効化時の処理
    /// </summary>
    public void OnPushed();

    /// <summary>
    /// 無効化時の処理(閉じられたとき)
    /// </summary>
    public void OnPopped();

    /// <summary>
    /// 上に被さられたとき。主にInputBlockerのInputBlockingUpを呼び出すために使用される。
    /// 基本的にしたのものは表示したままで、入力が止められる。
    /// </summary>
    public void OnCovered();

    /// <summary>
    /// 上がなくなり、トップページになったとき。主にInputBlockerのInputBlockingDownを呼び出すために使用される。
    /// </summary>
    public void OnBecomeTopPage();

    /// <summary>
    /// 自信をUIPageManagerに積む。積まれたUIPageはOnPusedを実行。
    /// </summary>
    public void PushSelf();
}

/// <summary>
/// このUIPageは必ず最初に一つのみ積まれることを前提としている。
/// </summary>
public interface IBaseUIPage:IUIPage
{

}

public interface IUIPageNeedsPopCheck:IUIPage
{
    //Trueなら必ずPopされる
    public bool PopCheckFlag { get; set; }

    /// <summary>
    /// UIPageを閉じる前に、閉じていいかの確認をする。閉じていいならtrueを返す。
    /// </summary>
    public bool CheckCanPop();

    public void PopUIPage();
}