using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;

public class GameController : MonoBehaviour
{
    public GameObject[] agents;
    public GameObject enemy;

    private List<GameObject> enemyList = new List<GameObject>();

    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void EnvironmentReset()
    {
        ArenaGenerator arenaGenerator = GetComponentInParent<ArenaGenerator>();
        agents = GameObject.FindGameObjectsWithTag("agent");

        arenaGenerator.DespawnArenaObstacles();
        arenaGenerator.SpawnArenaObstacles();
        
        for (int i = 0; i < 10; i++) { 
            GameObject newEnemy = Instantiate(enemy);
            enemyList.Add(newEnemy);

        //    arenaGenerator.placeUnits(newEnemy);
        }
        arenaGenerator.placeUnits(enemyList.ToArray());

        //arenaGenerator.placeUnits(agents);

    }
}
