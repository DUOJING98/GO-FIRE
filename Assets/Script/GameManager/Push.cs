using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class Push : MonoBehaviour
{

    public string playerName = " ";
    public GameManager manager;
    private SpriteRenderer spriteRenderer;
    [Header("姿勢")]
    [SerializeField] Sprite standSprite;// 通常立ち
    [SerializeField] Sprite fireStandSprite;// 立ち撃ち
    [SerializeField] Sprite fireCrouchSprite;// しゃがみ撃ち
    [SerializeField] Sprite fireJumpSprite;// ジャンプ撃ち

    

    private InputSystem_Actions action;
    [Header("状態")]
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
        // 回合終了後に立ち姿に戻す
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
        if (!canPress || hasPressed) return;
        // 他のプレイヤーが既にボタンを押していたら無効
        if (!string.IsNullOrEmpty(manager.FirstPlayerPressed)) return;
        hasPressed = true;
        Debug.Log("Fire");


        // 姿勢を撃つ姿に変更
        if (spriteRenderer != null && fireStandSprite != null)
            spriteRenderer.sprite = fireStandSprite;

        //if (isRealGo)
        manager.PlayerPressed(playerName, true);
        //else
        //    manager.PlayerPressed(playerName, false);
    }

}
