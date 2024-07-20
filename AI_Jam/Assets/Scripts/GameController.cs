using UnityEngine;
using Unity.MLAgents;

public class GameController : MonoBehaviour
{

    public ArenaGenerator arenaGenerator;
    public GameObject[] agents;
    public GameObject arena;
    
    public void Awake()
    {
        Academy.Instance.OnEnvironmentReset += EnvironmentReset;
    }

    void EnvironmentReset()
    {
        arenaGenerator = GetComponentInParent<ArenaGenerator>();
        arenaGenerator.DespawnArenaObstacles();
        arenaGenerator.SpawnArenaObstacles();

        agents = GameObject.FindGameObjectsWithTag("agent");

    }
}
