using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

///
/// UI演出の処理中に、入力を受け付けないようにする(Dungeon)
///
public class InputBlocker : MonoBehaviour
{
    [SerializeField] List<GraphicRaycaster> graphicRaycasterList;

    //入力をブロックする必要のある処理の数
    //複数の処理によってブロックされる状態に対応するため、
    //boolではなく数値にし、これが0以下の場合のみ
    //graphicRaycasterをオンにする
    int blockingNum;

    public void Init()
    {
        graphicRaycasterList.ForEach(gr => gr.enabled = true);
        blockingNum = 0;
    }

    /// <summary>
    /// 何らかの演出の発生中に使用
    /// 入力をブロックするためのblockingNumの値を1上げる
    /// 1以上ならブロック状態になる
    /// </summary>
    public void InputBlockingUp()
    {
        blockingNum++;
        CheckBlocking();
    }

    /// <summary>
    /// 何らかの演出の終了時に使用
    /// 入力をブロックするためのblockingNumの値を1下げる
    /// 1以上ならブロック状態になる
    /// </summary>
    public void InputBlockingDown()
    {
        blockingNum--;
        if(blockingNum < 0)
        {
            Debug.Log("OverDowned");
            blockingNum = 0;
        }
        CheckBlocking();
    }

    /// <summary>
    /// blockingNumの値を参照して、入力を止めるかどうかを判定する
    /// </summary>
    private void CheckBlocking()
    {
        if (blockingNum <= 0)
        {
            graphicRaycasterList.ForEach(gr => gr.enabled = true);
        }
        else
        {
            graphicRaycasterList.ForEach(gr => gr.enabled = false);
        }
    }
}
