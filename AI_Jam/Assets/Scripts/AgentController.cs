using UnityEngine;
using Unity.MLAgents;
using Unity.VisualScripting;
using Unity.MLAgents.Actuators;
using System.Runtime.CompilerServices;

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
    public int maxAttempts = 10;
    public Vector3 characterSize = new Vector3(1, 1, 1);
    public Vector2 spawnArea;

    private float smoothYawChange = 0f;

    private Rigidbody rb;

    EnvironmentParameters m_ResetParams;

    public override void Initialize()
    {
        cooldownTimer = cooldownTime;
        rb = GetComponent<Rigidbody>();
        SpawnUnit();

    }



    //cont[0] = speed, cont[1] = yaw, disc[0] = shoot
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        if(Input.GetKey(KeyCode.W))
        {
            continuousActionsOut[0] = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            continuousActionsOut[1] -= rotateSpeed;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            continuousActionsOut[1] += rotateSpeed;
        }
        Debug.Log("Discreet actions");
        var discreteActionsOut = actionsOut.DiscreteActions;
        Debug.Log(discreteActionsOut);
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }

    //cont[0] = speed, cont[1] = yaw, disc[0] = shoot
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (dead)
        {
            SetReward(-1.0f);
            EndEpisode();
        }
        var continuousActions = actions.ContinuousActions;
        var discreteActions = actions.DiscreteActions;

        rb.linearVelocity = transform.forward * continuousActions[0];

        var yawChange = continuousActions[1];
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        Vector3 rotationVector = transform.rotation.eulerAngles;
        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;

        // Apply the new rotation
        transform.rotation = Quaternion.Euler(0f, yaw, 0f);

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
        Debug.Log("Raycast shot trigger");
        Physics.Raycast(transform.position + transform.forward, transform.forward * 50f, out RaycastHit hit, 25f);
        Debug.DrawLine(transform.position, hit.point, Color.red, 0.2f);
        if (hit.collider != null)
        {
            var controller = hit.collider.gameObject.GetComponent<AgentController>();
            if (controller != null)
            {
                Debug.Log("ENEMY HIT");
                EndEpisode();
                //controller.dead = true;
                //deadLocation = new Vector3(0f, -15f, 0f);
                //controller.transform.position = deadLocation;
                SetReward(1.0f);
                controller.SetReward(-1.0f);
                //controller.EndEpisode();
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        SpawnUnit();
    }

    private void SpawnUnit()
    {
        bool IsPositionOccupied(Vector3 position)
        {
            Collider[] colliders = Physics.OverlapBox(position, characterSize, Quaternion.identity, collisionLayer);
            return colliders.Length > 0;
        }

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            potentialPosition = new Vector3(Random.Range(-spawnArea.x / 2, spawnArea.x / 2), 2, Random.Range(-spawnArea.y / 2, spawnArea.y / 2));
            if (IsPositionOccupied(potentialPosition))
            {
                transform.position = potentialPosition;
            }
        }
    }
}
