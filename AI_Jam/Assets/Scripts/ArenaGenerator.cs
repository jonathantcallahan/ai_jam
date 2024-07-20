using UnityEngine;

public class ArenaGenerator : MonoBehaviour
{
    public Transform spawnTransform;

    public GameObject barrierPrefab;
    public GameObject columnPrefab;

    public Vector2 spawnArea;
    public int obstaclesCount;
    public int spawnCountVariance;

    public bool spawned;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    public void SpawnArenaObstacles()
    {
        for (int i = 0; i < obstaclesCount + Random.Range(-spawnCountVariance, spawnCountVariance); i++)
        {
            var spawned = transform;
            if (Random.Range(0,2) == 0)
                spawned = Instantiate(barrierPrefab, spawnTransform).transform;
            else
                spawned = Instantiate(columnPrefab, spawnTransform).transform;

            spawned.position = new Vector3(Random.Range(-spawnArea.x / 2, spawnArea.x / 2), 2, Random.Range(-spawnArea.y / 2, spawnArea.y / 2));
            spawned.Rotate(0, Random.Range(0, 360), 0);
        }
        spawned = true;
    }

    public void DespawnArenaObstacles()
    {
        foreach (Transform child in spawnTransform)
        {
            Destroy(child.gameObject);
        }
        spawned = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !spawned)
            SpawnArenaObstacles();
        else if (Input.GetKeyDown(KeyCode.Space) && spawned)
            DespawnArenaObstacles();
    }
}
