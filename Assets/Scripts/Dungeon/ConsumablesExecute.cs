using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ダンジョン中の消耗品の実行クラス
/// </summary>
public class ConsumablesExecute : MonoBehaviour
{
    [SerializeField] DungeonManager dungeonManager;
    [SerializeField] Player player;

    public void Execute(List<CardEffectClass> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect.Effect.cardEffect)
            {
                case CardEffectDefine.CardEffect.SelfHeal:
                    player.Damage(-1* effect.Effect.value);
                    break;
                case CardEffectDefine.CardEffect.Damage:
                    player.Damage(effect.Effect.value);
                    break;
                case CardEffectDefine.CardEffect.GainStamina:
                    dungeonManager.GainStamina(effect.Effect.value);
                    break;
                case CardEffectDefine.CardEffect.LoseStamina:
                    dungeonManager.ConsumeStamina(effect.Effect.value);
                    break;
            }
        }
    }
}
