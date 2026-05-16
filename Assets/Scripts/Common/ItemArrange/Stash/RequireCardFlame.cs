using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RequireCardFlame : MonoBehaviour
{
    [SerializeField] private PlaceCardZone mainCardZone;
    [SerializeField] private PlaceCardZone virtualCardZone;

    [SerializeField] private TMPro.TextMeshProUGUI virtualStackText;
    [SerializeField] private TMPro.TextMeshProUGUI realStackText;

    [SerializeField] private GameObject stackObject;

    public PlaceCardZone GetRealCardZone()
    {
        return mainCardZone;
    }

    public PlaceCardZone GetVirtualCardZone()
    {
        return virtualCardZone;
    }

    public void SetVirtualStack(int stack)
    {
        virtualStackText.text = stack.ToString();
    }

    public void SetRealStack(int stack)
    {
        realStackText.text = stack.ToString();
    }
}
