using UnityEngine;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

public class PlayerController : MonoBehaviour, IPlayer1Actions, IPlayer2Actions
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnFire(InputAction.CallbackContext context)
    {
        // 实际的射击逻辑就在这里
        Debug.Log("Firing weapon!");
        //if ()
        //{

        //}
    }
}
