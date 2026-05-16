using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StateQuickDescription : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Canvas canvas;
    [SerializeField] LayoutGroup layoutGroup;
    [SerializeField] ContentSizeFitter contentSizeFitter;

    /// <summary>
    /// 詳細のテキストをセットする
    /// 非表示にする
    /// </summary>
    /// <param name="stateData">状態異常の情報</param>
    public void SetText(StateDefine.StateNew stateData)
    {
        //状態異常の情報を文字列に変換してセット.名前も追加。
        descriptionText.text = "<color=#AF4141>" + stateData.Name + "</color> : " + stateData.Description;
        canvas.enabled = false;

        //レイアウトグループが更新されないので手動更新
        layoutGroup.CalculateLayoutInputHorizontal();
        layoutGroup.CalculateLayoutInputVertical();
        layoutGroup.SetLayoutHorizontal();
        layoutGroup.SetLayoutVertical();
        //contentSizeFitterも更新
        //縦と横のサイズ計算
        contentSizeFitter.SetLayoutHorizontal();
        contentSizeFitter.SetLayoutVertical();
        //レイアウトを即時更新
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentSizeFitter.GetComponent<RectTransform>());
    }

    /// <summary>
    /// Stateのクイック詳細を表示する
    /// </summary>
    public void DescriptionOn()
    {
        canvas.enabled = true;
    }

    /// <summary>
    /// Stateのクイック詳細を閉じる
    /// </summary>
    public void DescriptionOff()
    {
        canvas.enabled = false;
    }
}
