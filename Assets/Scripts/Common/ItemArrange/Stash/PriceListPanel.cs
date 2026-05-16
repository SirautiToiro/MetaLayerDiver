using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// 店画面の価格表パネルのプレハブにアタッチする
/// </summary>
public class PriceListPanel : MonoBehaviour
{
    [SerializeField] private GameObject noValueTextObject;
    [SerializeField] private GameObject priceListObject;

    [SerializeField] private TextMeshProUGUI commomPriceText;
    [SerializeField] private TextMeshProUGUI rarePriceText;
    [SerializeField] private TextMeshProUGUI metaPriceText;

    public void Init(ShopPriceRate rate)
    {
        if(rate.CommonRate ==0 && rate.RareRate == 0 && rate.MetaRate == 0)
        {//料金がない支給品
            noValueTextObject.SetActive(true);
            priceListObject.SetActive(false);
        }
        else
        {//料金を表示
            noValueTextObject.SetActive(false);
            priceListObject.SetActive(true);
            if(rate.CommonRate == -1)
            {//-1の場合、販売しないのでXを表示
                commomPriceText.text = "X";
            }
            else
            {
                commomPriceText.text = rate.CommonRate.ToString()+"G";
            }
            if (rate.RareRate == -1)
            {//-1の場合、販売しないのでXを表示
                rarePriceText.text = "X";
            }
            else
            {
                rarePriceText.text = rate.RareRate.ToString() + "G";
            }
            if (rate.MetaRate == -1)
            {//-1の場合、販売しないのでXを表示
                metaPriceText.text = "X";
            }
            else
            {
                metaPriceText.text = rate.MetaRate.ToString() + "G";
            }
        }
    }
}
