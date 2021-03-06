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
	public GamingUI gamingUI;

	string matchId;
	List<WaypointProgressTracker> carList = new List<WaypointProgressTracker>();
	int GamePlayerCount { get { return carList.Count; } }



	private void Start()
	{
		string gameParamJsonString = PlayerPrefs.GetString("GameParam", "{}");
		matchId = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + UnityStandardAssets.Utility.MD5.GenerateMD5(DateTime.Now.Ticks + gameParamJsonString);
		Debug.Log("matchId = " + matchId);
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
			miniMapCamera.orthographicSize = Track.orthographicSize * (16f / 9) / miniMapCamera.aspect;

			ArrayList playerList = (ArrayList)gameParamJson["Players"];
			WaypointProgressTracker showCar = null;
			for (int i = 0; i < playerList.Count; i++)
			{
				Hashtable playerJson = playerList[i] as Hashtable;
				GameObject arcadeKart = Instantiate(arcadeKartPrefab);
				arcadeKart.name += "|" + (string)playerJson["PlayerName"] + "_" + i;
				WaypointProgressTracker waypointProgressTracker = arcadeKart.GetComponent<WaypointProgressTracker>();
				ArcadeKartAgent agent = arcadeKart.GetComponent<ArcadeKartAgent>();
				if (waypointProgressTracker && agent)
				{
					agent.arcadeKart.playerNameText.text = (string)playerJson["PlayerName"];
					waypointProgressTracker.iPlayingManager = this;
					waypointProgressTracker.circuit = Track;
					waypointProgressTracker.totalLapCount = (int)(double)gameParamJson["TotalLapCount"];
					waypointProgressTracker.record = (bool)gameParamJson["PlayRecord"];
					BehaviorParameters behaviorParameters = agent.GetComponent<BehaviorParameters>();
					if (behaviorParameters)
					{
						behaviorParameters.TeamId = i;
						behaviorParameters.BehaviorType = (Unity.MLAgents.Policies.BehaviorType)(int)(double)playerJson["BehaviorType"];
						waypointProgressTracker.behaviorType = (ArcadeKartAgent.BehaviorType)behaviorParameters.BehaviorType;
						if (playerJson.ContainsKey("ShadowRecordFilePath") && !string.IsNullOrEmpty((string)playerJson["ShadowRecordFilePath"]))
						{
							behaviorParameters.BehaviorType = BehaviorType.HeuristicOnly;
							waypointProgressTracker.record = false;
							waypointProgressTracker.behaviorType = ArcadeKartAgent.BehaviorType.ShadowPlay;
							waypointProgressTracker.shadowRecordContent = new ShadowRecordContent((string)playerJson["ShadowRecordFilePath"], agent, Track);
						}
						if (behaviorParameters.BehaviorType == BehaviorType.HeuristicOnly)
						{
							showCar = waypointProgressTracker;
						}
					}
					carList.Add(waypointProgressTracker);
					gamingUI.SetCar(waypointProgressTracker, arcadeKart.GetComponentInChildren<Camera>());
				} else {
					Debug.LogError(arcadeKart.name + " " + i + " Instantiate Fail.");
				}
			}
			gamingUI.ChangeShowCar(showCar);
		}
	}

	public Vector3 GetStartPointPosition(WaypointProgressTracker car)
	{
		Vector3 startPointPosition = Track.GetRoutePointByProgress(0).position;
		if (!carList.Contains(car))
		{
			return startPointPosition + new Vector3(0, 1, 0);
		}
		//Debug.Log(car.name + " carbodyCollider.x = " + car.arcadeKart.bodyCollider.bounds.size.x);
		Vector3 horiForward = Quaternion.Euler(0, -90, 0) * Track.WayCheckPoints[0].forward;
		int carStartRank = carList.IndexOf(car);
		float carWidth = car.arcadeKart.bodyCollider.bounds.size.x * 2;
		float carLength = car.arcadeKart.bodyCollider.bounds.size.y * 3;
		float trackWidth = Mathf.Abs(Vector3.Dot(horiForward, Track.WayCheckPoints[0].GetComponent<Collider>().bounds.size));
		//Debug.Log("trackCollider width = " + trackWidth);
		int horiCarCount = (int)((trackWidth - 2) / carWidth);
		int carHoriIndex = carStartRank % horiCarCount;
		Vector3 offset = horiForward * -carWidth * (carHoriIndex - (horiCarCount / 2f) + 0.5f);
		offset -= Track.WayCheckPoints[0].forward * (carStartRank / horiCarCount) * carLength;
		//Debug.Log("offset " + offset);
		return startPointPosition + offset + new Vector3(0, 1, 0);
	}

	public Quaternion GetStartPointRotation(WaypointProgressTracker car)
	{
		return Track.WayCheckPoints[0].rotation;
	}

	private void Update()
	{
		/*for(int i = 0; i < catList?.Count; i++)
		{
			
		}*/
	}

	public string GetMatchId()
	{
		return matchId;
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
		string GetMatchId();
		bool MatchFinish(WaypointProgressTracker car);
		int GetRank(WaypointProgressTracker car);
		int GetRacingCarCount();
		Vector3 GetStartPointPosition(WaypointProgressTracker car);
		Quaternion GetStartPointRotation(WaypointProgressTracker car);
	}


}