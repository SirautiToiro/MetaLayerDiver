using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//会話の際のキャラクターの立ち絵を管理するクラス
public class TalkingCharacter : MonoBehaviour
{
    [SerializeField] private CharacterImageManager characterImageManager;

    //左右に配置する立ち絵
    [SerializeField] private Image imageRight;
    [SerializeField] private Image imageLeft;

    private CharacterImageManager.Character currentRightCharacter = CharacterImageManager.Character.Null;
    private CharacterImageManager.Character currentLeftCharacter = CharacterImageManager.Character.Null;

    [SerializeField] private Image ArrowRight;
    [SerializeField] private Image ArrowLeft;

    public void ChangeCharacterImage(CharacterImageManager.Character character, bool isRight, bool inversion)
    {
        if (isRight)
        {
            currentRightCharacter = character;

            if (character == CharacterImageManager.Character.Null)
            {
                imageRight.sprite = null;
                imageRight.gameObject.SetActive(false);
                return;
            }
            else
            {
                imageRight.gameObject.SetActive(true);
            }

            imageRight.sprite = characterImageManager.GetImage(character);
            if (inversion)
            {
                imageRight.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                imageRight.transform.localScale = new Vector3(1, 1, 1);
            }
        }
        else
        {
            currentLeftCharacter = character;

            if (character == CharacterImageManager.Character.Null)
            {
                imageLeft.sprite = null;
                imageLeft.gameObject.SetActive(false);
                return;
            }
            else
            {
                imageLeft.gameObject.SetActive(true);
            }

            imageLeft.sprite = characterImageManager.GetImage(character);
            if (inversion)
            {
                imageLeft.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                imageLeft.transform.localScale = new Vector3(1, 1, 1);
            }
        }

    }

    /// <summary>
    /// 会話中に会話者に応じて立ち絵の明るさを変える
    /// </summary>
    /// <param name="IsRight">右であるか</param>
    /// <param name="brightness">明るさ</param>
    /// <param name="isArrow">矢印を表示するか(brightnessの値と連動するはずだが、途中修正のため)</param>
    public void SetCharacterImageBrightness(bool IsRight, float brightness,bool isArrow)
    {
        if (IsRight)
        {
            Color color = new Color(brightness, brightness, brightness, imageRight.color.a);
            imageRight.color = color;

            if (isArrow)
            {
                ArrowRight.gameObject.SetActive(true);
            }
            else
            {
                ArrowRight.gameObject.SetActive(false);
            }
        }
        else
        {
            Color color = new Color(brightness, brightness, brightness, imageLeft.color.a);
            imageLeft.color = color;

            if (isArrow)
            {
                ArrowLeft.gameObject.SetActive(true);
            }
            else
            {
                ArrowLeft.gameObject.SetActive(false);
            }
        }
    }

    public bool? GetCharacterPos(CharacterImageManager.Character character)
    {
        if (character == currentRightCharacter)
        {
            return true;
        }
        else if (character == currentLeftCharacter)
        {
            return false;
        }
        return null;
    } 
}
