/*
SSBB4455 2020-10-19
*/
using UnityEngine;
using System.Collections.Generic;
using UnityStandardAssets.Utility;
using System.IO;

public class GamePlayingManager : MonoBehaviour
{
	public int gamePlayerCount;

	List<WaypointProgressTracker> catList;



	private void Update()
	{
		for(int i = 0; i < catList?.Count; i++)
		{
			
		}
	}

	//每辆车完成后
	public void SceneFinish()
	{
		
	}
}