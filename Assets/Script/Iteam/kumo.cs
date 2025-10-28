using UnityEngine;

public class Cloud : MonoBehaviour
{
    [Tooltip("…•½ˆÚ“®")]
    public float moveSpeed = 0.3f;

    [Tooltip("á`–ÊŠO”»’è‚Ì—]”’")]
    public float respawnMargin = 1.0f;

    private float screenLeftX;
    private float screenRightX;

    void Start()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            float zDist = transform.position.z - cam.transform.position.z;
            Vector3 left = cam.ViewportToWorldPoint(new Vector3(0, 0.5f, zDist));
            Vector3 right = cam.ViewportToWorldPoint(new Vector3(1, 0.5f, zDist));
            screenLeftX = left.x;
            screenRightX = right.x;
        }
    }

    void Update()
    {
        transform.Translate(Vector3.right * moveSpeed * Time.deltaTime, Space.World);

        if (transform.position.x > screenRightX + respawnMargin)
        {
            transform.position = new Vector3(screenLeftX - respawnMargin, transform.position.y, transform.position.z);
        }
    }
}