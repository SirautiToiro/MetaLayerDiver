using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// ボス出現演出のパネルが複数周囲から現れる演出のひとつのパネル
/// </summary>
public class BossEnteringPiece : MonoBehaviour
{
    //StartPos->EndPosと動く
    [SerializeField] Vector3 StartPos;
    [SerializeField] Vector3 EndPos;

    /// <summary>
    /// このピースに移動指示をする
    /// </summary>
    /// <param name="phase">演出のフェーズ</param>
    public void Move(int phase)
    {
        if(phase == 0)
        {
            this.transform.DOLocalMove(EndPos, DungeonConstants.BossEnteringPieceMoveTime0)
                .SetEase(Ease.OutBack);
        }
        else if(phase == 1)
        {
            this.transform.DOLocalMove(EndPos, DungeonConstants.BossEnteringPieceMoveTime1)
                .SetEase(Ease.OutBack);
        }
    }

    /// <summary>
    /// このピースの座標をスタート位置にする
    /// </summary>
    public void ResetPos()
    {
        this.transform.localPosition = StartPos;
    }

    /// <summary>
    /// このピースの移動処理を中断する
    /// </summary>
    public void MoveComplete()
    {
        this.transform.DOComplete();
    }
}
