/*
SSBB4455 2020-09-18
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Utility
{
	public class TrainingDetailShowUI : MonoBehaviour
	{
		public Canvas canvas;
		public ICircuitRacingObserver racingObserver;
		public Transform obstacleStockpile;

		float topLength;
		float timeRange = 5;	//1 10
		DateTime cruveStartTime, cruveEndTime;

		List<Image> obstacleList = new List<Image>();
		List<Image> obstacleCache = new List<Image>();



		void Start()
		{
			
			
		}

		void Update()
		{
			
		}

		void AddShowData(DateTime dataTime, float data)
		{

		}

		Image GetObstacle(Vector3 startPosition)
		{
			Image obstacleObj = null;
			if (obstacleCache.Count > 0)
			{
				obstacleObj = obstacleCache[0];
				obstacleCache.RemoveAt(0);
			} else {
				//obstacleObj = Instantiate<Image>(obstaclePrefab);
			}
			return obstacleObj;
		}

		public void RecycleObstacle(Image obstacleObj)
		{
			obstacleList.Remove(obstacleObj);
			obstacleObj.gameObject.SetActive(false);
			obstacleObj.transform.parent = obstacleStockpile;
			obstacleCache.Add(obstacleObj);
		}
	}
}