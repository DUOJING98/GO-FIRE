using System.Linq;
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
    [SerializeField] Sprite[] fireStandSprite;// 
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


    /// <summary>
    /// 待機（スタンド）ポーズへ切替（演出）。
    /// </summary>
    public void SetStandPose(bool isMiss)
    {
        if (isMiss)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            if (spriteRenderer != null && standSprite != null)
                spriteRenderer.sprite = standSprite;
        }
    }


    /// <summary>
    /// ラウンド開始時の表示初期化（将来拡張に備え、メソッドとして分離）。
    /// </summary>
    //public void ResetRound()
    //{
    //    SetStandPose(false);
    //}


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
        string keyName = context.control.displayName; // 按键名称
        var digits = new string(keyName.Where(char.IsDigit).ToArray());
        int num = digits.Length > 0 ? int.Parse(digits) : -1;
        if (manager.isWaitingForReady)
        {
            Debug.Log("waiting ready state");
            manager.PlayerPressed(playerName, num);
            return;
        }

        if (!manager.CDM.canInput)
        {
            return;
        }

        if (num > 0)
        {
            //manager.PlayerPressed(playerName, num);
        }
        // 
        //if (!string.IsNullOrEmpty(manager.FirstPlayerPressed)) return;
        ////hasPressed = true;
        //Debug.Log("Fire");

        // 
        if (spriteRenderer != null && fireStandSprite != null && num > 0)
        {
            spriteRenderer.sprite = fireStandSprite[num - 1];
            manager.PlayerPressed(playerName, num);
        }
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
