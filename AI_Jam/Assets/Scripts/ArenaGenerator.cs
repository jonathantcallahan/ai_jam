using System.Runtime.CompilerServices;
using UnityEngine;

public class ArenaGenerator : MonoBehaviour
{
    public Transform spawnTransform;

    public GameObject barrierPrefab;
    public GameObject columnPrefab;


    public LayerMask collisionLayer;
    public int maxAttempts = 10;
    public Vector3 characterSize = new Vector3(1, 1, 1);

    public Vector2 spawnArea;
    public int obstaclesCount;
    public int spawnCountVariance;

    public bool spawned;

    private Vector3 potentialPosition;
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

    bool IsPositionOccupied(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapBox(position, characterSize, Quaternion.identity, collisionLayer);
        return colliders.Length > 0;
    }

    public void placeUnits(GameObject[] units)
    {
        foreach (var unit in units)
        {
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                potentialPosition = new Vector3(Random.Range(-spawnArea.x / 2, spawnArea.x / 2), 0.5f, Random.Range(-spawnArea.y / 2, spawnArea.y / 2));
                Debug.Log("Attempting to place enemy");
                if (!IsPositionOccupied(potentialPosition))
                {
                    Debug.Log("Successfully placed enemy");
                    unit.transform.position = potentialPosition;
                }

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        int CountActiveAgents()
        {
            int activeCount = 0;
            GameObject[] agents = GameObject.FindGameObjectsWithTag("agent");
            foreach (GameObject agent in agents) {
                AgentController agentController = agent.GetComponent<AgentController>();
                if (agentController != null && !agentController.dead)
                    activeCount++;
                {
                    
                }
            }
            return activeCount;
        }
        int remainingPlayers = CountActiveAgents();
        if (remainingPlayers < 2)
        {

        }
    }
}
