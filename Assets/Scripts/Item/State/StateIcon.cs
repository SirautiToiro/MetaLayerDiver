using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

public class StateIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image stateImage;
    [SerializeField] private GameObject stateValueObject;
    [SerializeField] private TextMeshProUGUI stateValue;

    public IState stateInstance;

    [SerializeField] private float pointerEnteringSize;//ポインターが重なっている時に大きさを大きくする値

    [SerializeField] private StateQuickDescription stateQuickDescription;

    /// <summary>
    /// 初期化処理
    /// アイコンと値を設定
    /// </summary>
    public void Init(IState state)
    {
        StateDefine.StateNew stateData = StateData.GetState(state);

        stateImage.sprite = stateData.IconSprite;
        //stateValue.text = value.ToString();
        
        stateInstance = state;

        stateQuickDescription.SetText(stateData);

        SetValue(state);
    }

    /// <summary>
    /// Stateの値を更新
    /// </summary>
    /// <param name="value">Stateの値</param>
    private void SetValue(int value)
    {
        stateValue.text = value.ToString();
    }

    public void SetValue(IState state)
    {
        SetValue(state.value);

        if (state is IStateHasCount cState)
        {
            //カウントするステータスなら
            //カウントを表示する
            //stateValue.text = cState.IStateCounter.GetCount().ToString();
            SetValue(cState.IStateCounter.GetCount());
            stateValueObject.SetActive(true);
        }
        else
        {
            if (state is StateContinueTypeEternalBase)
            {//永久のステータスなら(値を持たない)
                stateValueObject.SetActive(false);
            }
            else
            {
                stateValueObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Stateにマウスが重なった場合
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (stateQuickDescription == null)
        {
        }
        else
        {
            //詳細説明を見せる
            stateQuickDescription.DescriptionOn();
            //拡大
            Vector3 scale = this.gameObject.transform.localScale;
            scale.x = pointerEnteringSize;
            scale.y = pointerEnteringSize;
            this.gameObject.transform.localScale = scale;
        }
        
    }

    /// <summary>
    /// Stateからマウスが離れた場合
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (stateQuickDescription == null)
        {
        }
        else
        {
            //詳細説明をオフ
            stateQuickDescription.DescriptionOff();
            //拡大終了
            Vector3 scale = this.gameObject.transform.localScale;
            scale.x = 1.0f;
            scale.y = 1.0f;
            this.gameObject.transform.localScale = scale;
        }
            
    }
}
