/*
SSBB4455 2020-10-19
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Utility;

public class MenuSceneScript : MonoBehaviour
{
	public Camera trackPreviewCamera;
	public WaypointCircuit[] trackPrefabs;
	int currentIndex = 0;
	WaypointCircuit trackObj;

	public RacerDetailScript racerDetailPrefab;
	public Text trackInfoText;
	public InputField totalLapCountInputField;
	public AddRacerDetailPanelScript addRacerDetailPanelScript;
	public Button createOnlineRoomButton;
	public Button addRacerButton;
	public Toggle isTrainingToggle;
	public Toggle savePlayRecordToggle;
	List<RacerDetailScript> racerDetailList = new List<RacerDetailScript>();
	int racerCountLimit = 2;



	private void Awake()
	{
		addRacerDetailPanelScript.Init(this);

	}

	private void Start()
	{
		trackPreviewCamera.fieldOfView *= (16f / 9) / Camera.main.aspect;

		addRacerButton.onClick.AddListener(() => this.ShowRacerDetailPanelButton());
		ChangeTrackButton(0);


		RacerDetailScript racerDetail = Instantiate<RacerDetailScript>(racerDetailPrefab);
		racerDetail.GetComponent<Button>().onClick.AddListener(() => this.ShowRacerDetailPanelButton(racerDetail));
		racerDetail.transform.SetParent(addRacerButton.transform.parent);
		racerDetail.transform.localScale = Vector3.one;
		racerDetail.transform.SetSiblingIndex(addRacerButton.transform.parent.childCount - 2);
		racerDetail.PlayerName = "欧阳双钻";
		racerDetail.CarName = "KartClassic";
		racerDetail.ModelName= "AI_Racer1";
		racerDetail.BehaviorType = (int)Unity.MLAgents.Policies.BehaviorType.HeuristicOnly;
		racerDetail.gameObject.SetActive(true);
		AddRacerDetail(racerDetail);

	}

	public void ChangeTrackButton(int moveOffset)
	{
		if (InOnlineRoom)
		{
			return;
		}
		if (trackObj)
		{
			if (moveOffset == 0)
			{
				Debug.LogWarning("切换赛道切换了个寂寞！……");
				return;
			}
			Destroy(trackObj.gameObject);
			trackObj = null;
		}
		currentIndex += moveOffset;
		currentIndex = currentIndex % trackPrefabs.Length;
		currentIndex = currentIndex < 0 ? currentIndex + trackPrefabs.Length : currentIndex;

		trackObj = Instantiate<WaypointCircuit>(trackPrefabs[currentIndex]);
		trackInfoText.text = "Circuit: " + trackObj.trackName + "\t\tLength: " + (int)trackObj.CircuitLength + "\n" + trackObj.trackInfo;
		addRacerDetailPanelScript.SetShadowModeCircuit(trackObj.trackName);
	}

	public void ShowRacerDetailPanelButton(RacerDetailScript racerDetail = null)
	{
		if (racerDetail == null)
		{
			racerDetail = Instantiate<RacerDetailScript>(racerDetailPrefab);
			racerDetail.GetComponent<Button>().onClick.AddListener(() => this.ShowRacerDetailPanelButton(racerDetail));
			racerDetail.transform.SetParent(addRacerButton.transform.parent);
			racerDetail.transform.localScale = Vector3.one;
			racerDetail.transform.SetSiblingIndex(addRacerButton.transform.parent.childCount - 2);
		}

		addRacerDetailPanelScript.Show(racerDetail);
	}

	public void AddRacerDetail(RacerDetailScript racerDetail)
	{
		if (!racerDetailList.Contains(racerDetail))
		{
			racerDetailList.Add(racerDetail);
		}

		addRacerButton.gameObject.SetActive(racerDetailList.Count < racerCountLimit);
		addRacerButton.gameObject.SetActive(!InOnlineRoom && addRacerButton.gameObject.activeSelf);
		createOnlineRoomButton.gameObject.SetActive(racerDetailList.Count > 0);
	}

	public void DeleteRacerDetail(RacerDetailScript racerDetail)
	{
		if (racerDetail != null)
		{
			racerDetailList.Remove(racerDetail);
			Destroy(racerDetail.gameObject);
			racerDetail = null;
		}
		addRacerButton.gameObject.SetActive(racerDetailList.Count < racerCountLimit);
		createOnlineRoomButton.gameObject.SetActive(racerDetailList.Count > 0);
	}

	bool InOnlineRoom = false;
	public bool EnterOnlineRoom(string trackId, int lapCount)
	{
		InOnlineRoom = true;
		for (int i = 1; i < racerDetailList.Count; i++)
		{
			DeleteRacerDetail(racerDetailList[i]);
		}
		addRacerButton.gameObject.SetActive(false);
		for (int i = 0; i < trackPrefabs.Length; i++)
		{
			if (trackPrefabs[currentIndex].Id == trackId)
			{
				trackObj = Instantiate<WaypointCircuit>(trackPrefabs[currentIndex]);
				trackInfoText.text = "Circuit: " + trackObj.trackName + "\t\tLength: " + (int)trackObj.CircuitLength + "\n" + trackObj.trackInfo;
				addRacerDetailPanelScript.SetShadowModeCircuit(trackObj.trackName);
				break;
			}
		}
		totalLapCountInputField.text = lapCount.ToString();
		totalLapCountInputField.enabled = false;

		createOnlineRoomButton.gameObject.SetActive(false);
		
		return true;
	}

	public void LeaveOnlineRoom()
	{
		InOnlineRoom = false;
		totalLapCountInputField.enabled = true;
		createOnlineRoomButton.gameObject.SetActive(true);
	}

	public void StartButton()
	{
		Hashtable gameParamJson = new Hashtable() { 
			{ "Track", trackObj?.trackName }, 
			{ "TotalLapCount", int.Parse(totalLapCountInputField.text) } };
		ArrayList playerList = new ArrayList();
		foreach (RacerDetailScript racerDetail in racerDetailList)
		{
			Hashtable playerJson = new Hashtable() {
				{ "PlayerName", racerDetail.PlayerName },
				{ "Car", racerDetail.CarName },
				{ "AgentModel", racerDetail.ModelName },
				{ "BehaviorType", racerDetail.BehaviorType },
				{ "ShadowRecordFilePath", racerDetail.ShadowRecordFilePath } };
			playerList.Add(playerJson);
		}
		gameParamJson.Add("Players", playerList);
		gameParamJson.Add("PlayRecord", savePlayRecordToggle.isOn);
		PlayerPrefs.SetString("GameParam", gameParamJson.toJson());
		UnityEngine.SceneManagement.SceneManager.LoadScene("PlayingScene");
	}
}