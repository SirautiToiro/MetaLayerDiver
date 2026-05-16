using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using ScenarioFlow.Tasks;
using System.Threading;
using UnityEngine;

public class EscapekeyCancellationNotifier : ICancellationNotifier
{
    private ScenarioManager scenarioManager;
    public EscapekeyCancellationNotifier(ScenarioManager manager)
    {
        this.scenarioManager = manager;
    }

    public UniTask NotifyCancellationAsync(CancellationToken cancellationToken)
    {
        return UniTaskAsyncEnumerable.EveryUpdate()
            .Select(_ => Input.GetKeyDown(KeyCode.Escape))
            .Where(x => x)
            .Do(_ => Debug.Log("Escapekey pushed!"))
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);
    }
}