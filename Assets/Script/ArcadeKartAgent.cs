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

		arcadeKart.CarRigidbody.velocity = Vector3.zero;
		trainingLog = "Circuit = " + racingObserver.GetCircuitName() + "\tlength = " + racingObserver.GetCircuitLength() + "\n";
	}

	/*public new void AddReward(float increment)
	{
		trainingLog += "AddReward\t" + DateTime.Now.ToString("HH:mm:ss.fff") + "\t" + increment + "\n";
		base.AddReward(increment);
	}

	public new void EndEpisode()
	{
		File.WriteAllText(Path.Combine(Directory.GetParent(Application.dataPath).ToString(), 
			"ml-agents_config", "ArcadeKartAgent_training_log_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt"), trainingLog);
		base.EndEpisode();
	}*/

	float currentLoopProgress;
	public override void CollectObservations(VectorSensor sensor)
	{
		// Car type
		sensor.AddObservation(0.1f);

		// Agent velocity
		sensor.AddObservation(arcadeKart.CarRigidbody.velocity);

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
		sensor.AddObservation(GetGuideLineDirectionObservation(currentLoopProgress, 0));
		sensor.AddObservation(GetGuideLineDirectionObservation(currentLoopProgress, 10));
		sensor.AddObservation(GetGuideLineDirectionObservation(currentLoopProgress, 20));
	}

	public override void OnActionReceived(float[] vectorAction)
	{
		// Actions, size = 2
		agentInput.x = vectorAction[0];
		agentInput.y = vectorAction[1];

		AddReward(arcadeKart.SpeedForwardValue / 10f / arcadeKart.baseStats.TopSpeed);

		// Fell off platform
		if (this.transform.localPosition.y < 0)
		{
			Debug.LogWarning("Fell off platform");
			SetReward(-1);
			EndEpisode();
		}

		// Match finish reward
		if (racingObserver.MatchFinish())
		{
			TimeSpan playingTime = racingObserver.GetMatchTime();
			Debug.Log("finish playingTime = " + playingTime + "\tplayingTime = " + (playingTime.TotalSeconds * Time.timeScale) + "(*Time.timeScale)");
			SetReward((racingObserver.GetTotalLoopCount() * racingObserver.GetCircuitLength() / arcadeKart.baseStats.TopSpeed) / ((float)playingTime.TotalSeconds * Time.timeScale));
			trainingLog += "finish CircuitTime = \t" + playingTime + "\tplayingTime = " + (playingTime.TotalSeconds * Time.timeScale) + "(*Time.timeScale)\n";
			EndEpisode();
		}
	}

	public Vector2 GenerateInput()
	{
		return agentInput;
	}

	public override void Heuristic(float[] actionsOut)
	{
		actionsOut[0] = Input.GetAxis("Horizontal");
		actionsOut[1] = Input.GetAxis("Vertical");
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

	List<float> GetGuideLineDirectionObservation(float roadProgress, float offset)
	{
		Vector3 direction = racingObserver.GetGuideLineDirection(roadProgress + (offset / racingObserver.GetCircuitLength()));
		return new List<float> { offset, direction.x, direction.y, direction.z };
	}


	//ArcadeKartGuideLinePropertie : MonoBehaviour
	//public int forwardDistance;
	//public bool ShowRaycasts;
}