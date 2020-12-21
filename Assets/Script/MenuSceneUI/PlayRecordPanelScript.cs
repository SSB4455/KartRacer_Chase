/*
SSBB4455 2020-12-21
*/
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class PlayRecordPanelScript : MonoBehaviour
{
	public ToggleGroup recordFileToggleGroup;
	public Toggle togglePrefab;
	List<Toggle> toggleList;
	List<Toggle> recordFileList;

	RecordDetail[] allRecords;

	public Image backgroundImage;
	public Button deleteRecordFileButton;



	private void Awake()
	{
		string recordFilePath = Path.Combine(Application.persistentDataPath, "ml-agents_config");
#if UNITY_EDITOR
		recordFilePath = Path.Combine(Directory.GetParent(Application.dataPath).ToString(), "ml-agents_config");
#endif
		string[] recordFiles = Directory.GetFiles(recordFilePath);
		allRecords = new RecordDetail[recordFiles.Length];
		for (int i = 0; i < recordFiles.Length; i++)
		{
			List<string> recordContentLines = new List<string>(File.ReadLines(recordFiles[i]));
			RecordDetail recordDetail = new RecordDetail();
			recordDetail.playTime = new DateTime(long.Parse(recordContentLines[0].Split('\t')[1]));
			recordDetail.carName = recordContentLines[7].Split('\t')[1];
			recordDetail.agentName = recordContentLines[8].Split('\t')[1];
			recordDetail.behaviorType = recordContentLines[10].Split('\t')[1];
			recordDetail.finishCircuitTime = new DateTime(long.Parse(recordContentLines[recordContentLines.Count - 1].Split('\t')[1]));
			allRecords[i] = recordDetail;
		}

		for (int i = 0; i < 10; i++)
		{
			Toggle toggle = Instantiate<Toggle>(togglePrefab);
			toggle.gameObject.SetActive(false);
			toggleList.Add(toggle);
		}
	}

	public void SetCircuit(string circuitName)
	{
//carNameText.
	}

	public string CurrentSelectFile;


	struct RecordDetail
	{
		bool dataCorrect;
		internal DateTime playTime;        //启动时间
		int timeScale;  //时间缩放倍数
		UnityStandardAssets.Utility.WaypointCircuit circuit;   //赛道
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
	}
}