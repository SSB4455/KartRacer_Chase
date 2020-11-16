using System;
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
		TimeSpan matchtTime = TimeSpan.Zero;
		internal int totalLapCount = 1;
		int finishLapCount = 0;
		int maxFinishLapCount = 0;
		internal GamePlayingManager.IPlayingManager iPlayingManager;



		// setup script properties
		private void Start()
		{
			Reset();
		}

		public string GetCircuitName()
		{
			return circuit.circuitName;
		}

		// reset the object to sensible values
		public void Reset()
		{
			finishLapCount = 0;
			maxFinishLapCount = 0;
			startTime = DateTime.Now;
			lapStartTime = startTime;
			lastInWayRoutePoint = circuit.GetRoutePointByProgress(0);
			inWayRoutePoint = circuit.GetRoutePoint(arcadeKart.transform.position);
		}

		public Vector3 GetStartPointPosition(int rank)
		{
			return circuit.GetRoutePointByProgress(0).position + new Vector3(0, 1, 0);
		}

		public Quaternion GetStartPointRotation(int rank)
		{
			return Quaternion.Euler(circuit.GetRoutePointByProgress(0).direction);
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
			if (lastInWayRoutePoint.percent > 0.8f && inWayRoutePoint.percent < 0.2f)
			{
				finishLapCount++;
				if (maxFinishLapCount < finishLapCount)
				{
					maxFinishLapCount = finishLapCount;
					if (bestLapTime == TimeSpan.Zero || DateTime.Now - lapStartTime < bestLapTime)
					{
						bestLapTime = DateTime.Now - lapStartTime;
					}
					lapStartTime = DateTime.Now;
					if (MatchFinish())
					{
						matchtTime = DateTime.Now - startTime;
						iPlayingManager.MatchFinish(this);
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
			return (finishLapCount < 0 ? -1 : 0) + inWayRoutePoint.percent;
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
			return matchtTime == TimeSpan.Zero ? DateTime.Now - startTime : matchtTime;
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
