using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static QuestLineData;

/// <summary>
/// クエスト概要が表示されるタイル
/// </summary>
public class QuestTile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questNameText;
    [SerializeField] private TextMeshProUGUI questClientText;

    private QuestLineData.QuestLineAndProgress questlineAndProgress;

    //クエスト画面で何番目か
    private int index;

    public int Index { get { return index; } set { index = value; } }

    private QuestPanel questPanel;

    public void Init(QuestLineData.QuestLineAndProgress questlineAndProgress,int index,QuestPanel questPanel)
    {
        this.questlineAndProgress = questlineAndProgress;
        this.questPanel = questPanel;
        this.index = index;

        IQuest quest = questlineAndProgress.GetCurrentQuest();

        string title, client;
        if (quest is StoryQuest storyQuest)
        {
            title = storyQuest.QuestTitle;
            client = storyQuest.QuestClient;
        }//TODO:他のクエストタイプも追加
        else
        {
            title = "Unknown Quest";
            client = "Unknown Client";
        }

        questNameText.text = title;
        questClientText.text = client;
    }

    public void SetQuestDescription(string name,string client)
    {
        questNameText.text = name;
        questClientText.text = client;
    }

    public void OnClicked()
    {
        questPanel.QuestTileClicked(index);
    }

    public IQuest GetQuest()
    {
        return questlineAndProgress.GetCurrentQuest();
    }

    public QuestLineData.questLineStatus GetQuestLineStatus()
    {
        return questlineAndProgress.status;
    }   

    public void SetQuestLineStatus(QuestLineData.questLineStatus status)
    {
        this.questlineAndProgress.status = status;
    }

    public void SetQuestStarted()
    {
        IQuest quest = questlineAndProgress.GetCurrentQuest();
        if (quest is not StoryQuest storyQuest)
        {//特殊クエストはストーリーがないと仮定
            this.questlineAndProgress.status = QuestLineData.questLineStatus.Quest;
        }
        else
        {
            if(storyQuest.GetQuestStoryType() == QuestStoryType.Full||
                storyQuest.GetQuestStoryType() == QuestStoryType.OnlyInitial)
            {//開始ストーリーがある
                this.questlineAndProgress.status = QuestLineData.questLineStatus.Story;
            }
            else
            {//開始ストーリーがない
                this.questlineAndProgress.status = QuestLineData.questLineStatus.Quest;
            }
        }
    }

    public void SetQuestCompleted()
    {

        IQuest quest = questlineAndProgress.GetCurrentQuest();

        if(quest is not StoryQuest storyQuest)
        {//特殊クエストはストーリーがないと仮定
            this.questlineAndProgress.status = QuestLineData.questLineStatus.CompletedAndReward;
        }
        else
        {
            if(storyQuest.GetQuestStoryType() == QuestStoryType.Full||
                storyQuest.GetQuestStoryType() == QuestStoryType.OnlyEnding)
            {//エンディングストーリーがある
                this.questlineAndProgress.status = QuestLineData.questLineStatus.CompletedAndstory;
            }
            else
            {//エンディングストーリーがない
                this.questlineAndProgress.status = QuestLineData.questLineStatus.CompletedAndReward;

            }
        }
    }

    //見せるクエストのインデックスを一つ進める
    public void ProgressQuest()
    {
        this.questlineAndProgress.Progress++;
    }
}
