using UnityEngine;
using UnityEngine.InputSystem;

public class Push : MonoBehaviour
{
    [SerializeField]    InputSystem_Actions Turnaction;

    protected bool isTurn = false;


    protected virtual void OnEnable()
    {
        Turnaction.Enable();
    }

    protected virtual void OnDisable( ) 
    {
        Turnaction.Disable();
    }

    private void OnTurn()
    {
        Flip();
    }
    protected virtual void Flip()
    {
        isTurn = !isTurn;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
