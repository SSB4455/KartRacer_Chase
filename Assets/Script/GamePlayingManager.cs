/*
SSBB4455 2020-10-19
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using Unity.MLAgents.Policies;

public class GamePlayingManager : MonoBehaviour, GamePlayingManager.IPlayingManager
{
	public Camera miniMapCamera;
	public WaypointCircuit[] trackPrefabs;
	internal WaypointCircuit Track { private set; get; }
	public Unity.Barracuda.NNModel[] agentModels;
	public GameObject arcadeKartPrefab;
	//public GameObject gamingUIPrefab;
	public GamingUI gamingUI;

	List<WaypointProgressTracker> carList = new List<WaypointProgressTracker>();
	int GamePlayerCount { get { return carList.Count; } }



	private void Start()
	{
		string gameParamJsonString = PlayerPrefs.GetString("GameParam", "{}");
		Hashtable gameParamJson = MiniJSON.jsonDecode(gameParamJsonString) as Hashtable;
		if (GameSuteJsonCheck(gameParamJson))
		{
			string trackName = gameParamJson["Track"].ToString();
			Debug.Log("PlayTrack = " + trackName);
			WaypointCircuit[] waypointCircuits = FindObjectsOfType<WaypointCircuit>();
			for (int i = 0; i < waypointCircuits?.Length; i++)
			{
				if (waypointCircuits[i].trackName == trackName)
				{
					Track = waypointCircuits[i];
				} else {
					Destroy(waypointCircuits[i].gameObject);
					waypointCircuits[i] = null;
				}
			}
			if (!Track)
			{
				for (int i = 0; i < trackPrefabs?.Length; i++)
				{
					if (trackPrefabs[i].trackName == trackName)
					{
						Track = Instantiate<WaypointCircuit>(trackPrefabs[i]);
						break;
					}
				}
				if (!Track)
				{
					Debug.LogError("未找到赛道 " + trackName);
				}
			}
			miniMapCamera.transform.position = Track.trackTransform.position + new Vector3(0, 1000, 0);
			miniMapCamera.orthographicSize = Track.orthographicSize;

			ArrayList playerList = (ArrayList)gameParamJson["Players"];
			WaypointProgressTracker showCar = null;
			for (int i = 0; i < playerList.Count; i++)
			{
				Hashtable playerJson = playerList[i] as Hashtable;
				GameObject arcadeKart = Instantiate(arcadeKartPrefab);
				arcadeKart.name = (string)playerJson["Name"] + "_" + (string)playerJson["Car"];
				WaypointProgressTracker waypointProgressTracker = arcadeKart.GetComponent<WaypointProgressTracker>();
				ArcadeKartAgent agent = arcadeKart.GetComponent<ArcadeKartAgent>();
				if (waypointProgressTracker && agent)
				{
					waypointProgressTracker.iPlayingManager = this;
					waypointProgressTracker.circuit = Track;
					waypointProgressTracker.totalLapCount = (int)(double)gameParamJson["TotalLapCount"];
					BehaviorParameters behaviorParameters = agent.GetComponent<BehaviorParameters>();
					if (behaviorParameters)
					{
						int playerBehaviorType = (int)(double)playerJson["BehaviorType"];
						behaviorParameters.BehaviorType = playerBehaviorType == 1 ? BehaviorType.HeuristicOnly : BehaviorType.InferenceOnly;
						if (playerBehaviorType == 1)
						{
							showCar = waypointProgressTracker;
						}
					}
					carList.Add(waypointProgressTracker);
					gamingUI.SetCar(waypointProgressTracker, arcadeKart.GetComponent<Camera>());
				} else {
					Debug.LogError(arcadeKart.name + " " + i + " Instantiate Fail.");
				}
			}
			gamingUI.ChangeShowCar(showCar);
		}
	}

	public Vector3 GetStartPointPositionOffset(int rank)
	{
		Vector3 positionOffset = Track.GetRoutePointByProgress(0).position;
		switch (rank)
		{
			case 1: positionOffset.x -= arcadeKartPrefab.GetComponent<Collider>().bounds.size.x * 0.8f; break;
			case 2: positionOffset.x += arcadeKartPrefab.GetComponent<Collider>().bounds.size.x * 0.8f; break;

		}
		return positionOffset;
	}

	public Quaternion GetStartPointRotationOffset(int rank)
	{
		Quaternion rotationOffset = Quaternion.FromToRotation(arcadeKartPrefab.transform.forward, Track.GetRoutePointByProgress(0).direction);
		return Quaternion.RotateTowards(rotationOffset, Track.WayCheckPoints[0].rotation, 360);
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
		if (car?.GetMatchTime() == TimeSpan.Zero && carList.Contains(car))
		{
			return carList.IndexOf(car) + 1;
		}
		int rank = 1;
		for (int i = 0; i < carList?.Count; i++)
		{
			if (carList[i] != car && carList[i].GetMatchProgress() < car?.GetMatchProgress())
			{
				rank++;
			}
		}
		return rank;
	}

	public int GetRacingCarCount()
	{
		return carList == null ? 1 : carList.Count;
	}

	bool GameSuteJsonCheck(Hashtable gameSuteJson)
	{
		if (gameSuteJson != null && gameSuteJson.ContainsKey("Track") && gameSuteJson.ContainsKey("Players"))
		{
			if (((ArrayList)gameSuteJson["Players"])?.Count > 0)
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
		Vector3 GetStartPointPositionOffset(int rank);
		Quaternion GetStartPointRotationOffset(int rank);
	}


}