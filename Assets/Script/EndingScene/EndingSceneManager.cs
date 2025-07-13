using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndingSceneManager : MonoBehaviour
{
    [SerializeField] private Button ReStartButton;   //ÉQÅ[ÉÄçƒäJ
    [SerializeField] private Button QuitButton;      //ëﬁèo
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
