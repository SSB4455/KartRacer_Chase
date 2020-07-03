/*
SSBB4455 2020-07-03
*/
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using KartGame.KartSystems;

public class ArcadeKartAgent : Agent
{
	public ArcadeKart arcadeKart;
	public AgentInput agentInput;




	public override void OnEpisodeBegin()
	{
		//卡丁车位置重置
		//arcadeKart
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		// Target and Agent positions
		sensor.AddObservation(arcadeKart.transform.localPosition);
		//sensor.AddObservation(this.transform.localPosition);

		// Agent velocity
		sensor.AddObservation(arcadeKart.Rigidbody.velocity.x);
		sensor.AddObservation(arcadeKart.Rigidbody.velocity.y);
		sensor.AddObservation(arcadeKart.Rigidbody.velocity.z);
	}

	public override void OnActionReceived(float[] vectorAction)
	{
		// Actions, size = 2
		agentInput.x = vectorAction[0];
		agentInput.y = vectorAction[1];

		// Rewards
		//float distanceToTarget = Vector3.Distance(this.transform.localPosition, Target.localPosition);

		// Reached target
		//if (distanceToTarget < 1.42f)
		{
			SetReward(1.0f);
			EndEpisode();
		}

		// Fell off platform
		if (this.transform.localPosition.y < 0)
		{
			EndEpisode();
		}
	}

}