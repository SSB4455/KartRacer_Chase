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
	public Text trackInfoText;



	private void Start()
	{
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
		
	}

	public void StartButton()
	{
		PlayerPrefs.SetString("PlayTrack", trackObj?.trackName);
		UnityEngine.SceneManagement.SceneManager.LoadScene("PlayingScene");
	}
}