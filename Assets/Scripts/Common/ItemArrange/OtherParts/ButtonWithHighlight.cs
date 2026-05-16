using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonWithHighlight : MonoBehaviour
{
    [SerializeField] private Image highlightImage;

    private IButtonWithHighlightManager manager;

    public void Init(IButtonWithHighlightManager manager,bool isOn)
    {
        this.manager = manager;
        SetHighlight(isOn);
    }

    public void SetHighlight(bool flag)
    {
        highlightImage.enabled = flag;
    }

    public void OnClick()
    {
        manager?.OnButtonWithHighlightClicked(this);
    }
}
