using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class Push : MonoBehaviour
{

    public string playerName = " ";
    public GameManager manager;
    private SpriteRenderer spriteRenderer;
    [Header("�˄�")]
    [SerializeField] Sprite standSprite;// ͨ������
    [SerializeField] Sprite fireStandSprite;// �����Ĥ�
    [SerializeField] Sprite fireCrouchSprite;// ���㤬�ߓĤ�
    [SerializeField] Sprite fireJumpSprite;// �����דĤ�

    

    private InputSystem_Actions action;
    [Header("״�B")]
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
        // �غϽK����������ˤˑ���
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
        // ���Υץ쥤��`���Ȥ˥ܥ����Ѻ���Ƥ�����o��
        if (!string.IsNullOrEmpty(manager.FirstPlayerPressed)) return;
        hasPressed = true;
        Debug.Log("Fire");


        // �˄ݤ�Ĥ��ˤˉ��
        if (spriteRenderer != null && fireStandSprite != null)
            spriteRenderer.sprite = fireStandSprite;

        //if (isRealGo)
        manager.PlayerPressed(playerName, true);
        //else
        //    manager.PlayerPressed(playerName, false);
    }

}
