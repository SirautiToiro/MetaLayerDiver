using ScenarioFlow;
using ScenarioFlow.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ItemArrange中の、クエストを管理するパネル
/// </summary>
public class QuestPanel : MonoBehaviour, IUIPage
{
    [SerializeField] private Canvas canvas;
    [SerializeField] ItemArrangeManager itemArrangeManager;
    [SerializeField] private CardArrangeManager cardArrangeManager;

    [SerializeField] private GameObject questPrefab;
    [SerializeField] private Transform questTileParent;

    //UI
    [SerializeField] private TextMeshProUGUI questDescriptionTitleText;
    [SerializeField] private TextMeshProUGUI questDescriptionIntroductionText;
    [SerializeField] private TextMeshProUGUI questDescriptionRequestText;
    [SerializeField] private TextMeshProUGUI questDescriptionRewardText;
    [SerializeField] private TextMeshProUGUI questDescriptionClientText;
    [SerializeField] private Button questButton;
    [SerializeField] private TextMeshProUGUI questButtonText;

    [SerializeField] private RequireCardHolder requiredCardHolder;

    //クエスト画面の入力ブロック用
    [SerializeField] private InputBlocker inputBlocker;

    private int selectedQuestIndex = 0;

    private List<QuestTile> questTiles = new List<QuestTile>();

    [SerializeField] private UIPageManager uiPageManager;

    [SerializeField] private InventoryAndRequireCardsUIPage inventoryAndRequireCardsUIPage;
    [SerializeField] private InventoryAndRequireItemsUIPage inventoryAndRequireItemsUIPage;

    public UIPageManager UIPageManager { get { return uiPageManager; } }

    [SerializeField] private InventoryAndRewardUIPage inventoryAndRewardUIPage;

    [SerializeField] PhysicalItemDataSO goldSO;

    //ItemArrangeの外からの取得になるためInitで取得
    private ScenarioManager scenarioManager;

    public void Init(ScenarioManager scenarioManager)
    {
        canvas.enabled = false;
        this.scenarioManager = scenarioManager;
    }

    private void Open()
    {
        canvas.enabled = true;

        List<QuestLineData.QuestLineAndProgress> activeQuests = QuestLineData.GetActiveQuestLines();
        //作成したクエストを削除
        for (int i = questTiles.Count - 1; i >= 0; i--)
        {
            Destroy(questTiles[i].gameObject);
            questTiles.RemoveAt(i);
        }

        int index = 0;
        //クエストを実体化
        foreach (var ql in activeQuests)
        {
            if (ql.QuestLine.QuestList[ql.Progress] is BranchQuest)
            {
                continue;
            }

            QuestTile questTile = InstantiateQuestTile(ql,index);
            questTiles.Add(questTile);
            index++;
        }

        selectedQuestIndex = 0;
        ShowQuestDescription();
    }

    private void Close()
    {
        canvas.enabled = false;
    }

    public void FinishQuest()
    {
        OpenRewardPanel();
    }

    /// <summary>
    /// クエストで必要な要求のパネルを開く
    /// </summary>
    public void OpenRequestPanel()
    {
        IQuest targetQuest = questTiles[selectedQuestIndex].GetQuest();
        if (targetQuest is BranchQuest) return;//本来は入らないはず

        QuestRequest questRequest = targetQuest.GetQuestRequest();
        if(!(questRequest.requestType==QuestRequest.RequestType.CollectCards||
            questRequest.requestType == QuestRequest.RequestType.GatherItems))
        {//アイテムを集める系のクエストでないなら、終了。(本来は入らないはず)
            return;
        }

        //this.canvas.enabled = false;//自身を見えなくする

        if (questRequest.requestType == QuestRequest.RequestType.CollectCards)
        {//カードを集めるクエスト
            //要求画面をUIにPush
            inventoryAndRequireCardsUIPage.Init(StorageData.DataListToNumList(questRequest.RequiredCards), () => 
            {//終了時処理
                //報酬受け取り画面を表示する
                questTiles[selectedQuestIndex].SetQuestCompleted();
                //再表示
                ShowQuestDescription();
            });
            inventoryAndRequireCardsUIPage.PushSelf();
        }
        else if(questRequest.requestType == QuestRequest.RequestType.GatherItems)
        {//アイテムを集めるクエスト
            inventoryAndRequireItemsUIPage.Init(questRequest.RequiredItems, () =>
            {//終了時処理
                //報酬受け取り画面を表示する
                questTiles[selectedQuestIndex].SetQuestCompleted();
                //再表示
                ShowQuestDescription();
            });
            inventoryAndRequireItemsUIPage.PushSelf();
        }
    }

    public void EndEnemyDefeatQuest()
    {//敵を十分倒しているか判定
        //倒された敵のデータはQuestLineDataにしかないため、そこから取得する
        foreach (var ql in QuestLineData.GetActiveQuestLines())
        {
            if(ReferenceEquals(ql.GetCurrentQuest(), questTiles[selectedQuestIndex].GetQuest()))
            {//現在表示されているクエストと同じデータを探す
                if(ql.DefeatEnemyProgress >= ql.GetCurrentQuest().GetQuestRequest().RequiredEnemies.Num)
                {//十分倒しているなら、クエスト完了
                    questTiles[selectedQuestIndex].SetQuestCompleted();
                    ShowQuestDescription();
                }
            }
        }
    }

    /// <summary>
    /// クエスト報酬のパネルを開く
    /// </summary>
    private void OpenRewardPanel()
    {
        IQuest targetQuest = questTiles[selectedQuestIndex].GetQuest();
        if (targetQuest is BranchQuest) return;//本来は入らないはず

        //this.canvas.enabled = false;//自身を見えなくする

        QuestReward questReward = targetQuest.GetQuestReward();

        List<int> rewardCardData = new List<int>();
        foreach (var item in questReward.RewardCards)
        {
            for(int i = 0; i < item.Stack;i++)
            {
                rewardCardData.Add(item.CardData.serialNum);
            }
        }

        int rewardGold = questReward.RewardGold;

        List<PhysicalItemDataSO> rewardItemData = new List<PhysicalItemDataSO>();
        foreach (var item in questReward.RewardItems)
        {
            for (int i = 0; i < item.Stack; i++)
            {
                rewardItemData.Add(item.ItemData);
            }
        }
        for (int i = 0; i < rewardGold; i++)
        {
            rewardItemData.Add(goldSO);
        }

        //itemArrangeManager.OpenItemDropInventory(rewardCardData, rewardItemData, StashPanel.DropFrameType.QuestReward);
        //ドロップメニューが閉じた後はEndBattleでバトル終了処理。
        //ドロップメニューに必要なドロップ品を渡している。
        inventoryAndRewardUIPage.Init(rewardCardData, rewardItemData, StashPanel.DropFrameType.QuestReward, () => {
            EndQuestReward();
        });

        inventoryAndRewardUIPage.PushSelf();
    }

    public void EndQuestReward()
    {
        //それまで見せていたクエストは終了しているので、更新
        questTiles[selectedQuestIndex].ProgressQuest();
        if(questTiles[selectedQuestIndex].GetQuest() is null)
        {//クエストが出終わったので、最初のクエストを見せる
            //それまでのQuestTileは削除
            Destroy(questTiles[selectedQuestIndex].gameObject);
            questTiles.RemoveAt(selectedQuestIndex);
            selectedQuestIndex = 0;

            //クエストのインデックスを正しくする
            for(int i = 0; i < questTiles.Count; i++)
            {
                questTiles[i].Index = i;
            }
        }

        //BranchQuestの処理
        if (questTiles[selectedQuestIndex].GetQuest() is BranchQuest bQuest)
        {
            //新しいクエストを有効化
            var newQuest = QuestLineData.ActivateQuest(bQuest.GetNextQuestLine());
            //追加
            if (newQuest.QuestLine.QuestList[newQuest.Progress] is BranchQuest)
            {//本来起こらないはず。
                
            }
            else
            {
                QuestTile questTile = InstantiateQuestTile(newQuest, questTiles.Count);
                //クエストを開始状態に
                questTile.SetQuestStarted();
                questTiles.Add(questTile);

                //BranchQuestが連続する場合のため、再度自身を呼び出す
                EndQuestReward();
                return;
            }
        }

        //残っているクエストのStatusをクエスト開始時の状態に
        questTiles[selectedQuestIndex].SetQuestStarted();

        //最初のクエストの詳細を見る
        ShowQuestDescription();
    }

    public void QuestTileClicked(int index)
    {
        selectedQuestIndex = index;
        ShowQuestDescription();
    }

    private void ShowQuestDescription()
    {
        if (questTiles.Count == 0)
        {//クエストがない場合
            questDescriptionTitleText.text = "";
            questDescriptionIntroductionText.text = "";
            questDescriptionRequestText.text = "";
            questDescriptionRewardText.text = "";
            return;
        }

        //クエスト情報を表示
        IQuest quest = questTiles[selectedQuestIndex].GetQuest();
        questDescriptionTitleText.text = quest.QuestTitle;
        questDescriptionIntroductionText.text = quest.IntroductionText;
        questDescriptionRequestText.text = QuestLineData.GetRequestText(quest);
        questDescriptionRewardText.text = quest.GetQuestReward().GetRewardText();
        questDescriptionClientText.text = quest.QuestClient+"：";

        //左のパネルも変更
        questTiles[selectedQuestIndex].SetQuestDescription(quest.QuestTitle, quest.QuestClient);

        //クエストアクションボタンの設定
        switch (questTiles[selectedQuestIndex].GetQuestLineStatus())
        {
            case QuestLineData.questLineStatus.CompletedAndReward:
                //報酬をまだもらっていないなら、報酬ボタン
                questButtonText.text = "報酬";
                questButton.onClick.RemoveAllListeners();
                questButton.onClick.AddListener(OpenRewardPanel);
                break;
            case QuestLineData.questLineStatus.CompletedAndstory:
                //クエストは終了しているがストーリーがある
                //ストーリーがある
                questButtonText.text = "会話";
                questButton.onClick.RemoveAllListeners();
                questButton.onClick.AddListener(StartScenario);
                break;
            case QuestLineData.questLineStatus.Quest:
                if(quest.GetQuestRequest().requestType == QuestRequest.RequestType.DefeatEnemies)
                {
                    //要求がないクエストなら、クエスト完了処理へ
                    questButtonText.text = "完了";
                    questButton.onClick.RemoveAllListeners();
                    questButton.onClick.AddListener(EndEnemyDefeatQuest);
                }
                else if(quest.GetQuestRequest().requestType == QuestRequest.RequestType.CollectCards||
                    quest.GetQuestRequest().requestType == QuestRequest.RequestType.GatherItems)
                {
                    //クエスト提示中。提出ボタン。
                    questButtonText.text = "提出";
                    questButton.onClick.RemoveAllListeners();
                    questButton.onClick.AddListener(OpenRequestPanel);
                }
                if (quest.GetQuestRequest().requestType == QuestRequest.RequestType.None)
                {
                    //クエストを持たないなら即座に完了
                    questTiles[selectedQuestIndex].SetQuestCompleted();
                    ShowQuestDescription();
                }
                break;
            case QuestLineData.questLineStatus.Story:
                //クエストが出題されておらず、最初のストーリーを開始
                questButtonText.text = "会話";
                questButton.onClick.RemoveAllListeners();
                questButton.onClick.AddListener(StartScenario);
                break;

        }
    }

    public void StartScenario()
    {
        QuestLineData.questLineStatus status = questTiles[selectedQuestIndex].GetQuestLineStatus();
        if(status == QuestLineData.questLineStatus.CompletedAndstory)
        {
            //クエストは終了しているがストーリーがある状態なら、ストーリーを開始
            ScenarioScript script = questTiles[selectedQuestIndex].GetQuest().GetEndingScenarioScript();
            if (script is null) return;
            scenarioManager.Init(script, EndScenario);
            scenarioManager.PushSelf();
        }
        else if(status == QuestLineData.questLineStatus.Story)
        {
            //クエストが出題されている状態なら、最初のストーリーを開始
            ScenarioScript script = questTiles[selectedQuestIndex].GetQuest().GetInitialScenarioScript();
            if (script is null) return;
            scenarioManager.Init(script, EndScenario);
            scenarioManager.PushSelf();
        }
    }

    /// <summary>
    /// シナリオ終了後の処理。クエストの状態を更新して、クエストの内容を更新する。
    /// </summary>
    public void EndScenario()
    {
        QuestLineData.questLineStatus status = questTiles[selectedQuestIndex].GetQuestLineStatus();
        if (status == QuestLineData.questLineStatus.CompletedAndstory)
        {
            //クエストは終了しているがストーリーがある状態なら、報酬
            questTiles[selectedQuestIndex].SetQuestLineStatus(QuestLineData.questLineStatus.CompletedAndReward);
        }
        else if (status == QuestLineData.questLineStatus.Story)
        {
            //クエストが出題されている状態なら、クエストを開始 
            questTiles[selectedQuestIndex].SetQuestLineStatus(QuestLineData.questLineStatus.Quest);
        }
        ShowQuestDescription();
    }

    private QuestTile InstantiateQuestTile(QuestLineData.QuestLineAndProgress questlineAndProgress, int index)
    {
        //QuestTileをインスタンス化
        GameObject questObj = Instantiate(questPrefab, questTileParent.position, Quaternion.identity, questTileParent);
        QuestTile questTile = questObj.GetComponent<QuestTile>();
        
        questTile.Init(questlineAndProgress, index,this);
        return questTile;
    }

    public void OnPushed()
    {
        this.Open();
    }

    public void OnPopped()
    {
        Close();
    }

    public void PushSelf()
    {
        UIPageManager.PushUIPage(this);
    }

    public void OnCovered()
    {
        inputBlocker.InputBlockingUp();
    }

    public void OnBecomeTopPage()
    {
        inputBlocker.InputBlockingDown();
    }
}
