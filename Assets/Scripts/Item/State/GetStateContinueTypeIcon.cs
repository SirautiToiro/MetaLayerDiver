using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 状態異常の継続タイプを受け取り、それに対応するアイコンを返す
/// </summary>
public class GetStateContinueTypeIcon : MonoBehaviour
{
    [SerializeField] Sprite decreaseIcon;
    [SerializeField] Sprite constantIcon;
    [SerializeField] Sprite eternalIcon;

    /// <summary>
    /// 状態異常の継続タイプを受け取り、それに対応するアイコンを返す
    /// </summary>
    /// <param name="state">状態異常</param>
    /// <returns>対応するアイコン</returns>
    public Sprite GetIcon(IState state)
    {
        if(state is StateContinueTypeDecreaseBase)
        {
            return decreaseIcon;
        }else if(state is StateContinueTypeConstantBase)
        {
            return constantIcon;
        }else if(state is StateContinueTypeEternalBase)
        {
            return eternalIcon;
        }

        Debug.Log("ContinueType Error:GetStateContinueTypeIcon");
        return constantIcon;
    }
}
