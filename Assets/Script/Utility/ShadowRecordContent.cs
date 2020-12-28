/*
SSBB4455 2020-12-23
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
	public class ShadowRecordContent
	{
		string shadowRecordFilePath;
		private ArcadeKartAgent agent;


		DateTime playTime;
		float timeScale;
		string circuitName;
		string circuitId;
		string circuitVersion;
		float circuitLength;
		string playerName;
		string carName;
		string agentName;
		string modelName;
		string modelMD5;
		int behaviorType;
		int totalLapCount;
		string matchId;
		int totalCarCount;
		Vector3 circuitPosition;
		TimeSpan finishCircuitTime;
		bool shadowRecordAccordance;

		Vector3 trackPositionOffset = Vector3.zero;
		private TimePosition[] timePositions;
		private TimeActions[] timeActionss;
		int[] timePositionsOptimizedIndices = new int[100];
		int[] timeActionssOptimizedIndices = new int[100];



		public ShadowRecordContent(string shadowRecordFilePath, ArcadeKartAgent agent, WaypointCircuit circuit)
		{
			this.shadowRecordFilePath = shadowRecordFilePath;
			this.agent = agent;

			if (!File.Exists(shadowRecordFilePath))
			{
				Debug.LogError("shadowRecordFile not exist " + shadowRecordFilePath);
				return;
			}

			List<TimePosition> timePositionList = new List<TimePosition>();
			List<TimeActions> timeActionsList = new List<TimeActions>();

			string[] lines = File.ReadAllLines(shadowRecordFilePath);
			for (int i = 0; i < lines.Length; i++)
			{
				string[] splits = lines[i].Split('\t');
				if (i < 20 && splits.Length > 1)
				{
					switch (splits[0])
					{
						case "PlayTime": playTime = new DateTime(long.Parse(splits[1])); break;
						case "TimeScale": timeScale = float.Parse(splits[1]); break;
						case "CircuitName": 
							circuitName = splits[1];
							shadowRecordAccordance = circuit.trackName == circuitName;
							break;
						case "CircuitId": circuitId = splits[1]; break;
						case "CircuitVersion": circuitVersion = splits[1]; break;
						case "CircuitLength": circuitLength = float.Parse(splits[1]); break;
						case "PlayerName": playerName = splits[1]; break;
						case "CarName": carName = splits[1]; break;
						case "AgentName": agentName = splits[1]; break;
						case "ModelName": modelName = splits[1]; break;
						case "ModelMD5": modelMD5 = splits[1]; break;
						case "BehaviorType": behaviorType = int.Parse(splits[1]); break;
						case "TotalLapCount": totalLapCount = int.Parse(splits[1]); break;
						case "MatchId": matchId = splits[1]; break;
						case "TotalCarCount": totalCarCount = int.Parse(splits[1]); break;
						case "CircuitPosition":
							string[] dataSplits = splits[1].Split(',');
							circuitPosition = new Vector3(float.Parse(dataSplits[0]), float.Parse(dataSplits[1]), float.Parse(dataSplits[2]));
							break;
					}
				}
				if (i > 10 && splits.Length > 2)
				{
					TimeSpan time;
					string[] dataSplits;
					switch (splits[0])
					{
						case "CarPosition":
							time = new TimeSpan(long.Parse(splits[1]));
							dataSplits = splits[2].Split(',');
							Vector3 position = new Vector3(float.Parse(dataSplits[0]), float.Parse(dataSplits[1]), float.Parse(dataSplits[2]));
							dataSplits = splits[3].Split(',');
							//Quaternion rotation = new Quaternion(float.Parse(dataSplits[0]), float.Parse(dataSplits[1]), float.Parse(dataSplits[2]), float.Parse(dataSplits[3]));
							//float progress = float.Parse(dataSplits[4]);
							//dataSplits = splits[5].Split(',');
							//Vector3 velocity = new Vector3(float.Parse(dataSplits[0]), float.Parse(dataSplits[1]), float.Parse(dataSplits[2]));
							timePositionList.Add(new TimePosition(time, position));
							break;
						case "AgentActions":
							time = new TimeSpan(long.Parse(splits[1]));
							dataSplits = splits[2].Split(',');
							float[] vectorAction = new float[dataSplits.Length];
							for (int j = 0; j < dataSplits.Length; j++)
							{
								vectorAction[j] = float.Parse(dataSplits[j]);
							}
							timeActionsList.Add(new TimeActions(time, vectorAction));
							break;
					}
				}
				timePositions = timePositionList.ToArray();
				timeActionss = timeActionsList.ToArray();
			}
			finishCircuitTime = new TimeSpan(long.Parse(lines[lines.Length - 1].Split('\t')[1]));

			CacheTimePrecent();
		}

		private void CacheTimePrecent()
		{
			for (int i = 1, j = 1; i < timePositionsOptimizedIndices.Length; i++)
			{
				timePositionsOptimizedIndices[i] = 0;
				float samplePercent = (float)i / timePositionsOptimizedIndices.Length;
				timePositionsOptimizedIndices[i] = j;
				for (; j < timePositions.Length; j++)
				{
					if (((float)timePositions[j].time.Ticks / finishCircuitTime.Ticks) > samplePercent)
					{
						break;
					}
					timePositionsOptimizedIndices[i] = j;
				}
			}
			for (int i = 1, j = 1; i < timeActionssOptimizedIndices.Length; i++)
			{
				timeActionssOptimizedIndices[i] = 0;
				float samplePercent = (float)i / timeActionssOptimizedIndices.Length;
				timeActionssOptimizedIndices[i] = j;
				for (; j < timeActionss.Length; j++)
				{
					if (((float)timeActionss[j].time.Ticks / finishCircuitTime.Ticks) > samplePercent)
					{
						break;
					}
					timeActionssOptimizedIndices[i] = j;
				}
			}
		}

		public Vector3 GetShadowCircuitPosition()
		{
			throw new NotImplementedException();
		}

		public Vector3 GetShadowPositionOrgine(TimeSpan time)
		{
			int precentIndex = (int)(time.Ticks * 100f / finishCircuitTime.Ticks);
			precentIndex = Mathf.Max(0, precentIndex);
			precentIndex = Mathf.Min(precentIndex, timePositionsOptimizedIndices.Length - 2);
			int startIndex = timePositionsOptimizedIndices[precentIndex];
			int endIndex = timePositionsOptimizedIndices[precentIndex + 1];
			while (endIndex - startIndex > 1)
			{
				if (time <= timePositions[startIndex].time)
				{
					break;
				}
				else if (timePositions[endIndex].time <= time)
				{
					break;
				}
				int midIndex = (startIndex + endIndex) / 2;
				if (timePositions[midIndex].time < time)
				{
					startIndex = midIndex;
				} else {
					endIndex = midIndex;
				}
			}
			return TimePosition.LerpPositionInTime(timePositions[startIndex], timePositions[endIndex], time);
		}

		public Vector3 GetShadowPosition(TimeSpan time)
		{
			return GetShadowPositionOrgine(time) + trackPositionOffset;
		}

		public float[] GetShadowActions(TimeSpan time)
		{
			int precentIndex = (int)(time.Ticks * 100f / finishCircuitTime.Ticks);
			precentIndex = Mathf.Max(0, precentIndex);
			precentIndex = Mathf.Min(precentIndex, timeActionssOptimizedIndices.Length - 2);
			int startIndex = timeActionssOptimizedIndices[precentIndex];
			int endIndex = timeActionssOptimizedIndices[precentIndex + 1];
			while (endIndex - startIndex > 1)
			{
				if (time <= timeActionss[startIndex].time)
				{
					break;
				}
				else if (timeActionss[endIndex].time <= time)
				{
					break;
				}
				int midIndex = (startIndex + endIndex) / 2;
				if (timeActionss[midIndex].time < time)
				{
					startIndex = midIndex;
				} else {
					endIndex = midIndex;
				}
			}
			return TimeActions.LerpActionsInTime(timeActionss[startIndex], timeActionss[endIndex], time);
		}

		public void ShadowRun(TimeSpan time)
		{
			agent.arcadeKart.transform.position = GetShadowPosition(time);
			if (agent.joystick)
			{
				float[] actions = GetShadowActions(time);
				if (actions.Length >= 2)
				{
					agent.joystick.SystemMove(new Vector3(actions[0], actions[1], 0));
				}
			}
		}








		public struct TimePosition
		{
			public TimeSpan time { get; private set; }
			public Vector3 position { get; private set; }
			//public Quaternion rotation { get; private set; }



			public TimePosition(TimeSpan time, Vector3 position)
			{
				this.time = time;
				this.position = position;
				//this.rotation = rotation;
			}

			public static Vector3 LerpPosition(TimePosition a, TimePosition b, float lerp)
			{
				return Vector3.Lerp(a.position, b.position, lerp);
			}

			public static Vector3 LerpPositionInTime(TimePosition a, TimePosition b, TimeSpan time)
			{
				if (b.time <= a.time)
				{
					return b.position;
				}
				return LerpPosition(a, b, (long)(time - a.time).Ticks / (b.time - a.time).Ticks);
			}
		}


		public struct TimeActions
		{
			public TimeSpan time { get; private set; }
			public float[] actions { get; private set; }



			public TimeActions(TimeSpan time, float[] actions)
			{
				this.time = time;
				this.actions = actions;
			}

			public static float[] LerpActions(TimeActions a, TimeActions b, float lerp)
			{
				int minLength = Mathf.Min(a.actions.Length, b.actions.Length);
				float[] lerpActions = new float[minLength];
				for (int i = 0; i < lerpActions.Length; i++)
				{
					lerpActions[i] = Mathf.Lerp(a.actions[i], b.actions[i], lerp);
				}
				return lerpActions;
			}

			public static float[] LerpActionsInTime(TimeActions a, TimeActions b, TimeSpan time)
			{
				if (b.time <= a.time)
				{
					return b.actions;
				}
				return LerpActions(a, b, (long)(time - a.time).Ticks / (b.time - a.time).Ticks);
			}
		}
	}
}
