using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StackCardFrame : MonoBehaviour
{
    [SerializeField] private PlaceCardZone placeCardZone;
    [SerializeField] private TMPro.TextMeshProUGUI stackText;

    [SerializeField] private GameObject stackObject;

    public PlaceCardZone GetPlaceCardZone()
    {
        return placeCardZone;
    }

    public void SetStack(int stack)
    {
        stackText.text = stack.ToString();

        if(stack ==0)stackObject.SetActive(false);
        else stackObject.SetActive(true);
    }
}
