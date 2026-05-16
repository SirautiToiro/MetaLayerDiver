using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class QuestReward
{
    public int RewardGold;

    public List<StorageData.CardStackData> RewardCards;

    public List<PhysicalItemGridPosNumData> RewardItems;

    public string GetRewardText()
    {
        List<string> rewardTexts = new List<string>();
        rewardTexts.Add("ïÒèVÅF");
        if (RewardGold > 0)
        {
            rewardTexts.Add($"ÅE{RewardGold}G");
        }
        if (RewardCards != null)
        {
            foreach (var cardData in RewardCards)
            {
                rewardTexts.Add($"ÅE{cardData.CardData.cardName}Å~{cardData.Stack}");
            }
        }
        if (RewardItems != null)
        {
            foreach (var itemData in RewardItems)
            {
                rewardTexts.Add($"ÅE{itemData.ItemData.ItemName}Å~{itemData.Stack}");
            }
        }
        return string.Join("\n", rewardTexts);
    }
}
