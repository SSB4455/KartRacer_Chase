/*
SSBB4455 2020-11-19
*/
using UnityEngine;
using UnityEngine.UI;

public class RacerDetailScript : MonoBehaviour
{
	public Text carNameText;
	public Text agentNameText;
	public Text behaviorTypeText;
	public Image backgroundImage;
	internal string playerName;
	internal string shadowRecordFilePath;
	public bool IsShdowRecord { get { return !string.IsNullOrEmpty(shadowRecordFilePath); } }
}