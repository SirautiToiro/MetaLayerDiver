using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeToAttackEffect : MonoBehaviour
{
    //属性に対応するエフェクト
    //行動種類は攻撃
    //物理属性
    [SerializeField] Sprite spriteAttackBash;
    [SerializeField] Sprite spriteAttackPierce;
    [SerializeField] Sprite spriteAttackSlash;
    //生成属性
    [SerializeField] Sprite spriteCreate;
    //念動属性
    [SerializeField] Sprite spritePsycho;
    //信仰属性
    [SerializeField] Sprite spriteFaith;
    //エネルギー属性
    [SerializeField] Sprite spriteEnergy;
    //発火属性
    [SerializeField] Sprite spritePyro;
    //精神属性
    [SerializeField] Sprite spriteMind;

    //行動種類に対応するエフェクト
    [SerializeField] Sprite spriteBuff;
    [SerializeField] Sprite spriteDebuff;
    [SerializeField] Sprite spriteHeal;
    [SerializeField] Sprite spriteShield;

    //回避が発生した時のエフェクト
    [SerializeField] Sprite spriteAvoid;

    //エラー用
    [SerializeField] Sprite spriteError;

    /// <summary>
    /// 敵の行動タイプから、それに対応する行動エフェクトを返す
    /// Attackの属性分岐に関しては判断しない。
    /// </summary>
    /// <param name="enemyActionType">敵の行動タイプ</param>
    /// <param name="enemyAction">敵の行動</param>
    /// <returns>行動エフェクトのSprite</returns>
    public Sprite GetEffectSpriteFromEnemyAction(EnemyActionDefine.EnemyActionType enemyActionType, EnemyActionDefine.EnemyAction enemyAction)
    {
        switch (enemyActionType)
        {
            //Damageは属性によってエフェクトを変える
            case EnemyActionDefine.EnemyActionType.Damage:
                switch (enemyAction)
                {
                    case EnemyActionDefine.EnemyAction.DamagePhysics:
                        //物理攻撃は三種類からランダム
                        int i = UnityEngine.Random.Range(0, 3);
                        switch (i)
                        {
                            case 0:
                                return spriteAttackBash;
                            case 1:
                                return spriteAttackPierce;
                            case 2:
                                return spriteAttackSlash;
                        }
                        return spriteError;
                    case EnemyActionDefine.EnemyAction.DamageFaith:
                        return spriteFaith;
                    case EnemyActionDefine.EnemyAction.DamageCreate:
                        return spriteCreate;
                    case EnemyActionDefine.EnemyAction.DamagePyro:
                        return spritePyro;
                    case EnemyActionDefine.EnemyAction.DamageMind:
                        return spriteMind;
                    case EnemyActionDefine.EnemyAction.DamagePsycho:
                        return spritePsycho;
                    case EnemyActionDefine.EnemyAction.DamageEnergy:
                        return spriteEnergy;

                    default:
                        return spriteError;
                }

            //後はタイプごとに羅列
            case EnemyActionDefine.EnemyActionType.Block:
                return spriteShield;

            case EnemyActionDefine.EnemyActionType.BuffSelf:
            case EnemyActionDefine.EnemyActionType.BuffAll:
                return spriteBuff;

            case EnemyActionDefine.EnemyActionType.Debuff:
                return spriteDebuff;

            case EnemyActionDefine.EnemyActionType.EnemyHeal:
            case EnemyActionDefine.EnemyActionType.PlayerHeal:
                return spriteHeal;

            case EnemyActionDefine.EnemyActionType.Avoided:
                return spriteAvoid;

            //エラー
            default:
                return spriteError;
        }
    }

    /// <summary>
    /// Cardに対応するエフェクトを返す
    /// CardEffectTypeとAttribute を使用して判別する
    /// </summary>
    /// <param name="cardEffectType">カード効果のタイプ</param>
    /// <param name="firstAttribute">カードの属性</param>
    /// <returns>行動エフェクトのSprite</returns>
    public Sprite GetEffectSpriteFromCard(CardEffectDefine.CardEffectType cardEffectType, AttributeDefine.Attribute firstAttribute)
    {
        switch (cardEffectType)
        {
            //Damageは属性によってエフェクトを変える
            case CardEffectDefine.CardEffectType.Damage:

                switch (firstAttribute)
                {
                    case AttributeDefine.Attribute.Psycho:
                        return spritePsycho;
                    case AttributeDefine.Attribute.Faith:
                        return spriteFaith;
                    case AttributeDefine.Attribute.Energy:
                        return spriteEnergy;
                    case AttributeDefine.Attribute.Pyro:
                        return spritePyro;
                    case AttributeDefine.Attribute.Create:
                        return spriteCreate;
                    case AttributeDefine.Attribute.Physics:
                        //物理攻撃は三種類からランダム
                        int i = UnityEngine.Random.Range(0, 3);
                        switch (i)
                        {
                            case 0:
                                return spriteAttackBash;
                            case 1:
                                return spriteAttackPierce;
                            case 2:
                                return spriteAttackSlash;
                        }
                        return spriteError;
                    case AttributeDefine.Attribute.Mind:
                        return spriteMind;
                    default:
                        return spriteError;
                }

            //後はタイプごとに羅列
            case CardEffectDefine.CardEffectType.Block:
                return spriteShield;

            case CardEffectDefine.CardEffectType.Debuff:
                return spriteDebuff;
            case CardEffectDefine.CardEffectType.Buff:
                return spriteBuff;
            case CardEffectDefine.CardEffectType.Heal:
                return spriteHeal;

            case CardEffectDefine.CardEffectType.Avoided:
                return spriteAvoid;

            //エラー
            default:
                return spriteError;
        }
    }
}
