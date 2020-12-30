/*
SSBB4455 2020-12-30
*/
using System;
using UnityEngine;

namespace UnityStandardAssets.Utility
{
	public class EasterEggsScript : MonoBehaviour
	{
		public GameObject easterEggsPrefab;
		bool bigvegasDance;




		void Awake()
		{
			if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "PlayingScene")
			{
				bigvegasDance = DateTime.Now.Month == 12 && DateTime.Now.Day == 25 ||
					DateTime.Now.Minute % 7 == 1;
			}
		}

		void Start()
		{
			if (bigvegasDance)
			{
				Instantiate(easterEggsPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero));
			}
		}
	}
}