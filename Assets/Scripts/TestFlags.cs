using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFlags : SingletonMonoBehaviour<TestFlags>
{
    #region シングルトン処理
    public void Awake()
    {
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    public bool useTestDataFlag;//テスト用データを使用するかのフラグ
    public bool useTestQusetFlag;//テスト用クエストを使用するかのフラグ

    public bool showAllCardFlag;//全てのカードを表示するフラグ

    public bool testMode;//テストモードかどうかのフラグ。これがtrueのときは、ゲーム内の特定の場所でテスト用の処理が走る。
}
