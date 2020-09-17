using System;
using UnityEngine;

#pragma warning disable 649
namespace UnityStandardAssets.Utility
{
	public class WaypointProgressTracker : MonoBehaviour, ICircuitRacingObserver
	{
		// This script can be used with any object that is supposed to follow a
		// route marked out by waypoints.

		// This script manages the amount to look ahead along the route,
		// and keeps track of progress and laps.

		[SerializeField] private WaypointCircuit circuit; // A reference to the waypoint-based route we should follow

		[SerializeField] private float lookAheadForTargetOffset = 5;
		// The offset ahead along the route that the we will aim for

		[SerializeField] private float lookAheadForTargetFactor = .1f;
		// A multiplier adding distance ahead along the route to aim for, based on current speed

		[SerializeField] private float lookAheadForSpeedOffset = 10;
		// The offset ahead only the route for speed adjustments (applied as the rotation of the waypoint target transform)

		[SerializeField] private float lookAheadForSpeedFactor = .2f;
		// A multiplier adding distance ahead along the route for speed adjustments

		[SerializeField] private ProgressStyle progressStyle = ProgressStyle.SmoothAlongRoute;
		// whether to update the position smoothly along the route (good for curved paths) or just when we reach each waypoint.

		[SerializeField] private float pointToPointThreshold = 4;
		// proximity to waypoint which must be reached to switch target to next waypoint : only used in PointToPoint mode.

		public enum ProgressStyle
		{
			SmoothAlongRoute,
			PointToPoint,
		}

		// these are public, readable by other objects - i.e. for an AI to know where to head!
		public WaypointCircuit.RoutePoint targetPoint { get; private set; }
		public WaypointCircuit.RoutePoint speedPoint { get; private set; }
		public WaypointCircuit.RoutePoint inWayRoutePoint { get; private set; }
		public WaypointCircuit.RoutePoint lastInWayRoutePoint { get; private set; }

		Transform target;
		DateTime startTime;
		int loopCount = 1;
		int finishLoopCount = 0;

		private float progressDistance; // The progress round the route, used in smooth mode.
		private int progressNum; // the current waypoint number, used in point-to-point mode.
		private Vector3 lastPosition; // Used to calculate current speed (since we may not have a rigidbody component)
		private float speed; // current speed of this object (calculated from delta since last frame)



		// setup script properties
		private void Start()
		{
			// we use a transform to represent the point to aim for, and the point which
			// is considered for upcoming changes-of-speed. This allows this component
			// to communicate this information to the AI without requiring further dependencies.

			// You can manually create a transform and assign it to this component *and* the AI,
			// then this component will update it, and the AI can read it.
			if (target == null)
			{
				target = new GameObject(name + " Waypoint Target").transform;
			}
		}

		// reset the object to sensible values
		public void Reset()
		{
			if (progressStyle == ProgressStyle.PointToPoint)
			{
				target.position = GetStartPointPosition();
				target.rotation = GetStartPointRotation();
			}
			finishLoopCount = 0;
			startTime = DateTime.Now;
		}

		public Vector3 GetStartPointPosition()
		{
			return circuit.GetRoutePointByProgress(0).position + new Vector3(0, 1, 0);
		}

		public Quaternion GetStartPointRotation()
		{
			return Quaternion.Euler(circuit.GetRoutePointByProgress(0).direction);
		}

		private void Update()
		{
			inWayRoutePoint = circuit.GetRoutePoint(transform.position);
			if (lastInWayRoutePoint.percent > 0.8f && inWayRoutePoint.percent < 0.2f)
			{
				finishLoopCount++;
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
			return GetLoopCount() <= finishLoopCount;
		}

		public int GetLoopCount()
		{
			return loopCount;
		}

		public float GetCircuitLength()
		{
			return circuit.CircuitLength;
		}

		public Vector3 GetCircuitWayDirection(float circuitProgress)
		{
			return circuit.GetRoutePointByProgress(circuitProgress).direction;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns>返回从开始到现在的时间(毫秒)</returns>
		public long GetCircuitTime()
		{
			//Debug.Log(DateTime.Now + " -> " + startTime);
			return ((DateTime.Now - startTime).Ticks / 10000);
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
