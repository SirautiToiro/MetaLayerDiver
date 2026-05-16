using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ターンごとの統計データ
/// ターン終了時にリセット
/// </summary>
public class DataPerTurn : MonoBehaviour
{
    private List<UsedCardName> UsedCardList;

    /// <summary>
    /// 使用されたカード名の統計データ
    /// 演舞カードは全て同名としてカウントする
    /// </summary>
    public class UsedCardName
    {
        public int SerialNum;
        public bool IsEnbu;
        public int UsedCount;
    }

    public void ResetData()
    {
        UsedCardList = new List<UsedCardName>();
    }

    /// <summary>
    /// cardが使用された時に、それを記録する
    /// </summary>
    /// <param name="card">使用されたCard</param>
    public void SetUsedCard(Card card)
    {
        if(card.IsItemHasTag(CardTagDefine.CardTag.Enbu))
        {
            //演舞カードは全て同名としてカウント
            AddUsedCard(card.serialNum, true);
        }
        else
        {
            AddUsedCard(card.serialNum, false);
        }
    }

    private void AddUsedCard(int serialNum, bool isEnbu)
    {
        if(isEnbu)
        {
            foreach (UsedCardName usedCard in UsedCardList)
            {
                if (usedCard.IsEnbu)
                {
                    usedCard.UsedCount++;
                    return; // 既に存在する演舞カードの使用回数を増やす
                }
            }

            // 存在しない場合は新規追加
            UsedCardList.Add(new UsedCardName
            {
                SerialNum = serialNum,
                IsEnbu = true,
                UsedCount = 1
            });
        }
        else
        {
            foreach (UsedCardName usedCard in UsedCardList)
            {
                if (usedCard.SerialNum == serialNum)
                {
                    usedCard.UsedCount++;
                    return; // 既に存在するカードの使用回数を増やす
                }
            }
            // 存在しない場合は新規追加
            UsedCardList.Add(new UsedCardName
            {
                SerialNum = serialNum,
                IsEnbu = false,
                UsedCount = 1
            });
        }
    }

    /// <summary>
    /// どれかのカードがn回以上使用されていたらtrue
    /// </summary>
    /// <param name="n">測定する下限の使用された回数</param>
    /// <returns>n回以上使用されたカードがあるならtrue</returns>
    public bool IsSomeCardUsedNTimes(int n)
    {
        foreach (UsedCardName usedCard in UsedCardList)
        {
            if (usedCard.UsedCount >= n)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsCardOfAttributeUsed(AttributeDefine.Attribute attribute)
    {
        foreach (UsedCardName usedCard in UsedCardList)
        {
            CardDataSO cardData = PlayerCardData.GetCardDataFromSerialNum(usedCard.SerialNum);
            if (cardData is not null)
            {
                foreach (AttributeDefine attr in cardData.attributeList)
                {
                    if (attr.attribute == attribute)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    public int GetEnbuUsedCount()
    {
        foreach (UsedCardName usedCard in UsedCardList)
        {
            if (usedCard.IsEnbu)
            {
                return usedCard.UsedCount;
            }
        }
        return 0;
    }
}
