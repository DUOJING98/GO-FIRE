using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class Push : MonoBehaviour
{

    public string playerName = " ";
    public GameManager manager;
    private SpriteRenderer spriteRenderer;
    [Header("ﾗﾋ・")]
    [SerializeField] Sprite standSprite;// ﾍｨｳ｣ﾁ｢､ﾁ
    [SerializeField] Sprite fireStandSprite;// ﾁ｢､ﾁ篤､ﾁ
    [SerializeField] Sprite fireCrouchSprite;// ､ｷ､网ｬ､ﾟ篤､ﾁ
    [SerializeField] Sprite fireJumpSprite;// ･ｸ･罕ﾗ篤､ﾁ

    

    private InputSystem_Actions action;
    [Header("ﾗｴ腺")]
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
        // ｻﾘｺﾏｽKﾁﾋ矣､ﾋﾁ｢､ﾁﾗﾋ､ﾋ諾､ｹ
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
        // ﾋ訷ﾎ･ﾗ･・､･茫`､ｬｼﾈ､ﾋ･ﾜ･ｿ･ｺ､ｷ､ﾆ､､､ｿ､髻o・
        if (!string.IsNullOrEmpty(manager.FirstPlayerPressed)) return;
        hasPressed = true;
        Debug.Log("Fire");


        // ﾗﾋ・､ﾄ､ﾄﾗﾋ､ﾋ我ｸ・
        if (spriteRenderer != null && fireStandSprite != null)
            spriteRenderer.sprite = fireStandSprite;

        //if (isRealGo)
        manager.PlayerPressed(playerName, true);
        //else
        //    manager.PlayerPressed(playerName, false);
    }

}
