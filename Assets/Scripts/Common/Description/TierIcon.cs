using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// レア度を表示する際にアイコンにする
/// </summary>
public class TierIcon : MonoBehaviour
{
    [SerializeField] private Image tierIcon;

    [SerializeField] private Sprite commonSprite;
    [SerializeField] private Sprite rareSprite;
    [SerializeField] private Sprite metaSprite;

    /// <summary>
    /// レア度に応じたspriteをセットする
    /// </summary>
    /// <param name="tier">与えられたレア度</param>
    public void SetSprite(TierDefine.Tier tier)
    {
        switch (tier)
        {
            case TierDefine.Tier.Common:
                tierIcon.sprite = commonSprite;
                break;
            case TierDefine.Tier.Rare:
                tierIcon.sprite = rareSprite;
                break;
            case TierDefine.Tier.Meta:
                tierIcon.sprite = metaSprite;
                break;
        }
    }
}
