/*
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
		public ICircuitRacingObserver RacingObserver
		{
			set
			{
				racingObserver = value;
				circuitNameText.text = "Circuit: " + racingObserver.GetCircuitName() + "\tLength: " + (int)racingObserver.GetCircuitLength();
			}
		}



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


	}
}