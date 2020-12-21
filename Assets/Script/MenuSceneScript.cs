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
	public Button addRacerButton;
	public Toggle savePlayRecordToggle;
	public GameObject addCarDetailPanel;
	public Dropdown carDropdown;
	public Dropdown agentModelDropdown;
	public Dropdown behaviourTypeDropdown;
	public Button deleteRacerDetailButton;
	public PlayRecordPanelScript playRecordPanelScript;
	List<RacerDetailScript> racerDetailList = new List<RacerDetailScript>();
	int racerCountLimit = 2;



	private void Start()
	{
		addCarDetailPanel.gameObject.SetActive(false);
		trackPreviewCamera.fieldOfView *= (16f / 9) / Camera.main.aspect;

		ChangeTrackButton(0);

		AddRacerDetail("欧阳双钻", "KartClassic", "AI_Racer1", 1);
	}

	public void ChangeTrackButton(int moveOffset)
	{
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
		trackInfoText.text = "Circuit: " + trackObj.trackName + "\tLength: " + (int)trackObj.CircuitLength + "\n" + trackObj.trackInfo;
	}

	public void ShowAddRacerDetailButton()
	{
		addCarDetailPanel.gameObject.SetActive(true);
		deleteRacerDetailButton.gameObject.SetActive(false);
		for (int i = 0; i < racerDetailList?.Count; i++)
		{
			if (!racerDetailList[i].IsShdowRecord && racerDetailList[i].behaviourTypeText.text == behaviourTypeDropdown.options[1].text)
			{       //只能有1个人工操作的车手
				behaviourTypeDropdown.options.RemoveAt(1);
				break;
			}
		}
	}

	public void AddRacerDetailButton()
	{
		addCarDetailPanel.gameObject.SetActive(false);
		AddRacerDetail("欧阳双钻", carDropdown.options[carDropdown.value].text, agentModelDropdown.options[agentModelDropdown.value].text, behaviourTypeDropdown.value);
	}

	public void AddRacerDetail(string playerName, string carName, string agentName, int behaviourType)
	{
		RacerDetailScript racerDetail = onChangeRacerDetail;
		if (racerDetail == null) 
		{
			racerDetail = Instantiate<RacerDetailScript>(racerDetailPrefab);
			racerDetail.GetComponent<Button>().onClick.AddListener(() => this.ChangeRacerDetailButton(racerDetail));
			racerDetailList.Add(racerDetail);
		}
		racerDetail.playerName = playerName;
		racerDetail.carNameText.text = carName;
		racerDetail.agentNameText.text = agentName;
		racerDetail.behaviourTypeText.text = behaviourTypeDropdown.options[behaviourType].text;
		racerDetail.transform.SetParent(addRacerButton.transform.parent);
		racerDetail.transform.localScale = Vector3.one;
		racerDetail.transform.SetSiblingIndex(addRacerButton.transform.parent.childCount - 2);

		addRacerButton.gameObject.SetActive(racerDetailList.Count < racerCountLimit);
		onChangeRacerDetail = null;
	}

	public void ShowAddPlayRecordButton()
	{
		playRecordPanelScript.AddPlayRecord();
	}

	public void AddPlayRecordButton(string playerName, string carName, string agentName, string behaviourType, string shadowRecordFilePath)
	{
		RacerDetailScript racerDetail = onChangeRacerDetail;
		if (racerDetail == null)
		{
			racerDetail = Instantiate<RacerDetailScript>(racerDetailPrefab);
			racerDetail.GetComponent<Button>().onClick.AddListener(() => playRecordPanelScript.SwitchPlayRecord()(racerDetail));
			racerDetailList.Add(racerDetail);
		}
		
		racerDetail.playerName = playerName;
		racerDetail.carNameText.text = carName;
		racerDetail.agentNameText.text = agentName;
		racerDetail.behaviourTypeText.text = behaviourType;
		racerDetail.shadowRecordFilePath = shadowRecordFilePath;
		racerDetail.transform.SetParent(addRacerButton.transform.parent);
		racerDetail.transform.localScale = Vector3.one;
		racerDetail.transform.SetSiblingIndex(addRacerButton.transform.parent.childCount - 2);
	}

	void ChangeRacerDetailButton(RacerDetailScript racerDetail)
	{
		addCarDetailPanel.gameObject.SetActive(true);
		deleteRacerDetailButton.gameObject.SetActive(true);
		int behaviourTypeInt = 0;
		for (int i = 0; i < behaviourTypeDropdown.options.Count; i++)
		{
			if (behaviourTypeDropdown.options[i].text == racerDetail.behaviourTypeText.text)
			{
				behaviourTypeInt = i;
				break;
			}
		}
		behaviourTypeDropdown.value = behaviourTypeInt;
	}

	public void RacerDetailCancelButton()
	{
		addCarDetailPanel.gameObject.SetActive(false);
		addRacerButton.gameObject.SetActive(racerDetailList.Count < racerCountLimit);
		onChangeRacerDetail = null;
	}

	public void DeleteRacerDetailButton()
	{
		if (onChangeRacerDetail != null)
		{
			racerDetailList.Remove(onChangeRacerDetail);
			Destroy(onChangeRacerDetail.gameObject);
			onChangeRacerDetail = null;
		}
		addCarDetailPanel.gameObject.SetActive(false);
		addRacerButton.gameObject.SetActive(racerDetailList.Count < racerCountLimit);
		onChangeRacerDetail = null;
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
				{ "IsShadowRecord", racerDetail.IsShdowRecord },
				{ "Name", racerDetail.playerName },
				{ "Car", racerDetail.carNameText.text },
				{ "AgentModel", racerDetail.agentNameText.text },
				{ "BehaviorType", racerDetail.behaviourTypeText.text } };
			playerList.Add(playerJson);
		}
		gameParamJson.Add("Players", playerList);
		gameParamJson.Add("PlayRecord", savePlayRecordToggle.isOn);
		PlayerPrefs.SetString("GameParam", gameParamJson.toJson());
		UnityEngine.SceneManagement.SceneManager.LoadScene("PlayingScene");
	}
}