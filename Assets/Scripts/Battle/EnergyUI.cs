using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnergyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI energyMaxText;

    /// <summary>
    /// 戦闘中に使用するエネルギーの値を表示する
    /// </summary>
    /// <param name="energy">エネルギー</param>
    /// <param name="energyMax">エネルギー最大値</param>
    public void SetEnergy(int energy,int energyMax)
    {
        energyText.text = energy.ToString();
        energyMaxText.text = energyMax.ToString();
    }
}
