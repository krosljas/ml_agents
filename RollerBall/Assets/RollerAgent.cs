using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class RollerAgent : Agent
{
    Rigidbody rBody; // Reference to the RollerAgent's Rigidbody component
    // Start is called before the first frame update
    void Start()
    {
        rBody = GetComponent<Rigidbody>();
    }

    public Transform Target; // Reference to the target cube's transform
    // Called at the start of each episode to set-up the environment
    public override void OnEpisodeBegin()
    {
        // If the Agent fell, zero its momentum
        if (this.transform.localPosition.y < 0)
        {
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }

        // Move the target to a new spot
        Target.localPosition = new Vector3(Random.value * 8 - 4,
                                           0.5f,
                                           Random.value * 8 - 4);
    }

    // Provides feature vector of size 8 to the brain for decision making
    public override void CollectObservations(VectorSensor sensor)
    {
    /* Here we gather the agent and target positions, in addition to the
       velocity of the agent. This helps the agent navigate the ball to
       the target cube without overshooting - ie need Velocity control
    */
        // Target and Agent positions
        sensor.AddObservation(Target.localPosition);
        sensor.AddObservation(this.transform.localPosition);

        // Agent velocity
        sensor.AddObservation(this.rBody.velocity.x);
        sensor.AddObservation(this.rBody.velocity.z);
    }

    // Public forceMultiplier allows for manipulation from Inspector window
    public float forceMultiplier = 10;
    // Receives actions and assigns rewards
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
    // The agent needs two actions: Determine force in x,z directions
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        rBody.AddForce(controlSignal * forceMultiplier);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);
        //Reached Target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            EndEpisode();
        }

        // Fell off platform
        else if (this.transform.localPosition.y < 0)
        {
            EndEpisode();
        }
    }

    // Provide agent control via keyboard for testing prior to training
    /*
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    */
}
