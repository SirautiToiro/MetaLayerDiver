using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WindowManager : MonoBehaviour
{
    /// <summary>
    /// クリックを防ぐためのパネルにCanvasがついていると判定を防げないようなので、
    /// UIBlockPanelはSetActiveを使用する
    /// </summary>
    [SerializeField] GameObject UIBlockPanel;
    [SerializeField] Canvas messageWindow;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] Button okButton;
    [SerializeField] TextMeshProUGUI okButtonText;

    [SerializeField] DungeonManager dungeonManager;
    [SerializeField] DungeonSceneManager dungeonSceneManager;

    /// <summary>
    /// 初期化
    /// </summary>
    public void Init()
    {
        //UI要素を非表示
        UIBlockPanel.SetActive(false);
        messageWindow.enabled = false;
    }

    /// <summary>
    /// Battle中、プレイヤーが敗北したときのメッセージを表示する。
    /// </summary>
    public void DefeatMessage()
    {
        titleText.text = "敗北……";
        okButtonText.text="街へ";
        //UI要素を表示
        UIBlockPanel.SetActive(true);
        messageWindow.enabled = true;

        //TODO:街に戻るようにする
        okButton.onClick.AddListener(() => { dungeonSceneManager.GoVillage(); });
    }
}
