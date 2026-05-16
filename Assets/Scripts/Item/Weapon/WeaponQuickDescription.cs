using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponQuickDescription : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI descriptionText;
    [SerializeField] Canvas canvas;
    [SerializeField] LayoutGroup layoutGroup;
    [SerializeField] ContentSizeFitter contentSizeFitter;
    [SerializeField] EffectTagToText effectTagToText;

    /// <summary>
    /// 詳細のテキストをセットする
    /// 非表示にする
    /// </summary>
    /// <param name="weapon"></param>
    public void SetText(Weapon weapon)
    {
        //効果とタグの情報を文字列に変換してセット
        descriptionText.text =  effectTagToText.ConvertToText(weapon.effects, weapon.tags); ;
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
    /// 武器のクイック詳細を表示する
    /// </summary>
    public void DescriptionOn()
    {
        canvas.enabled = true;
    }

    /// <summary>
    /// 武器のクイック詳細を閉じる
    /// </summary>
    public void DescriptionOff()
    {
        canvas.enabled = false;
    }
}
