/*
SSBB4455 2020-10-19
*/
using UnityEngine;
using Unity.MLAgents;
using System.Collections.Generic;
using UnityStandardAssets.Utility;
using System.IO;
using Unity.MLAgents.Policies;
using System;

public class GamePlayingManager : MonoBehaviour
{
	int gamePlayerCount;
	public Camera miniMapCamera;
	public WaypointCircuit[] trackPrefabs;
	public GameObject arcadeKartPrefab;

	List<WaypointProgressTracker> catList = new List<WaypointProgressTracker>();



	private void Start()
	{
		gamePlayerCount = PlayerPrefs.GetInt("", 1);

		string circuitName = "TraningTrack1";
		WaypointCircuit circuit = null;
		WaypointCircuit[] waypointCircuits = FindObjectsOfType<WaypointCircuit>();
		for (int i = 0; i < waypointCircuits?.Length; i++)
		{
			if (waypointCircuits[i].circuitName == circuitName)
			{
				circuit = waypointCircuits[i];
			} else {
				Destroy(waypointCircuits[i].gameObject);
				waypointCircuits[i] = null;
			}
		}
		if (!circuit)
		{
			circuit = Instantiate<WaypointCircuit>(trackPrefabs[0]);
		}
		miniMapCamera.transform.position = circuit.trackTransform.position + new Vector3(0, 1000, 0);
		miniMapCamera.orthographicSize = circuit.orthographicSize;

		Camera carCamera = null;
		for (int i = 0; i < gamePlayerCount; i++)
		{
			GameObject arcadeKart = Instantiate(arcadeKartPrefab);
			WaypointProgressTracker waypointProgressTracker = arcadeKart.GetComponent<WaypointProgressTracker>();
			ArcadeKartAgent agent = arcadeKart.GetComponent<ArcadeKartAgent>();
			if (waypointProgressTracker && agent)
			{
				if (carCamera)
				{
					carCamera.gameObject.SetActive(false);
				}
				carCamera = arcadeKart.GetComponent<Camera>();
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
		/*for(int i = 0; i < catList?.Count; i++)
		{
			
		}*/
	}

	//每辆车完成后
	public void SceneFinish()
	{
		
	}

	internal int GetRank(WaypointProgressTracker car) 
	{
		int rank = 1;
		for (int i = 0; i < catList?.Count; i++)
		{
			if (catList[i] != car && catList[i].GetMatchProgress() < car.GetMatchProgress())
			{
				rank++;
			}
		}
		return rank;
	}

	internal int GetRacingCarCount()
	{
		return catList == null ? 1 : catList.Count;
	}
}