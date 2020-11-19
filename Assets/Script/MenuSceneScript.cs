/*
SSBB4455 2020-10-19
*/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Utility;

public class MenuSceneScript : MonoBehaviour
{
	public WaypointCircuit[] trackPrefabs;
	int currentIndex = 0;
	WaypointCircuit trackObj;

	public Button[] Buttons;
	Button[] changeTrackButtons;
	public RacerDetailScript racerDetailPrefab;
	public Text trackInfoText;
	public InputField totalLapCountInputField;
	public Button addRacerButton;
	public GameObject addCarDetailPanel;
	public Dropdown behaviourTypeDropdown;



	private void Start()
	{
		addCarDetailPanel.gameObject.SetActive(false);

		ChangeTrackButton(0);
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
		trackInfoText.text = "Circuit: " + trackObj.trackName + "\tLength: " + (int)trackObj.CircuitLength + "\n" +
			trackObj.trackInfo;
	}

	public void AddRacerButton()
	{
		addCarDetailPanel.gameObject.SetActive(true);
		addRacerButton.gameObject.SetActive(false);
	}

	public void RacerDetailSureButton()
	{
		addCarDetailPanel.gameObject.SetActive(false);
		RacerDetailScript racerDetail = Instantiate<RacerDetailScript>(racerDetailPrefab);
		racerDetail.carNameText.text = "KartClassic";
		racerDetail.agentNameText.text = "AI_Racer1";
		racerDetail.behaviourTypeText.text = behaviourTypeDropdown.options[behaviourTypeDropdown.value].text;
		PlayerPrefs.SetInt("BehaviorType", behaviourTypeDropdown.value);

		racerDetail.transform.SetParent(addRacerButton.transform.parent);
	}

	public void RacerDetailCancelButton()
	{
		addCarDetailPanel.gameObject.SetActive(false);
		addRacerButton.gameObject.SetActive(true);
	}

	public void StartButton()
	{
		PlayerPrefs.SetString("PlayTrack", trackObj?.trackName);
		PlayerPrefs.SetInt("TotalLapCount", int.Parse(totalLapCountInputField.text));
		UnityEngine.SceneManagement.SceneManager.LoadScene("PlayingScene");
	}
}