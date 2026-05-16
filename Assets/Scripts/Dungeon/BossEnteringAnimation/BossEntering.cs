using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using System;

public class BossEntering : MonoBehaviour
{
    //第一弾で動くピース
    [SerializeField] List<BossEnteringPiece> bossEnteringPieces0 = new List<BossEnteringPiece>();
    //第二段で動くピース
    [SerializeField] List<BossEnteringPiece> bossEnteringPieces1 = new List<BossEnteringPiece>();

    [SerializeField] Transform closeRightTransform;
    [SerializeField] Transform closeLeftTransform;

    public void SetActive(bool b)
    {
        this.gameObject.SetActive(b);
    }

    /// <summary>
    /// ピースの座標を全て初期位置に戻す
    /// </summary>
    public void PosReset()
    {
        PiecePosReset();
        ClosePosReset();
    }

    private void PiecePosReset()
    {
        //位置リセット
        foreach (BossEnteringPiece piece in bossEnteringPieces0)
        {
            piece.ResetPos();
        }
        foreach (BossEnteringPiece piece in bossEnteringPieces1)
        {
            piece.ResetPos();
        }
    }

    private void ClosePosReset()
    {
        //閉じる扉は閉まった位置のX座標が0になるように調整してある
        //初期位置に設定
        closeRightTransform.localPosition = new Vector3(DungeonConstants.BossEnteringCloseX, 0, 0);
        closeLeftTransform.localPosition = new Vector3(-1*DungeonConstants.BossEnteringCloseX, 0, 0);
    }

    /// <summary>
    /// ボスの出現演出を実行する。
    /// 実行終了後にcallbackの関数を発生させる
    /// </summary>
    /// <param name="sceneChange">途中で行うシーン切り替えの動作</param>
    /// <param name="callback">実行終了後に行う関数</param>
    public void PlayEntering(Action sceneChange,Action callback)
    {
        Sequence sequence = DOTween.Sequence();
        //それぞれのピースに対してアニメーションを実行する
        //実行したあと少し待つ
        foreach (BossEnteringPiece piece in bossEnteringPieces0)
        {
            sequence.AppendCallback(() => piece.Move(0));
            sequence.AppendInterval(DungeonConstants.BossEnteringPieceWaitTime0);
        }

        //フェーズの間の時間
        sequence.AppendInterval(DungeonConstants.BossEnteringPhaseTime);

        //第二フェーズ
        foreach (BossEnteringPiece piece in bossEnteringPieces1)
        {
            sequence.AppendCallback(() => piece.Move(1));
            sequence.AppendInterval(DungeonConstants.BossEnteringPieceWaitTime1);
        }

        //扉を閉じる
        sequence.Append(closeRightTransform.DOLocalMoveX(0, DungeonConstants.BossEnteringCloseTime));
        sequence.Join(closeLeftTransform.DOLocalMoveX(0, DungeonConstants.BossEnteringCloseTime));

        //待つ
        sequence.AppendInterval(DungeonConstants.BossEnteringCloseWaitTime);

        //フェーズ1の移動を中断する
        foreach (BossEnteringPiece piece in bossEnteringPieces1)
        {
            piece.MoveComplete();
        }
        sequence.AppendCallback(() =>
        {
            PiecePosReset();//ピースの位置をリセット
        });

        //シーン切り替えの実行
        sequence.AppendCallback(() =>
        {
            sceneChange.Invoke();
        });

        //扉を開く
        sequence.Append(closeRightTransform.DOLocalMoveX(DungeonConstants.BossEnteringCloseX, DungeonConstants.BossEnteringCloseTime));
        sequence.Join(closeLeftTransform.DOLocalMoveX(-1*DungeonConstants.BossEnteringCloseX, DungeonConstants.BossEnteringCloseTime));

        //コールバックの実行
        sequence.AppendCallback(() =>
        {
            callback.Invoke();
        });
    }
}
