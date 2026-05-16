using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StateDescription : MonoBehaviour
{
    [SerializeField] private Image stateIcon;
    [SerializeField] private Image stateContinueTypeIcon;
    [SerializeField] private TextMeshProUGUI stateDescription;

    [SerializeField] private GetStateContinueTypeIcon stateContinueType;
    
    public void Init(IState state)
    {
        StateDefine.StateNew stateData=StateData.GetState(state);
        //アイコンを表示
        stateIcon.sprite = stateData.IconSprite;
        //継続方式のアイコンを表示
        stateContinueTypeIcon.sprite = stateContinueType.GetIcon(state);
        //状態異常の説明文を表示
        stateDescription.text = "<color=#AF4141>" + stateData.Name+"</color> : "+ stateData.Description;
    }
}
