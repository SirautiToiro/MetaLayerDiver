using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static SaveDataManager;

public class StorageData : SingletonMonoBehaviour<StorageData>
{
    [SerializeField]private List<CardStackData> testStorageCards;

    private static List<CardStack> playerStorageCards;

    [SerializeField] private List<ItemStoragePageData> testStorageItems;

    //倉庫のアイテムデータ.ItemStoragePageMax枚のページがある
    private static List<List<PhysicalItemGridPosNumData>> playerStorageItems;

    //倉庫のアイテムのページ最大数
    [SerializeField] public int ItemStoragePageMax;

    [System.Serializable]
    public class CardStackData
    {
        public CardDataSO CardData;
        public int Stack;
    }

    [System.Serializable]
    public class CardStack
    {
        public int cardSerialNum;
        public int Stack;
    }

    /// <summary>
    /// 倉庫アイテムの一ページのデータ
    /// </summary>
    [System.Serializable]
    private class ItemStoragePageData
    {
        public List<PhysicalItemGridPosNumData> Items;
    }

    public void Init()
    {
        //アイテムデータが初期化されてから持ち物を初期化
        //シングルトンの処理
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        //シングルトン処理終了

        if (TestFlags.Instance.useTestDataFlag)
        {//テストデータを使用する
            //倉庫のカードデータをテストデータからセット
            playerStorageCards = new List<CardStack>();
            foreach (CardStackData cardStackData in testStorageCards)
            {
                CardStack cardStack = new CardStack
                {
                    cardSerialNum = cardStackData.CardData.serialNum,
                    Stack = cardStackData.Stack
                };
                playerStorageCards.Add(cardStack);
            }

            //倉庫のアイテムデータをテストデータからセット
            //初期化
            playerStorageItems = new List<List<PhysicalItemGridPosNumData>>();
            for (int i = 0; i < ItemStoragePageMax; i++)
            {
                playerStorageItems.Add(new List<PhysicalItemGridPosNumData>());
            }
            //テストデータをコピー
            for (int i = 0; i < testStorageItems.Count; i++)
            {
                playerStorageItems[i] = new List<PhysicalItemGridPosNumData>(testStorageItems[i].Items);
            }
        }
        else
        {//セーブデータを用いた初期化
            SaveDataManager.StorageSaveData data= SaveDataManager.LoadStorageData();

            playerStorageCards = data.storageCards;
            playerStorageItems = new List<List<PhysicalItemGridPosNumData>>();
            for (int i = 0; i < ItemStoragePageMax; i++)
            {
                playerStorageItems.Add(new List<PhysicalItemGridPosNumData>());
            }

            for(int i=0; i < data.storageItems0.Count; i++)
            {
                playerStorageItems[0].Add(data.storageItems0[i].ToPhysicalItemGridPosNumData());
            }
            for(int i=0; i < data.storageItems1.Count; i++)
            {
                playerStorageItems[1].Add(data.storageItems1[i].ToPhysicalItemGridPosNumData());
            }
            for(int i=0; i < data.storageItems2.Count; i++)
            {
                playerStorageItems[2].Add(data.storageItems2[i].ToPhysicalItemGridPosNumData());
            }
            for(int i=0; i < data.storageItems3.Count; i++)
            {
                playerStorageItems[3].Add(data.storageItems3[i].ToPhysicalItemGridPosNumData());
            }
        }

        //カードをシリアル番号でソート
        SortStorageCards();
    }

    /// <summary>
    /// 倉庫のカードをシリアル番号でソートし、重複をまとめる
    /// </summary>
    public void SortStorageCards()
    {
        //カードをシリアル番号でソート
        playerStorageCards.Sort((x, y) => x.cardSerialNum.CompareTo(y.cardSerialNum));

        //重複しているシリアル番号をまとめる
        for (int i = 0; i < playerStorageCards.Count - 1; i++)
        {
            if (playerStorageCards[i].cardSerialNum == playerStorageCards[i + 1].cardSerialNum)
            {
                playerStorageCards[i].Stack += playerStorageCards[i + 1].Stack;
                playerStorageCards.RemoveAt(i + 1);
                i--; // 前の要素に戻る
            }
        }
    }

    /// <summary>
    /// 倉庫のカードを記録する
    /// </summary>
    /// <param name="cardList">記録するリスト</param>
    public static void SetStorageCards(List<StorageData.CardStack> cardList)
    {
        //コピー
        playerStorageCards = new List<CardStack>(cardList);
    }

    /// <summary>
    /// 倉庫のアイテムを記録する
    /// </summary>
    /// <param name="itemData">アイテム</param>
    /// <param name="page">記録するページ</param>
    public static void SetStorageItems(List<PhysicalItemGridPosNumData> itemData,int page)
    {
        if (page < 0 || page >= Instance.ItemStoragePageMax)
        {
            Debug.LogError("StorageData.SetStorageItems:存在しないページが指定されました");
            return;
        }
        //コピー
        playerStorageItems[page] = new List<PhysicalItemGridPosNumData>(itemData);
    }

    public static List<CardStack> GetPlayerStorageCards()
    {
        return playerStorageCards;
    }

    public static List<PhysicalItemGridPosNumData> GetPlayerStorageItems(int page)
    {
        if(page < 0 || page >= Instance.ItemStoragePageMax)
        {
            Debug.LogError("StorageData.GetPlayerStorageItems:存在しないページが指定されました");
            return null;
        }
        return playerStorageItems[page];
    }

    public static int GetPageNum()
    {
        return Instance.ItemStoragePageMax;
    }

    /// <summary>
    /// 倉庫のアイテムを減らす
    /// 0になったら削除する
    /// </summary>
    /// <param name="data">減少させるアイテムの種類と位置(種類は使わない)</param>
    /// <param name="page">減らすページ</param>
    /// <param name="num">減らす数</param>
    public static void ReduceStorageItem(PhysicalItemGridPosNumData data, int page,int num)
    {
        PhysicalItemGridPosNumData targetData = playerStorageItems[page].Find(item => item.X == data.X && item.Y == data.Y);
        if (targetData is null) return;
        targetData.Stack -= num;
        if (targetData.Stack <= 0)
        {//0になったらデータから削除
            playerStorageItems[page].Remove(targetData);
        }
    }

    public static List<CardStack> DataListToNumList(List<CardStackData> data)
    {
        List < CardStack > result = new List<CardStack>();

        foreach (CardStackData cardStackData in data)
        {
            CardStack cardStack = new CardStack
            {
                cardSerialNum = cardStackData.CardData.serialNum,
                Stack = cardStackData.Stack
            };
            result.Add(cardStack);
        }

        return result;
    }

    public static List<CardStackData> NumListToDataList(List<CardStack> numList)
    {
        List<CardStackData> result = new List<CardStackData>();
        foreach (CardStack cardStack in numList)
        {
            CardDataSO cardDataSO = PlayerCardData.GetCardDataFromSerialNum(cardStack.cardSerialNum);
            if (cardDataSO is null) continue;
            CardStackData cardStackData = new CardStackData
            {
                CardData = cardDataSO,
                Stack = cardStack.Stack
            };
            result.Add(cardStackData);
        }
        return result;
    }
}
