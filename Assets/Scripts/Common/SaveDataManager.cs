using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static QuestLineData;

public class SaveDataManager : SingletonMonoBehaviour<SaveDataManager>
{
    #region シングルトン処理
    public void Awake()
    {
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);

        //ゲーム起動時処理
        InitialProcess();
    }

    #endregion

    private const string SAVE_FILE_PATH_INVENTORY = "inventory.json";
    private const string SAVE_FILE_PATH_STORAGE = "storage.json";
    private const string SAVE_FILE_PATH_QUEST = "questData.json";
    private const string SAVE_FILE_PATH_SETTING = "setting.json";

    private void InitialProcess()
    {
        //TODO:セーブデータの初期化
    }

    [System.Serializable]
    public class DeckForSave
    {
        public List<int> cards;
        public int cardsNum;
        public string name;
    }

    [System.Serializable]
    public class PhysicalItemForSave
    {
        public int id;
        public string type;

        public PhysicalItemForSave(PhysicalItemDataSO data)
        {
            if (data is null)
            {
                id = -1;
                type = "None";
            }
            else
            {
                id = data.SerialNum;
                type = data.PhysicalItemType.itemType.ToString();
            }
        }

        public PhysicalItemDataSO ToPhysicalItemDataSO()
        {
            if (type == "None")
            {
                return null;
            }
            else
            {
                PhysicalItemTypeDefine.PhysicalItemType itemType = (PhysicalItemTypeDefine.PhysicalItemType)System.Enum.Parse(typeof(PhysicalItemTypeDefine.PhysicalItemType), type);
                //IDとタイプから対応するPhysicalItemDataSOを取得する処理
                return PhysicalItemData.GetPhysicalItemDataSO(id, itemType);
            }
        }
    }

    [System.Serializable]
    public class PhysicalItemGridPosNumForSave
    {
        public int id;
        public string type;
        public int x;
        public int y;
        public int stack;

        public PhysicalItemGridPosNumForSave(PhysicalItemGridPosNumData data)
        {
            id = data.ItemData.SerialNum;
            type = data.ItemData.PhysicalItemType.itemType.ToString();
            x = data.X;
            y = data.Y;
            stack = data.Stack;
        }
        public PhysicalItemGridPosNumData ToPhysicalItemGridPosNumData()
        {
            PhysicalItemTypeDefine.PhysicalItemType itemType = (PhysicalItemTypeDefine.PhysicalItemType)System.Enum.Parse(typeof(PhysicalItemTypeDefine.PhysicalItemType), type);

            return new PhysicalItemGridPosNumData(PhysicalItemData.GetPhysicalItemDataSO(id, itemType), x, y, stack);
        }
    }

    [System.Serializable]
    public class InventorySaveData
    {
        //装備しているデッキ
        public DeckForSave playerDeck;

        //鞄に入っているカードのリスト
        public List<int> playerBackpackCards;

        //装備している物理アイテムのデータ,IDとタイプを保存
        public List<PhysicalItemForSave> equippingPhysicalWeapons;
        public PhysicalItemForSave equippingGear;
        public List<PhysicalItemForSave> equippingConsumables;

        //所持しているアイテムのリスト,IDとタイプを保存
        public List<PhysicalItemGridPosNumForSave> playerBackpackItems;
    }

    [System.Serializable]
    public class StorageSaveData
    {
        public List<StorageData.CardStack> storageCards;

        public List<PhysicalItemGridPosNumForSave> storageItems0;
        public List<PhysicalItemGridPosNumForSave> storageItems1;
        public List<PhysicalItemGridPosNumForSave> storageItems2;
        public List<PhysicalItemGridPosNumForSave> storageItems3;
    }

    [System.Serializable]
    public class QuestForSave
    {
        public int questLineID;
        public int progress;

        //クエストはクリアしているが、報酬を受け取っていない状態かどうか.などのクエスト状況
        public questLineStatus status;

        //現在表示されているクエストの、敵を倒す系のクエストの進行度を管理するための変数。クエストの内容によって、どのように使うかは変わる。
        public int DefeatEnemyProgress;

        public QuestForSave(QuestLineAndProgress questLineAndProgress)
        {
            this.questLineID = questLineAndProgress.QuestLine.ID;
            this.progress = questLineAndProgress.Progress;
            this.status = questLineAndProgress.status;
            this.DefeatEnemyProgress = questLineAndProgress.DefeatEnemyProgress;
        }
    }

    [System.Serializable]
    public class QuestSaveData
    {
        public List<QuestForSave> questLines;
    }

    [System.Serializable]
    public class SettingSaveData
    {
        //コストが残っている時にその表示をするポップアップを表示するか
        public bool IsShowCostRemainedPopup;

        //アイテムやカードを捨てる時にポップアップを表示するか
        public bool IsShowItemDiscardPopup;

        public bool IsShowCardDiscardPopup;

        //バトル終了時のアイテムドロップ終了画面の終了にポップアップを表示するか
        public bool IsShowEndBattlePopup;
        //クエスト終了時の報酬画面のポップアップを表示するか
        public bool IsShowQuestRewardPopup;
    }

    public void SaveInventoryData()
    {
#if UNITY_EDITOR
        string path = Directory.GetCurrentDirectory() + "/Assets/";
#else
        string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
#endif

        path = path + "/save/" + SAVE_FILE_PATH_INVENTORY;

        InventorySaveData saveData = new InventorySaveData();

        //デッキの保存
        Deck deck = InventoryData.GetPlayerDeck();
        saveData.playerDeck = new DeckForSave
        {
            cards = deck.GetCards(),
            cardsNum = deck.GetCards().Count,
            name = deck.Name
        };

        //鞄内カードの保存
        saveData.playerBackpackCards = InventoryData.GetBackpackCards();

        var equippingPhysicalWeapons = InventoryData.GetEquippingPhysicalWeapons();
        saveData.equippingPhysicalWeapons = new List<PhysicalItemForSave>();
        for (int i = 0; i < equippingPhysicalWeapons.Count; i++)
        {
            saveData.equippingPhysicalWeapons.Add(new PhysicalItemForSave(equippingPhysicalWeapons[i]));
        }
        var equippingGear = InventoryData.GetEquippingGear();
        saveData.equippingGear = new PhysicalItemForSave(equippingGear);
        var equippingConsumables = InventoryData.GetEquippingConsumables();
        saveData.equippingConsumables = new List<PhysicalItemForSave>();
        for (int i = 0; i < equippingConsumables.Count; i++)
        {
            saveData.equippingConsumables.Add(new PhysicalItemForSave(equippingConsumables[i]));
        }

        saveData.playerBackpackItems = new List<PhysicalItemGridPosNumForSave>();
        var backpackItems = InventoryData.GetBackpackItems();
        for (int i = 0; i < backpackItems.Count; i++)
        {
            saveData.playerBackpackItems.Add(new PhysicalItemGridPosNumForSave(backpackItems[i]));
        }

        string json = JsonUtility.ToJson(saveData);

        //セーブデータの保存
        //多分、ゲームが配置されているフォルダに

        File.WriteAllText(path, json);
    }

    public static InventorySaveData LoadInventoryData()
    {
#if UNITY_EDITOR
        string path = Directory.GetCurrentDirectory() + "/Assets/";
#else
        string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
#endif

        path = path + "/save/" + SAVE_FILE_PATH_INVENTORY;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);
            return saveData;
        }
        else
        {
            Debug.Log("セーブデータが存在しません");
            return null;
        }
    }

    public void SaveStorageData()
    {
#if UNITY_EDITOR
        string path = Directory.GetCurrentDirectory() + "/Assets/";
#else
        string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
#endif

        StorageSaveData saveData = new StorageSaveData();

        path = path + "/save/" + SAVE_FILE_PATH_STORAGE;

        saveData.storageCards = StorageData.GetPlayerStorageCards();

        saveData.storageItems0 = new List<PhysicalItemGridPosNumForSave>();
        saveData.storageItems1 = new List<PhysicalItemGridPosNumForSave>();
        saveData.storageItems2 = new List<PhysicalItemGridPosNumForSave>();
        saveData.storageItems3 = new List<PhysicalItemGridPosNumForSave>();

        var items = StorageData.GetPlayerStorageItems(0);
        for (int j = 0; j < items.Count; j++)
        {
            saveData.storageItems0.Add(new PhysicalItemGridPosNumForSave(items[j]));
        }
        items = StorageData.GetPlayerStorageItems(1);
        for (int j = 0; j < items.Count; j++)
        {
            saveData.storageItems1.Add(new PhysicalItemGridPosNumForSave(items[j]));
        }
        items = StorageData.GetPlayerStorageItems(2);
        for (int j = 0; j < items.Count; j++)
        {
            saveData.storageItems2.Add(new PhysicalItemGridPosNumForSave(items[j]));
        }
        items = StorageData.GetPlayerStorageItems(3);
        for (int j = 0; j < items.Count; j++)
        {
            saveData.storageItems3.Add(new PhysicalItemGridPosNumForSave(items[j]));
        }


        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(path, json);
    }

    public static StorageSaveData LoadStorageData()
    {
#if UNITY_EDITOR
        string path = Directory.GetCurrentDirectory() + "/Assets/";
#else
        string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
#endif

        path = path + "/save/" + SAVE_FILE_PATH_STORAGE;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            StorageSaveData saveData = JsonUtility.FromJson<StorageSaveData>(json);
            return saveData;
        }
        else
        {
            Debug.Log("セーブデータが存在しません");
            return null;
        }
    }

    public static void SaveQuestData()
    {
#if UNITY_EDITOR
        string path = Directory.GetCurrentDirectory() + "/Assets/";
#else
        string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
#endif

        QuestSaveData saveData = new QuestSaveData();

        path = path + "/save/" + SAVE_FILE_PATH_QUEST;

        var data = QuestLineData.GetAllQuestLines();
        saveData.questLines = new List<QuestForSave>();
        foreach (var ql in data)
        {
            saveData.questLines.Add(new QuestForSave(ql));
        }

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(path, json);
    }

    public static QuestSaveData LoadQuestData()
    {
#if UNITY_EDITOR
        string path = Directory.GetCurrentDirectory() + "/Assets/";
#else
        string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
#endif

        path = path + "/save/" + SAVE_FILE_PATH_QUEST;
        
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            QuestSaveData saveData = JsonUtility.FromJson<QuestSaveData>(json);
            return saveData;
        }
        else
        {
            Debug.Log("セーブデータが存在しません");
            return null;
        }
    }

    public void SaveSettingData()
    {
#if UNITY_EDITOR
        string path = Directory.GetCurrentDirectory() + "/Assets/";
#else
        string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
#endif

        SettingSaveData saveData = new SettingSaveData();

        path = path + "/save/" + SAVE_FILE_PATH_SETTING;

        saveData.IsShowCostRemainedPopup = SettingManager.IsShowCostRemainedPopup;
        saveData.IsShowItemDiscardPopup = SettingManager.IsShowItemDiscardPopup;
        saveData.IsShowCardDiscardPopup = SettingManager.IsShowCardDiscardPopup;
        saveData.IsShowEndBattlePopup = SettingManager.IsShowEndBattlePopup;
        saveData.IsShowQuestRewardPopup = SettingManager.IsShowQuestRewardPopup;

        string json = JsonUtility.ToJson(saveData);
        File.WriteAllText(path, json);
    }

    public static SettingSaveData LoadSettingData()
    {
#if UNITY_EDITOR
        string path = Directory.GetCurrentDirectory() + "/Assets/";
#else
        string path = AppDomain.CurrentDomain.BaseDirectory.TrimEnd('\\');
#endif

        path = path + "/save/" + SAVE_FILE_PATH_SETTING;
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            SettingSaveData saveData = JsonUtility.FromJson<SettingSaveData>(json);
            return saveData;
        }
        else
        {
            Debug.Log("セーブデータが存在しません");
            return null;
        }
    }

    private void OnApplicationQuit()
    {
        SaveInventoryData();
        SaveStorageData();
        SaveQuestData();
        SaveSettingData();
    }
}