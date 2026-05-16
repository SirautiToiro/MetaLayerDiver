using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

//カーソルを重ねた時に、textObjectをオンオフする。文章も設定する。
public class CursorEnterMessage : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private GameObject textObject;
    [SerializeField] private TextMeshProUGUI messageText;
    [Multiline(7)]
    [SerializeField] private string message;

    [SerializeField] private LayoutGroup layoutGroup;
    [SerializeField] private ContentSizeFitter contentSizeFitter;

    public void Start()
    {
        messageText.text = message;
        textObject.SetActive(false);

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

    public void OnPointerExit(PointerEventData eventData)
    {
        textObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        textObject.SetActive(true);
    }
}
