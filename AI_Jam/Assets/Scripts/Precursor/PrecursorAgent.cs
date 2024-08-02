using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;

public class PrecursorAgent : Agent
{
    public GameObject bullet;
    public GameObject area;
    private AreaController areaController;


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continousActionsOut = actionsOut.ContinuousActions;
        discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        continousActionsOut[0] = Input.GetAxisRaw("Vertical");
        continousActionsOut[1] = Input.GetAxisRaw("Horizontal");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        var discreteActions = actions.DiscreteActions;
        var moveForward = continuousActions[0];
        var moveRotate = continuousActions[1];

        SetReward(-0.0001f);
        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * moveForward * Time.deltaTime);
        transform.Rotate(0f, moveRotate, 0f, Space.Self);
        
        if (discreteActions[0] == 1)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        GameObject newBullet = Instantiate(bullet, transform.position + transform.forward, Quaternion.Euler(90f, transform.rotation.eulerAngles.y, 0f));
        newBullet.GetComponent<Rigidbody>().linearVelocity = transform.forward * 10;
    }

    private void EnvironmentReset()
    {
        areaController = area.GetComponent<AreaController>();
        areaController.placeAgent(gameObject);
        areaController.instantiateEnemy();
    }

    public override void Initialize()
    {
        EnvironmentReset();   
    }

    public override void OnEpisodeBegin()
    {
        EnvironmentReset();
    }
}
