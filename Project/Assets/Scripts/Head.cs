using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enums{
	public class Head : MonoBehaviour {
		public static int VERTICAL = -1, HORIZONTAL = -1, phase = 0;
		public static int[,] stage = new int[5,5];
		public static bool initiative = true;

		private static GameObject blackBall, whiteBall;

		public static void SetStone(int v, int h){
			if (phase == 0) {
				Resources.UnloadUnusedAssets ();
				blackBall = Resources.Load ("BlackBall") as GameObject;
				whiteBall = Resources.Load ("WhiteBall") as GameObject;
			}

			phase++;
			stage [v, h]++;

			if (phase % 2 == 1) {
				Instantiate (blackBall, new Vector3 (h * 5, 15, v * -5), Quaternion.identity);
			} else if(phase % 2 == 0){
				Instantiate (whiteBall, new Vector3 (h * 5, 15, v * -5), Quaternion.identity);
			}
		}
	}
}
