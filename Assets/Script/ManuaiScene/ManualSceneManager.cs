using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManualSceneManager : MonoBehaviour
{
    [SerializeField] Button ToGame;
    [SerializeField] Button ToTitle;


    private void Awake()
    {
        if (ToGame != null)
        {
            ToGame.onClick.AddListener(ToGameScene);
        }
        if(ToTitle != null)
        {
            ToTitle.onClick.AddListener(ToTitleScene);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void ToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }

    void ToTitleScene()
    {
        SceneManager.LoadScene("TitleScene");
    }


    void OnQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        //Debug.Log("GameOver");
#endif
    }
}
