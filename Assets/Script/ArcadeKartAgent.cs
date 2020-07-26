/*
SSBB4455 2020-07-03
*/
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using KartGame.KartSystems;

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
	[Tooltip("How far should the agent shoot raycasts to detect the world?")]
	public float RaycastDistance;
	[Tooltip("What objects should the raycasts hit and detect?")]
	public LayerMask Mask;
	[Tooltip("Sensors contain ray information to sense out the world, you can have as many sensors as you need.")]
	public Sensor[] Sensors;
	/// <summary>
	/// Sensors hold information such as the position of rotation of the origin of the raycast and its hit threshold
	/// to consider a "crash".
	/// </summary>
	[System.Serializable]
	public struct Sensor
	{
		public Transform Transform;
		public float HitThreshold;
	}

	[Header("Checkpoints")]
	[Tooltip("What are the series of checkpoints for the agent to seek and pass through?")]
	public Collider[] Colliders;
	[Tooltip("What layer are the checkpoints on? This should be an exclusive layer for the agent to use.")]
	public LayerMask CheckpointMask;

	[Space]
	[Tooltip("Would the agent need a custom transform to be able to raycast and hit the track? " +
		"If not assigned, then the root transform will be used.")]
	public Transform AgentSensorTransform;
	#endregion

	#region Debugging
	[Header("Debug Option")]
	[Tooltip("Should we visualize the rays that the agent draws?")]
	public bool ShowRaycasts;
	#endregion



	public override void OnEpisodeBegin()
	{
		//卡丁车位置重置
		//arcadeKart = GetComponent<ArcadeKart>();
		if (AgentSensorTransform == null)
		{
			AgentSensorTransform = transform;
		}
	}

	public override void CollectObservations(VectorSensor sensor)
	{
		// Agent velocity
		sensor.AddObservation(arcadeKart.Rigidbody.velocity);



		for (int i = 0; i < Sensors.Length; i++)
		{
			var current = Sensors[i];
			var xform = current.Transform;
			var hit = Physics.Raycast(AgentSensorTransform.position, xform.forward, out var hitInfo,
				RaycastDistance, Mask, QueryTriggerInteraction.Ignore);

			if (ShowRaycasts)
			{
				Debug.DrawRay(AgentSensorTransform.position, xform.forward * RaycastDistance, Color.green);
				Debug.DrawRay(AgentSensorTransform.position, xform.forward * current.HitThreshold, Color.red);
			}

			var hitDistance = (hit ? hitInfo.distance : RaycastDistance) / RaycastDistance;
			sensor.AddObservation(hitDistance);

			if (hitDistance < current.HitThreshold)
			{
				AddReward(hitPenalty);
			}
		}
	}

	public void Update()
	{

		for (int i = 0; i < Sensors.Length; i++)
		{
			var current = Sensors[i];
			var xform = current.Transform;
			var hit = Physics.Raycast(AgentSensorTransform.position, xform.forward, out var hitInfo,
				RaycastDistance, Mask, QueryTriggerInteraction.Ignore);

			if (ShowRaycasts)
			{
				Debug.DrawRay(AgentSensorTransform.position, xform.forward * RaycastDistance, Color.green);
				Debug.DrawRay(AgentSensorTransform.position, xform.forward * current.HitThreshold, Color.red);
			}

		}
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
			AddReward(hitPenalty);
		}

		//检测碰到终点 计算时间给予奖励
	}

	void OnTriggerEnter(Collider other)
	{
		int maskedValue = 1 << other.gameObject.layer;
		int triggered = maskedValue & CheckpointMask;

		
	}

	public Vector2 GenerateInput()
	{
		return agentInput;
	}
}