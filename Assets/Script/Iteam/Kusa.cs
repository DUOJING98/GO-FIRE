using System.Runtime.CompilerServices;
using UnityEngine;

public class Kusa : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotateSpeed = 180f;
    public float endX = 12f;

    private int direction = 1;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (transform.position.x > 0)
        {
            direction = -1;
            rotateSpeed = Mathf.Abs(rotateSpeed);
            FlipSprite();
        }
        else
        {
            direction = -1;
            rotateSpeed = -Mathf.Abs(rotateSpeed);
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);

        transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);

        if ((direction == 1 && transform.position.x > endX)||
            (direction == -1 && transform.position.x < -endX)) 
        {
            Destroy(gameObject);
        }
    }

    void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
