using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterImageManager : MonoBehaviour
{
    public enum Character
    {
        MultiTasker,    //マルチタスカー
        Kyomeigen,      //共鳴弦
        Sakuzuka,       //作図家
        SinkoMusya,     //信仰武者
        Kosyomen,       //哄笑面
        BarMaster,      //バーマスター
        Clown_San,      //ピエロさん
        Ka_doya,        //カード屋
        Tenbinya,        //天秤屋
        Null,           //なし
    }

    [System.Serializable]
    private class CharacterImage
    {
        public Character character;
        public Sprite image;
    }

    [SerializeField] List<CharacterImage> characterImageList;

    //日本語名からCharacter列挙型への変換辞書
    readonly public static Dictionary<string,Character> Dic_CharacterName = new Dictionary<string, Character>()
    {
        {"マルチタスカー", Character.MultiTasker },
        {"共鳴弦", Character.Kyomeigen },
        {"作図家", Character.Sakuzuka },
        {"信仰武者", Character.SinkoMusya },
        {"哄笑面", Character.Kosyomen },
        {"バーマスター", Character.BarMaster },
        {"ピエロさん", Character.Clown_San },
        {"カード屋", Character.Ka_doya },
        {"カード商", Character.Ka_doya },
        {"天秤屋", Character.Tenbinya },
        { "Null",Character.Null},
    };

    public static Character GetCharacterFromString(string name)
    {
        if (Dic_CharacterName.ContainsKey(name))
        {
            return Dic_CharacterName[name];
        }
        return Character.Null;
    }

    public Sprite GetImage(Character character)
    {
        foreach (var characterImage in characterImageList)
        {
            if (characterImage.character == character)
            {
                return characterImage.image;
            }
        }
        return null;
    }
}
