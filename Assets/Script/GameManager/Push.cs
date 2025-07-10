using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class Push : MonoBehaviour, InputSystem_Actions.IPlayer1Actions, InputSystem_Actions.IPlayer2Actions
{
    private InputSystem_Actions inputActions;
    private CountDownManager cdm;
    public GameObject player1, player2;
    private void Awake()
    {
        cdm = GameObject.Find("CountDownManager").GetComponent<CountDownManager>();
        inputActions = new InputSystem_Actions();
        inputActions.Player1.SetCallbacks(this);
        inputActions.Player2.SetCallbacks(this);
    }
    private void OnEnable()
    {
        inputActions.Player1.Enable();
        inputActions.Player2.Enable();
    }
    void OnDisable()
    {
        inputActions.Player1.Disable();
        inputActions.Player2.Disable();
    }
    void IPlayer1Actions.OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            float damage = player1.GetComponent<Player>().attackPower;
            //que ren ji neng 
            var control = context.control;
            Debug.Log(control.name);
            if (control.name == "digit1")
            {
                Debug.Log("attackpowerup");
            }
            if (control.name == "digit2")
            {

            }
            if (control.name == "digit3")
            {

            }
            if (cdm.signal == "START")
            {
                player2.GetComponent<Player>().hp -= damage;
                cdm.canPress = false;
            }
            //shi bai ,zi ji kou xue
            else
            {
                player1.GetComponent<Player>().hp -= damage;
            }
        }
    }
    void IPlayer2Actions.OnFire(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (cdm.canPress)
            {
                float damage = player2.GetComponent<Player>().attackPower;
                //que ren ji neng 
                var control = context.control;
                if (control.name == "numpad1")
                {
                    damage *= 1.5f;
                }
                if (control.name == "numpad2")
                {

                }
                if (control.name == "numpad3")
                {

                }
               
                //gong ji cheng gong
                if (cdm.signal == "START")
                {
                    player1.GetComponent<Player>().hp -= damage;
                    cdm.canPress = false;
                }
                //shi bai ,zi ji kou xue
                else
                {
                    player2.GetComponent<Player>().hp -= damage;
                }
            }
        }
    }
}
