using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// 各種の演出で使用する一定の定数を管理するクラス(Battle)
/// </summary>

[System.Serializable]
public class BattleConstants
{
    //ゲーム設定
    public const int EnergyMax = 3;//エネルギーの最大値

    //Card
    public const float CardWeaponMoveTime=0.3f;//カードと武器の移動演出の時間

    //Enemy
    public const float EnemyActionJumpTime=0.3f;//敵の行動演出の上下に動く時間
    public const float EnemyActionJumpHeight = 15;//敵の行動演出の動く幅
    public const float EnemyDestroyFadeTime = 0.5f;//敵の撃破時演出の消滅時間
    public const float EnemyDestroyUpY = 100f;//敵の撃破時演出の上に上がっていく幅

    public const float CharacterAppearTime = 0.3f;//キャラクターの出現演出の出現時間
    public const float CharacterAppearHeight = 30;//キャラクターの出現演出の落ちてくる高さ

    //Effect
    public const float CardEffectInterval = 0.2f;//カードのエフェクトのそれぞれの発生間隔
    public const float EffectAppearTime = 0.3f;//攻撃などのエフェクトの出現時間
    public const float EffectCardDisappearWait = 0.2f;//カード効果のエフェクトが消えるまで待つ時間
    public const float EffectDisAppearTime = 0.6f;//エフェクトの消滅時間
    public const float DestroyEffectAppearTime = 0.6f;//撃破時効果のエフェクトが発生するまでの待ち時間
    public const float EffectDisAppearX = 80f;//エフェクトの消滅演出の落ちるX幅
    public const float EffectDisAppearY = -100f;//エフェクトの消滅演出の落ちる高さ
    public const float EffectShakeStrength = 8f;//エフェクト震動の強さ
    public const int EffectShakeNum = 30;//エフェクト震動の回数
    public const int Effectrandomness = 1;//エフェクト手振れ値
    public const float EffectUpDown = 40f;//エフェクトの上下に移動する幅
    public const float EffectDeltaX = 50f;//エフェクトのX軸にずれる値
    public const float EffectDeltaY = 60f;//エフェクトのY軸にずれる値
    //エフェクトの補整閾値
    public const int EffectThreshold_Big = 5;//エフェクトがそのvalueに応じてサイズを変動する値
    public const int EffectThreshold_Small = 3;
    public const int EffectThreshold_Big_Max = 10;//エフェクトがそのvalueに応じてサイズを変動する値
    public const int EffectThreshold_Small_Max = 6;
}
