﻿/*
SSBB4455 2020-11-11
*/
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityStandardAssets.Utility
{
	public class GamingUI : MonoBehaviour
	{
		public Canvas canvas;
		public Text circuitNameText;
		public Text lapTimeText;
		public Text rankText;
		public Text speedText;
		
		ICircuitRacingObserver racingObserver;
		Dictionary<ICircuitRacingObserver, Camera> carCameras = new Dictionary<ICircuitRacingObserver, Camera>();
		List<ICircuitRacingObserver> carList = new List<ICircuitRacingObserver>();



		void Start()
		{
			
			
		}

		void Update()
		{
			rankText.text = "Rank: " + racingObserver.GetRank() + " / " + racingObserver.GetRacingCarCount() + 
				"\tLap: " + racingObserver.GetMaxFinishLapCount() + " / " + racingObserver.GetTotalLapCount();
			lapTimeText.text = "Match Time: " + racingObserver.GetMatchTime().ToString("mm':'ss':'fff") + 
				"\nBest Lap Time: " + (racingObserver.GetBestLapTime() == System.TimeSpan.Zero ? "--:--:--" : racingObserver.GetBestLapTime().ToString("mm':'ss':'fff")) +
				"\nLap Time: " + racingObserver.GetCurrentLapTime().ToString("mm':'ss':'fff");
			speedText.text = "Speed: " + racingObserver.GetForwardSpeed().ToString("f2");
		}

		internal void SetCar(ICircuitRacingObserver racingObserver, Camera carCamera)
		{
			this.racingObserver = racingObserver;
			carList.Add(racingObserver);
			carCameras.Add(racingObserver, carCamera);
			ChangeShowCar(racingObserver);
		}

		/// <summary>
		/// 切换GamingUI展示哪辆车的信息
		/// </summary>
		/// <param name="racingObserver"></param>
		/// <returns>传入车辆在GamingUI的车辆列表中所在的位置 不存在则返回-1</returns>
		internal int ChangeShowCar(ICircuitRacingObserver racingObserver)
		{
			if (carCameras.ContainsKey(racingObserver) && carList.Contains(racingObserver))
			{
				this.racingObserver = racingObserver;
				circuitNameText.text = "Circuit: " + racingObserver.GetCircuitName() + "\tLength: " + (int)racingObserver.GetCircuitLength();
				return carList.IndexOf(racingObserver);
			}
			return -1;
		}

		int showCarIndex = 0;
		public void ChangeVehicleButton()
		{
			showCarIndex = (showCarIndex + 1) % carList.Count;
			ChangeShowCar(carList[showCarIndex]);
		}
	}
}