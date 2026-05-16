using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyActionToActionIcon : MonoBehaviour
{
    //それぞれの敵の行動に対応する画像
    [SerializeField] Sprite damageSprite;
    [SerializeField] Sprite blockSprite;
    [SerializeField] Sprite errorSprite;
    [SerializeField] Sprite debuffSprite;
    [SerializeField] Sprite buffSprite;


    [SerializeField] Sprite notMoveSprite;

    /// <summary>
    /// EnemyのActionを引数に取り、それに対応した画像を返す
    /// </summary>
    /// <param name="action">敵の行動</param>
    /// <returns>敵の行動に対応するアイコン画像</returns>
    public Sprite GetActionIcon(EnemyActionDefine.EnemyActionType type)
    {
        switch (type)
        {
            case EnemyActionDefine.EnemyActionType.Damage: 
                return damageSprite;
            case EnemyActionDefine.EnemyActionType.Block:
                return blockSprite;

            //デバフ系
            case EnemyActionDefine.EnemyActionType.Debuff:
                return debuffSprite;

            //バフ系
            case EnemyActionDefine.EnemyActionType.BuffAll:
            case EnemyActionDefine.EnemyActionType.BuffSelf:
                return buffSprite;

            default:
                return errorSprite;
        }
    }

    /// <summary>
    /// アクションタイプがOtherの場合
    /// それに応じた画像を表示する
    /// </summary>
    /// <param name="action">Otherであるアクション</param>
    /// <returns>敵の行動に対応するアイコン画像</returns>
    public Sprite GetActionIcon(EnemyActionDefine.EnemyAction action)
    {
        switch (action)
        {
            case EnemyActionDefine.EnemyAction.NotMove:
                //何も動かない
                return notMoveSprite;
            default:
                return errorSprite;
        }
    }

}
