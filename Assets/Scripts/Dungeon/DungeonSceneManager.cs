using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// シーン遷移を管理。
/// ダンジョン側。
/// </summary>
public class DungeonSceneManager : MonoBehaviour
{
    [SerializeField] private string villageSceneName;

    /// <summary>
    /// 村シーンへ移動する
    /// </summary>
    public void GoVillage()
    {
        SceneManager.LoadScene(villageSceneName);
    }
}
