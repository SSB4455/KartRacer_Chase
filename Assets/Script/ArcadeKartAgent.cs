/*
SSBB4455 2020-07-03
*/
using KartGame.KartSystems;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class ArcadeKartAgent : Agent, IInput
{
	public ArcadeKart arcadeKart;
	Vector2 agentInput = new Vector2();
	[Tooltip("Which layers the wheels will detect.")]
	public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

	GameObject lastGroundCollided = null;

	public float hitPenalty = -0.1f;

#region Senses
	[Header("Observation Params")]
	public ArcadeKartRaycastSensor[] raycastSensors;
#endregion

	#region Debugging
	[Header("Debug Option")]
	[Tooltip("Should we visualize the rays that the agent draws?")]
	public bool ShowRaycasts;
	#endregion



	public override void OnEpisodeBegin()
	{
		
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		// Car type
		sensor.AddObservation(0.1f);

		// Agent velocity
		sensor.AddObservation(arcadeKart.Rigidbody.velocity);

		AddReward(arcadeKart.LocalSpeed() / 10f);

		// Car forward
		sensor.AddObservation(arcadeKart.transform.forward);

		// Sensors
		for (int i = 0; i < raycastSensors?.Length; i++)
		{
			sensor.AddObservation(raycastSensors[i].GetObservationV1());
		}

		//比赛完成度 前一辆车比赛完成度 后一辆车比赛完成度

		//当前排名 目标排名

		// Road forward (current 10m 20m)
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
			//SetReward(1.0f);
			//EndEpisode();
		}

		// Fell off platform
		if (this.transform.localPosition.y < 0)
		{
			EndEpisode();
		}
	}

	void OnCollisionEnter(Collision other)
	{
		//检测碰到墙壁 
		if (GroundLayers == (GroundLayers | (1 << other.collider.gameObject.layer)))
		{
			lastGroundCollided = other.collider.gameObject;
			Debug.Log(other.gameObject.name);
			//AddReward(hitPenalty);
		}

		//检测碰到终点 计算时间给予奖励
	}

	void OnTriggerEnter(Collider other)
	{
		//int maskedValue = 1 << other.gameObject.layer;
		//int triggered = maskedValue & CheckpointMask;

		
	}

	public Vector2 GenerateInput()
	{
		return agentInput;
	}
}