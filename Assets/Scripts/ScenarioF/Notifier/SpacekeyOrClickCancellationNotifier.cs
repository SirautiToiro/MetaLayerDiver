using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using ScenarioFlow.Tasks;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class SpacekeyOrClickCancellationNotifier : ICancellationNotifier
{
    private ScenarioManager scenarioManager;
    private GameObject textWindow;
    private Camera mainCamera;
    public SpacekeyOrClickCancellationNotifier(ScenarioManager manager, GameObject textWindow, Camera mainCamera)
    {
        this.scenarioManager = manager;
        this.mainCamera = mainCamera;
        this.textWindow = textWindow;
    }

    public UniTask NotifyCancellationAsync(CancellationToken cancellationToken)
    {
        //ManagerのTextReadingFlagがtrueのときはスペースキーを押すとシナリオのキャンセルが発生するようにする
        return UniTaskAsyncEnumerable.EveryUpdate()
            .Select(_ => (scenarioManager.TextReadingFlag) &&
            (Input.GetKeyDown(KeyCode.Space) ||
            (Input.GetMouseButtonDown(0)&&IsHitTextWindow())
            ))
            .Where(x => x)
            .Do(_ => { scenarioManager.TextReadingFlag = false; })
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
