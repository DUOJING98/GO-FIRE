using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class Push : MonoBehaviour
{

    public string playerName = " ";
    public GameManager manager;
    private SpriteRenderer spriteRenderer;
    [Header("p¨")]
    [SerializeField] Sprite standSprite;// —§‚Â
    [SerializeField] Sprite fireStandSprite;// ‚½‚¿Œ‚‚Â
    [SerializeField] Sprite fireCrouchSprite;// ‚µ‚á‚ª‚ŞŒ‚‚Â
    [SerializeField] Sprite fireJumpSprite;// ƒWƒƒƒ“ƒvŒ‚‚Â

    

    private InputSystem_Actions action;
    [Header("ó‘Ô")]
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
        //idleó‘Ô
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
        if (!manager.CDM.canInput) return;
        if (!canPress || hasPressed) return;
        // ‘‚¢•ûæ“¾
        if (!string.IsNullOrEmpty(manager.FirstPlayerPressed)) return;
        hasPressed = true;
        Debug.Log("Fire");


        // Œ‚‚Âó‘Ô
        if (spriteRenderer != null && fireStandSprite != null)
            spriteRenderer.sprite = fireStandSprite;

        //if (isRealGo)
        manager.PlayerPressed(playerName, true);
        //else
        //    manager.PlayerPressed(playerName, false);
    }

}
