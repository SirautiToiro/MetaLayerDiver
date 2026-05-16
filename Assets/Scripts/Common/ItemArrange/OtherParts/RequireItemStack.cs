using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// アイテム要求画面での要求数と実際に配置されている数を表示するUI
/// </summary>
public class RequireItemStack : MonoBehaviour
{
    [SerializeField] private TMPro.TextMeshProUGUI virtualStackText;
    [SerializeField] private TMPro.TextMeshProUGUI realStackText;

    public void SetVirtualStack(int stack)
    {
        virtualStackText.text = stack.ToString();
    }

    public void SetRealStack(int stack)
    {
        realStackText.text = stack.ToString();
    }
}
