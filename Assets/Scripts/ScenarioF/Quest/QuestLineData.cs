using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static QuestRequest;

public class QuestLineData : SingletonMonoBehaviour<QuestLineData>
{
    //管理しているすべてのQuestLine
    private List<QuestLineAndProgress> questLines;

    //クエストデータの大本.セーブデータがある場合、これから更新
    [SerializeField] private List<QuestLineAndProgress> originalQuestLines;

    /// <summary>
    /// 一連のクエストの情報と、それをどこまで読んだかを格納するクラス
    /// </summary>
    [System.Serializable]
    public class QuestLineAndProgress
    {
        public QuestLine QuestLine;
        //-1なら、出現していない。0なら、最初のクエストが未クリア。
        //1なら、1つ目のクエストをクリア済み(報酬も受け取っている)、2なら2つ目のクエストをクリア済み、というように進行度を管理。
        //ProgressがQuestListの数と同じなら、すべてのクエストをクリアしている状態(表示しない)。

        [SerializeField] private int progress;

        public int Progress
        {
            get { return progress; }
            set
            {
                progress = value;
                DefeatEnemyProgress = 0;//Progressが変わるたびに、敵を倒す系のクエストの進行度をリセットする
            }
        }

        //クエストはクリアしているが、報酬を受け取っていない状態かどうか.などのクエスト状況
        public questLineStatus status;

        //現在表示されているクエストの、敵を倒す系のクエストの進行度を管理するための変数。クエストの内容によって、どのように使うかは変わる。
        public int DefeatEnemyProgress;

        public IQuest GetCurrentQuest()
        {
            if (this.Progress == -1 || this.Progress == this.QuestLine.QuestList.Count)
            {
                return null;
            }
            else
            {
                return QuestLine.QuestList[Progress];
            }
        }

        public void SetQuestStarted()
        {
            IQuest quest = GetCurrentQuest();
            if (quest is not StoryQuest storyQuest)
            {//特殊クエストはストーリーがないと仮定
                this.status = QuestLineData.questLineStatus.Quest;
            }
            else
            {
                if (storyQuest.GetQuestStoryType() == QuestStoryType.Full ||
                    storyQuest.GetQuestStoryType() == QuestStoryType.OnlyInitial)
                {//開始ストーリーがある
                    this.status = QuestLineData.questLineStatus.Story;
                }
                else
                {//開始ストーリーがない
                    this.status = QuestLineData.questLineStatus.Quest;
                }
            }
        }
    }

    [System.Serializable]
    public enum questLineStatus
    {
        Quest,              //クエストをクリアしていない
        CompletedAndstory,  //クエストをクリアし、ストーリーは解放されているが、報酬を受け取っていない状態
        CompletedAndReward, //クエストをクリアし、報酬を受け取っていない状態
        Story,              //クエストが解放されておらず、ストーリーのみがある状態
    }

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

        Init();
    }

    public void Init()
    {
        questLines = new List<QuestLineAndProgress>();
        foreach (var ql in originalQuestLines)
        {
            QuestLineAndProgress newOne = new QuestLineAndProgress()
            {
                QuestLine = ql.QuestLine,
                Progress = ql.Progress,
                status = ql.status,//TODO:データから読み取るのではなく自動で設定する初期化の場合、これを正しく決める。
                DefeatEnemyProgress = ql.DefeatEnemyProgress,
            };
            //クエストを開始状態のステータスへ。Statusの値の不備を防ぐため。
            //For Test
            newOne.SetQuestStarted();
            questLines.Add(newOne);
        }

        if (TestFlags.Instance.useTestDataFlag)
        {//テストデータを使う場合は、セーブデータから読み取らない
            return;
        }

        //セーブデータから読み取り、上書き更新
        var data = SaveDataManager.LoadQuestData();
        if (data is null) return;

        foreach (var ql in data.questLines)
        {
            foreach (var originalQl in questLines)
            {
                if (originalQl.QuestLine.ID == ql.questLineID)
                {
                    originalQl.Progress = ql.progress;
                    originalQl.status = ql.status;
                    originalQl.DefeatEnemyProgress = ql.DefeatEnemyProgress;
                    break;
                }
            }
        }
    }

    public static List<QuestLineAndProgress> GetActiveQuestLines()
    {
        List<QuestLineAndProgress> activeQuestLines = new List<QuestLineAndProgress>();
        foreach (var ql in Instance.questLines)//Instanceから取れるの本当か？
        {
            if (ql.Progress >= 0 && ql.Progress < ql.QuestLine.QuestList.Count)
            {//Progressが-1なら出現していない、QuestListの数と同じならすべてクリアしている状態なので、どちらも表示しない
                activeQuestLines.Add(ql);
            }
        }
        return activeQuestLines;
    }

    /// <summary>
    /// 戦闘終了時に、敵を倒す系のクエストの進捗を更新
    /// </summary>
    /// <param name="enemies"></param>
    public static void MemoryDefeatedEnemy(List<EnemyDataSO> enemies)
    {
        List<QuestLineAndProgress> activeQuestLines = GetActiveQuestLines();

        foreach (var ql in activeQuestLines)
        {
            var currentQuest = ql.GetCurrentQuest();
            if (currentQuest is not StoryQuest sQuest) return;
            if (sQuest.GetQuestRequest().requestType == QuestRequest.RequestType.DefeatEnemies)
            {
                enemies.ForEach(defeatedEnemy =>
                {
                    if (sQuest.GetQuestRequest().RequiredEnemies.IsAnyEnemy)
                    {//どれでもいい場合
                        ql.DefeatEnemyProgress++;
                        Debug.Log("AnyAdd!");
                    }
                    else
                    {//特定の敵を倒す場合
                        if (sQuest.GetQuestRequest().RequiredEnemies.EnemyData == defeatedEnemy)
                        {
                            ql.DefeatEnemyProgress++;
                            Debug.Log("Add!");
                        }
                    }
                });
            }
        }
    }

    /// <summary>
    /// questで指定されたものと一致するクエストを非表示から有効化する
    /// </summary>
    /// <param name="quest"></param>
    /// <returns></returns>
    public static QuestLineAndProgress ActivateQuest(QuestLine quest)
    {
        foreach (var ql in Instance.questLines)
        {
            if (ql.QuestLine == quest)
            {
                ql.Progress = 0;//出現させる
                return ql;
            }
        }

        return null;
    }

    public static string GetRequestText(IQuest quest)
    {
        List<string> requestTexts = new List<string>();
        requestTexts.Add("依頼内容：");

        switch (quest.GetQuestRequest().requestType)
        {
            case RequestType.CollectCards:
                if (quest.GetQuestRequest().RequiredCards != null)
                {
                    foreach (var cardData in quest.GetQuestRequest().RequiredCards)
                    {
                        requestTexts.Add($"・{cardData.CardData.cardName}×{cardData.Stack}");
                    }
                }
                break;
            case RequestType.GatherItems:
                if (quest.GetQuestRequest().RequiredItems != null)
                {
                    foreach (var itemData in quest.GetQuestRequest().RequiredItems)
                    {
                        requestTexts.Add($"・{itemData.ItemData.ItemName}×{itemData.Stack}");
                    }
                }
                break;
            case RequestType.DefeatEnemies:
                if (quest.GetQuestRequest().RequiredEnemies != null)
                {
                    QuestLineAndProgress sameQuestLineAndProgress = null;
                    foreach (var ql in QuestLineData.GetActiveQuestLines())
                    {
                        if (ql.GetCurrentQuest() == quest)
                        {
                            sameQuestLineAndProgress = ql;
                            break;
                        }
                    }

                    if (sameQuestLineAndProgress is null)
                    {
                        requestTexts.Add("・クエスト進行中の情報が見つかりません"); break;
                    }

                    int currentNum = sameQuestLineAndProgress.DefeatEnemyProgress >= quest.GetQuestRequest().RequiredEnemies.Num ? quest.GetQuestRequest().RequiredEnemies.Num : sameQuestLineAndProgress.DefeatEnemyProgress;

                    if (quest.GetQuestRequest().RequiredEnemies.IsAnyEnemy)
                    {
                        requestTexts.Add($"・{quest.GetQuestRequest().RequiredEnemies.Num}体の敵を倒す" +
                            $"{currentNum}/{quest.GetQuestRequest().RequiredEnemies.Num}");
                    }
                    else
                    {
                        requestTexts.Add($"・{quest.GetQuestRequest().RequiredEnemies.EnemyData.enemyName}を{quest.GetQuestRequest().RequiredEnemies.Num}体倒す" +
                                                       $"{currentNum}/{quest.GetQuestRequest().RequiredEnemies.Num}");
                    }
                }
                break;
        }

        return string.Join("\n", requestTexts);
    }

    public static List<QuestLineAndProgress> GetAllQuestLines()
    {
        return Instance.questLines;
    }

    public QuestLine GetQuestLineByID(int id)
    {
        foreach (var ql in questLines)
        {
            if (ql.QuestLine.ID == id)
            {
                return ql.QuestLine;
            }
        }
        return null;
    }
}
