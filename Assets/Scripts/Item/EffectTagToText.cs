using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// カードや武器のエフェクトとタグの情報から説明文のテキストを生成する
/// </summary>
public class EffectTagToText : MonoBehaviour
{
    private HandManager handManager = null;

    public void Init(HandManager _handManager)
    {
        handManager = _handManager;
    }

    /// <summary>
    /// カードや武器のエフェクトとタグの情報から説明文のテキストを生成する
    /// </summary>
    public string ConvertToText(List<ActualEffect> effects, List<CardTagDefine> tags)
    {
        string effectText = "";
        int effectLength = effects.Count;
        int tagLength = tags.Count;
        int count = 1;

        //冒頭に特殊タグ情報を入れる
        foreach (CardTagDefine tag in tags)
        {//タグは改行せず、コンマ区切り,最後のみ改行
            effectText += CardTagDefine.Dic_TagName[tag.cardTag];
            if (count < tagLength)
            {
                effectText += ",";
            }
            else
            {
                effectText += "\n";
            }
            count++;
        }

        count = 1;
        //補正後の値が入力されてくるので、そのまま出力する
        foreach (ActualEffect actualEffect in effects)
        {
            //引数が多いカード効果の対応
            switch (actualEffect.effect.cardEffect)
            {
                case CardEffectDefine.CardEffect.IsShuffleNum:
                    int num = 0;
                    if (handManager != null)
                    {//handManagerがある(バトル画面からの呼び出し)なら
                        num = handManager.GetShuffleNum();
                    }
                    effectText += string.Format(CardEffectDefine.Dic_EffectName[actualEffect.effect.cardEffect], actualEffect.actualEffectValue, num);
                    break;
                case CardEffectDefine.CardEffect.CauseState:
                    //状態を使用する。永久のステートを付与する場合は効果説明が変わる
                    if (actualEffect.UseState is StateContinueTypeEternalBase)
                    {
                        effectText += string.Format("永続的な{0}を付与する", StateData.GetState(actualEffect.UseState).Name);
                    }
                    else
                    {
                        effectText += string.Format(
                        CardEffectDefine.Dic_EffectName[actualEffect.effect.cardEffect],
                        actualEffect.actualEffectValue,
                        StateData.GetState(actualEffect.UseState).Name
                        );
                    }
                    break;
                case CardEffectDefine.CardEffect.GetStateBuff:
                case CardEffectDefine.CardEffect.GetStateDebuff:
                case CardEffectDefine.CardEffect.UseStateConstant:
                    //状態を使用する効果なら(使用する値が存在)
                    effectText += string.Format(
                        CardEffectDefine.Dic_EffectName[actualEffect.effect.cardEffect],
                        actualEffect.actualEffectValue,
                        StateData.GetState(actualEffect.UseState).Name
                        );
                    break;
                case CardEffectDefine.CardEffect.UseStateEternal:
                    //永続状態を使用する効果なら(値なし)
                    effectText += string.Format(
                        CardEffectDefine.Dic_EffectName[actualEffect.effect.cardEffect],
                        StateData.GetState(actualEffect.UseState).Name
                        );
                    break;
                default:
                    effectText += string.Format(CardEffectDefine.Dic_EffectName[actualEffect.effect.cardEffect], actualEffect.actualEffectValue);
                    break;
            }

            //最終行のみ改行しない
            if (count < effectLength)
            {
                if (actualEffect.effect.cardEffect == CardEffectDefine.CardEffect.NextEffectIsSelf)
                {//改行を行わないテキスト

                }
                else
                {
                    effectText += "\n";
                }
            }
            count++;
        }

        return effectText;
    }
}
