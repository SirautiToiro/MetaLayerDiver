using Cysharp.Threading.Tasks;
using ScenarioFlow;
using System.Threading;
using System;
using UnityEngine;
using TMPro;
using ScenarioFlow.Scripts.SFText;

public class DialogueMessage : IReflectable
{
    private TalkingCharacter talkingCharacter;

    private TextMeshProUGUI messageText;
    private TextMeshProUGUI speakerText;


    public DialogueMessage(TextMeshProUGUI messageText, TextMeshProUGUI speakerText,TalkingCharacter talkingCharacter)
    {
        this.talkingCharacter = talkingCharacter;

        this.messageText = messageText;
        this.speakerText = speakerText;
    }

    [CommandMethod("write dialogue async")]
    [Category("Dialogue")]
    [Description("Show dialogue")]
    [Snippet("{${1:characterName}} say that {${2:message}}")]
    public async UniTask WriteDialogueAsync(string characterName, string message, CancellationToken cancellationToken)
    {
        //左右のどちらかに話者がいるなら、それ以外をグレーにする
        CharacterImageManager.Character character = CharacterImageManager.GetCharacterFromString(characterName);
        bool? pos = talkingCharacter.GetCharacterPos(character);
        if (pos.HasValue)
        {
            talkingCharacter.SetCharacterImageBrightness(!pos.Value, 0.5f,false);
            talkingCharacter.SetCharacterImageBrightness(pos.Value, 1.0f,true);
        }

        try
        {
            messageText.text = $"{message}";

            if(characterName.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                speakerText.text = $"";
            }
            else
            {
                speakerText.text = $"{characterName}";
            }

                // Simulate writing dialogue with a delay
                int messageLength = message.Length;
            for(int i=0;i< messageLength; i++)
            {
                messageText.maxVisibleCharacters = i+1;
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            messageText.text = $"{message}";
            messageText.maxVisibleCharacters = message.Length;
            if (characterName.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                speakerText.text = $"";
            }
            else
            {
                speakerText.text = $"{characterName}";
            }
            throw;
        }
    }

}
