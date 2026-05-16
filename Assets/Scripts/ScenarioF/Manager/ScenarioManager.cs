using Cysharp.Threading.Tasks;
using ScenarioFlow;
using ScenarioFlow.Scripts;
using ScenarioFlow.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

/// <summary>
/// 会話シーンを実装するManager.会話の表示場所などを管理する。
/// IUIPageインターフェースを実装しているため、UIPageManagerで管理されるUIページとしても機能する。
/// </summary>

public class ScenarioManager : MonoBehaviour,IUIPage
{
    [SerializeField] CharacterImageManager characterImageManager;
    [SerializeField] TalkingCharacter talkingCharacter;

    //会話メッセージが表示される場所
    [SerializeField] TextMeshProUGUI messageText;

    //会話者が表示される場所
    [SerializeField] TextMeshProUGUI speakerText;

    [SerializeField] private UIPageManager uiPageManager;

    [SerializeField] InputBlocker inputBlocker;

    public UIPageManager UIPageManager { get { return uiPageManager; } }

    private ScenarioScript roadingScenarioScript;

    private Action onCompletion = null;

    [SerializeField]  private Canvas canvas;

    private Camera mainCamera;
    [SerializeField] private GameObject textWindow;

    //Trueのとき、シナリオのテキストの一文字ずつの表示が進行している状態。
    //これがtrueのとき、スペースキーを押すと、テキストの一文字ずつの表示が一気に進む。
    //falseのとき、スペースキーを押すと、次のシナリオに進む。
    public bool TextReadingFlag{ get; set; }

private void Start()
    {
        //自身を非表示に
        canvas.enabled = false;
        roadingScenarioScript = null;
        mainCamera = Camera.main;
    }

    /// <summary>
    /// シナリオを表示する。
    /// </summary>
    /// <param name="scenariaScript">表示する対象のシナリオスクリプト</param>
    /// <returns>UniTaskVoid</returns>
    public async UniTaskVoid ShowScenario(ScenarioScript scenarioScript)
    {
        //自身を表示する
        canvas.enabled = true;

        // シナリオブックを実行するシステムを構成
        ScenarioTaskExecutor scenarioTaskExecutor = new(
            new SpacekeyOrClickNextNotifier(this,textWindow,mainCamera),
            //new EscapekeyCancellationNotifier()
            new SpacekeyOrClickCancellationNotifier(this, textWindow, mainCamera)
            );
        ScenarioBookReader scenarioBookReader = new(scenarioTaskExecutor);
        // シナリオブックをシナリオスクリプトから生成する変換器を構成
        ScenarioBookPublisher scenarioBookPublisher = new(
            new IReflectable[]
            {
                // デコーダ
                new CancellationTokenDecoder(scenarioTaskExecutor),
                new PrimitiveDecoder(),
                new CharacterImageDecoder(),
                new BoolDecoder(),
                // コマンド
                new MessageLogger(),
                new DialogueMessage(messageText, speakerText,talkingCharacter),
                new ShowCharacterImage(characterImageManager,talkingCharacter),
            });
        // シナリオスクリプトをシナリオブックに変換
        ScenarioBook scenarioBook = scenarioBookPublisher.Publish(scenarioScript);
        //最初はテキストが進んでいる
        TextReadingFlag = true;
        try
        {
            // シナリオブックを実行
            await scenarioBookReader.ReadAsync(scenarioBook, this.GetCancellationTokenOnDestroy());
        }
        finally
        {
            // ScenarioTaskExecutorクラスはIDisposableインターフェースを実装する
            scenarioTaskExecutor.Dispose();
            uiPageManager.PopUIPage();
        }
    }

    public void Init(ScenarioScript scenario,Action onCompletion)
    {
        this.roadingScenarioScript = scenario;
        this.onCompletion = onCompletion;
    }

    public void OnPushed()
    {
        if (roadingScenarioScript is null) return;
        ShowScenario(roadingScenarioScript).Forget();
    }

    public void OnPopped()
    {
        onCompletion?.Invoke();
        roadingScenarioScript = null;
        onCompletion = null;
        //自身を非表示に
        canvas.enabled = false;
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
        uiPageManager.PushUIPage(this);
    }

    /// <summary>
    /// 表示するべきすべての文字を表示し終わった状態か
    /// </summary>
    /// <returns>すべて表示しているならTrue</returns>
    public bool IsShowedAllMessage()
    {
        if(messageText.maxVisibleCharacters >= messageText.text.Length)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
