using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static Enemy.ActualNextAction;
using UnityEngine.EventSystems;

public class EnemyActionIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image backGround;
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private EnemyActionIconQuickDescription quickDescription;

    private void SetSprite(Sprite sprite)
    {
        if (sprite is null) return;

        icon.sprite = sprite;
    }

    public void SetSprite(EnemyActionDefine.EnemyAction enemyAction, EnemyActionToActionIcon enemyActionToActionIcon)
    {
        //その敵の行動のタイプを取得
        EnemyActionDefine.EnemyActionType type = EnemyActionDefine.GetActionType(enemyAction);

        //画像の取得
        if (type == EnemyActionDefine.EnemyActionType.Other)
        {//Otherのタイプなら特殊画像を用意する
            SetSprite(enemyActionToActionIcon.GetActionIcon(enemyAction));
        }
        else
        {//Otherでないならタイプに応じた画像
            SetSprite(enemyActionToActionIcon.GetActionIcon(type));
        }

        //クイック説明を表示
        quickDescription.SetText(type);
    }

    public void SetActive(bool b)
    {
        backGround.gameObject.SetActive(b);
    }

    public void SetValueActive(bool b)
    {
        text.gameObject.SetActive(b);
    }

    public void SetValue(int value)
    {
        text.text = value.ToString();
    }

    /// <summary>
    /// マウスが重なった場合
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        quickDescription.DescriptionOn();
    }

    /// <summary>
    /// マウスが離れた場合
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        quickDescription.DescriptionOff();
    }
}
