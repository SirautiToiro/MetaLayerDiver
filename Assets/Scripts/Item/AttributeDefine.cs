using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードや装備、アイテムなどの属性を記す
/// </summary>
[System.Serializable]
public class AttributeDefine
{
    public AttributeDefine(Attribute a)
    {
        this.attribute = a;
    }

    [Header("属性名")]
    public Attribute attribute;

    public enum Attribute
    {
        Psycho,
        Faith,
        Energy,
        Pyro,
        Create,
        Physics,
        Mind,
    }

    readonly public static Dictionary<Attribute, string> Dic_AttributeName=new Dictionary<Attribute, string>()
    {
        {Attribute.Psycho, "念動"} ,
        {Attribute.Faith,"信仰"},
        {Attribute.Energy,"エネルギー"},
        {Attribute.Pyro,"火炎" },
        {Attribute.Create,"生成" },
        {Attribute.Physics,"物理"},
        {Attribute.Mind,"精神"},
    };
}
