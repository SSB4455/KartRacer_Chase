/*
SSBB4455 2020-10-19
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using Unity.MLAgents.Policies;

public class GamePlayingManager : MonoBehaviour, GamePlayingManager.IPlayingManager
{
	int gamePlayerCount;
	public Camera miniMapCamera;
	public WaypointCircuit[] trackPrefabs;
	public GameObject arcadeKartPrefab;
	//public GameObject gamingUIPrefab;
	public GamingUI gamingUI;

	List<WaypointProgressTracker> catList = new List<WaypointProgressTracker>();



	private void Start()
	{
		gamePlayerCount = PlayerPrefs.GetInt("", 1);

		string trackName = PlayerPrefs.GetString("PlayTrack", "TraningTrack1");
		Debug.Log("PlayTrack = " + trackName);
		WaypointCircuit circuit = null;
		WaypointCircuit[] waypointCircuits = FindObjectsOfType<WaypointCircuit>();
		for (int i = 0; i < waypointCircuits?.Length; i++)
		{
			if (waypointCircuits[i].trackName == trackName)
			{
				circuit = waypointCircuits[i];
			} else {
				Destroy(waypointCircuits[i].gameObject);
				waypointCircuits[i] = null;
			}
		}
		if (!circuit)
		{
			for (int i = 0; i < trackPrefabs?.Length; i++)
			{
				if (trackPrefabs[i].trackName == trackName)
				{
					circuit = Instantiate<WaypointCircuit>(trackPrefabs[i]);
					break;
				}
			}
			if (!circuit)
			{
				Debug.LogError("未找到赛道 " + trackName);
			}
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
				waypointProgressTracker.iPlayingManager = this;
				waypointProgressTracker.circuit = circuit;
				catList.Add(waypointProgressTracker);
				BehaviorParameters behaviorParameters = agent.GetComponent<BehaviorParameters>();
				if (behaviorParameters)
				{
					behaviorParameters.BehaviorType = BehaviorType.HeuristicOnly;
				}
				gamingUI.RacingObserver = waypointProgressTracker;
			} else {
				Debug.LogError("ArcadeKart " + i + " Instantiate Fail.");
			}
		}

		//gamingUI = Instantiate(gamingUIPrefab).GetComponent<GamingUI>();
	}

	private void Update()
	{
		/*for(int i = 0; i < catList?.Count; i++)
		{
			
		}*/
	}

	public bool MatchFinish(WaypointProgressTracker car)
	{
		//throw new NotImplementedException();
		return true;
	}

	//每辆车完成后
	public void SceneFinish()
	{
		
	}

	public int GetRank(WaypointProgressTracker car) 
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

	public int GetRacingCarCount()
	{
		return catList == null ? 1 : catList.Count;
	}






	/// <summary>
	/// 游戏管理 管理所有车辆
	/// </summary>
	public interface IPlayingManager
	{
		bool MatchFinish(UnityStandardAssets.Utility.WaypointProgressTracker car);
		int GetRank(UnityStandardAssets.Utility.WaypointProgressTracker car);
		int GetRacingCarCount();
	}


}