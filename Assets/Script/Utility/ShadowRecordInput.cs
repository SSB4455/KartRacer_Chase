/*
SSBB4455 2020-12-30
*/
using UnityEngine;

namespace UnityStandardAssets.Utility
{
	public class ShadowRecordInput : KartGame.KartSystems.BaseInput
	{
		public float inputX;
		public float inputY;



		public override Vector2 GenerateInput()
		{
			return new Vector2(inputX, inputY);
		}
	}
}
