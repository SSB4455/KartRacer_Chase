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

		// these are public, readable by other objects - i.e. for an AI to know where to head!
		public WaypointCircuit.RoutePoint inWayRoutePoint { get; private set; }
		public WaypointCircuit.RoutePoint lastInWayRoutePoint { get; private set; }

		DateTime startTime;
		DateTime loopStartTime;
		TimeSpan matchtTime = TimeSpan.Zero;
		int loopCount = 1;
		int finishLoopCount = 0;
		int maxFinishLoopCount = 0;

		private float progressDistance; // The progress round the route, used in smooth mode.
		private int progressNum; // the current waypoint number, used in point-to-point mode.
		private Vector3 lastPosition; // Used to calculate current speed (since we may not have a rigidbody component)
		private float speed; // current speed of this object (calculated from delta since last frame)



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
			finishLoopCount = 0;
			startTime = DateTime.Now;
			loopStartTime = startTime;
			lastInWayRoutePoint = circuit.GetRoutePointByProgress(0);
			inWayRoutePoint = circuit.GetRoutePoint(transform.position);
		}

		public Vector3 GetStartPointPosition(int rank)
		{
			return circuit.GetRoutePointByProgress(0).position + new Vector3(0, 1, 0);
		}

		public Quaternion GetStartPointRotation(int rank)
		{
			return Quaternion.Euler(circuit.GetRoutePointByProgress(0).direction);
		}

		public int GetMaxFinishLoopCount()
		{
			return maxFinishLoopCount;
		}

		public int GetTotalLoopCount()
		{
			return loopCount;
		}

		public float GetCircuitLength()
		{
			return circuit.CircuitLength;
		}

		private void Update()
		{
			inWayRoutePoint = circuit.GetRoutePoint(transform.position);
			if (lastInWayRoutePoint.percent > 0.8f && inWayRoutePoint.percent < 0.2f)
			{
				finishLoopCount++;
				if (maxFinishLoopCount < finishLoopCount)
				{
					maxFinishLoopCount = finishLoopCount;
					loopStartTime = DateTime.Now;
					if (MatchFinish())
					{
						matchtTime = DateTime.Now - startTime;
					}
				}
			}
			if (lastInWayRoutePoint.percent < 0.2f && inWayRoutePoint.percent > 0.8f)
			{
				finishLoopCount--;
			}
			lastInWayRoutePoint = inWayRoutePoint;
			//Debug.Log("finishLoopCount = " + finishLoopCount + "\t LoopProgress = " + GetLoopProgress());
		}

		public float GetMatchProgress()
		{
			return finishLoopCount + GetLoopProgress();
		}

		public float GetLoopProgress()
		{
			return (finishLoopCount < 0 ? -1 : 0) + inWayRoutePoint.percent;
		}

		public bool MatchFinish()
		{
			return GetTotalLoopCount() <= finishLoopCount;
		}

		public Vector3 GetGuideLinePosition(float loopProgress)
		{
			return circuit.GetRoutePointByProgress(loopProgress).position;
		}

		public Vector3 GetGuideLineDirection(float loopProgress)
		{
			return circuit.GetRoutePointByProgress(loopProgress).direction;
		}

		public TimeSpan GetLoopTime()
		{
			return DateTime.Now - loopStartTime;
		}

		public TimeSpan GetMatchTime()
		{
			//Debug.Log(DateTime.Now + " -> " + startTime);
			return matchtTime == TimeSpan.Zero ? matchtTime : DateTime.Now - startTime;
		}

		public int GetRank()
		{
			return 0;
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
