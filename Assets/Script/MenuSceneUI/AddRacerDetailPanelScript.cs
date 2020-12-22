/*
SSBB4455 2020-12-21
*/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class AddRacerDetailPanelScript : MonoBehaviour
{
	MenuSceneScript menuSceneScript;

	public Button driveModeButton;
	public Button shadowModeButton;
	public Image driveModeBackgroundImage;
	public Dropdown carDropdown;
	public Dropdown agentModelDropdown;
	public Dropdown behaviorTypeDropdown;
	public Button deleteRacerDetailButton;

	bool detailIsShadow;
	public Image shadowModeBackgroundImage;
	public ToggleGroup recordFileToggleGroup;
	public RecordToggleScript togglePrefab;
	List<RecordToggleScript> toggleList = new List<RecordToggleScript>();
	List<RecordToggleScript> recordFileList = new List<RecordToggleScript>();
	RecordDetail[] allRecords;



	public void Init(MenuSceneScript menuSceneScript)
	{
		this.menuSceneScript = menuSceneScript;
		gameObject.SetActive(false);

		string recordFilePath = Path.Combine(Application.persistentDataPath, "ml-agents_config");
#if UNITY_EDITOR
		recordFilePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "ml-agents_config");
#endif
		string[] recordFiles = Directory.GetFiles(recordFilePath, "ArcadeKartAgent_playRecord_*.txt");
		allRecords = new RecordDetail[recordFiles.Length];
		for (int i = 0; i < recordFiles.Length; i++)
		{
			//Debug.Log(recordFiles[i]);
			List<string> recordContentLines = new List<string>(File.ReadLines(recordFiles[i]));
			RecordDetail recordDetail = new RecordDetail();
			recordDetail.playTime = new DateTime(long.Parse(recordContentLines[0].Split('\t')[1]));
			for (int j = 0; j < menuSceneScript?.trackPrefabs.Length; j++)
			{
				if (menuSceneScript.trackPrefabs[j].trackName == recordContentLines[2].Split('\t')[1])
				{
					recordDetail.circuit = menuSceneScript.trackPrefabs[j];
					recordDetail.dataCorrect = true;
					break;
				}
			}
			recordDetail.filePath = recordFiles[i];
			recordDetail.carName = recordContentLines[7].Split('\t')[1];
			recordDetail.agentName = recordContentLines[8].Split('\t')[1];
			recordDetail.behaviorType = recordContentLines[10].Split('\t')[1];
			recordDetail.finishCircuitTime = new DateTime(long.Parse(recordContentLines[recordContentLines.Count - 1].Split('\t')[1]));
			recordDetail.recordToggleScript = Instantiate<RecordToggleScript>(togglePrefab);
			recordDetail.recordToggleScript.recordToggle.group = recordFileToggleGroup;
			recordDetail.recordToggleScript.transform.SetParent(recordFileToggleGroup.transform);
			recordDetail.recordToggleScript.transform.localScale = Vector3.one;
			recordDetail.recordToggleScript.gameObject.SetActive(false);
			recordDetail.recordToggleScript.recordText.text = recordDetail.carName + "\n" + recordDetail.agentName + "\n" + recordDetail.playTime.ToString("yyyy-MM-dd HH:mm:ss");
			allRecords[i] = recordDetail;
		}
	}

	public void SetShadowModeCircuit(string trackName)
	{
		while (recordFileList.Count > 0)
		{
			recordFileList[0].gameObject.SetActive(false);
			recordFileList.RemoveAt(0);
		}
		recordFileToggleGroup.SetAllTogglesOff();
		for (int i = 0; i < allRecords.Length; i++)
		{
			if (allRecords[i].dataCorrect && allRecords[i].circuit.trackName == trackName)
			{
				RecordToggleScript recordToggleScript = allRecords[i].recordToggleScript;
				recordToggleScript.gameObject.SetActive(true);
				recordToggleScript.recordToggle.isOn = false;
				recordFileList.Add(recordToggleScript);
			}
		}
	}

	RacerDetailScript racerDetail;
	internal void Show(RacerDetailScript racerDetail)
	{
		this.racerDetail = racerDetail;
		racerDetail.gameObject.SetActive(false);

		detailIsShadow = racerDetail.IsShdowRecord;
		deleteRacerDetailButton.gameObject.SetActive(false);
		driveModeBackgroundImage.gameObject.SetActive(!detailIsShadow);
		shadowModeBackgroundImage.gameObject.SetActive(detailIsShadow);
		if (!racerDetail.IsShdowRecord)
		{
			if (!string.IsNullOrEmpty(racerDetail.playerName))
			{
				deleteRacerDetailButton.gameObject.SetActive(true);

				//同步显示选中的车辆
				//同步显示选中的AI模型
				int behaviorTypeInt = 0;		//同步显示操作方式
				for (int i = 0; i < behaviorTypeDropdown.options.Count; i++)
				{
					if (behaviorTypeDropdown.options[i].text == racerDetail.behaviorTypeText.text)
					{
						behaviorTypeInt = i;
						break;
					}
				}
				behaviorTypeDropdown.value = behaviorTypeInt;
			}
		} else {
			deleteRacerDetailButton.gameObject.SetActive(true);
			for (int i = 0; i < allRecords.Length; i++)
			{
				if (racerDetail.shadowRecordFilePath == allRecords[i].filePath)
				{
					allRecords[i].recordToggleScript.recordToggle.isOn = true;
					break;
				}
			}
		}

		gameObject.SetActive(true);
	}

	public void SwitchToDriveModeButton()
	{
		detailIsShadow = false;
		driveModeBackgroundImage.gameObject.SetActive(!detailIsShadow);
		shadowModeBackgroundImage.gameObject.SetActive(detailIsShadow);
	}

	public void SwitchToShadowModeButton()
	{
		detailIsShadow = true;
		driveModeBackgroundImage.gameObject.SetActive(!detailIsShadow);
		shadowModeBackgroundImage.gameObject.SetActive(detailIsShadow);
	}

	public void SureButton()
	{
		gameObject.SetActive(false);

		if (!detailIsShadow)
		{
			racerDetail.playerName = "欧阳双钻";
			racerDetail.carNameText.text = carDropdown.options[carDropdown.value].text;
			racerDetail.agentNameText.text = agentModelDropdown.options[agentModelDropdown.value].text;
			racerDetail.behaviorTypeText.text = behaviorTypeDropdown.options[behaviorTypeDropdown.value].text;
		}
		else
		{
			for (int i = 0; i < allRecords.Length; i++)
			{
				if (allRecords[i].recordToggleScript.recordToggle.isOn)
				{
					racerDetail.playerName = "shadow";
					racerDetail.carNameText.text = allRecords[i].carName + "(shadow)";
					racerDetail.agentNameText.text = allRecords[i].agentName;
					racerDetail.behaviorTypeText.text = allRecords[i].behaviorType + "\n" + allRecords[i].playTime.ToString("yyyy-MM-dd HH:mm:ss");
					racerDetail.shadowRecordFilePath = allRecords[i].filePath;
					racerDetail.gameObject.SetActive(true);
					break;
				}
			}
		}
		racerDetail.gameObject.SetActive(true);

		menuSceneScript.AddRacerDetail(racerDetail);
	}

	public void CancelButton()
	{
		gameObject.SetActive(false);
		racerDetail.gameObject.SetActive(true);
		if (string.IsNullOrEmpty(racerDetail.playerName))
		{
			Destroy(racerDetail.gameObject);
		}
		racerDetail = null;
	}

	public void DeleteButton()
	{
		gameObject.SetActive(false);
		menuSceneScript.DeleteRacerDetail(racerDetail);
	}





	struct RecordDetail
	{
		internal string filePath;
		internal bool dataCorrect;
		internal DateTime playTime;        //启动时间
		int timeScale;  //时间缩放倍数
		internal UnityStandardAssets.Utility.WaypointCircuit circuit;   //赛道
		string playerName;// 欧阳双钻 //玩家的名字
		internal string carName;// KartClassic //车的名字
		internal string agentName;// AI_Racer1 //AgentName
		string agentValueMD5;// xcvfe43470vb4jmgu7z1cvv2ntyn0hg43sd6i //AgentValueMD5
		internal string behaviorType;// Default    //AgentBehaviorType
		internal int totalLapCount;//	1	//跑几圈
		string matchId;//	20201217ttcwdf01    //比赛的id所有参加这场比赛的车辆都会有同样的比赛id
		int totalCarCount;//	1	//总共有几辆赛车参加比赛

		Vector3 circuitPosition;//	0.1,0.1,0	//赛道的位置
								//CarPosition	12313242332	3.5,12,0.3	0	0,0,0	//从比赛开始的时间戳	位置	比赛进度	三维速度

		internal DateTime finishCircuitTime;//	505363343	00:00:50.5363343	//完成的全部耗时	耗时时间戳	小时分钟秒毫秒格式

		internal RecordToggleScript recordToggleScript;
	}
}