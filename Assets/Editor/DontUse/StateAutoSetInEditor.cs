using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UIElements;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

public class StateAutoSetInEditor : EditorWindow
{
    [MenuItem("Window/Uni/SetState")]

    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(StateAutoSetInEditor));
    }

    [SerializeField] private VisualTreeAsset _rootVisualTreeAsset;
    [SerializeField] private StyleSheet _rootStyleSheet;
    [SerializeField] private TextAsset stateDataCSV;
    [SerializeField] private GameObject dataPrefab;

    //この中のどこかの配下にStateのアイコンを、名前と同じファイル名で保存している
    private String[] pathOfStateIcon = new String[]
    {
        "Assets/Images/UI/StateIcon",
        "Assets/Images/UI/StateIcon/UpDownMinus"
    };

    private String[] pathOfDataPrefab = new String[]
    {
        "Assets/Prefabs/Singleton"
    };

    private void CreateGUI()
    {
        _rootVisualTreeAsset.CloneTree(rootVisualElement);
        rootVisualElement.styleSheets.Add(_rootStyleSheet);

        var copyButton = rootVisualElement.Q<Button>("SetState");
        copyButton.clicked += () => {
            //StateのデータはDataManagerに所属する,StateDataにある。
            //複数シーンをまたぐと機能しない
            /*
            GameObject dataObject = GameObject.Find("DataManager");
            StateData stateData = dataObject.GetComponent<StateData>();
            */

            //プレハブのコンポーネントから取得
            //DataManagerプレハブのGUIDを取得
            string[] guids = AssetDatabase.FindAssets("DataManager",pathOfDataPrefab);

            GameObject dataManagerPrefab = null;

            if (guids.Length == 0)
            {//見つからなかった場合
                Debug.Log("DataManager prefab not found(0)");
                return;
            }
            else
            {
                //パスを取得
                string[] paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
                if (paths == null) return;
                //先頭をGameObjectにする
                dataManagerPrefab = (GameObject)AssetDatabase.LoadAssetAtPath(paths[0], typeof(GameObject));
            }

            if(dataManagerPrefab is null)
            {
                Debug.Log("DataManager prefab not found(1)");
                return;
            }

            //コンポーネントを取得
            StateData stateData = dataManagerPrefab.GetComponent<StateData>();

            //TODO:変更の記録のために必要なようだが、本当？
            SerializedObject serializedObjectStateData = new SerializedObject(stateData);
            //このプロパティを変更する
            //https://light11.hatenadiary.com/entry/2018/03/15/225709
            var stateListProperty = serializedObjectStateData.FindProperty("stateNewData_Serialized");
            //一旦クリア
            stateListProperty.ClearArray();

            stateData.StateNewData_Serialized = new List<StateDefine.StateNew>();

            //test
            string assemblyName = typeof(StateAttackDown).Assembly.GetName().Name;

            //文字列の読み込み
            StringReader sr = new StringReader(stateDataCSV.text);
            string line;
            for(int i=0;(line = sr.ReadLine()) != null; i++)
            {//一行ずつ読む
                if (i == 0) continue;//一行目はスキップ

                string[] parts = line.Split(',');

                if(parts.Length >= 5)
                {//適正なデータがあるなら
                    //https://unity-yuji.xyz/type-gettype-null/
                    Type type = Type.GetType(parts[0] + ",Assembly-CSharp");//型を取得

                    if (type == null)
                    {//型を取得できなかったなら
                        Debug.Log("TypeNameError:line" + i+","+ parts[0]);
                        continue;
                    }

                    if(type.GetInterfaces().Contains(typeof(IStateHasAttribute)))
                    {//属性ごとの自動設定を行う状態異常

                        //設定のチェック
                        if (!IsCorrectStateTarget(type, parts[3]))
                        {
                            Debug.Log("Target Error:line" + i);
                            continue;
                        }
                        if (!IsCorrectContinueType(type, parts[4]))
                        {
                            Debug.Log("Continue Type Error:line" + i);
                            continue;
                        }

                        foreach (AttributeDefine.Attribute attribute in Enum.GetValues(typeof(AttributeDefine.Attribute)))
                        {//属性全てに対して
                            //日本語名の取得
                            string attributeJp = AttributeDefine.Dic_AttributeName[attribute];

                            StateDefine.StateNew state = new StateDefine.StateNew();
                            state.stateType = parts[0];//型名はすべて同じものとして扱う

                            state.Name = String.Format(parts[1], attributeJp);

                            state.Description = String.Format(parts[2], attributeJp);

                            AttributeDefine at = new AttributeDefine(attribute);

                            state.attribute = at;//属性の設定

                            //画像を検索してセット
                            Sprite sp = FindSprite(state.Name);
                            if (sp == null)
                            {
                                Debug.Log("Image not Found:" + state.Name);
                                continue;
                            }
                            else
                            {
                                state.IconSprite = sp;
                            }

                            stateData.StateNewData_Serialized.Add(state);
                        }
                    }
                    else
                    {
                        StateDefine.StateNew state = new StateDefine.StateNew();

                        state.stateType = parts[0];//型名の確保

                        state.Name = parts[1];

                        state.Description = parts[2];

                        state.attribute = null;//普通は属性なし

                        //設定のチェック
                        if (!IsCorrectStateTarget(type, parts[3]))
                        {
                            Debug.Log("Target Error:line" + i);
                            continue;
                        }
                        if (!IsCorrectContinueType(type, parts[4]))
                        {
                            Debug.Log("Continue Type Error:line" + i);
                            continue;
                        }

                        //画像を検索してセット
                        Sprite sp = FindSprite(parts[1]);
                        if(sp == null)
                        {
                            Debug.Log("Image not Found:" + parts[1]);
                            continue;
                        }
                        else
                        {
                            state.IconSprite = sp;
                        }

                        //リストにデータを記録
                        //newStateList.Add(state);

                        //要素の追加
                        stateListProperty.InsertArrayElementAtIndex(stateListProperty.arraySize);
                        //要素のセット
                        //stateListProperty.GetArrayElementAtIndex(stateListProperty.arraySize - 1).managedReferenceValue = state;

                        //stateListProperty.GetArrayElementAtIndex(stateListProperty.arraySize - 1).

                        //Old
                        //stateData.StateNewData_Serialized.Add(state);
                    }
                }
            }

            //https://light11.hatenadiary.com/entry/2018/03/15/225709
            //TODO:変更の記録のために必要なようだが、本当？
            serializedObjectStateData.ApplyModifiedProperties();

            Debug.Log("StateSet!");
        };
    }

    /// <summary>
    /// pathOfStateIconからnameの名前の画像を探し、Spriteにして返す
    /// </summary>
    /// <param name="name">検索する画像の名前</param>
    /// <returns>検索された画像</returns>
    private Sprite FindSprite(string name)
    {
        //画像を検索してセット
        //画像のGUIDを取得
        string[] guids = AssetDatabase.FindAssets(name, pathOfStateIcon);

        if (guids.Length == 0)
        {//見つからなかった場合
            return null;
        }
        else
        {
            //パスを取得
            string[] paths = guids.Select(guid => AssetDatabase.GUIDToAssetPath(guid)).ToArray();
            if (paths == null) return null;
            //先頭をSpriteにする
            Sprite sprite = (Sprite)AssetDatabase.LoadAssetAtPath(paths[0], typeof(Sprite));

            return sprite;
        }
    }

    /// <summary>
    /// IStateを設定する際に、データのCSVに書いてあることが
    /// Interfaceを用いて実装されているかを確認する
    /// 効果対象を確認
    /// </summary>
    /// <param name="type">IStateの型Type</param>
    /// <param name="str">効果対象の文字列</param>
    /// <returns>trueなら正常</returns>
    private bool IsCorrectStateTarget(Type type,string str)
    {
        if (String.Compare(str, "All", false) == 0)
        {
            
            if(type.GetInterface(typeof(IStatePlayer).ToString()) != null &&
                type.GetInterface(typeof(IStateEnemy).ToString()) != null &&
                type.GetInterface(typeof(IStateFellow).ToString()) != null)
            {
                return true;
            }
        }else if(String.Compare(str, "Enemy", false) == 0)
        {
            if (type.GetInterface(typeof(IStateEnemy).ToString()) != null)
            {
                return true;
            }
        }
        else if (String.Compare(str, "Player", false) == 0)
        {
            if (type.GetInterface(typeof(IStatePlayer).ToString()) != null)
            {
                return true;
            }
        }
        else if (String.Compare(str, "Fellow", false) == 0)
        {
            if (type.GetInterface(typeof(IStateFellow).ToString()) != null)
            {
                return true;
            }
        }
        else if (String.Compare(str, "PlayerFellow", false) == 0)
        {
            if (type.GetInterface(typeof(IStatePlayer).ToString()) != null &&
                type.GetInterface(typeof(IStateFellow).ToString()) != null)
            {
                return true;
            }
        }
        else if (String.Compare(str, "PlayerEnemy", false) == 0)
        {
            if (type.GetInterface(typeof(IStatePlayer).ToString()) != null &&
                type.GetInterface(typeof(IStateEnemy).ToString()) != null)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// IStateを設定する際に、データのCSVに書いてあることが
    /// Interfaceを用いて実装されているかを確認する
    /// 継続タイプを確認
    /// </summary>
    /// <param name="state">IStateのType</param>
    /// <param name="str">継続タイプの文字列</param>
    /// <returns>trueなら正常</returns>
    private bool IsCorrectContinueType(Type type,string str)
    {
        if (String.Compare(str, "Decrease", false) == 0)
        {
            if (type.IsSubclassOf(typeof(StateContinueTypeDecreaseBase)))
            {
                return true;
            }
        }
        else if (String.Compare(str, "Constant", false) == 0)
        {
            if (type.IsSubclassOf(typeof(StateContinueTypeConstantBase)))
            {
                return true;
            }
        }
        else if (String.Compare(str, "Eternal", false) == 0)
        {
            if (type.IsSubclassOf(typeof(StateContinueTypeEternalBase)))
            {
                return true;
            }
        }

        return false;
    }
}
