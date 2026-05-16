using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private AudioSource audioSource;

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

    private void InitialProcess()
    {
        //Application.targetFrameRate = 60;//フレームレートの設定(入れると録画でバグる？)
    }

    public static void SetAudio(AudioClip audioClip)
    {
        Instance.audioSource.clip = audioClip;
        Instance.audioSource.Play();
    }
}
