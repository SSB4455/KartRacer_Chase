/*
SSBB4455 2020-11-19
*/
using UnityEngine;
using UnityEngine.UI;

public class RacerDetailScript : MonoBehaviour
{
	public Image backgroundImage;
	[SerializeField] private Text detailText;
	string carName;
	public string CarName{
		set
		{
			carName = value;
			detailText.text = ToString();
		}
		get { return carName; }
	}
	string modelName;
	public string ModelName
	{
		set
		{
			modelName = value;
			detailText.text = ToString();
		}
		get { return modelName; }
	}
	Unity.MLAgents.Policies.BehaviorType behaviorType;
	public int BehaviorType
	{
		set
		{
			behaviorType = (Unity.MLAgents.Policies.BehaviorType)value;
			detailText.text = ToString();
		}
		get { return (int)behaviorType; }
	}
	string playerName;
	internal string PlayerName
	{
		set
		{
			playerName = value;
			detailText.text = ToString();
		}
		get { return playerName; }
	}
	string shadowRecordFilePath;
	internal string ShadowRecordFilePath
	{
		set
		{
			shadowRecordFilePath = value;
			detailText.text = ToString();
		}
		get { return shadowRecordFilePath; }
	}
	public bool IsShdowRecord { get { return !string.IsNullOrEmpty(shadowRecordFilePath); } }



	new string ToString()
	{
		string str = playerName + "\n" + carName + "\n" + modelName + "\n" + behaviorType.ToString();
		if (IsShdowRecord)
		{
			System.IO.FileInfo fileInfo = new System.IO.FileInfo(shadowRecordFilePath);
			str += "\n" + fileInfo.Name.Replace("ArcadeKartAgent_", "").Replace(".txt", "");
		}
		return str;
	}
}