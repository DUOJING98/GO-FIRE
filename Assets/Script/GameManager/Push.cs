using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class Push : MonoBehaviour, IPlayer1Actions, IPlayer2Actions
{


    protected bool isTurn = false;
    public string playerName = "P1";
    public GameManager manager;

    public Transform charaTransform;

    public void OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            manager.PlayerPressed(playerName);
            if(!isTurn) {
                charaTransform.Rotate(0f, 180f, 0f);
                isTurn = true;
            }
           

        }
        Debug.Log("Fire!");
    }

}
