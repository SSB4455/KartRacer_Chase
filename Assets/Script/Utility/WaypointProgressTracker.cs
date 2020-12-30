using System;
using System.IO;
using System.Text;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
	public class WaypointProgressTracker : MonoBehaviour, ICircuitRacingObserver
	{
		// This script can be used with any object that is supposed to follow a
		// route marked out by waypoints.

		// This script manages the amount to look ahead along the route,
		// and keeps track of progress and laps.

		public WaypointCircuit circuit; // A reference to the waypoint-based route we should follow
		public KartGame.KartSystems.ArcadeKart arcadeKart;
		internal ArcadeKartAgent.BehaviorType behaviorType;
		internal ShadowRecordContent shadowRecordContent;

		// these are public, readable by other objects - i.e. for an AI to know where to head!
		public WaypointCircuit.RoutePoint inWayRoutePoint { get; private set; }
		public WaypointCircuit.RoutePoint lastInWayRoutePoint { get; private set; }

		DateTime startTime;
		DateTime lapStartTime;
		TimeSpan bestLapTime = TimeSpan.Zero;
		TimeSpan matchFinishTime = TimeSpan.Zero;
		internal int totalLapCount = 1;
		int finishLapCount = 0;
		int maxFinishLapCount = 0;
		internal GamePlayingManager.IPlayingManager iPlayingManager;
		public bool record = true;
		bool recording;
		StringBuilder recordingStringBuilder;



		// setup script properties
		private void Start()
		{
			if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "ml-agents_config")))
			{
				Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "ml-agents_config"));
			}

			Reset();

			//Debug.Log(PlayerPrefs.GetInt("LapBestTime_" + circuit.trackName + arcadeKart.name, 0));
			bestLapTime = new TimeSpan(PlayerPrefs.GetInt("LapBestTime_" + circuit.trackName + arcadeKart.name, 0));
		}

		public string GetCircuitName()
		{
			return circuit.trackName;
		}

		// reset the object to sensible values
		public void Reset()
		{
			finishLapCount = 0;
			maxFinishLapCount = 0;
			startTime = DateTime.Now;
			lapStartTime = startTime;
			matchFinishTime = TimeSpan.Zero;
			lastInWayRoutePoint = circuit.GetRoutePointByProgress(0);
			inWayRoutePoint = circuit.GetRoutePoint(arcadeKart.transform.position);

			if (record)
			{
				recordingStringBuilder = new StringBuilder();
				recordingStringBuilder.Append("PlayTime\t").AppendLine(DateTime.Now.Ticks.ToString());
				recordingStringBuilder.Append("TimeScale\t").AppendLine(Time.timeScale.ToString());
				recordingStringBuilder.Append("CircuitName\t").AppendLine(circuit.trackName);
				recordingStringBuilder.Append("CircuitId\t").AppendLine(circuit.Id);
				recordingStringBuilder.Append("CircuitVersion\t").AppendLine("0\t//circuit.Version未完成");
				recordingStringBuilder.Append("CircuitLength\t").AppendLine(circuit.CircuitLength.ToString());
				recordingStringBuilder.Append("PlayerName\t").AppendLine(arcadeKart.playerNameText.text);
				recordingStringBuilder.Append("CarName\t").AppendLine(arcadeKart.name.Split('|')[0]);
				recordingStringBuilder.Append("AgentName\t").AppendLine("ArcadeKartAgent\t//AgentName未完成");
				recordingStringBuilder.Append("ModelName\t").AppendLine("AI_Racer1\t//ModelName未完成");
				recordingStringBuilder.Append("ModelMD5\t").AppendLine("ModelMD5未完成");
				recordingStringBuilder.Append("BehaviorType\t").Append((int)behaviorType).AppendLine("\t//0-Default 1-HeuristicOnly 2-InferenceOnly");
				recordingStringBuilder.Append("TotalLapCount\t").AppendLine(totalLapCount.ToString());
				recordingStringBuilder.Append("MatchId\t").AppendLine(iPlayingManager.GetMatchId());
				recordingStringBuilder.Append("TotalCarCount\t").AppendLine(iPlayingManager.GetRacingCarCount().ToString());
				recordingStringBuilder.Append("CircuitPosition").AppendLine(circuit.trackTransform.position.ToString().Replace('(', '\t').Replace(" ", "").Replace(')', '\n'));
				recording = true;
			}
		}

		public Vector3 GetStartPointPosition()
		{
			return iPlayingManager.GetStartPointPosition(this);
		}

		public Quaternion GetStartPointRotation()
		{
			return iPlayingManager.GetStartPointRotation(this);
		}

		public int GetMaxFinishLapCount()
		{
			return maxFinishLapCount;
		}

		public int GetTotalLapCount()
		{
			return totalLapCount;
		}

		public float GetCircuitLength()
		{
			return circuit.CircuitLength;
		}

		private void Update()
		{
			if (behaviorType == ArcadeKartAgent.BehaviorType.ShadowPlay)
			{
				shadowRecordContent.ShadowRun(GetMatchTime());
			}
			inWayRoutePoint = circuit.GetRoutePoint(arcadeKart.transform.position);

			AddCarStatusRecord(GetMatchTime());
			if (lastInWayRoutePoint.percent > 0.8f && inWayRoutePoint.percent < 0.2f)
			{
				finishLapCount++;
				if (maxFinishLapCount < finishLapCount)
				{
					maxFinishLapCount = finishLapCount;
					if (bestLapTime == TimeSpan.Zero || DateTime.Now - lapStartTime < bestLapTime)
					{
						bestLapTime = DateTime.Now - lapStartTime;
						PlayerPrefs.SetInt("LapBestTime_" + circuit.trackName + arcadeKart.name, (int)bestLapTime.Ticks);
					}
					Debug.Log("LapTime = " + (DateTime.Now - lapStartTime).Ticks);
					lapStartTime = DateTime.Now;
					if (matchFinishTime == TimeSpan.Zero && MatchFinish())
					{
						matchFinishTime = DateTime.Now - startTime;
						iPlayingManager.MatchFinish(this);
						if (record)
						{
							recording = false;
							recordingStringBuilder.Append("FinishCircuitTime\t").Append(GetMatchTime().Ticks).Append('\t').Append(GetMatchTime());
							string recordFilePath = Path.Combine(Application.persistentDataPath, "ml-agents_config",
								"ArcadeKartAgent_playRecord_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
#if UNITY_EDITOR
							recordFilePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "ml-agents_config", 
								"ArcadeKartAgent_playRecord_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
#endif
							File.WriteAllText(recordFilePath, recordingStringBuilder.ToString());
						}
					}
				}
			}
			if (lastInWayRoutePoint.percent < 0.2f && inWayRoutePoint.percent > 0.8f)
			{
				finishLapCount--;
			}
			lastInWayRoutePoint = inWayRoutePoint;
			//Debug.Log("finishLoopCount = " + finishLoopCount + "\t LoopProgress = " + GetLoopProgress());
		}

		public void SetShadowAction(float[] vectorAction)
		{
			if (record && vectorAction?.Length > 0)
			{
				TimeSpan time = GetMatchTime();
				recordingStringBuilder.Append("AgentActions\t").Append(time.Ticks).Append('\t');
				for (int i = 0; i < vectorAction?.Length; i++)
				{
					recordingStringBuilder.Append(vectorAction[i]).Append(',');
				}
				recordingStringBuilder.Remove(recordingStringBuilder.Length - 1, 1).AppendLine();
				AddCarStatusRecord(time);
			}
		}

		void AddCarStatusRecord(TimeSpan time)
		{
			if (recording)
			{
				recordingStringBuilder.Append("CarStatus\t").Append(time.Ticks).
					Append(arcadeKart.transform.position.ToString().Replace('(', '\t').Replace(" ", "").Replace(')', '\t')).
					Append(arcadeKart.transform.rotation.eulerAngles.ToString().Replace("(", "").Replace(" ", "").Replace(')', '\t')).
					Append(GetMatchProgress().ToString("f4")).
					Append(arcadeKart.CarRigidbody.velocity.ToString().Replace('(', '\t').Replace(" ", "").Replace(')', '\n'));
			}
		}

		public float GetForwardSpeed()
		{
			return arcadeKart.ForwardSpeedValue;
		}

		public float GetMatchProgress()
		{
			return (finishLapCount + GetLapProgress()) / totalLapCount;
		}

		public float GetLapProgress()
		{
			return inWayRoutePoint.percent;
		}

		public bool MatchFinish()
		{
			return GetTotalLapCount() <= maxFinishLapCount;
		}

		public Vector3 GetGuideLinePosition(float loopProgress)
		{
			return circuit.GetRoutePointByProgress(loopProgress).position;
		}

		public Vector3 GetGuideLineDirection(float loopProgress)
		{
			return circuit.GetRoutePointByProgress(loopProgress).direction;
		}

		public TimeSpan GetCurrentLapTime()
		{
			return DateTime.Now - lapStartTime;
		}

		public TimeSpan GetBestLapTime()
		{
			return bestLapTime;
		}

		public TimeSpan GetMatchTime()
		{
			//Debug.Log(DateTime.Now + " -> " + startTime);
			return matchFinishTime == TimeSpan.Zero ? DateTime.Now - startTime : matchFinishTime;
		}

		public int GetRank()
		{
			return iPlayingManager.GetRank(this);
		}

		public int GetRacingCarCount()
		{
			return iPlayingManager.GetRacingCarCount();
		}

		public ArcadeKartAgent.BehaviorType GetBehaviorType()
		{
			return behaviorType;
		}


		private void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(inWayRoutePoint.position, 1);
				Gizmos.DrawLine(inWayRoutePoint.position, inWayRoutePoint.position + inWayRoutePoint.direction * 3);
				Gizmos.color = Color.yellow;
				//Gizmos.DrawLine(target.position, target.position + target.forward * 3);
			}
		}
	}
}
