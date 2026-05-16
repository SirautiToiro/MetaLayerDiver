using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーが所持しているカードのデータ
/// シングルトン
/// </summary>
public class PlayerCardData : SingletonMonoBehaviour<PlayerCardData>
{
    //存在する全カードの一覧
    [SerializeField] private List<CardDataSO> allPlayerCardsList;

    //支給品に該当するカード
    [SerializeField] private List<CardDataSO> supplyCardsList;

    //カード効果などによってのみ出現するカード
    [SerializeField] private List<CardDataSO> specialOnlyCardsList;

    //所持しているデッキの一覧(Deckクラスのリスト)
    public static List<Deck> playerDecks;

    //初期デッキのSO
    [SerializeField] private List<DeckDataSO> initialDecks;

    //TODO:デッキ所持数の処理
    //private static int DECKSETMAX=8;//デッキの所持上限.後から変更可能
    //private static int selectedDeck = 0;//選択されているデッキの番号


    // プレイヤー側全カードデータと通し番号を紐づけたDictionary
    private static Dictionary<int, CardDataSO> CardDatasBySerialNum;

    // 各Tierごとのカードのシリアル番号リスト
    //0:Common,1:Rare,2:Meta
    private static List<int>[] cardDataByTier = new List<int>[3];

    //初期化処理
    public void Awake()
    {
        //シングルトンの処理
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        //シングルトン処理終了

        playerDecks = new List<Deck>();//宣言
        //Tierごとのカードのシリアル番号リストを初期化
        cardDataByTier = new List<int>[3];
        for (int i = 0; i < cardDataByTier.Length; i++)
        {
            cardDataByTier[i] = new List<int>();
        }

        
        CardDatasBySerialNum = new Dictionary<int, CardDataSO>();
        foreach(CardDataSO cardData in allPlayerCardsList)
        {//ドロップカードのリストにカードを追加
            //通し番号をDictionaryに設定
            CardDatasBySerialNum.Add(cardData.serialNum, cardData);

            //カードのTierごとに分類
            //Supplyカードは除外
            if (supplyCardsList.Contains(cardData))
            {
                continue;
            }

            //生成されるのみのカードは除外
            if(specialOnlyCardsList.Contains(cardData))
            {
                continue;
            }

            if (cardData.tier.tier == TierDefine.Tier.Common)
            {
                cardDataByTier[0].Add(cardData.serialNum);
            }
            else if (cardData.tier.tier == TierDefine.Tier.Rare)
            {
                cardDataByTier[1].Add(cardData.serialNum);
            }
            else if (cardData.tier.tier == TierDefine.Tier.Meta)
            {
                cardDataByTier[2].Add(cardData.serialNum);
            }
        }
    }

    /// <summary>
    /// シリアル番号からカード情報を得る
    /// </summary>
    /// <param name="serialNum">シリアル番号</param>
    /// <returns>カード情報</returns>
    public static CardDataSO GetCardDataFromSerialNum(int serialNum)
    {
        return CardDatasBySerialNum[serialNum];
    }

    public static int GetRandomCardSerialNumByTier(TierDefine.Tier tier)
    {
        //指定されたTierのカードのシリアル番号リストを取得
        List<int> serialNumList = cardDataByTier[(int)tier];
        if (serialNumList.Count == 0)
        {
            Debug.LogError("指定されたTierのカードが存在しません: " + tier);
            return -1; // エラー処理
        }
        //ランダムにシリアル番号を選択
        int randomIndex = Random.Range(0, serialNumList.Count);
        return serialNumList[randomIndex];
    }

    public static void TestCardDataByTierDump()
    {
        //各Tierごとのカードのシリアル番号リストをダンプ
        for (int i = 0; i < cardDataByTier.Length; i++)
        {
            Debug.Log(i);
            foreach (int serialNum in cardDataByTier[i])
            {
                Debug.Log(GetCardDataFromSerialNum(serialNum).cardName);
            }
        }
    }

    public static List<CardDataSO> GetAllCards()
    {
        return Instance.allPlayerCardsList;
    }

    public static List<CardDataSO> GetAllSupplyCards()
    {
        return Instance.supplyCardsList;
    }

    public static List<CardDataSO> GetAllSpecialOnlyCards()
    {
        return Instance.specialOnlyCardsList;
    }

    public static List<CardDataSO> TestGetCardByTier()
    {
        List<CardDataSO> cards = new List<CardDataSO>();

        foreach (List<int> data in cardDataByTier)
        {
            foreach (int serialNum in data)
            {
                cards.Add(GetCardDataFromSerialNum(serialNum));
            }
        }

        return cards;
    }
}
