using Cysharp.Threading.Tasks;
using ScenarioFlow;
using System.Threading;
using System;
using UnityEngine;
using UnityEngine.UI;
using ScenarioFlow.Scripts.SFText;

public class ShowCharacterImage : IReflectable
{
    private TalkingCharacter talkingCharacter;

    public ShowCharacterImage(CharacterImageManager characterImageManager,TalkingCharacter talkingCharacter)
    {
        this.talkingCharacter = talkingCharacter;
    }

    [CommandMethod("change character image")]
    [Category("Dialogue")]
    [Description("Change character image")]
    [Snippet("Change image to {${1:character}}.")]
    [Snippet("Right:{${2:isRight}} inversion:{${3:inversion}}")]
    public void ChangeCharacterImage(CharacterImageManager.Character character, bool isRight,bool inversion)
    {
        talkingCharacter.ChangeCharacterImage(character, isRight, inversion);
    }
}
