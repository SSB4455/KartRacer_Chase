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
		StringBuilder playRecordString;



		// setup script properties
		private void Start()
		{
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
				playRecordString = new StringBuilder();
				playRecordString.Append("PlayTime\t").AppendLine(DateTime.Now.Ticks.ToString());
				playRecordString.Append("TimeScale\t").AppendLine(Time.timeScale.ToString());
				playRecordString.Append("CircuitName\t").AppendLine(circuit.trackName);
				playRecordString.Append("CircuitId\t").AppendLine(circuit.Id);
				//playRecordString.Append("CircuitVersion\t").AppendLine(circuit.Version);
				playRecordString.Append("CircuitLength\t").AppendLine(circuit.CircuitLength.ToString());
				playRecordString.Append("PlayerName\t").AppendLine(arcadeKart.name);
				playRecordString.Append("CarName\t").AppendLine(arcadeKart.name);
				//playRecordString.Append("AgentName\t").AppendLine(AgentName);
				//playRecordString.Append("AgentValueMD5\t").AppendLine(AgentValueMD5);
				//playRecordString.Append("BehaviorType\t").AppendLine(BehaviorType);
				playRecordString.Append("TotalLapCount\t").AppendLine(totalLapCount.ToString());
				//playRecordString.Append("MatchId\t").AppendLine(iPlayingManager.GetMatchId());
				playRecordString.Append("TotalCarCount\t").AppendLine(iPlayingManager.GetRacingCarCount().ToString());
				playRecordString.Append("CircuitPosition").Append(circuit.trackTransform.position.ToString().Replace('(', '\t').Replace(')', '\n'));
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
			inWayRoutePoint = circuit.GetRoutePoint(arcadeKart.transform.position);
			if (record && recording)
			{
				playRecordString.Append("CarPosition\t").Append(GetMatchTime().Ticks).
					Append(arcadeKart.transform.position.ToString().Replace('(', '\t').Replace(" ", "").Replace(')', '\t')).
					Append(GetMatchProgress().ToString("f4")).
					Append(arcadeKart.CarRigidbody.velocity.ToString().Replace('(', '\t').Replace(" ", "").Replace(')', '\n'));
			}
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
						if (recording)
						{
							recording = false;
							playRecordString.Append("FinishCircuitTime\t").Append(GetMatchTime().Ticks).Append('\t').Append(GetMatchTime());
							string recordFilePath = Path.Combine(Application.persistentDataPath, "ml-agents_config",
								"ArcadeKartAgent_playRecord_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
#if UNITY_EDITOR
							recordFilePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "ml-agents_config", 
								"ArcadeKartAgent_playRecord_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt");
#endif
							File.WriteAllText(recordFilePath, playRecordString.ToString());
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
