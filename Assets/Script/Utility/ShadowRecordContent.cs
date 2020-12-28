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
		Vector3 shadowCircuitPosition;
		TimeSpan finishCircuitTime;
		bool shadowRecordAccordance;

		Vector3 trackPositionOffset = Vector3.zero;
		private TimeCarStatus[] timeStatuss;
		int[] timeStatussOptimizedIndices = new int[100];



		public ShadowRecordContent(string shadowRecordFilePath, ArcadeKartAgent agent, WaypointCircuit circuit)
		{
			this.shadowRecordFilePath = shadowRecordFilePath;
			this.agent = agent;

			if (!File.Exists(shadowRecordFilePath))
			{
				Debug.LogError("shadowRecordFile not exist " + shadowRecordFilePath);
				return;
			}

			List<TimeCarStatus> timePositionList = new List<TimeCarStatus>();
			float[] vectorAction = new float[2];
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
							shadowCircuitPosition = new Vector3(float.Parse(dataSplits[0]), float.Parse(dataSplits[1]), float.Parse(dataSplits[2]));
							break;
					}
				}
				if (i > 10 && splits.Length > 2)
				{
					TimeSpan time;
					string[] dataSplits;
					switch (splits[0])
					{
						case "CarStatus":
							time = new TimeSpan(long.Parse(splits[1]));
							dataSplits = splits[2].Split(',');
							Vector3 position = new Vector3(float.Parse(dataSplits[0]), float.Parse(dataSplits[1]), float.Parse(dataSplits[2]));
							dataSplits = splits[3].Split(',');
							Quaternion rotation = new Quaternion(float.Parse(dataSplits[0]), float.Parse(dataSplits[1]), float.Parse(dataSplits[2]), float.Parse(dataSplits[3]));
							//float progress = float.Parse(dataSplits[4]);
							//dataSplits = splits[5].Split(',');
							//Vector3 velocity = new Vector3(float.Parse(dataSplits[0]), float.Parse(dataSplits[1]), float.Parse(dataSplits[2]));
							timePositionList.Add(new TimeCarStatus(time, position, rotation, vectorAction));
							break;
						case "AgentActions":
							time = new TimeSpan(long.Parse(splits[1]));
							dataSplits = splits[2].Split(',');
							vectorAction = new float[dataSplits.Length];
							for (int j = 0; j < dataSplits.Length; j++)
							{
								vectorAction[j] = float.Parse(dataSplits[j]);
							}
							break;
					}
				}
				timeStatuss = timePositionList.ToArray();
			}
			finishCircuitTime = new TimeSpan(long.Parse(lines[lines.Length - 1].Split('\t')[1]));

			CacheTimeStatusPrecent();
		}

		private void CacheTimeStatusPrecent()
		{
			for (int i = 1, j = 1; i < timeStatussOptimizedIndices.Length; i++)
			{
				timeStatussOptimizedIndices[i] = 0;
				float samplePercent = (float)i / timeStatussOptimizedIndices.Length;
				timeStatussOptimizedIndices[i] = j;
				for (; j < timeStatuss.Length; j++)
				{
					if (((float)timeStatuss[j].time.Ticks / finishCircuitTime.Ticks) > samplePercent)
					{
						break;
					}
					timeStatussOptimizedIndices[i] = j;
				}
			}
		}

		public Vector3 GetShadowCircuitPosition()
		{
			return shadowCircuitPosition;
		}

		public TimeCarStatus GetShadowStatusOrgine(TimeSpan time)
		{
			int precentIndex = (int)(time.Ticks * 100f / finishCircuitTime.Ticks);
			precentIndex = Mathf.Max(0, precentIndex);
			precentIndex = Mathf.Min(precentIndex, timeStatussOptimizedIndices.Length - 2);
			int startIndex = timeStatussOptimizedIndices[precentIndex];
			int endIndex = timeStatussOptimizedIndices[precentIndex + 1];
			while (endIndex - startIndex > 1)
			{
				if (time <= timeStatuss[startIndex].time)
				{
					break;
				}
				else if (timeStatuss[endIndex].time <= time)
				{
					break;
				}
				int midIndex = (startIndex + endIndex) / 2;
				if (timeStatuss[midIndex].time < time)
				{
					startIndex = midIndex;
				} else {
					endIndex = midIndex;
				}
			}
			return TimeCarStatus.LerpInTime(timeStatuss[startIndex], timeStatuss[endIndex], time);
		}

		public void ShadowRun(TimeSpan time)
		{
			TimeCarStatus timeStatus = GetShadowStatusOrgine(time);
			agent.arcadeKart.transform.position = timeStatus.position + trackPositionOffset;
			agent.arcadeKart.transform.rotation = timeStatus.rotation;
			if (agent.joystick)
			{
				float[] actions = timeStatus.actions;
				if (actions.Length >= 2)
				{
					agent.joystick.SystemMove(new Vector3(actions[0], actions[1], 0));
				}
			}
		}








		public struct TimeCarStatus
		{
			public TimeSpan time { get; private set; }
			public Vector3 position { get; private set; }
			public Quaternion rotation { get; private set; }
			public float[] actions { get; private set; }



			public TimeCarStatus(TimeSpan time, Vector3 position, Quaternion rotation, float[] actions)
			{
				this.time = time;
				this.position = position;
				this.rotation = rotation;
				this.actions = actions;
			}

			public static TimeCarStatus LerpInTime(TimeCarStatus a, TimeCarStatus b, TimeSpan time)
			{
				if (b.time <= a.time)
				{
					return b;
				}
				return new TimeCarStatus(time, LerpPositionInTime(a, b, time), LerpRotationInTime(a, b, time), LerpActionsInTime(a, b, time));
			}

			public static Vector3 LerpPosition(TimeCarStatus a, TimeCarStatus b, float lerp)
			{
				return Vector3.Lerp(a.position, b.position, lerp);
			}

			public static Vector3 LerpPositionInTime(TimeCarStatus a, TimeCarStatus b, TimeSpan time)
			{
				if (b.time <= a.time)
				{
					return b.position;
				}
				return LerpPosition(a, b, (long)(time - a.time).Ticks / (b.time - a.time).Ticks);
			}

			public static Quaternion LerpRotation(TimeCarStatus a, TimeCarStatus b, float lerp)
			{
				return Quaternion.Lerp(a.rotation, b.rotation, lerp);
			}

			public static Quaternion LerpRotationInTime(TimeCarStatus a, TimeCarStatus b, TimeSpan time)
			{
				if (b.time <= a.time)
				{
					return b.rotation;
				}
				return LerpRotation(a, b, (long)(time - a.time).Ticks / (b.time - a.time).Ticks);
			}

			public static float[] LerpActions(TimeCarStatus a, TimeCarStatus b, float lerp)
			{
				int minLength = Mathf.Min(a.actions.Length, b.actions.Length);
				float[] lerpActions = new float[minLength];
				for (int i = 0; i < lerpActions.Length; i++)
				{
					lerpActions[i] = Mathf.Lerp(a.actions[i], b.actions[i], lerp);
				}
				return lerpActions;
			}

			public static float[] LerpActionsInTime(TimeCarStatus a, TimeCarStatus b, TimeSpan time)
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
