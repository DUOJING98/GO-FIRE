using System.Runtime.CompilerServices;
using UnityEngine;

public class Kusa : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float rotateSpeed = 180f;
    public float endX = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);

        transform.Rotate(Vector3.forward, -rotateSpeed * Time.deltaTime);

        if (transform.position.x > endX)
        {
            Destroy(gameObject);
        }
    }
}
