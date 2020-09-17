/*
SSBB4455 2020-07-03
*/
using KartGame.KartSystems;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;
using System;
using System.IO;

public class ArcadeKartAgent : Agent, IInput
{
	public ArcadeKart arcadeKart;
	ICircuitRacingObserver racingObserver;
	Vector2 agentInput = new Vector2();
	[Tooltip("Which layers the wheels will detect.")]
	public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

	GameObject lastGroundCollided = null;
	public int targetRank;

	public float hitPenalty = -0.1f;

#region Senses
	[Header("Observation Params")]
	public ArcadeKartRaycastSensor[] raycastSensors;
#endregion

#region Debugging
	[Header("Debug Option")]
	[Tooltip("Should we visualize the rays that the agent draws?")]
	public bool ShowRaycasts;

	string trainingLog;
#endregion



	public override void OnEpisodeBegin()
	{
		racingObserver = GetComponent<ICircuitRacingObserver>();
		racingObserver.Reset();

		transform.position = racingObserver.GetStartPointPosition();
		transform.rotation = racingObserver.GetStartPointRotation();

		arcadeKart.Rigidbody.velocity = Vector3.zero;
		trainingLog = "";
	}

	public new void AddReward(float increment)
	{
		trainingLog += "AddReward\t" + DateTime.Now + "\t" + increment + "\n";
		base.AddReward(increment);
	}

	public new void EndEpisode()
	{
		File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), 
			"ml-agents_config", "ArcadeKartAgent_training_log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt"), trainingLog);
		base.EndEpisode();
	}

	float currentLoopProgress;
	public override void CollectObservations(VectorSensor sensor)
	{
		// Car type
		sensor.AddObservation(0.1f);

		// Agent velocity
		sensor.AddObservation(arcadeKart.Rigidbody.velocity);

		// Car forward
		sensor.AddObservation(arcadeKart.transform.forward);

		// Sensors
		for (int i = 0; i < raycastSensors?.Length; i++)
		{
			sensor.AddObservation(raycastSensors[i].GetObservationV1());
		}

		//比赛完成度 前一辆车比赛完成度 后一辆车比赛完成度
		sensor.AddObservation(racingObserver.GetMatchProgress());
		sensor.AddObservation(racingObserver.GetMatchProgress());
		sensor.AddObservation(racingObserver.GetMatchProgress());

		//当前排名 目标排名
		sensor.AddObservation(racingObserver.GetRank());
		sensor.AddObservation(targetRank);

		// Road forward (current 10m 20m)
		currentLoopProgress = racingObserver.GetLoopProgress();
		sensor.AddObservation(WayDirectionObservation(currentLoopProgress, 0));
		sensor.AddObservation(WayDirectionObservation(currentLoopProgress, 10));
		sensor.AddObservation(WayDirectionObservation(currentLoopProgress, 20));
	}

	public override void OnActionReceived(float[] vectorAction)
	{
		// Actions, size = 2
		agentInput.x = vectorAction[0];
		agentInput.y = vectorAction[1];

		// Reward Speed
		Vector3 carSpeed = arcadeKart.Speed;
		Vector3 wayDirection = racingObserver.GetCircuitWayDirection(currentLoopProgress);
		int speedFollow = Vector3.Dot(carSpeed, wayDirection) < 0 ? -10 : 1;
		AddReward(speedFollow * carSpeed.magnitude / 100f);
		

		// Reached target
		//if (distanceToTarget < 1.42f)
		{
			//SetReward(1.0f);
			//EndEpisode();
		}

		// Fell off platform
		if (this.transform.localPosition.y < 0)
		{
			Debug.LogWarning("Fell off platform");
			AddReward(-1);
			EndEpisode();
		}

		// Match finish reward
		if (racingObserver.MatchFinish())
		{
			long matchTime = racingObserver.GetCircuitTime();
			Debug.Log("finish CircuitTime = " + matchTime);
			AddReward(1000000f / matchTime);
			trainingLog += "finish CircuitTime = \t" + matchTime / 1000f + "s\n";
			EndEpisode();
		}
	}

	public override void Heuristic(float[] actionsOut)
	{

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

	List<float> WayDirectionObservation(float roadProgress, float offset)
	{
		Vector3 direction = racingObserver.GetCircuitWayDirection(roadProgress + (offset / racingObserver.GetCircuitLength()));
		return new List<float> { offset, direction.x, direction.y, direction.z };
	}

	public Vector2 GenerateInput()
	{
		return agentInput;
	}
}