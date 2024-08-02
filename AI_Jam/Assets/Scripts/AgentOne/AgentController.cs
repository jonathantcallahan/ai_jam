using UnityEngine;
using Unity.MLAgents;
using Unity.VisualScripting;
using Unity.MLAgents.Actuators;
using System.Runtime.CompilerServices;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class AgentController : Agent
{

    // laser controls
    public float cooldownTime = 5.0f;
    private float cooldownTimer;
    private bool cooldown;

    // prefabs
    public GameObject barrierPrefab;
    public GameObject columnPrefab;
    public GameObject enemy;

    // spawning
    private Vector3 potentialPosition;
    public int obstaclesCount;
    public int spawnCountVariance;
    public int maxAttempts = 100;
    public Vector3 characterSize = new Vector3(1, 1, 1);
    public Vector2 spawnArea;
    private Vector3 tempNewPosition;
    private bool positionHitDetect;
    public Transform spawnTransform;

    // enemy management
    private float remainingEnemies;
    private GameObject[] enemies;

    public override void Initialize()
    {
        if (!Academy.Instance.IsCommunicatorOn)
        {
            this.MaxStep = 0;
        }
    }

    public void EnvironmentReset()
    {
        cooldownTimer = cooldownTime;

        enemies = GameObject.FindGameObjectsWithTag("enemy");
        foreach (GameObject enemyToDestroy in enemies)
        {
            Destroy(enemyToDestroy);
        }

        DespawnArenaObstacles();
        SpawnArenaObstacles();

        tempNewPosition = UnoccupiedPosition();
        gameObject.transform.position = tempNewPosition;
        gameObject.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

        for (int i = 0; i < 20; i++)
        {
            tempNewPosition = UnoccupiedPosition();
            GameObject newEnemy = Instantiate(enemy, tempNewPosition, transform.rotation);
        }
    }

    public void SpawnArenaObstacles()
    {
        for (int i = 0; i < obstaclesCount + Random.Range(-spawnCountVariance, spawnCountVariance); i++)
        {
            var spawned = transform;
            if (Random.Range(0, 2) == 0)
            {
                spawned = Instantiate(barrierPrefab, spawnTransform).transform;
            }
            else
            {
                spawned = Instantiate(columnPrefab, spawnTransform).transform;
            }
            spawned.position = new Vector3(Random.Range(-spawnArea.x / 2, spawnArea.x / 2), 2, Random.Range(-spawnArea.y / 2, spawnArea.y / 2));
            spawned.Rotate(0, Random.Range(0, 360), 0);
        }
    }

    public void DespawnArenaObstacles()
    {
        foreach (Transform child in spawnTransform)
        {
            Destroy(child.gameObject);
        }
    }


    //cont[0] = speed, cont[1] = yaw, disc[0] = shoot
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continousActionsOut = actionsOut.ContinuousActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        continousActionsOut[0] = Input.GetAxisRaw("Vertical");
        continousActionsOut[1] = Input.GetAxisRaw("Horizontal");
    }

    //cont[0] = speed, cont[1] = yaw, disc[0] = shoot
    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        var discreteActions = actions.DiscreteActions;
        var moveForward = continuousActions[0];
        var moveRotate = continuousActions[1];

        SetReward(-0.0001f);
        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * moveForward * Time.deltaTime);
        transform.Rotate(0f, moveRotate, 0f, Space.Self);
        if (cooldown)
        {
            cooldownTimer -= .02f;
            if (cooldownTimer <= 0)
            {
                cooldown = false;
                cooldownTimer = cooldownTime;
            }
        }
        if (discreteActions[0] == 1 && !cooldown)
        {
            Shoot();
            cooldownTimer = 5f;
            cooldown = true;
        }
    }

    private void Shoot()
    {
        Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 10f);
        if (hit.collider != null)
        {
            var controller = hit.collider.gameObject;
            if (controller.tag == "enemy")
            {
                Debug.DrawLine(transform.position, hit.point, Color.red, 0.2f);
                Destroy(controller);
                remainingEnemies = GameObject.FindGameObjectsWithTag("enemy").Length;

                SetReward(1f);
                if (remainingEnemies < 2)
                {  
                    EndEpisode();
                    Debug.Log("Episode ended");
                } 
            } else
            {
                Debug.DrawLine(transform.position, hit.point, Color.red, 0.2f);
            }
        }
        else
        {
            Debug.DrawLine(transform.position, transform.forward * 10f, Color.blue, 0.2f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "floor")
        {
            
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //sensor.AddObservation(transform.position);
        //sensor.AddObservation(transform.rotation);
        sensor.AddObservation(cooldown ? 1 : 0);
    }

    public override void OnEpisodeBegin()
    {
        EnvironmentReset();
    }

    private Vector3 randomPosition()
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        return new Vector3(Random.Range(mesh.bounds.min.x, mesh.bounds.max.x), 0.75f, Random.Range(mesh.bounds.min.y, mesh.bounds.max.y));
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        Vector3 worldPosition = transform.TransformPoint(position);
        Collider[] positionHitDetect = Physics.OverlapBox(worldPosition, characterSize / 2, Quaternion.identity);
        return positionHitDetect.Length > 1;
    }
    public Vector3 UnoccupiedPosition()
    {
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
}
