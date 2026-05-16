using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyActionIconQuickDescription : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Canvas canvas;
    [SerializeField] LayoutGroup layoutGroup;
    [SerializeField] ContentSizeFitter contentSizeFitter;

    public void SetText(EnemyActionDefine.EnemyActionType enemyActionType)
    {
        //効果とタグの情報を文字列に変換してセット
        descriptionText.text = EnemyActionDefine.Dic_ActionDescription[enemyActionType];
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
    /// クイック詳細を表示する
    /// </summary>
    public void DescriptionOn()
    {
        canvas.enabled = true;
    }

    /// <summary>
    /// クイック詳細を閉じる
    /// </summary>
    public void DescriptionOff()
    {
        canvas.enabled = false;
    }
}
