using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// シングルトン。カード以外の情報を格納
/// </summary>
public class DataManager : SingletonMonoBehaviour<DataManager>
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

        //ゲーム起動時処理
        InitialProcess();
    }

    #endregion

    //TODO:最終的には必要ない
    private void InitialProcess()
    {
        //!!!!!!!!
        //!!!!!!!!
        //EnemyActionDefine,CardEffectExecuteはチェックできていないため注意
        //!!!!!!!!
        //!!!!!!!!

        //test部位
        //CardEffectDefineの実装内容チェック
        //実際には途中でエラーが出るのでわかる
        Array array1 = Enum.GetValues(typeof(CardEffectDefine.CardEffect));
        foreach (CardEffectDefine.CardEffect item in array1)
        {
            if (!CheckCardEffect(item))
            {
                Debug.Log(String.Format("{0}がCardEffectDefineに用意されていません", item.ToString()));
            }
        }

        //AttributeDefineの実装内容チェック
        Array array2 = Enum.GetValues(typeof(AttributeDefine.Attribute));
        foreach (AttributeDefine.Attribute item in array2)
        {
            if (!CheckAttribute(item))
            {
                Debug.Log(String.Format("{0}がAttributeDefineに用意されていません", item.ToString()));
            }
        }

        //TargetDefineの実装内容チェック
        Array array3 = Enum.GetValues(typeof(TargetDefine.EffectTarget));
        foreach (TargetDefine.EffectTarget item in array3)
        {
            if (!CheckTarget(item))
            {
                Debug.Log(String.Format("{0}がTargetDefineに用意されていません", item.ToString()));
            }
        }

        //CardTagDefineの実装内容チェック
        Array array4 = Enum.GetValues(typeof(CardTagDefine.CardTag));
        foreach (CardTagDefine.CardTag item in array4)
        {
            if (!CheckCardTag(item))
            {
                Debug.Log(String.Format("{0}がCardTagDefineに用意されていません", item.ToString()));
            }
        }

    }

    private bool CheckCardEffect(CardEffectDefine.CardEffect effect)
    {
        if (effect==CardEffectDefine.CardEffect._MAX || CardEffectDefine.Dic_EffectName[effect] != null)
        {//辞書にあるなら
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckAttribute(AttributeDefine.Attribute attribute)
    {
        if (AttributeDefine.Dic_AttributeName[attribute] != null)
        {//辞書にあるなら
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckTarget(TargetDefine.EffectTarget target)
    {
        if (TargetDefine.Dic_EffectTarget[target] != null)
        {//辞書にあるなら
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool CheckCardTag(CardTagDefine.CardTag target)
    {
        if (CardTagDefine.Dic_TagName[target] != null)
        {//辞書にあるなら
            return true;
        }
        else
        {
            return false;
        }
    }
}
