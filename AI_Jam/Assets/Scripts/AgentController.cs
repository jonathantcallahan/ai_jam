using UnityEngine;
using Unity.MLAgents;
using Unity.VisualScripting;
using Unity.MLAgents.Actuators;

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

    private float smoothYawChange = 0f;

    private Rigidbody rb;

    public override void Initialize()
    {
        cooldownTimer = cooldownTime;
        rb = GetComponent<Rigidbody>();
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
        var discreteActionsOut = actionsOut.DiscreteActions;
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
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                cooldown = false;
                cooldownTimer = cooldownTime;
            }
        }

        if (discreteActions[0] == 1 && !cooldown)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        Physics.Raycast(transform.position + transform.forward, transform.forward, out RaycastHit hit);
        if (hit.collider != null)
        {
            var controller = hit.collider.gameObject.GetComponent<AgentController>();
            if (controller != null)
            {
                controller.dead = true;
                SetReward(1.0f);
                Gizmos.DrawRay(transform.position + transform.forward, transform.forward * 10);
            }
        }
    }
}
