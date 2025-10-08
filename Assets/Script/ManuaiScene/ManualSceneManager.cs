using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManualSceneManager : MonoBehaviour
{
    [SerializeField] Button ToManual;
    [SerializeField] Button ToTitle;


    private void Awake()
    {
        if (ToManual != null)
        {
            ToManual.onClick.AddListener(ToManual2);
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

    void ToManual2()
    {
        SceneManager.LoadScene("ManualScene2");
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
