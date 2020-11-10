/*
SSBB4455 2020-10-19
*/
using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;
using UnityStandardAssets.Utility;
using System.IO;
using Unity.MLAgents.Policies;

public class GamePlayingManager : MonoBehaviour
{
	int gamePlayerCount;
	public WaypointCircuit[] trackPrefabs;
	public GameObject arcadeKartPrefab;

	List<WaypointProgressTracker> catList = new List<WaypointProgressTracker>();



	private void Start()
	{
		gamePlayerCount = PlayerPrefs.GetInt("", 1);
		WaypointCircuit circuit = Instantiate<WaypointCircuit>(trackPrefabs[0]);

		for (int i = 0; i < gamePlayerCount; i++)
		{
			GameObject arcadeKart = Instantiate(arcadeKartPrefab);
			WaypointProgressTracker waypointProgressTracker = arcadeKart.GetComponent<WaypointProgressTracker>();
			ArcadeKartAgent agent = arcadeKart.GetComponent<ArcadeKartAgent>();
			if (waypointProgressTracker && agent)
			{
				waypointProgressTracker.circuit = circuit;
				catList.Add(waypointProgressTracker);
				BehaviorParameters behaviorParameters = agent.GetComponent<BehaviorParameters>();
				if (behaviorParameters)
				{
					behaviorParameters.BehaviorType = BehaviorType.InferenceOnly;
				}
			} else {
				Debug.LogError("ArcadeKart " + i + " Instantiate Fail.");
			}
		}
	}

	private void Update()
	{
		for(int i = 0; i < catList?.Count; i++)
		{
			
		}
	}

	//每辆车完成后
	public void SceneFinish()
	{
		
	}
}