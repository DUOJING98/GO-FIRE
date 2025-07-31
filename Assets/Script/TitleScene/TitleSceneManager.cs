using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private Button StartButton;         //開始
    [SerializeField] private Button ManualButton;//オープション＆説明
    [SerializeField] private Button QuitButton;          //退出

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (StartButton != null)
        {
            StartButton.onClick.AddListener(OnStartGame);
        }

        if (ManualButton != null)
        {
            ManualButton.onClick.AddListener(OnOpenManual);
        }

        if (QuitButton != null)
        {
            QuitButton.onClick.AddListener(OnQuitGame);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnStartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    void OnOpenManual()
    {
        SceneManager.LoadScene("ManualScene");
    }

    void OnQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("GameOver");
#endif
    }
}
