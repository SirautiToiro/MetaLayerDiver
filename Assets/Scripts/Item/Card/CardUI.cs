using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CardUI : MonoBehaviour
{
    private Card card;//対応しているCard

    [SerializeField] TextMeshProUGUI cardName;
    [SerializeField] TextMeshProUGUI cardDiscription;
    [SerializeField] TextMeshProUGUI cardAttributeAndTarget;
    [SerializeField] TextMeshProUGUI cardCost;
    [SerializeField] Image cardImage;

    [SerializeField] EffectTagToText effectTagToText;

    /// <summary>
    /// 初期化処理。バトル中の呼び出し。
    /// </summary>
    /// <param name="cardData"></param>
    /// <param name="_card"></param>
    /// <param name="actualEffect"></param>
    /// <param name="handManager"></param>
    public void Init(CardDataSO cardData, Card _card, List<ActualEffect> actualEffect,HandManager handManager)
    {
        effectTagToText.Init(handManager);
        Init(cardData, _card, actualEffect);
    }

    /// <summary>
    /// 初期化処理.Cardから呼出
    /// </summary>
    /// <param name="cardData">本来のカードデータ</param>
    /// <param name="_card">対応しているCard</param>
    /// <param name="actualEffect">補正後の効果のデータ</param>
    public void Init(CardDataSO cardData,Card _card, List<ActualEffect> actualEffect)
    {
        card = _card;

        SetCardCost(cardData.cost);
        SetCardNameText(cardData.cardName);
        SetCardIconSprite(cardData.iconSprite);
        SetCardEffectString(actualEffect,cardData.tagList);
        SetCardAttributeString(cardData.attributeList,cardData.targetDefine);
    }


    /// <summary>
    /// カードの諸情報(名前と画像以外)を書き換える
    /// </summary>
    /// <param name="cost">変更先のコスト</param>
    /// <param name="actualEffect">変更先のカードエフェクト</param>
    /// <param name="attributes">変更先のカード属性</param>
    /// <param name="targetDefine">変更先のカード対象</param>
    /// <param name="tags">変更先のカードタグ</param>
    public void RefreshCard(int cost,List<ActualEffect> actualEffect, List<AttributeDefine> attributes, TargetDefine effectTarget, List<CardTagDefine> tags)
    {
        SetCardCost(cost);
        SetCardEffectString(actualEffect, tags);
        SetCardAttributeString(attributes, effectTarget);
    }

    //カードコストの変更
    private void SetCardCost(int cost)
    {
        cardCost.text = cost.ToString();
    }

    //カード名の変更
    private void SetCardNameText(string name)
    {
        cardName.text = name;
    }

    //カードアイコンの変更s
    private void SetCardIconSprite(Sprite sprite)
    {
        cardImage.sprite=sprite;
    }

    //カード効果テキストの変更
    private void SetCardEffectString(List<ActualEffect> cardEffects,List<CardTagDefine> tags)
    {
        cardDiscription.text = effectTagToText.ConvertToText(cardEffects,tags);
    }

    //カード属性テキストの変更
    //カード効果対象も記述
    private void SetCardAttributeString(List<AttributeDefine> cardAttributes,TargetDefine targetDefine)
    {
        string cardText = "";
        int attributeLength = cardAttributes.Count;
        int count = 1;

        foreach(AttributeDefine attributeDefine in cardAttributes)
        {
            cardText += AttributeDefine.Dic_AttributeName[attributeDefine.attribute];

            //最後のみ・を入れない
            if (count < attributeLength)
            {
                cardText += "・";
            }
            count++;
        }

        cardText += "/";
        cardText += TargetDefine.Dic_EffectTarget[targetDefine.effectTarget];

        cardAttributeAndTarget.text = cardText;
    }
}
