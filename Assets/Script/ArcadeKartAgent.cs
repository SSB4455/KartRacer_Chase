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
	internal bl_Joystick joystick;
	[Tooltip("Which layers the wheels will detect.")]
	public LayerMask GroundLayers = Physics.DefaultRaycastLayers;

	GameObject lastGroundCollided = null;
	public int targetRank = 1;
	float targetRankNormalized = 0;

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



	private void Start()
	{
	}

	public override void OnEpisodeBegin()
	{
		racingObserver = GetComponent<ICircuitRacingObserver>();
		racingObserver.Reset();
		targetRankNormalized = (targetRank - 1) / (float)racingObserver.GetRacingCarCount();

		transform.position = racingObserver.GetStartPointPosition();
		Debug.Log(name + " position " + transform.position);
		transform.rotation = racingObserver.GetStartPointRotation();

		arcadeKart.CarRigidbody.velocity = Vector3.zero;
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
		//Car type
		sensor.AddObservation(0.1f);

		//Car forward
		sensor.AddObservation(arcadeKart.transform.forward);

		//Car velocity
		sensor.AddObservation(arcadeKart.CarRigidbody.velocity);
		sensor.AddObservation(arcadeKart.ForwardSpeedValue);

		//Sensors
		for (int i = 0; i < raycastSensors?.Length; i++)
		{
			sensor.AddObservation(raycastSensors[i].GetObservationV1());
		}

		//当前圈完成进度
		sensor.AddObservation(racingObserver.GetLapProgress());
		//单圈占比赛总圈数比例
		sensor.AddObservation(1f / racingObserver.GetTotalLapCount());

		//比赛完成度 前一辆车比赛完成度 后一辆车比赛完成度
		sensor.AddObservation(racingObserver.GetMatchProgress());
		sensor.AddObservation(racingObserver.GetMatchProgress());
		sensor.AddObservation(racingObserver.GetMatchProgress());

		//当前排名 目标排名
		sensor.AddObservation((racingObserver.GetRank() - 1) / (float)racingObserver.GetRacingCarCount());
		sensor.AddObservation(targetRankNormalized);

		//Road forward (current 10m 20m)
		currentLoopProgress = racingObserver.GetLapProgress();
		sensor.AddObservation(GetGuideLineDirectionObservation(currentLoopProgress, 0));
		sensor.AddObservation(GetGuideLineDirectionObservation(currentLoopProgress, 0));//10));
		sensor.AddObservation(GetGuideLineDirectionObservation(currentLoopProgress, 0));//20));
	}

	public override void OnActionReceived(float[] vectorAction)
	{
		// Actions, size = 2
		agentInput.x = vectorAction[0];
		agentInput.y = vectorAction[1];

		float carSpeed = arcadeKart.ForwardSpeedValue;
		if (carSpeed > 0)
		{
			carSpeed /= 10f / arcadeKart.baseStats.TopSpeed;
		} else {
			carSpeed /= 5f / arcadeKart.baseStats.ReverseSpeed;
		}
		AddReward(carSpeed);

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
			SetReward((racingObserver.GetTotalLapCount() * racingObserver.GetCircuitLength() / arcadeKart.baseStats.TopSpeed) / ((float)playingTime.TotalSeconds * Time.timeScale));
			EndEpisode();
		}

		// 倒着开的惩罚
		if (racingObserver.GetMatchProgress() < 0)
		{
			Debug.Log("倒着开过了起点");
			SetReward(-1);
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

		if (Application.isMobilePlatform && joystick)
		{
			actionsOut[0] = joystick.Horizontal / 5;
			actionsOut[1] = joystick.Vertical / 5;
		}
		//Debug.Log("actionsOut " + actionsOut[0] + " " + actionsOut[1]);
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

	/// <summary>
	/// 获取包含偏移进度的道路方向的观测变量
	/// </summary>
	/// <param name="roadProgress">当前圈行驶进度</param>
	/// <param name="offset">前后偏移的距离</param>
	/// <returns></returns>
	List<float> GetGuideLineDirectionObservation(float roadProgress, float offset)
	{
		Vector3 direction = racingObserver.GetGuideLineDirection(roadProgress + (offset / racingObserver.GetCircuitLength()));
		return new List<float> { offset, direction.x, direction.y, direction.z };
	}


	//ArcadeKartGuideLinePropertie : MonoBehaviour
	//public int forwardDistance;
	//public bool ShowRaycasts;
}