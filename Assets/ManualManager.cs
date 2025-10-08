using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ManualManager : MonoBehaviour
{
    [SerializeField] Button ToGame;
    [SerializeField] Button ToManual;


    private void Awake()
    {
        if (ToManual != null)
        {
            ToManual.onClick.AddListener(ToManualScene);
        }
        if ( ToGame!= null)
        {
            ToGame.onClick.AddListener(ToGameScene);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void ToManualScene()
    {
        SceneManager.LoadScene("ManualScene1");
    }

    void ToGameScene()
    {
        SceneManager.LoadScene("GameScene");
    }


    void OnQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        //Debug.Log("GameOver");
#endif
    }
}
