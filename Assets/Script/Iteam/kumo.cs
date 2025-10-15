using UnityEngine;

public class Cloud : MonoBehaviour
{
    [Tooltip("�����ړ�")]
    public float moveSpeed = -0.5f;

    void Update()
    {
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);
    }
}