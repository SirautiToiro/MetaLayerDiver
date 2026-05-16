using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ShopMessageData
{
    public string Key;
    [Multiline(3)]
    public List<string> Messages;
}
