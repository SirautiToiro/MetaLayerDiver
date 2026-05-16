using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IButtonWithHighlightManager
{
    /// <summary>
    ///ハイライト付きボタンがクリックされた時、必ずこれが呼ばれる
    ///おおむね、他のハイライト付きボタンを消す処理
    /// </summary>
    /// <param name="buttonWithHighlight">クリックされたボタン</param>
    public void OnButtonWithHighlightClicked(ButtonWithHighlight buttonWithHighlight);
}
