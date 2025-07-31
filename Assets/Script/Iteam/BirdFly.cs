using UnityEngine;

public class BirdFly : MonoBehaviour
{
    public float speed = 2f;
    public float leftX = -8f;
    public float rightX = 8f;

    private bool goingRight = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float direction = goingRight ? 1f : -1f;
        transform.Translate(Vector3.right * speed * direction * Time.deltaTime, Space.World);

        if(goingRight && transform.position.x >= rightX)
        {
            goingRight = false;
            Flip();
        }
        else if (!goingRight && transform.position.x <= leftX)
        {
            goingRight = true;
            Flip();
        }
       
        void Flip()
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}
