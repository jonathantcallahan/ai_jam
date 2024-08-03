using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.VisualScripting;
using UnityEngine.InputSystem;

public class PrecursorAgent : Agent
{
    public GameObject bullet;
    public GameObject area;
    private AreaController areaController;
    private float totalScore = 0;
    private bool enemyInSights = false;


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        var continousActionsOut = actionsOut.ContinuousActions;
        //discreteActionsOut[0] = Input.GetKey(KeyCode.Space) ? 1 : 0;
        continousActionsOut[0] = Input.GetAxisRaw("Vertical");
        continousActionsOut[1] = Input.GetAxisRaw("Horizontal");
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var continuousActions = actions.ContinuousActions;
        var discreteActions = actions.DiscreteActions;
        var moveForward = continuousActions[0];
        var moveRotate = continuousActions[1];

        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * moveForward * Time.deltaTime * 3);
        transform.Rotate(0f, moveRotate, 0f, Space.Self);
        
        //if (discreteActions[0] == 1)
        //{
        //    Shoot();
        //}

        checkRayCast();

        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 15f))
        {
            var controller = hit.collider.gameObject;
            enemyInSights = controller.tag == "enemy";
        }

        if (enemyInSights) {
            AddReward(.1f);
            totalScore += 0.1f;
            if (totalScore > 9)
            {
                EndEpisode();
            }
        } else
        {
            AddReward(-0.01f);
        }
    }

    private void Shoot()
    {
        GameObject newBullet = Instantiate(bullet, transform.position + transform.forward, Quaternion.Euler(90f, transform.rotation.eulerAngles.y, 0f));
        newBullet.GetComponent<Rigidbody>().linearVelocity = transform.forward * 10;
    }

    private void EnvironmentReset()
    {
        var enemies = GameObject.FindGameObjectsWithTag("enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

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

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(enemyInSights ? 1 : 0);
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
                //Debug.Log(dispStr);
            }
        }
    }
}
