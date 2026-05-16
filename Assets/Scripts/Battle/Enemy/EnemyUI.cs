using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI enemyName;
    [SerializeField] TextMeshProUGUI enemyHp;
    [SerializeField] Slider enemyHpBar;
    [SerializeField] Image enemyImage;
    [SerializeField] GameObject shieldUI;//盾表示部分の親
    [SerializeField] TextMeshProUGUI shieldValue;
    /*old
    [SerializeField] Image[] actionBackGround;//次のアクションのアイコンの背景。これが親。
    [SerializeField] Image[] actionIcons;//次のアクションのアイコンを表示する場所
    [SerializeField] TextMeshProUGUI[] actionValues;//次のアクションの値
    */
    [SerializeField]
    EnemyActionIcon[] actionIcons;//次のアクションのアイコンを表示する場所(3つ)

    

    [SerializeField] GameObject uiExceptCharacterImage;//キャラクター画像以外のUIのゲームオブジェクト

    [SerializeField] private GameObject stateIconPrefab;
    [SerializeField] private GameObject stateIconParent;//StateIconの配置される親
    private List<StateIcon> stateIcons;//状態異常のアイコンリスト

    [SerializeField] private GameObject attackEffectPrefab;
    [SerializeField] private GameObject attackEffectParent;//AttackEffectの配置される親
    private List<AttackEffect> attackEffectList;

    private EnemyActionToActionIcon enemyActionToActionIcon;

    private Enemy enemy;//紐づいているEnemy

    private List<Sprite> sprites;

    /// <summary>
    /// 初期化.Enemyから呼出
    /// </summary>
    /// <param name="enemyData">UIの元となるSO</param>
    public void Init(EnemyDataSO enemyData,EnemyActionToActionIcon _enemyActionToActionIcon, Enemy _enemy)
    {
        attackEffectList = new List<AttackEffect>();

        enemy = _enemy;

        //画像データを保存
        sprites = enemyData.picSprites;

        SetEnemyName(enemyData.enemyName);
        SetEnemyHp(enemyData.hp, enemyData.hp);
        SetEnemyHpBar(enemyData.hp, enemyData.hp);
        SetEnemySprite(0);
        enemyActionToActionIcon = _enemyActionToActionIcon;

        stateIcons = new List<StateIcon>();

        
    }

    #region パラメータ表示
    public void SetEnemyName(string name)
    {
        enemyName.text = name;
    }

    public void SetEnemyHp(int hp,int maxHp)
    {
        enemyHp.text = hp.ToString()+"/"+maxHp.ToString();
    }

    public void SetEnemyHpBar(int hp,int maxHp)
    {
        enemyHpBar.value=(float)hp/(float)maxHp;
    }

    public void SetEnemySprite(int num)
    {
        if (sprites.Count <= num) return;
        enemyImage.sprite = sprites[num];
    }

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
    #endregion

    #region 状態異常UI
    //これを調整するとき、PlayerUIも変化させること

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
    /// Stateのリストの中のStateの値を変動させる
    /// valueが0なら消滅させる
    /// </summary>
    /// <param name="state">変動した状態異常の変動後の値</param>
    public void ChangeStateValue(IState state,bool removeFlag=false)
    {
        int length = stateIcons.Count;
        //ループの途中でリストからの削除を行うので、リストの最後から実行している
        for (int i = length - 1; i >= 0; i--)
        {
            if (stateIcons[i].stateInstance != null && ReferenceEquals(state, stateIcons[i].stateInstance))
            {//変動対象のStateを検索して見つけたなら
             //削除フラグを与えられたなら、そのStateを必ず削除する
                if (removeFlag)
                {
                    Destroy(stateIcons[i].gameObject);//gameObjectを削除
                    stateIcons.RemoveAt(i);
                    break;
                }

                if (state.value > 0||state is StateContinueTypeEternalBase)
                {//値が0より大きい,あるいは永久のステートなら
                    stateIcons[i].SetValue(state);
                    
                }
                else
                {//Stateの値が0以下なら
                    Destroy(stateIcons[i].gameObject);//gameObjectを削除
                    stateIcons.RemoveAt(i);//リストから取り除く
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
    public void ChangeStateCount(IStateCountPerTurn cState, int count)
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

    #endregion

    #region 効果エフェクト系
    /// <summary>
    /// Enemyの行動による効果を受けた時のエフェクト効果を発生させる
    /// </summary>
    /// <param name="actionType">エフェクトの種類</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Effect(EnemyActionDefine.EnemyActionType actionType, EnemyActionDefine.EnemyAction enemyAction, int value)
    {
        //エフェクトを一つインスタンス化してリストに追加
        attackEffectList.Add(InstantiateAttackEffect(actionType,enemyAction, value));
    }

    /// <summary>
    /// Card効果によるエフェクトを発生させる
    /// </summary>
    /// <param name="effectType">カード効果の種類</param>
    /// <param name="attribute">エフェクトの属性</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Effect(CardEffectDefine.CardEffectType effectType, AttributeDefine.Attribute attribute, int value)
    {
        //エフェクトを一つインスタンス化してリストに追加
        attackEffectList.Add(InstantiateAttackEffect(effectType, attribute, value));

    }

    /// <summary>
    /// 全てのエフェクトの削除演出を行う
    /// </summary>
    public void DestroyEffects()
    {
        int length = attackEffectList.Count;
        //ループの途中でリストからの削除を行うので、リストの最後から実行している
        for (int i = length - 1; i >= 0; i--)
        {
            attackEffectList[i].DestroyEffect();
            attackEffectList.RemoveAt(i);
        }
    }

    /// <summary>
    /// Enemyの行動による効果を受けた時効果AttackEffectをインスタンス化して
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

    #region 行動表示
    /// <summary>
    /// 次の行動を受け取って、UIに表示する
    /// </summary>
    /// <param name="action">次の行動</param>
    public void ShowNextAction(Enemy.ActualNextAction actualNextAction)
    {
        foreach(EnemyActionIcon icon in actionIcons)
        {
            icon.SetActive(false);//全て非表示にする
        }
        for (int i = 0; i < actualNextAction.GetActualActionCount(); i++)
        {
            //ひとつの行動と、その実行時の値を取り出す
            Enemy.ActualNextAction.ActualAction actualAction = actualNextAction.GetActualAction(i);
            int actualValue = actualAction.actualActionValue;
            //その敵の行動のタイプを取得
            EnemyActionDefine.EnemyActionType type = EnemyActionDefine.GetActionType(actualAction.actionDefine.enemyAction);

            if (i>= actionIcons.Length)
            {
                Debug.Log("Action num is over");
                break;
            }
            //アイコンを表示
            actionIcons[i].gameObject.SetActive(true);
            //画像をセット
            actionIcons[i].SetSprite(actualAction.actionDefine.enemyAction,enemyActionToActionIcon);
            
            if(type == EnemyActionDefine.EnemyActionType.Damage||
                type == EnemyActionDefine.EnemyActionType.Block||
                type==EnemyActionDefine.EnemyActionType.PlayerHeal||
                type==EnemyActionDefine.EnemyActionType.EnemyHeal)
            {//値を必要とする行動なら(0でも)値を表示
                actionIcons[i].SetValueActive(true);//値を表示する

                actionIcons[i].SetValue(actualValue);
            }
            else
            {
                actionIcons[i].SetValueActive(false);//値は非表示にする
            }
        }
    }
    #endregion

    #region 撃破時演出
    /// <summary>
    /// Enemyの撃破時の演出を行う
    /// 上に透明になりながら消える
    /// </summary>
    public void DestroyEffect()
    {
        //画像以外のUIは全て非表示
        uiExceptCharacterImage.SetActive(false);

        Sequence sc = DOTween.Sequence();
        sc.Append(enemyImage.DOFade(0.0f, BattleConstants.EnemyDestroyFadeTime))//透明にする演出
        .Join(enemy.gameObject.transform.DOLocalMoveY(BattleConstants.EnemyDestroyUpY, BattleConstants.EnemyDestroyFadeTime))//上に上がる演出
            .SetLink(this.gameObject)
            .OnComplete(() =>
            {
                Destroy(this.gameObject);//消滅演出が終了するとこのゲームオブジェクトを削除
            });
    }
    #endregion
}
