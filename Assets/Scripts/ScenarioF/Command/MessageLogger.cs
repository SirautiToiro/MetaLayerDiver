using Cysharp.Threading.Tasks;
using ScenarioFlow;
using System.Threading;
using System;
using UnityEngine;
using ScenarioFlow.Scripts.SFText;

public class MessageLogger : IReflectable
{
    [CommandMethod("log message async")]
    [Category("Dialogue")]
    [Description("Show log async")]
    [Snippet("Log {${1:message}} async")]
    public async UniTask LogMessageAsync(string message, CancellationToken cancellationToken)
    {
        try
        {
            await UniTask.Delay(TimeSpan.FromSeconds(2.0f), cancellationToken: cancellationToken);

            Debug.Log(message);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Message canceled!");
            throw;
        }
    }

    [CommandMethod("log message")]
    [Category("Dialogue")]
    [Description("Show log")]
    [Snippet("Log {${1:message}}")]
    public void LogMessage(string message)
    {
        try
        {
            Debug.Log(message);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("Message canceled!");
            throw;
        }
    }

}
