using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// トグルなどの機能のない普通のポップアップの
/// UIの定義
/// </summary>
public class PopupMessageUI : MonoBehaviour
{
    private IPopupMessageController popupMessageController;

    [SerializeField] TextMeshProUGUI messageText;

    public void Init(IPopupMessageController _popupMessageController)
    {
        popupMessageController = _popupMessageController;
    }

    public void SetText(string message)
    {
        messageText.text = message;
    }

    /// <summary>
    /// OKのボタンが押されたことを持っている
    /// IPopupMessageControllerに受け渡す
    /// </summary>
    public void OnCancelButtonPressed()
    {
        popupMessageController.OnCancelButtonPressed();
    }

    /// <summary>
    /// OKのボタンが押されたことを持っている
    /// IPopupMessageControllerに受け渡す
    /// </summary>
    public void OnOKButtonPressed()
    {
        popupMessageController.OnOKButtonPressed();
    }
}
