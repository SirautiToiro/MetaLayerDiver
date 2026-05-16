using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// バトル中PlayerのUIを制御
/// </summary>
public class PlayerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerHp;
    [SerializeField] private Slider playerHpBar;
    [SerializeField] private GameObject shieldUI;//盾表示部分の親
    [SerializeField] private TextMeshProUGUI shieldValue;

    [SerializeField] private GameObject stateIconPrefab;
    [SerializeField] private GameObject stateIconParent;//StateIconの配置される親

    [SerializeField] private GameObject attackEffectPrefab;
    [SerializeField] private GameObject attackEffectParent;//AttackEffectの配置される親
    private List<AttackEffect> attackEffectList;

    private List<StateIcon> stateIcons;//状態異常のアイコンリスト

    public void Init(int hp,int maxHp, int shield)
    {
        attackEffectList=new List<AttackEffect>();

        SetPlayerHp(hp,maxHp);
        SetPlayerHpBar(hp, maxHp);
        SetShield(shield);

        stateIcons= new List<StateIcon>();
    }

    public void SetPlayerHp(int hp,int maxHp)
    {
        playerHp.text = hp.ToString() + "/" + maxHp.ToString();
    }

    public void SetPlayerHpBar(int hp, int maxHp)
    {
        playerHpBar.value = (float)hp / (float)maxHp;
    }

    /// <summary>
    /// 盾を指定した値に設定する
    /// 0なら盾を非表示
    /// </summary>
    /// <param name="shield">盾の値</param>
    public void SetShield(int shield)
    {
        if (shield <= 0)
        {//盾がないなら
            //非表示に
            shieldUI.SetActive(false);
        }
        else
        {//盾があるなら
            //表示して値をセット
            shieldUI.SetActive(true);
            shieldValue.text = shield.ToString();
        }
    }

    # region 状態異常UI
    //これを調整するとき、EnemyUIも変化させること
    /// <summary>
    /// stateのリストに新しいアイコンを加える
    /// </summary>
    /// <param name="state">状態異常のクラス</param>
    public void AddState(IState state)
    {
        StateIcon stateIcon = InstantiateStateIcon(state);//アイコンインスタンス化
        stateIcons.Add(stateIcon);//リストに追加
    }

    /// <summary>
    /// StateIconのインスタンス化
    /// </summary>
    /// <param name="state">状態異常のクラス</param>
    /// <returns>インスタンス化したStateIcon</returns>
    private StateIcon InstantiateStateIcon(IState state)
    {
        //インスタンス化
        //親はstateIconParent
        GameObject stateIconObj = Instantiate(stateIconPrefab, stateIconParent.transform.position, Quaternion.identity, stateIconParent.transform);
        StateIcon stateIcon = stateIconObj.GetComponent<StateIcon>();
        stateIcon.Init(state);//初期化

        return stateIcon;
    }

    /// <summary>
    /// IStateのリストの中のstateと一致するものを検索して、
    /// stateの値に変動させる
    /// valueが0なら消滅させる
    /// </summary>
    /// <param name="state">変動した状態異常の変動後の値</param>
    public void ChangeStateValue(IState state)
    {
        int length = stateIcons.Count;
        //ループの途中でリストからの削除を行うので、リストの最後から実行している
        for (int i = length - 1; i >= 0; i--)
        {
            if (stateIcons[i].stateInstance!=null&& ReferenceEquals(state, stateIcons[i].stateInstance))
            {//変動対象のStateを検索して見つけたなら
                if (state.value <= 0)
                {//Stateの値が0以下なら
                    Destroy(stateIcons[i].gameObject);//gameObjectを削除
                    stateIcons.RemoveAt(i);//リストから取り除く
                }
                else
                {//0より大きいなら
                    stateIcons[i].SetValue(state);
                }

                //見つかったので終了
                break;
            }
        }
    }

    /// <summary>
    /// カウントを持つStateの値の表示を変動させる
    /// </summary>
    /// <param name="cState">カウントを持つState</param>
    public void ChangeStateCount(IStateHasCount cState,int count)
    {
        int length = stateIcons.Count;
        //ループの途中でリストからの削除を行うので、リストの最後から実行している
        for (int i = length - 1; i >= 0; i--)
        {
            if (stateIcons[i].stateInstance != null && ReferenceEquals(cState, stateIcons[i].stateInstance))
            {//変動対象のStateを検索して見つけたなら
                stateIcons[i].SetValue(cState);
            }
        }
    }

    /// <summary>
    /// Stateのリストから指定したStateを削除する
    /// </summary>
    /// <param name="state">削除するState</param>
    public void DeleteState(IState state)
    {
        int length = stateIcons.Count;
        //ループの途中でリストからの削除を行うので、リストの最後から実行している
        for (int i = length - 1; i >= 0; i--)
        {
            if (stateIcons[i].stateInstance != null && ReferenceEquals(state, stateIcons[i].stateInstance))
            {//変動対象のStateを検索して見つけたなら
                Destroy(stateIcons[i].gameObject);//gameObjectを削除
                stateIcons.RemoveAt(i);//リストから取り除く

                //見つかったので終了
                break;
            }
        }
    }


    #endregion

    #region 効果エフェクト系
    /// <summary>
    /// プレイヤーが攻撃された際のエフェクト効果を発生させる
    /// </summary>
    /// <param name="actionType">エフェクトの種類</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Effect(EnemyActionDefine.EnemyActionType actionType, EnemyActionDefine.EnemyAction enemyAction, int value)
    {
        //エフェクトを一つインスタンス化してリストに追加
        attackEffectList.Add(InstantiateAttackEffect(actionType, enemyAction, value));
    }

    /// <summary>
    /// Card効果によるエフェクトを発生させる
    /// </summary>
    /// <param name="effectType">カード効果の種類</param>
    /// <param name="attribute">エフェクトの属性</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Effect(CardEffectDefine.CardEffectType effectType, AttributeDefine.Attribute attribute, int value)
    {
        attackEffectList.Add(InstantiateAttackEffect(effectType, attribute, value));
    }

    /// <summary>
    /// 全てのエフェクトの削除演出を行う
    /// </summary>
    public void DestroyEffects()
    {
        int length=attackEffectList.Count;
        //ループの途中でリストからの削除を行うので、リストの最後から実行している
        for (int i=length-1; i>=0; i--)
        {
            attackEffectList[i].DestroyEffect();
            attackEffectList.RemoveAt(i);
        }
    }

    /// <summary>
    /// 攻撃された時のエフェクトをインスタンス化して
    /// リストに追加する
    /// </summary>
    /// <param name="actionType">エフェクトの種類</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <param name="value">エフェクトの大きさ</param>
    /// <returns>生成されたAttackEffect</returns>
    private AttackEffect InstantiateAttackEffect(EnemyActionDefine.EnemyActionType actionType, EnemyActionDefine.EnemyAction enemyAction, int value)
    {
        //インスタンス化
        //親はAttackEffectParent
        GameObject attackEffectObj = Instantiate(attackEffectPrefab, attackEffectParent.gameObject.transform.position, Quaternion.identity, attackEffectParent.gameObject.transform);
        AttackEffect attackEffect = attackEffectObj.GetComponent<AttackEffect>();
        attackEffect.Init(actionType, enemyAction, value);//初期化

        return attackEffect;
    }

    /// <summary>
    /// Card効果を受けた時のエフェクトをインスタンス化して
    /// リストに追加する
    /// </summary>
    /// <param name="effectType">エフェクトの種類</param>
    /// <param name="attribute">エフェクトの属性</param>
    /// <param name="value">エフェクトの大きさ</param>
    /// <returns>生成されたAttackEffect</returns>
    private AttackEffect InstantiateAttackEffect(CardEffectDefine.CardEffectType effectType, AttributeDefine.Attribute attribute, int value)
    {
        //インスタンス化
        //親はAttackEffectParent
        GameObject attackEffectObj = Instantiate(attackEffectPrefab, attackEffectParent.gameObject.transform.position, Quaternion.identity, attackEffectParent.gameObject.transform);
        AttackEffect attackEffect = attackEffectObj.GetComponent<AttackEffect>();
        attackEffect.Init(effectType, attribute, value);//初期化

        return attackEffect;
    }
    #endregion
}
