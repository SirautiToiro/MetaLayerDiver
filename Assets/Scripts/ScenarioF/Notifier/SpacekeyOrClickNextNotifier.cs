using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using ScenarioFlow.Tasks;
using System.Threading;
using UnityEngine;

public class SpacekeyOrClickNextNotifier : INextNotifier
{
    private ScenarioManager scenarioManager;
    private GameObject textWindow;
    private Camera mainCamera;

    public SpacekeyOrClickNextNotifier(ScenarioManager manager, GameObject textWindow,Camera mainCamera)
    {
        this.scenarioManager = manager;
        this.textWindow = textWindow;
        this.mainCamera = mainCamera;
    }

    public UniTask NotifyNextAsync(CancellationToken cancellationToken)
    {
        return UniTaskAsyncEnumerable.EveryUpdate()
            //ManagerのTextReadingFlagがtrueのときはスペースキーを押しても次のシナリオに進まないようにする(キャンセル優先)
            //また、フラグがうまく変更されないときの保険として、フラグがtrueのときにすべての文字が表示されている状態ならシナリオ進行
            .Select(_ => (!scenarioManager.TextReadingFlag || scenarioManager.IsShowedAllMessage()) &&
            (Input.GetKeyDown(KeyCode.Space) ||
            (Input.GetMouseButtonDown(0)&&IsHitTextWindow())
            )
            )
            .Where(x => x)
            .Do(_ => { scenarioManager.TextReadingFlag = true; })
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }

    private bool IsHitTextWindow()
    {
        //マウスのスクリーン座標を取得する
        // マウス位置を取得
        Vector2 tapPos = Input.mousePosition;
        Vector3 mouseScreenPoint = new Vector3(tapPos.x, tapPos.y, 10);

        // メインカメラから上記で取得した座標に向けてRayを飛ばす
        Ray ray = mainCamera.ScreenPointToRay(mouseScreenPoint);

        foreach (RaycastHit2D hit in Physics2D.RaycastAll(ray.origin, ray.direction, 10.0f))
        {
            if (!hit.collider)
            {//コライダーのないオブジェクトなら
                continue;
            }

            // 当たったオブジェクトがテキストウィンドウなら、シナリオ進行
            var hitObj = hit.collider.gameObject;
            if (hitObj == textWindow)
            {
                return true;
            }
        }
        return false;
    }
}
