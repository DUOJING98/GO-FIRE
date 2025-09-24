using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;

public class Kusa : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotateSpeed = 180f;
    public float rightX = 10f;
    public float leftX = -10f;
>>>>>>> Stashed changes

    private int direction = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        direction = transform.position.x > 0 ? -1 : 1;

        if (direction == -1)
        {
            FlipSprite();
        }
>>>>>>> Stashed changes
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);

        transform.Rotate(Vector3.forward, -rotateSpeed * direction * moveSpeed * Time.deltaTime);

        if (direction == 1 && transform.position.x > rightX)
>>>>>>> Stashed changes
        {
            direction = -1;
            FlipSprite();
        }
        else if (direction == -1 && transform.position.x < leftX)
        {
            direction = 1;
            FlipSprite();
        }
    }

    void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
