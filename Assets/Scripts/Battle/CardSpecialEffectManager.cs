using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MG.GIF;

public class CardSpecialEffectManager : MonoBehaviour
{
    [SerializeField] private AnimatedImage animatedImage;

    public void Start()
    {
        animatedImage.gameObject.SetActive(false);
    }

    public void PlayCardSpecialEffect(CardSpecialEffectDefine.SpecialEffect specialEffect)
    {

        switch (specialEffect)
        {
            case CardSpecialEffectDefine.SpecialEffect.None:
                break;//なにもしない
            case CardSpecialEffectDefine.SpecialEffect.ArcEngine:
                //アークエンジンの演出を再生
                animatedImage.gameObject.SetActive(true);
                Debug.Log("アークエンジンの演出を再生");
                animatedImage.Load("GIF/アークエンジン_Gif.gif");
                animatedImage.Play(() =>
                {
                    animatedImage.gameObject.SetActive(false);
                });
                break;
            default:
                break;
        }
    }
}
