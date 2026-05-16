using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーがダンジョン中所持しているカード,アイテムのクラス
/// シングルトン
/// </summary>
public class InventoryData : SingletonMonoBehaviour<InventoryData>
{
    //装備中の武器の個数の最大値
    private static int weaponEquipMax;

    //Playerの現在所持しているデッキ.Deck.DECKCARDMAXに上限
    //シーンに入った際に取得,デッキ編成で変更
    private static Deck playerDeck;

    //バックパックに入っているカードのリスト
    private static List<int> playerBackpackCards;

    //装備している物理アイテムのデータ
    private static List<PhysicalItemDataSO> equippingPhysicalWeapons;
    private static PhysicalItemDataSO equippingGear;
    private static List<PhysicalItemDataSO> equippingConsumables;

    //所持中のアイテムのリスト
    private static List<PhysicalItemGridPosNumData> playerBackpackItems;

    [SerializeField] private int inventoryHeight;//インベントリの高さ
    [SerializeField] private int inventoryWidth;//インベントリの幅

    //ForTest
    //テストで初期に生成するデッキ
    [SerializeField] private DeckDataSO testDeckData;
    [SerializeField] private List<CardDataSO> testBackpackCards;
    //テストで初期に持っている武器
    [SerializeField] private List<WeaponDataSO> testWeapons;
    [SerializeField] private int testWeaponMax;//テスト用武器データ最大値

    //テスト用装備データ
    [SerializeField] private List<PhysicalItemDataSO> testEquippingPhysicalWeapons;//テスト用装備中物理アイテム
    [SerializeField] private PhysicalItemDataSO testEquippingGear;//テスト用装備中ギア
    [SerializeField] private List<PhysicalItemDataSO> testEquippingConsumables;//テスト用装備中消費アイテム

    //テスト用アイテムデータ
    [SerializeField] private List<PhysicalItemGridPosNumData> testItemGridPosNum;//テスト用アイテムデータの位置(X,Y),stack

    //初期化
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

        if(TestFlags.Instance.useTestDataFlag)
        {//テストデータを用いた初期化
            playerDeck = new Deck("TestDeck");
            playerBackpackCards = new List<int>();

            //デッキをテスト用デッキにする
            for (int i = 0; i < Deck.DECKCARDMAX; i++)
            {
                if (testDeckData.cardList.Count < i) break;

                playerDeck.SetCard(i, testDeckData.cardList[i].serialNum);
            }

            //バックパック内カードをテスト用にする
            for (int i = 0; i < testBackpackCards.Count; i++)
            {
                playerBackpackCards.Add(testBackpackCards[i].serialNum);
            }
            weaponEquipMax = testWeaponMax;
            //アイテムをテスト用にする
            playerBackpackItems = new List<PhysicalItemGridPosNumData>();
            for (int i = 0; i < testItemGridPosNum.Count; i++)
            {
                playerBackpackItems.Add(testItemGridPosNum[i]);
            }

            //装備中物理アイテムをテスト用にする
            equippingPhysicalWeapons = new List<PhysicalItemDataSO>();
            for (int i = 0; i < testEquippingPhysicalWeapons.Count; i++)
            {
                equippingPhysicalWeapons.Add(testEquippingPhysicalWeapons[i]);
            }
            equippingGear = testEquippingGear;
            equippingConsumables = new List<PhysicalItemDataSO>();
            for (int i = 0; i < testEquippingConsumables.Count; i++)
            {
                equippingConsumables.Add(testEquippingConsumables[i]);
            }
        }
        else
        {//セーブデータを用いた初期化
            var saveData  = SaveDataManager.LoadInventoryData();

            //デッキを設定
            var tmpPlayerDeck = saveData.playerDeck;
            Deck deck = new Deck(tmpPlayerDeck.name);
            for(int i=0;i< tmpPlayerDeck.cardsNum; i++)
            {
                deck.SetCard(i, tmpPlayerDeck.cards[i]);
            }
            playerDeck = deck;

            playerBackpackCards = new List<int>();
            //バックパック内カードを設定
            for (int i = 0; i < saveData.playerBackpackCards.Count; i++)
            {
                playerBackpackCards.Add(saveData.playerBackpackCards[i]);
            }


            equippingPhysicalWeapons = new List<PhysicalItemDataSO>();
            for (int i = 0; i < saveData.equippingPhysicalWeapons.Count; i++)
            {
                equippingPhysicalWeapons.Add(saveData.equippingPhysicalWeapons[i].ToPhysicalItemDataSO());
            }
            equippingGear = saveData.equippingGear.ToPhysicalItemDataSO();
            equippingConsumables = new List<PhysicalItemDataSO>();
            for (int i = 0; i < saveData.equippingConsumables.Count; i++)
            {
                equippingConsumables.Add(saveData.equippingConsumables[i].ToPhysicalItemDataSO());
            }

            playerBackpackItems = new List<PhysicalItemGridPosNumData>();
            if(saveData.playerBackpackItems is not null)
            {
                for (int i = 0; i < saveData.playerBackpackItems.Count; i++)
                {
                    playerBackpackItems.Add(saveData.playerBackpackItems[i].ToPhysicalItemGridPosNumData());
                }
            }
        }
    }

    /// <summary>
    /// プレイヤーがダンジョン中で所持しているデッキを得る
    /// </summary>
    /// <returns>プレイヤーがダンジョン中で所持しているデッキ</returns>
    public static Deck GetPlayerDeck()
    {
        return playerDeck;
    }

    /// <summary>
    /// プレイヤーがダンジョン中で所持しているデッキを変更する
    /// </summary>
    /// <param name="deck">変更先のデッキ</param>
    /// <returns>trueなら成功</returns>
    public static bool SetPlayerDeck(Deck deck)
    {
        //名前設定
        playerDeck.Name = deck.Name;

        playerDeck = new Deck(deck.Name);
        //デッキをコピー(不正でも記録する)
        for (int i = 0; i < deck.GetCount(); i++)
        {
            playerDeck.SetCard(i, deck.GetCard(i));
        }

        if (deck.GetCount() != Deck.DECKCARDMAX)
        {//デッキ枚数の判定
            //デッキ枚数が不正
            return false;
        }
        //TODO:デッキ内3枚の判定

        return true;
    }

    /// <summary>
    /// プレイヤーが鞄に入れているカードを返す
    /// </summary>
    /// <returns>プレイヤーが鞄に入れているカード</returns>
    public static List<int> GetBackpackCards()
    {
        return playerBackpackCards;
    }

    /// <summary>
    /// プレイヤーが鞄に入れているカードを変更する
    /// 変更可能かを判定する
    /// </summary>
    /// <param name="cardSerials">変更後のリスト</param>
    /// <returns>変更が正しくできるならtrue</returns>
    public static bool SetBackpackCards(List<int> cardSerials)
    {
        playerBackpackCards = new List<int>();
        for(int i = 0; i < cardSerials.Count; i++)
        {
            playerBackpackCards.Add(cardSerials[i]);
        }

        if(cardSerials.Count > ItemArrangeConstants.BackpackCardMax)
        {//鞄内カードの最大数を超えている
            return false;
        }

        return true;
    }

    /// <summary>
    /// プレイヤーが鞄に入れているアイテム(の配置含め)を変更する
    /// 変更可能かを判定する
    /// </summary>
    /// <param name="items">変更後のリスト</param>
    /// <returns>変更が正しくできるならtrue</returns>
    public static bool SetBackpackItems(List<PhysicalItemGridPosNumData> items)
    {
        playerBackpackItems = new List<PhysicalItemGridPosNumData>();
        for (int i = 0; i < items.Count; i++)
        {
            playerBackpackItems.Add(items[i]);
        }
        return true;
    }

    public static bool SetEquippingItems(List<PhysicalItemDataSO> weapons, PhysicalItemDataSO gear, List<PhysicalItemDataSO> consumables)
    {
        equippingPhysicalWeapons = new List<PhysicalItemDataSO>();
        for (int i = 0; i < weapons.Count; i++)
        {
            equippingPhysicalWeapons.Add(weapons[i]);
        }
        equippingGear = gear;
        equippingConsumables = new List<PhysicalItemDataSO>();
        for (int i = 0; i < consumables.Count; i++)
        {
            equippingConsumables.Add(consumables[i]);
        }

        return true;
    }

    public static void SetEquippingConsumables(List<PhysicalItemDataSO> consumables)
    {
        equippingConsumables = new List<PhysicalItemDataSO>();

        for (int i = 0; i < consumables.Count; i++)
        {
            equippingConsumables.Add(consumables[i]);
        }
    }

    public static List<PhysicalItemGridPosNumData> GetBackpackItems()
    {
        return playerBackpackItems;
    }

    /// <summary>
    /// dataの位置にあるアイテムの数をnum減らす
    /// 0になったら削除する
    /// </summary>
    /// <param name="data">減少させるアイテムの種類と位置(種類は使わない)</param>
    /// <param name="num">減らす数</param>
    public static void ReduceBackpackItem(PhysicalItemGridPosNumData data,int num)
    {
        PhysicalItemGridPosNumData targetData = playerBackpackItems.Find(item => item.X == data.X && item.Y == data.Y);
        if (targetData is null) return;
        targetData.Stack -= num;
        if(targetData.Stack <= 0)
        {//0になったらデータから削除
            playerBackpackItems.Remove(targetData);
        }
    }

    public static List<PhysicalItemDataSO> GetEquippingPhysicalWeapons()
    {
        return equippingPhysicalWeapons;
    }

    public static PhysicalItemDataSO GetEquippingGear()
    {
        return equippingGear;
    }

    public static List<PhysicalItemDataSO> GetEquippingConsumables()
    {
        return equippingConsumables;
    }

    /// <summary>
    /// 武器の所持最大値を返す
    /// </summary>
    /// <returns>武器の所持最大値</returns>
    public static int GetWeaponEquipMax()
    {
        return weaponEquipMax;
    }
}
