using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class AreaController : MonoBehaviour
{

    public GameObject arenaFloor;
    public GameObject enemy;

    private Vector3 characterSize = new Vector3(1f, 1f, 1f);    
    private int maxAttempts = 50;
    private int obstaclesCount = 10;
    private int spawnCountVariance = 10;
    
    public void placeAgent(GameObject agentRB)
    {
        agentRB.transform.localPosition = randomPosition();
        agentRB.transform.rotation = Quaternion.Euler(0f, Random.Range(0, 360), 0f);
    }
    public void instantiateEnemy()
    {
        Instantiate(enemy, randomPosition(), Quaternion.Euler(0f, Random.Range(0, 360), 0f));
    }

    private Vector3 randomPosition()
    {
        var spawnArea = arenaFloor.GetComponent<MeshRenderer>().bounds.size - new Vector3(2f,0f,2f);
        return new Vector3(Random.Range(-spawnArea.x / 2, spawnArea.x / 2), 0.75f, Random.Range(-spawnArea.y / 2, spawnArea.y / 2));
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        Collider[] positionHitDetect = Physics.OverlapBox(position, characterSize / 2, Quaternion.identity);
        return positionHitDetect.Length > 1;
    }
    public Vector3 UnoccupiedPosition()
    {
        var potentialPosition = Vector3.zero;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            potentialPosition = randomPosition();

            if (!IsPositionOccupied(potentialPosition))
            {
                return potentialPosition;
            }
        }
        return potentialPosition;
    }

    private void checkRayCast()
    {
        RayPerceptionSensorComponent3D m_rayPerceptionSensorComponent3D = GetComponent<RayPerceptionSensorComponent3D>();

        var rayOutputs = RayPerceptionSensor.Perceive(m_rayPerceptionSensorComponent3D.GetRayPerceptionInput()).RayOutputs;
        int lengthOfRayOutputs = rayOutputs.Length;

        // Alternating Ray Order: it gives an order of
        // (0, -delta, delta, -2delta, 2delta, ..., -ndelta, ndelta)
        // index 0 indicates the center of raycasts
        for (int i = 0; i < lengthOfRayOutputs; i++)
        {
            GameObject goHit = rayOutputs[i].HitGameObject;
            if (goHit != null)
            {
                var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
                var scaledRayLength = rayDirection.magnitude;
                float rayHitDistance = rayOutputs[i].HitFraction * scaledRayLength;

                // Print info:
                string dispStr = "";
                dispStr = dispStr + "__RayPerceptionSensor - HitInfo__:\r\n";
                dispStr = dispStr + "GameObject name: " + goHit.name + "\r\n";
                dispStr = dispStr + "Hit distance of Ray: " + rayHitDistance + "\r\n";
                dispStr = dispStr + "GameObject tag: " + goHit.tag + "\r\n";
                Debug.Log(dispStr);
            }
        }
    }
}
