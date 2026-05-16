using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GearUI : MonoBehaviour
{
    [SerializeField] Image image;
    [SerializeField] private GameObject stateValueObject;
    [SerializeField] private TextMeshProUGUI stateValue;

    private Gear gear; // 対応しているGear

    /// <summary>
    /// 初期化処理
    /// </summary>
    /// <param name="pGearData">装備の物理データ</param>
    /// <param name="clonedState">装備のデータからクローンされた、実際に使用するIState</param>
    /// <param name="gear">紐づいている装備インスタンス</param>
    /// <param name="isInBattle">バトル中の呼び出しならtrue</param>
    public void Init(PhysicalItemDataSO pGearData,IStateInGear clonedState,Gear gear,bool isInBattle)
    {
        GearDataSO gearData = pGearData.GearData;
        this.gear = gear;
        //表示すべき事項が少ないので、全て通常アイコンで表示
        if (pGearData.SizeX == 1 && pGearData.SizeY == 1)
        {
            //1*1のアイテムなので通常のアイコン
            SetIconSprite(pGearData.IconSprite);
        }
        else
        {//1*1以外のサイズのアイテムなので小アイコン
            SetIconSprite(pGearData.MiniIconSprite);
        }

        SetValue(clonedState.value);//状態異常の値をセット

        if (isInBattle)
        {//バトル中なら状態異常の値を表示する
            if (clonedState is IStateCountPerTurn cState)
            {
                //ターンごとにカウントするステータスなら
                //カウントを表示する
                SetValue(cState.IStateCounter.GetCount());
                stateValueObject.SetActive(true);
            }
            else
            {
                if (clonedState is StateContinueTypeEternalBase)
                {//永久のステータスなら(値を持たない)
                    stateValueObject.SetActive(false);
                }
                else
                {
                    stateValueObject.SetActive(true);
                }
            }
        }
        else
        {//表示しない
            stateValueObject.SetActive(false);
        }
    }

    private void SetIconSprite(Sprite iconSprite)
    {
        image.sprite = iconSprite;
    }

    /// <summary>
    /// Stateの値を更新
    /// </summary>
    /// <param name="value">Stateの値</param>
    public void SetValue(int value)
    {
        stateValue.text = value.ToString();
    }
}
