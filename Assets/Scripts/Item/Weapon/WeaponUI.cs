using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI weaponCost;
    [SerializeField] TextMeshProUGUI weaponValue;

    [SerializeField] Image weaponImage;

    private Weapon weapon;//対応しているWeapon

    /// <summary>
    /// 初期化処理。Weaponから呼出
    /// </summary>
    /// <param name="pWeaponData">物理武器データ</param>
    public void Init(PhysicalItemDataSO pWeaponData, Weapon _weapon)
    {
        WeaponDataSO weaponData = pWeaponData.WeaponData;

        weapon = _weapon;

        SetWeaponCost(weaponData.cost);
        SetWeaponValue(weaponData.effectList[0].Effect.value);//先頭の値を表示する
        if(pWeaponData.SizeX == 1&&pWeaponData.SizeY == 1) {
            //1*1のアイテムなので通常のアイコン
            SetWeaponIconSprite(pWeaponData.IconSprite);
        }
        else
        {//1*1以外のサイズのアイテムなので小アイコン
            SetWeaponIconSprite(pWeaponData.MiniIconSprite);
        }
    }

    /// <summary>
    /// 武器の諸情報(名前と画像以外)を書き換える
    /// </summary>
    /// <param name="weaponData">変更先のweaponData</param>
    public void RefreshWeapon(int cost,List<ActualEffect> actualEffect)
    {
        SetWeaponCost(cost);
        SetWeaponValue(actualEffect[0].actualEffectValue);//先頭の値を表示する
    }

    /// <summary>
    /// 武器コストの設定
    /// </summary>
    /// <param name="cost">武器コスト</param>
    private void SetWeaponCost(int cost)
    {
        weaponCost.text = cost.ToString();
    }

    /// <summary>
    /// 武器の値(武器効果リストの先頭の効果の値)を設定
    /// </summary>
    /// <param name="value">武器の値</param>
    private void SetWeaponValue(int value)
    {
        weaponValue.text = value.ToString();
    }

    /// <summary>
    /// 武器画像の変更
    /// </summary>
    /// <param name="sprite">武器画像</param>
    private void SetWeaponIconSprite(Sprite sprite)
    {
        weaponImage.sprite = sprite;
    }
}
