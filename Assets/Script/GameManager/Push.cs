using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class Push : MonoBehaviour
{

    public string playerName = " ";
    public GameManager manager;
    private SpriteRenderer spriteRenderer;
    [Header("pose")]
    [SerializeField] Sprite standSprite;// 
    [SerializeField] Sprite fireStandSprite;// 
                                            //[SerializeField] Sprite fireCrouchSprite;// 
                                            //[SerializeField] Sprite fireJumpSprite;// 

    // [SerializeField] GameObject readyPrefab;
    // private GameObject readyText;

    private InputSystem_Actions action;
    [Header("item")]
    private bool canPress = false;
    private bool isRealGo = false;
    private bool hasPressed = false;
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        action = new InputSystem_Actions();

        if (playerName == "P1")
        {
            action.Player1.Fire.performed += OnFire;
        }
        else if (playerName == "P2")
        {
            action.Player2.Fire.performed += OnFire;
        }
    }

    public void BeginRound(bool isGO)
    {
        canPress = true;
        isRealGo = isGO;
        hasPressed = false;

    }

    public void ResetRound()
    {
        //
        if (spriteRenderer != null && standSprite != null)
            spriteRenderer.sprite = standSprite;
    }

    private void OnEnable()
    {
        if (playerName == "P1") action.Player1.Enable();

        else if (playerName == "P2") action.Player2.Enable();
    }

    private void OnDisable()
    {
        action.Player1.Disable();
        action.Player2.Disable();
    }

    void OnFire(InputAction.CallbackContext context)
    {
        if (manager.isWaitingForReady)
        {
            Debug.Log("waiting ready state");
            manager.PlayerPressed(playerName, true);
            return;
        }

        if (!manager.CDM.canInput)
        {
            return;
        }

        Debug.Log("can input state");
        manager.PlayerPressed(playerName, true);
        // 
        //if (!string.IsNullOrEmpty(manager.FirstPlayerPressed)) return;
        ////hasPressed = true;
        //Debug.Log("Fire");

        // 
        if (spriteRenderer != null && fireStandSprite != null)
            spriteRenderer.sprite = fireStandSprite;

        //if (isRealGo)

    }

    //public void ClearReady()
    //{
    //    if (readyText != null)
    //    {
    //        Destroy(readyText);
    //        readyText = null;
    //    }
    //}

}
