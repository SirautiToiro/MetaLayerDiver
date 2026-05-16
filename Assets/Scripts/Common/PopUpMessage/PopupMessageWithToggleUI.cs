using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PopupMessageWithToggleUI : PopupMessageUI
{
    //持っているトグルボタン
    [SerializeField] Toggle Toggle;

    public void SetToggleState(bool isOn)
    {
        Toggle.isOn = isOn;
    }

    public bool IsToggleOn()
    {
        return Toggle.isOn;
    }
}
