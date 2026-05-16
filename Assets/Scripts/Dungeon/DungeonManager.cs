using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ダンジョン中からの画面の遷移
/// </summary>
public class DungeonManager : MonoBehaviour,IBaseUIPage
{
    [SerializeField] FieldManager fieldManager;
    [SerializeField] Player player;
    [SerializeField] WindowManager windowManager;
    [SerializeField] Dungeon dungeon;
    [SerializeField] InputBlocker inputBlocker;
    [SerializeField] ItemArrangeManager itemArrangeManager;
    [SerializeField] DungeonInformation dungeonInformation;
    [SerializeField] ConsumablesExecute consumablesExecute;

    [SerializeField] Canvas dungeonCanvas;
    [SerializeField] Canvas battleCanvas;
    [SerializeField] Camera mainCamera;

    private DungeonTypeDefine.DungeonType dungeonType;

    [Header("敵の生成のされやすさ")]
    [SerializeField] private DungeonSpawnRate enemySpawnRate;

    private int floor;//現在の階層、0or1

    private bool showDoorFlag;

    private int stamina;//元気。0になるとダンジョン攻略失敗。移動時に1減る
    private int staminaMax;//最大値

    [SerializeField] private ScenarioManager scenarioManager;

    //ForTest
    //テスト用ダンジョンタイプ
    [SerializeField]private DungeonTypeDefine.DungeonType testDungeonType;

    [SerializeField] UIPageManager uiPageManager;
    public UIPageManager UIPageManager { get { return uiPageManager; } }

    [SerializeField] private AudioClip kojoBGM;

    [System.Serializable]
    //敵の生成のされやすさを格納するクラス
    //ダンジョンのひとつの層に対して、それぞれのレア度の生成のされやすさを格納
    public class FloorSpawnRate
    {
        public int common;
        public int rare;
        public int meta;
    }

    [System.Serializable]
    //敵の生成のされやすさを格納するクラス
    //ダンジョン全体に対して、1,2層とその宝箱のレートを格納
    public class DungeonSpawnRate
    {
        public FloorSpawnRate stage0Rate;
        public FloorSpawnRate stage0TreasureRate;
        public FloorSpawnRate stage1Rate;
        public FloorSpawnRate stage1TreasureRate;
    }

    // Start is called before the first frame update
    void Start()
    {
        //最初はダンジョン画面から始まる
        dungeonCanvas.enabled = true;
        battleCanvas.enabled = false;
        showDoorFlag = false;

        //BGM再生
        GameManager.SetAudio(kojoBGM);
        dungeonType = testDungeonType;

        //現在の階層を設定
        floor = 0;//test
        //元気設定
        stamina = DungeonConstants.StaminaMax;
        staminaMax = DungeonConstants.StaminaMax;

        inputBlocker.Init();
        windowManager.Init();
        itemArrangeManager.Init(this, mainCamera);

        //装備のダンジョン入場時効果(タグがダンジョン入場時なら)
        if(InventoryData.GetEquippingGear().GearData.tag is IGearOnEnterDungeon targetTag)
        {
            targetTag.OnEnterDungeon(this);
        }

        dungeon.Init(showDoorFlag);//ダンジョン生成
        player.Init(50,5);//プレイヤー生成

        dungeonInformation.SetHP(player.GetHp(),player.GetHpMax());
        dungeonInformation.SetStamina(stamina, staminaMax);//元気の初期値を設定
    }

    /// <summary>
    /// ボトルシーンを開始する
    /// </summary>
    /// <param name="encountType">どのような種類のバトルか</param>
    public void StartBattle(EncountTypeDefine.EnecountType encountType)
    {
        //バトル画面に変更
        dungeonCanvas.enabled = false;
        battleCanvas.enabled = true;

        fieldManager.Init(encountType);
    }

    /// <summary>
    /// FieldManagerのバトル終了時に呼ばれる
    /// </summary>
    public void EndBattle()
    {
        battleCanvas.enabled = false;
        dungeonCanvas.enabled = true;
        UpdateHp(player.GetHp());//プレイヤーのHPを更新

        //バトル終了後の処理でアイテム操作を初期化
        itemArrangeManager.Init(this, mainCamera);
    }

    /// <summary>
    /// ダンジョン内でインベントリを開く
    /// ボタンから呼出
    /// </summary>
    public void OpenInventory()
    {
        itemArrangeManager.OpenInventoryInDungeon();//インベントリ画面を開く
    }

    /// <summary>
    /// インベントリが閉じた時に起動
    /// ダンジョン入力を開始
    /// </summary>
    public void InventoryClosed()
    {
        //UIPageManagerがあるのでここではInputBlockerを操作しない
        //inputBlocker.InputBlockingDown();
    }

    /// <summary>
    /// 現在のダンジョンのタイプを返す
    /// </summary>
    /// <returns>現在のダンジョンのタイプ</returns>
    public DungeonTypeDefine.DungeonType GetDungeonType()
    {
        return dungeonType;
    }

    /// <summary>
    /// 現在の階層を返す
    /// </summary>
    /// <returns>現在の階層</returns>
    public int GetFloor()
    {
        return floor;
    }

    /// <summary>
    /// ダンジョンのある位置でのある敵のスポーン確率を求める
    /// </summary>
    /// <param name="dungeon">確認するダンジョン</param>
    /// <param name="tier">確認する敵のレア度</param>
    /// <returns>スポーン確率の値.同じ層の敵の合計の中から割合を求める</returns>
    public int GetSpawnRate(TierDefine.Tier tier,bool isTreasure)
    {
        FloorSpawnRate targetRate;

        if (floor == 0)
        {
            if (isTreasure)
            {
                targetRate = enemySpawnRate.stage0TreasureRate;
            }
            else
            {
                targetRate = enemySpawnRate.stage0Rate;
            }
        }else if(floor == 1)
        {
            if (isTreasure)
            {
                targetRate = enemySpawnRate.stage1TreasureRate;
            }
            else
            {
                targetRate = enemySpawnRate.stage1Rate;
            }
        }
        else
        {
            targetRate = enemySpawnRate.stage0Rate;
        }

        switch (tier)
        {
            case TierDefine.Tier.Common:
                return targetRate.common;
            case TierDefine.Tier.Rare:
                return targetRate.rare;
            case TierDefine.Tier.Meta:
                return targetRate.meta;
            default:
                return 0;
        }
    }

    public void SetShowDoorFlag(bool b)
    {
        showDoorFlag = b;
    }

    /// <summary>
    /// 元気を消費する.無くなったらダンジョン失敗
    /// </summary>
    public void ConsumeStamina(int num)
    {
        /*
        stamina-=num;
        dungeonInformation.SetStamina(stamina, staminaMax);
        if (stamina <= 0)
        {
            stamina = 0;
            Debug.Log("元気がなくなった。ダンジョン攻略失敗");
        }
        */
    }

    public void GainStamina(int num)
    {
        this.stamina += num;
        if(stamina>=staminaMax)
        {
            stamina = staminaMax;
        }
        dungeonInformation.SetStamina(stamina, staminaMax);
    }

    public void UpdateHp(int hp)
    {
        dungeonInformation.SetHP(hp, player.GetHpMax());
    }

    public void UseItem(PhysicalItemBase item)
    {
        if(item.ItemType == PhysicalItemTypeDefine.PhysicalItemType.Consumables)
        {//消費アイテムのみ使用可能
            consumablesExecute.Execute(item.BaseItemData.ConsumablesData.EffectList);
        }
    }

    public ScenarioManager GetScenarioManager()
    {
        return scenarioManager;
    }

    #region UIPageManagerの実装

    public void OnPushed()
    {

    }

    public void OnPopped()
    {
        //popされない前提なので、実装しない
    }

    public void OnCovered()
    {
        inputBlocker.InputBlockingUp();
    }

    public void OnBecomeTopPage()
    {
        inputBlocker.InputBlockingDown();
    }

    public void PushSelf()
    {
        //pushされない前提なので、実装しない
    }

    #endregion

    #region test

    public void TestLoseHP()
    {
        player.Damage(5);
    }

    public void TestLoseStamina()
    {
        ConsumeStamina(1);
    }

    #endregion
}
