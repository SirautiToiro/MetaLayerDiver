using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// Playerに関係するアニメーション
/// </summary>
public class PlayerAnimation : MonoBehaviour
{
    /// <summary>
    /// プレイヤーの出現時のアニメーション
    /// </summary>
    /// <param name="playerUI">動かすPlayerUI</param>
    public void PlayerAppeearAnimation(PlayerUI playerUI)
    {
        
        //Playerを少し上に動かしておく
        Vector3 pos = playerUI.gameObject.transform.localPosition;
        pos.y += BattleConstants.CharacterAppearHeight;
        playerUI.gameObject.transform.localPosition = pos;

        //動かす
        playerUI.gameObject.transform.DOLocalMoveY(-1 * BattleConstants.CharacterAppearHeight, BattleConstants.CharacterAppearTime)
            .SetRelative(true).SetLink(playerUI.gameObject); ;
        
    }
}
