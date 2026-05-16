using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CardEffectDefineに加えて、実際に表示される効果値を加えたクラス
/// CardとWeaponで共通
/// </summary>
[System.Serializable]
public class ActualEffect
{
    public CardEffectDefine effect { get; set; }//効果の種類と本来の値のクラス
    public int actualEffectValue { get; set; }//補整などが加わって変動した値

    public IState UseState;//カード効果が使用する状態異常

    public ActualEffect(CardEffectDefine _effect, int _actualEffectValue, IState useState)
    {
        this.actualEffectValue = _actualEffectValue;
        this.effect = _effect;
        UseState = useState;
    }
}
