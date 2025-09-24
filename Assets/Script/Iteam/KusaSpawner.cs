using UnityEngine;

public class KusaSpawner : MonoBehaviour
{
    public GameObject KusaPrefab;
    public float spawnerInterval = 5f;
    private float timer = 0f;

    public float spawnY = -2f;
    public float spawnXRange = 10f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > spawnerInterval)
        {
            timer = 0f;
            bool fromLeft = Random.value > 0.5f;
            float x = fromLeft ? -spawnXRange : spawnXRange;
            Vector3 pos = new Vector3(x, spawnY, 0);

            Instantiate(KusaPrefab, pos, Quaternion.identity);
        }
    }
}
