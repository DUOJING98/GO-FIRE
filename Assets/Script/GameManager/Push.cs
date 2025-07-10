using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class Push : MonoBehaviour
{

    public string playerName = " ";
    public GameManager manager;

    protected bool isTurn = false;

    public Transform charaTransform;
    private InputSystem_Actions action;

    private bool canPress = false;
    private bool isRealGo = false;
    private bool hasPressed = false;
    private bool hasTurn = false;
    void Awake()
    {
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
        hasTurn = false;
    }

    public void RestRound()
    {
        //turn back
        if (charaTransform != null)
        {
            charaTransform.rotation = Quaternion.identity;
        }
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
        hasPressed = true;
        Debug.Log("Fire");

        

        if (!hasTurn && charaTransform != null)
        {
            charaTransform.Rotate(0, 180, 0);
            hasTurn = true;
        }

        //if (isRealGo)
            manager.PlayerPressed(playerName, true);
        //else
        //    manager.PlayerPressed(playerName, false);
    }

}
