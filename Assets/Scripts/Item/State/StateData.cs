using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// シングルトン。StateDefine.StateTypeとそれに関する
/// 様々なデータの対応を示すクラスStateの、
/// リストを格納
/// </summary>
public class StateData : SingletonMonoBehaviour<StateData>
{
    [SerializeField] StateAutoSetInGame stateAutoSetInGame;

    //staticではインスペクターで設定できず、
    //staticでなければ全クラスからのアクセスができないため
    //stateData_Serializedに入れたものを
    //stateData_Staticに移動して使用する

    [SerializeField] private List<StateDefine.StateNew> stateNewData_Serialized;

    public List<StateDefine.StateNew> StateNewData_Serialized { get { return stateNewData_Serialized; }set { stateNewData_Serialized = value; } }

    //staticなstateDataNew
    private static List<StateDefine.StateNew> stateNewData_Static;

    public void Awake()
    {
        //シングルトンの処理
        if (this != Instance)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        //シングルトン処理終了

        //初期化とデータの移動
        /*
        //state情報をSerializeFieldで設定しているもの
        stateNewData_Static = new List<StateDefine.StateNew>();
        foreach (StateDefine.StateNew state in stateNewData_Serialized)
        {
            stateNewData_Static.Add(state);
        }
        */

        //state情報をStateAutoSetInGameから取得
        stateNewData_Static = new List<StateDefine.StateNew>();
        List<StateDefine.StateNew> stateList = stateAutoSetInGame.GetStateList();
        foreach (StateDefine.StateNew state in stateList)
        {
            stateNewData_Static.Add(state);
        }
    }

    /// <summary>
    /// StateDefine.StateTypeでstateData_Staticを検索して、
    /// IStateの型の一致しているStateNewを返す
    /// </summary>
    /// <param name="state">検索する状態異常の型</param>
    /// <returns>検索された状態異常のデータ</returns>
    public static StateDefine.StateNew GetState(IState state)
    {
        if (state is IStateHasAttribute attributeState)
        {
            //属性ごとに分かれている状態異常
            foreach (StateDefine.StateNew stateData in stateNewData_Static)
            {
                if (stateData.attribute!=null&&
                    String.Compare(attributeState.GetType().ToString(), stateData.stateType) == 0&&
                    stateData.attribute.attribute == attributeState.stateAttribute)
                {//属性が一致しているならそれを返す
                    return stateData;
                }
            }
        }
        else
        {
            foreach (StateDefine.StateNew stateData in stateNewData_Static)
            {
                if (String.Compare(state.GetType().ToString(), stateData.stateType) == 0)
                {
                    return stateData;
                }
            }
        }
        return null;
    }
}
