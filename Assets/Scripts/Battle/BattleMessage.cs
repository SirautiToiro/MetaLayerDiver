using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class BattleMessage : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerMessage;
    [SerializeField] Canvas playerMessageCanvas;
    [SerializeField] TextMeshProUGUI[] enemyMessages;
    [SerializeField] Canvas[] enemyMessageCanvas;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        ClearAllMessage();
    }

    /// <summary>
    /// バトル中メッセージを全てなくす
    /// </summary>
    public void ClearAllMessage()
    {
        //messageを無しにする
        playerMessage.text = "";
        foreach (var message in enemyMessages)
        {
            message.text = "";
        }
        //UI要素を非表示
        playerMessageCanvas.enabled = false;
        foreach (var message in enemyMessages)
        {
            message.enabled = false;
        }
    }

    /// <summary>
    /// プレイヤーのメッセージを表示する
    /// </summary>
    /// <param name="message">表示するメッセージ</param>
    public void PlayerMessage(string message)
    {
        playerMessage.text=message;
        playerMessageCanvas.enabled = true;
    }
}
