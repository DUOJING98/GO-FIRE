<<<<<<< Updated upstream
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSceneManager : MonoBehaviour
{
    [SerializeField] private Button ReStartButton;   //ゲーム再開
    [SerializeField] private Button QuitButton;      //退出
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(ReStartButton != null)
        {
            ReStartButton.onClick.AddListener(OnReStartGame);
        }
        
        if(QuitButton != null)
        {
            QuitButton.onClick.AddListener(OnQuitGame);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnReStartGame()
    {
        SceneManager.LoadScene("GameScene");
    }

    void OnQuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("GameOver");
#endif
    }
}
=======
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSceneManager : MonoBehaviour
{
    [SerializeField] private Button TitleButton;   //ゲーム再開
    [SerializeField] private Button ReStartButton;   //ゲーム再開
    [SerializeField] private Button QuitButton;      //退出
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (TitleButton != null)
        {
            TitleButton.onClick.AddListener(ToTitle);
        }

        if(ReStartButton != null)
        {
            ReStartButton.onClick.AddListener(OnReStartGame);
        }
        
        if(QuitButton != null)
        {
            QuitButton.onClick.AddListener(OnQuitGame);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void ToTitle()
    {
        SceneManager.LoadScene("TitleScene");

    }

    void OnReStartGame()
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
>>>>>>> Stashed changes
