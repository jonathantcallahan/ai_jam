using UnityEngine;
using Unity.MLAgents;
using Unity.VisualScripting;
using Unity.MLAgents.Actuators;
using System.Runtime.CompilerServices;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class AgentController : Agent
{
    [Tooltip("Speed to rotate around the up axis")]
    public float yawSpeed = 100f;

    // logic variables
    public float cooldownTime = 2.0f;
    private float cooldownTimer;
    public float rotateSpeed;

    // input variables
    public bool cooldown;
    public bool dead;
    public float yaw;
    public float speed;

    // gun effects
    public Vector3 deadLocation;

    // Respawning
    private Vector3 potentialPosition;
    public LayerMask collisionLayer;
    public int maxAttempts = 50;
    public Vector3 characterSize = new Vector3(1, 1, 1);
    public Vector2 spawnArea;

    private float smoothYawChange = 0f;

    private Vector3 tempNewPosition;

    private bool positionHitDetect;

    public GameObject enemy;


    private Rigidbody rb;

    EnvironmentParameters m_ResetParams;

    private float remainingEnemies;

    private GameObject[] enemies;
    private GameObject arenaController;


    public void EnvironmentReset()
    {
        cooldownTimer = cooldownTime;

        enemies = GameObject.FindGameObjectsWithTag("enemy");
        foreach (GameObject enemyToDestroy in enemies)
        {
            Destroy(enemyToDestroy);
        }

        ArenaGenerator arenaGenerator = GetComponentInParent<ArenaGenerator>();
        arenaGenerator.DespawnArenaObstacles();
        arenaGenerator.SpawnArenaObstacles();

        tempNewPosition = UnoccupiedPosition();
        gameObject.transform.position = tempNewPosition;

        for (int i = 0; i < 10; i++)
        {
            tempNewPosition = UnoccupiedPosition();
            GameObject newEnemy = Instantiate(enemy, tempNewPosition, transform.rotation);

        }



    }

    public override void Initialize()
    {
        //EnvironmentReset();
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
            cooldownTimer = 2f;
            cooldown = true;
        }
    }

    private void Shoot()
    {

        Physics.Raycast(transform.position + transform.forward, transform.forward * 50f, out RaycastHit hit, 25f);
        Debug.DrawLine(transform.position, hit.point, Color.red, 0.2f);
        if (hit.collider != null)
        {
            
            var controller = hit.collider.gameObject;
            if (controller.tag == "enemy")
            {

                Destroy(controller);
                remainingEnemies = GameObject.FindGameObjectsWithTag("enemy").Length;
                
                if (remainingEnemies < 9)
                {
                    SetReward(3f);
                    EndEpisode();
                    Debug.Log("Episode ended");
                } else
                {
                    SetReward(1f);
                }

            } else
            {
                SetReward(-0.05f);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag != "floor")
        {
            Debug.Log("Collided with " + collision.gameObject.tag);
            SetReward(-0.5f);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        
        sensor.AddObservation(transform.position);

    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("Episode begin triggered");
        EnvironmentReset();
    }

    private Vector3 randomPosition()
    {
        return new Vector3(Random.Range(-spawnArea.x / 2, spawnArea.x / 2), 0.75f, Random.Range(-spawnArea.y / 2, spawnArea.y / 2));
    }

    private bool IsPositionOccupied(Vector3 position)
    {
        Collider[] positionHitDetect = Physics.OverlapBox(position, characterSize * 2, Quaternion.identity);
        foreach (Object item in positionHitDetect)
        {
            //Debug.Log(item.ToString());
        }
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
        Debug.Log("No safe position found, returning most recent random position.");
        return potentialPosition;
    }
}
