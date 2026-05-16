using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DungeonInformation : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI staminaText;

    public void SetHP(int hp,int hpMax)
    {
        hpText.text = $"{hp}/{hpMax}";
    }

    public void SetStamina(int stamina, int staminaMax)
    {
        staminaText.text = $"{stamina}/{staminaMax}";
    }
}
