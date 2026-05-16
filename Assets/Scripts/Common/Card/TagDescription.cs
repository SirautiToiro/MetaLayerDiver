using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TagDescription : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI tagDescription;

    /// <summary>
    /// カードのタグの説明を表示する
    /// </summary>
    /// <param name="tag">カードタグ</param>
    /// <param name="str">前の部分で取得したカードタグ説明</param>
    public void Init(CardTagDefine.CardTag? tag,string str)
    {
        if(tag is null)
        {//tagがnullなら説明文をそのまま表示
            tagDescription.text = str;
        }
        else
        {
            //タグの説明文を表示
            tagDescription.text = "<color=#AF4141>" + CardTagDefine.Dic_TagName[tag.Value] + "</color> : "
                + str;
        }
    }
}
