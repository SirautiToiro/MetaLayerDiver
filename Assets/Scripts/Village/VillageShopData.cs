using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VillageShopData : MonoBehaviour
{
    //カードショップで販売するもののデータ
    //ダンジョンから帰還時に設定
    public List<StorageData.CardStackData> specialPriceCardsData;
    public List<StorageData.CardStackData> normalCardsData;
    //カードショップにセットする枚数
    [SerializeField] private int[] specialPriceCardsNumPerTier;
    [SerializeField] private int[] normalCardsNumPerTier;

    /// <summary>
    /// CardShopで表示するカードを設定する
    /// ダンジョンから帰ってきたときに初期化
    /// </summary>
    public void SetShopData()
    {
        specialPriceCardsData = new List<StorageData.CardStackData>();
        normalCardsData = new List<StorageData.CardStackData>();

        for (int i = 0; i < 3; i++)
        {
            for(int j=0;j<specialPriceCardsNumPerTier[i]; j++)
            {
                int newSerial = PlayerCardData.GetRandomCardSerialNumByTier((TierDefine.Tier)i);

                if(specialPriceCardsData.Exists(x => x.CardData.serialNum == newSerial))
                {//既にあるならやり直し
                    j--;
                    continue;
                }

                StorageData.CardStackData cardStackData = new StorageData.CardStackData();
                cardStackData.CardData = PlayerCardData.GetCardDataFromSerialNum(newSerial);
                cardStackData.Stack = 1;

                specialPriceCardsData.Add(cardStackData);
            }

            for (int j = 0; j < normalCardsNumPerTier[i]; j++)
            {
                int newSerial = PlayerCardData.GetRandomCardSerialNumByTier((TierDefine.Tier)i);

                if (normalCardsData.Exists(x => x.CardData.serialNum == newSerial))
                {//既にあるならやり直し
                    j--;
                    continue;
                }

                StorageData.CardStackData cardStackData = new StorageData.CardStackData();
                cardStackData.CardData = PlayerCardData.GetCardDataFromSerialNum(newSerial);
                cardStackData.Stack = 1;

                normalCardsData.Add(cardStackData);
            }
        }

        
    }
    public void SpecialPriceCardsOnBuied(int serialNum)
    {
        specialPriceCardsData.RemoveAll(x => x.CardData.serialNum == serialNum);
    }

    public void NormalCardsOnBuied(int serialNum)
    {
        normalCardsData.RemoveAll(x => x.CardData.serialNum == serialNum);
    }
}
