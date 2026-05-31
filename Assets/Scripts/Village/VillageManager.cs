using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class VillageManager : MonoBehaviour,IBaseUIPage
{
    [SerializeField] ItemArrangeManager itemArrangeManager;
    [SerializeField] VillageSceneManager villageSceneManager;

    [SerializeField] InputBlocker inputBlocker;
    [SerializeField] Camera mainCamera;

    [SerializeField] Canvas villageMainCanvas;
    [SerializeField] Canvas pubCanvas;
    [SerializeField] Canvas marketCanvas;

    [SerializeField] VillageShopData villageShopData;

    [SerializeField] UIPageManager uiPageManager;

    [SerializeField] ScenarioManager scenarioManager;

    public UIPageManager UIPageManager { get { return uiPageManager; } }

    [SerializeField] private AudioClip villageBGM;

    [SerializeField] private Button dungeonButton;

    [SerializeField] private Image dungeonButtonSeal;

    void Start()
    {
        //BGM再生
        GameManager.SetAudio(villageBGM);

        Init();

        villageShopData.SetShopData();
    }

    /// <summary>
    /// 初期化処理。村に入るとき、村のほか画面からの遷移に呼び出す
    /// </summary>
    public void Init()
    {
        villageMainCanvas.enabled = true;
        pubCanvas.enabled = false;
        marketCanvas.enabled = false;

        itemArrangeManager.Init(this, mainCamera);

        if (!SettingManager.IsCanEnterDungeon&&!TestFlags.Instance.testMode)
        {//ダンジョンに入れない
            dungeonButton.interactable = false;
            dungeonButtonSeal.enabled = true;
        }
        else
        {
            dungeonButton.interactable = true;
            dungeonButtonSeal.enabled = false;
        }
    }

    /// <summary>
    /// ボタンからインベントリを開く
    /// </summary>
    public void OpenInventory()
    {
        itemArrangeManager.OpenInventoryInVillage();
    }

    /// <summary>
    /// 店画面を開く
    /// </summary>
    /// <param name="shopTypeInt">店のタイプに対応するint.enumは設定できないため</param>
    public void OpenShop(int shopTypeInt)
    {
        ShopTypeDefine.ShopType shopType = (ShopTypeDefine.ShopType)shopTypeInt;
        itemArrangeManager.OpenShop(shopType);
    }

    public void OpenStoryQuest()
    {
        itemArrangeManager.OpenStoryQuest();
    }

    /// <summary>
    /// インベントリが閉じた時に起動
    /// </summary>
    public void InventoryClosed()
    {
    }

    public void GoDungeonButton()
    {
        villageSceneManager.GoDungeonScene();
    }

    /// <summary>
    /// 酒場画面に移動
    /// </summary>
    public void GoPubButton()
    {
        villageMainCanvas.enabled = false;
        pubCanvas.enabled = true;
        marketCanvas.enabled = false;
    }

    public void GoVillageButton() { 
        villageMainCanvas.enabled = true;
        pubCanvas.enabled = false;
        marketCanvas.enabled = false;
        Init();
    }

    public void GoMarketButton()
    {
        villageMainCanvas.enabled = false;
        pubCanvas.enabled = false;
        marketCanvas.enabled = true;
    }

    public VillageShopData GetVillageShopData()
    {
        return villageShopData;
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
}
