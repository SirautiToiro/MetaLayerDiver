using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ShopMessage : MonoBehaviour
{
    [SerializeField] private List<ShopMessageData> shopMessageDatas;

    [SerializeField] private TextMeshProUGUI messageText;

    public void ShowMessage(string key)
    {
        List<string> Messages = shopMessageDatas.Find(x => key.Equals(x.Key))?.Messages;

        if(Messages == null || Messages.Count == 0)
        {
            messageText.text = "";
            return;
        }
        else
        {
            int index = Random.Range(0, Messages.Count);
            messageText.text = Messages[index];
        }
    }
}
