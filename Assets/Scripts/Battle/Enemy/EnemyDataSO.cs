using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 敵データのScriptableObject
/// </summary>
[System.Serializable]
[CreateAssetMenu(fileName = "EnemyData", menuName = " ScriptableObjects/EnemyData", order = 1)]
public class EnemyDataSO : ScriptableObject
{
    [Header("敵データ番号")]
    public int serialNum;

    [Header("キャラ名")]
    public string enemyName;

    [Header("HP")]
    public int hp;

    //0番地が初期の立ち絵、その他は特殊行動で遷移
    [Header("立ち絵リスト")]
    public List<Sprite> picSprites;

    [Header("行動リスト")]
    public List<ActionInTurn> actions1;

    [Header("第2パターン行動")]
    public List <ActionInTurn> actions2;

    [Header("第3パターン行動")]
    public List <ActionInTurn> actions3;

    //リストの形式だが最初の一つのみを使用
    [Header("戦闘開始時の行動")]
    public List<ActionInTurn> initialAction;

    [Header("行動ルーチン")]
    public EnemyRoutineDefine enemyRoutine;

    [Header("初期行動")]
    public List<EnemyInitialEffectDefine> enemyInitialEffects;

    //HPを全回復して耐える数
    [Header("ゲージ数")]
    public int gage;

    [Header("レアリティ")]
    public TierDefine tier;

    [Header("出現場所")]
    public DungeonTypeDefine spawn;

    [Header("特殊敵の種類")]
    public EnemyUniqueDefine uniqueTag;
}
