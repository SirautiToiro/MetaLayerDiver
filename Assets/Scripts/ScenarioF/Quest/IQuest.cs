using ScenarioFlow.Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 連続し、分岐するクエスト群の中で、一つのクエストを表す基底クラス
/// あるいは、クエスト終了時に解放される新しいQuestLineへのジャンプも含む。
/// 一連のQuestを格納したQuestLineが必要
/// </summary>
public interface IQuest
{
    public string QuestTitle { get; }
    public string QuestClient { get; }

    public string IntroductionText { get; }

    public QuestStoryType? GetQuestStoryType();

    public ScenarioScript GetInitialScenarioScript();
    public  ScenarioScript GetEndingScenarioScript();
    public QuestRequest GetQuestRequest();

    public QuestReward GetQuestReward();
}

[System.Serializable]
public enum QuestStoryType
{
    Full,       //初めと終わりのシナリオ両方を持つ
    OnlyInitial,//初めのシナリオのみ
    OnlyEnding, //終わりのシナリオのみ
    Temporary,  //クエスト画面でのみ文章の表現があるもの。シナリオを持たない
}

/// <summary>
/// ストーリーを持つクエスト
/// つまり、特殊クエストでないもの
/// </summary>
/*
//なぜかこれを経由するとsubclassselectorがうまく動かないので、直接StoryQuestに実装することにした。
public abstract class StoryQuestBase
{
    public abstract string QuestTitle { get; }
    public abstract string QuestClient { get; }

    public abstract ScenarioScript GetInitialScenarioScript();
    public abstract ScenarioScript GetEndingScenarioScript();
    public abstract QuestRequest GetQuestRequest();

    public abstract QuestStoryType GetQuestStoryType();
}
*/

/// <summary>
/// ストーリー上で発生するクエスト
/// </summary>
[System.Serializable]
public class StoryQuest : IQuest
{
    [Header("クエストのタイトル")]
    [SerializeField] string questTitle;

    public string QuestTitle { get { return questTitle; } }

    [Header("クエストのタイプ")]
    [SerializeField] QuestStoryType questStoryType;

    [Header("イントロダクション")]
    [Multiline(8)]
    [SerializeField]string introductionText;

    public string IntroductionText { get { return introductionText; } }
    

    [Header("クエストの発行者")]
    [SerializeField] string questClient;

    public  string QuestClient { get { return questClient; } }

    //このクエストで最初に読むシナリオスクリプト
    [Header("クエスト開始シナリオ")]
    [SerializeField]
    private ScenarioScript initialScenarioScript;

    //終了時に読むシナリオスクリプト
    [Header("クエスト終了シナリオ")]
    [SerializeField]
    private ScenarioScript endingScenarioScript;

    //クエストの達成条件
    [Header("クエスト達成条件")]
    [SerializeField]
    private QuestRequest questRequest;

    [Header("クエスト報酬")]
    [SerializeField]
    private QuestReward questReward;

    public ScenarioScript GetEndingScenarioScript()
    {
        return endingScenarioScript;
    }

    public ScenarioScript GetInitialScenarioScript()
    {
        return initialScenarioScript;
    }

    public  QuestRequest GetQuestRequest()
    {
        return questRequest;
    }

    public QuestStoryType? GetQuestStoryType()
    {
        return questStoryType;
    }

    public QuestReward GetQuestReward()
    {
        return questReward;
    }
}

/// <summary>
/// QuestLineを新しく発生させることを意味するクエスト
/// </summary>
[System.Serializable]
public class BranchQuest : IQuest
{
    [Header("分岐するQuestLine")]
    [SerializeField] QuestLine nextQuestLine;

    public string QuestTitle { get { return null; } }

    public string QuestClient { get { return null; } }

    public string IntroductionText { get { return null; } }

    public ScenarioScript GetEndingScenarioScript()
    {
        return null;
    }

    public ScenarioScript GetInitialScenarioScript()
    {
        return null;
    }

    public QuestRequest GetQuestRequest()
    {
        return null;
    }

    public QuestStoryType? GetQuestStoryType()
    {
        return null;
    }

    public QuestReward GetQuestReward()
    {
        return null;
    }

    public QuestLine GetNextQuestLine()
    {
        return nextQuestLine;
    }
}
