using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 攻撃されているときに発生するエフェクト
/// </summary>
public class AttackEffect : MonoBehaviour
{
    [SerializeField] Image effectImage;//エフェクト画像
    [SerializeField] TypeToAttackEffect typeToAttackEffect;

    //音声
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip soundDamage;
    [SerializeField] private AudioClip soundBuff; 
    [SerializeField] private AudioClip soundDebuff; 
    [SerializeField] private AudioClip soundShield;
    [SerializeField] private AudioClip soundHeal;
    [SerializeField] private AudioClip soundAvoid;


    /// <summary>
    /// 初期化
    /// actualActionに対応するエフェクトをImageに持つ
    /// actionTypeに対応した出現演出を行う
    /// valueによって演出の大きさが変わる
    /// </summary>
    /// <param name="actionType">エフェクトの種類</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Init(EnemyActionDefine.EnemyActionType actionType, EnemyActionDefine.EnemyAction enemyAction,int value)
    {
        if (actionType == EnemyActionDefine.EnemyActionType.Other)
        {//アクションタイプが「その他」なら演出を行わない
            Color color = effectImage.color;
            color.a = 0;
            effectImage.color = color;
            return;
        }

        Transform thisTransform=this.gameObject.transform;//自身のゲームオブジェクトのtransform
        effectImage.sprite = typeToAttackEffect.GetEffectSpriteFromEnemyAction(actionType, enemyAction);

        //エフェクトの出現場所をランダムに変更
        Vector3 tmpPosition = thisTransform.localPosition;
        tmpPosition.x += UnityEngine.Random.Range(-1* BattleConstants.EffectDeltaX, BattleConstants.EffectDeltaX);
        tmpPosition.y += UnityEngine.Random.Range(-1* BattleConstants.EffectDeltaY, BattleConstants.EffectDeltaY);

        thisTransform.localPosition = tmpPosition;

        //最終的な大きさを決定する
        Vector3 lastScale = thisTransform.localScale;
        float correctionScale = 1.0f;
        //actionTypeによって最終的なサイズの補正を変更する
        switch (actionType)
        {
            //補整を行う(大)
            case EnemyActionDefine.EnemyActionType.Damage:
                if (value >= BattleConstants.EffectThreshold_Big)
                {
                    int diff = value - BattleConstants.EffectThreshold_Big;
                    float addScale = diff / ((float)(BattleConstants.EffectThreshold_Big_Max - BattleConstants.EffectThreshold_Big) * 2);
                    correctionScale += addScale;
                }
                break;
            //補整を行う(小)
            case EnemyActionDefine.EnemyActionType.Block:
            case EnemyActionDefine.EnemyActionType.PlayerHeal:
            case EnemyActionDefine.EnemyActionType.EnemyHeal:
                if (value >= BattleConstants.EffectThreshold_Small)
                {
                    int diff = value - BattleConstants.EffectThreshold_Small;
                    float addScale = diff / ((float)(BattleConstants.EffectThreshold_Small_Max - BattleConstants.EffectThreshold_Small) * 2);
                    correctionScale += addScale;
                }
                break;
            //補整を行わない
            case EnemyActionDefine.EnemyActionType.Debuff:
            case EnemyActionDefine.EnemyActionType.BuffSelf:
            case EnemyActionDefine.EnemyActionType.BuffAll:
            case EnemyActionDefine.EnemyActionType.Avoided:
                break;
        }
        lastScale *= correctionScale;
        thisTransform.localScale = lastScale;

        //actionTypeによって演出を変える
        switch (actionType)
        {
            //震動と拡大パターン
            case EnemyActionDefine.EnemyActionType.Damage:
                thisTransform.localScale = Vector3.zero;
                thisTransform.DOScale(lastScale, BattleConstants.EffectAppearTime)
                    .SetLink(this.gameObject);
                thisTransform.DOShakePosition(BattleConstants.EffectAppearTime, 
                    BattleConstants.EffectShakeStrength,
                    BattleConstants.EffectShakeNum,
                    BattleConstants.Effectrandomness, false, true).SetLink(this.gameObject);
                break;
            //下から上がってくるパターン
            case EnemyActionDefine.EnemyActionType.BuffSelf:
            case EnemyActionDefine.EnemyActionType.BuffAll:
            case EnemyActionDefine.EnemyActionType.Block:
                tmpPosition.y -= BattleConstants.EffectUpDown;
                thisTransform.localPosition = tmpPosition;
                thisTransform.DOLocalMoveY(BattleConstants.EffectUpDown, BattleConstants.EffectAppearTime).SetLink(this.gameObject);
                break;
            //上から下がってくるパターン
            case EnemyActionDefine.EnemyActionType.Debuff:
                tmpPosition.y += BattleConstants.EffectUpDown;
                thisTransform.localPosition = tmpPosition;
                thisTransform.DOLocalMoveY(BattleConstants.EffectUpDown*(-1), BattleConstants.EffectAppearTime).SetLink(this.gameObject);
                break;
            //単純に拡大するパターン
            case EnemyActionDefine.EnemyActionType.PlayerHeal:
            case EnemyActionDefine.EnemyActionType.EnemyHeal:
                thisTransform.localScale = Vector3.zero;
                thisTransform.DOScale(lastScale, BattleConstants.EffectAppearTime)
                    .SetLink(this.gameObject);
                break;
            //震動のみのパターン
            case EnemyActionDefine.EnemyActionType.Avoided:
                thisTransform.DOShakePosition(BattleConstants.EffectAppearTime,
                    BattleConstants.EffectShakeStrength,
                    BattleConstants.EffectShakeNum,
                    BattleConstants.Effectrandomness, false, true).SetLink(this.gameObject);
                break;
        }

        //actionTypeによって効果音を変える
        switch (actionType)
        {
            //ダメージ
            case EnemyActionDefine.EnemyActionType.Damage:
                audioSource.PlayOneShot(soundDamage);
                break;
            //バフ
            case EnemyActionDefine.EnemyActionType.BuffSelf:
            case EnemyActionDefine.EnemyActionType.BuffAll:
                audioSource.PlayOneShot(soundBuff);
                break;
            //デバフ
            case EnemyActionDefine.EnemyActionType.Debuff:
                audioSource.PlayOneShot(soundDebuff);
                break;
            //防御
            case EnemyActionDefine.EnemyActionType.Block:
                audioSource.PlayOneShot(soundShield);
                break;
            //回復
            case EnemyActionDefine.EnemyActionType.PlayerHeal:
            case EnemyActionDefine.EnemyActionType.EnemyHeal:
                audioSource.PlayOneShot(soundHeal);
                break;
            case EnemyActionDefine.EnemyActionType.Avoided:
                audioSource.PlayOneShot(soundAvoid);
                break;
        }
    }

    /// <summary>
    /// 初期化
    /// CardEffectに対応した出現演出を行う
    /// valueによって演出の大きさが変わる
    /// </summary>
    /// <param name="effectType">エフェクトの種類</param>
    /// <param name="attribute">エフェクトの属性</param>
    /// <param name="value">エフェクトの大きさ</param>
    public void Init(CardEffectDefine.CardEffectType effectType, AttributeDefine.Attribute attribute, int value)
    {
        if(effectType == CardEffectDefine.CardEffectType.Other)
        {//効果タイプが「その他」なら演出を行わない
            Color color = effectImage.color;
            color.a = 0;
            effectImage.color = color;
            return;
        }

        Transform thisTransform = this.gameObject.transform;//自身のゲームオブジェクトのtransform
        effectImage.sprite = typeToAttackEffect.GetEffectSpriteFromCard(effectType, attribute);//画像をセット

        //エフェクトの出現場所をランダムに変更
        Vector3 tmpPosition = thisTransform.localPosition;
        tmpPosition.x += UnityEngine.Random.Range(-1 * BattleConstants.EffectDeltaX, BattleConstants.EffectDeltaX);
        tmpPosition.y += UnityEngine.Random.Range(-1 * BattleConstants.EffectDeltaY, BattleConstants.EffectDeltaY);

        thisTransform.localPosition = tmpPosition;

        //最終的な大きさを決定する
        Vector3 lastScale = thisTransform.localScale;
        //左右を反転する
        lastScale.x *= -1;
        
        float correctionScale = 1.0f;
        //actionTypeによって最終的なサイズの補正を変更する
        switch (effectType)
        {
            //補整を行う(大)
            case CardEffectDefine.CardEffectType.Damage:
                if(value>= BattleConstants.EffectThreshold_Big)
                {
                    int diff = value - BattleConstants.EffectThreshold_Big;
                    float addScale = diff / ((float)(BattleConstants.EffectThreshold_Big_Max- BattleConstants.EffectThreshold_Big)*2);
                    correctionScale+=addScale;
                }
                break;
            //補整を行う(小)
            case CardEffectDefine.CardEffectType.Block:
            case CardEffectDefine.CardEffectType.Heal:
                if (value >= BattleConstants.EffectThreshold_Small)
                {
                    int diff = value - BattleConstants.EffectThreshold_Small;
                    float addScale = diff / ((float)(BattleConstants.EffectThreshold_Small_Max - BattleConstants.EffectThreshold_Small) * 2);
                    correctionScale += addScale;
                }
                break;
            //補整を行わない
            case CardEffectDefine.CardEffectType.Debuff:
            case CardEffectDefine.CardEffectType.Buff:
            case CardEffectDefine.CardEffectType.Avoided:
                break;
        }
        lastScale *= correctionScale;
        thisTransform.localScale = lastScale;

        //actionTypeによって演出を変える
        switch (effectType)
        {
            //震動と拡大パターン
            case CardEffectDefine.CardEffectType.Damage:
                thisTransform.localScale = Vector3.zero;
                thisTransform.DOScale(lastScale, BattleConstants.EffectAppearTime)
                    .SetLink(this.gameObject);
                thisTransform.DOShakePosition(BattleConstants.EffectAppearTime,
                    BattleConstants.EffectShakeStrength,
                    BattleConstants.EffectShakeNum,
                    BattleConstants.Effectrandomness, false, true).SetLink(this.gameObject);
                break;
            //下から上がってくるパターン
            case CardEffectDefine.CardEffectType.Buff:
            case CardEffectDefine.CardEffectType.Block:
                tmpPosition.y -= BattleConstants.EffectUpDown;
                thisTransform.localPosition = tmpPosition;
                thisTransform.DOLocalMoveY(BattleConstants.EffectUpDown, BattleConstants.EffectAppearTime).SetLink(this.gameObject);
                break;
            //上から下がってくるパターン
            case CardEffectDefine.CardEffectType.Debuff:
                tmpPosition.y += BattleConstants.EffectUpDown;
                thisTransform.localPosition = tmpPosition;
                thisTransform.DOLocalMoveY(BattleConstants.EffectUpDown * (-1), BattleConstants.EffectAppearTime).SetLink(this.gameObject);
                break;
            //単純に拡大するパターン
            case CardEffectDefine.CardEffectType.Heal:
                thisTransform.localScale = Vector3.zero;
                thisTransform.DOScale(lastScale, BattleConstants.EffectAppearTime)
                    .SetLink(this.gameObject);
                break;
            //震動のみのパターン
            case CardEffectDefine.CardEffectType.Avoided:
                thisTransform.DOShakePosition(BattleConstants.EffectAppearTime,
                    BattleConstants.EffectShakeStrength,
                    BattleConstants.EffectShakeNum,
                    BattleConstants.Effectrandomness, false, true).SetLink(this.gameObject);
                break;
        }

        //actionTypeによって効果音を変える
        switch (effectType)
        {
            //ダメージ
            case CardEffectDefine.CardEffectType.Damage:
                audioSource.PlayOneShot(soundDamage);
                break;
            //バフ
            case CardEffectDefine.CardEffectType.Buff:
                audioSource.PlayOneShot(soundBuff);
                break;
            //デバフ
            case CardEffectDefine.CardEffectType.Debuff:
                audioSource.PlayOneShot(soundDebuff);
                break;
            //シールド
            case CardEffectDefine.CardEffectType.Block:
                audioSource.PlayOneShot(soundShield);
                break;
            //回復
            case CardEffectDefine.CardEffectType.Heal:
                audioSource.PlayOneShot(soundHeal);
                break;
            case CardEffectDefine.CardEffectType.Avoided:
                audioSource.PlayOneShot(soundAvoid);
                break;
        }
    }

    /// <summary>
    /// このエフェクトの消滅演出を行い、ゲームオブジェクトも削除する
    /// </summary>
    public void DestroyEffect()
    {
        Transform thisTransform = this.gameObject.transform;//自身のゲームオブジェクトのtransform

        //DOTweenを使用するためのSequenceを作成
        Sequence destroySequence = DOTween.Sequence();

        //演出
        destroySequence.Append(thisTransform.DOLocalMove(
            new Vector3(UnityEngine.Random.Range(-1* BattleConstants.EffectDisAppearX, BattleConstants.EffectDisAppearX),
            BattleConstants.EffectDisAppearY,0), BattleConstants.EffectDisAppearTime).SetLink(this.gameObject));
        destroySequence.Join(effectImage.DOFade(0.0f, BattleConstants.EffectDisAppearTime).SetLink(this.gameObject));
        
        //コールバックを設定して、オブジェクトを破壊
        destroySequence.AppendCallback(() => {//コールバック
            Destroy(this.gameObject);//このオブジェクトを破壊
        }).SetLink(this.gameObject);
    }
}
