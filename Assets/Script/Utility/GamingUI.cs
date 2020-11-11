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
				circuitNameText.text = "Current Circuit: " + racingObserver.GetCircuitName() + "\tLength: " + racingObserver.GetCircuitLength();
			}
		}



		void Start()
		{
			
			
		}

		void Update()
		{
			lapTimeText.text = "Match Time: " + racingObserver.GetMatchTime().ToString("mm':'ss':'fff") + "\nLap Time: " + racingObserver.GetCurrentLapTime().ToString("mm':'ss':'fff");
			rankText.text = "Rank: " + racingObserver.GetRank() + " / " + racingObserver.GetRacingCarCount();
			//speedText.text = racingObserver.g
		}


	}
}