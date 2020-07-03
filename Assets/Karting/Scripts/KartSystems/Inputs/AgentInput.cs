/*
SSBB4455 2020-07-03
*/
using UnityEngine;

namespace KartGame.KartSystems
{
	public class AgentInput : BaseInput
	{
		public float x;
		public float y;



		public override Vector2 GenerateInput()
		{
			return new Vector2(x, y);
		}
	}
}
