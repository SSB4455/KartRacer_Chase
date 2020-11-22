/*
SSBB4455 2020-10-19
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using Unity.MLAgents.Policies;
using System.Collections;

public class GamePlayingManager : MonoBehaviour, GamePlayingManager.IPlayingManager
{
	public Camera miniMapCamera;
	public WaypointCircuit[] trackPrefabs;
	public Unity.Barracuda.NNModel[] agentModels;
	public GameObject arcadeKartPrefab;
	//public GameObject gamingUIPrefab;
	public GamingUI gamingUI;

	List<WaypointProgressTracker> catList = new List<WaypointProgressTracker>();
	int GamePlayerCount { get { return catList.Count; } }



	private void Start()
	{
		string gameSuteJsonString = PlayerPrefs.GetString("GameSute", "{}");
		Hashtable gameSuteJson = MiniJSON.jsonDecode(gameSuteJsonString) as Hashtable;
		if (GameSuteJsonCheck(gameSuteJson))
		{
			string trackName = gameSuteJson["Track"].ToString();
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

			Hashtable playersParamJson = gameSuteJson["Players"] as Hashtable;
			Camera carCamera = null;
			foreach (string playerParamKey in playersParamJson.Keys)
			{
				Hashtable playerJson = playersParamJson[playerParamKey] as Hashtable;
				GameObject arcadeKart = Instantiate(arcadeKartPrefab);
				arcadeKart.name = playerJson["Name"].ToString() + "_" + playerJson["Car"].ToString();
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
						behaviorParameters.BehaviorType = PlayerPrefs.GetInt("BehaviorType", 1) == 1 ? BehaviorType.HeuristicOnly : BehaviorType.InferenceOnly;
					}
					gamingUI.RacingObserver = waypointProgressTracker;
				} else {
					Debug.LogError("ArcadeKart " + i + " Instantiate Fail.");
				}
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

	bool GameSuteJsonCheck(Hashtable gameSuteJson)
	{
		if (gameSuteJson?.ContainsKey("Track") && gameSuteJson?.ContainsKey("Players"))
		{
			if ((gameSuteJson["Players"] as Hashtable)?.Count > 0)
			{
				return true;
			}
		}
		return false;
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