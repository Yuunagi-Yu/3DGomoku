using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Enums{
	public class Head : MonoBehaviour {
		public static int VERTICAL = -1, HORIZONTAL = -1, phase = 0, depth = 4;
		public static int[,] lastPos = new int[5, 3];
		public static int[,] stage = new int[5,5];
		public static int[] stonePos = new int[3];
		public static bool initiative = true, end = false;

		private static GameObject blackBall, whiteBall, cube, setCube;
		private static int n = 0;

		public static void SetStone(int v, int h){
			if (phase == 0) {
				Resources.UnloadUnusedAssets ();
				blackBall = Resources.Load ("BlackBall") as GameObject;
				whiteBall = Resources.Load ("WhiteBall") as GameObject;
				cube = Resources.Load ("Cube") as GameObject;
				setCube = (GameObject)Instantiate (Resources.Load ("SetCube") as GameObject, Vector3.zero, Quaternion.identity);
				setCube.SetActive (false);
				n = 0;
			}

			stonePos [0] = stage [v, h];
			stonePos [1] = v;
			stonePos [2] = h;
			phase++;
			stage [v, h]++;

			if (phase % 2 == 1) {
				Instantiate (blackBall, new Vector3 (h * 5, 20, v * -5), Quaternion.identity);
			} else if(phase % 2 == 0){
				Instantiate (whiteBall, new Vector3 (h * 5, 20, v * -5), Quaternion.identity);
			}
			setCube.transform.position = new Vector3 (stonePos [2] * 5, stonePos [0] * 4, stonePos [1] * -5);
			setCube.SetActive (true);

			if (end) {
				setCube.SetActive (false);
				VERTICAL = -1;
				HORIZONTAL = -1;
			}
		}

		public static void indicate(int z, int x, int y){
			lastPos [n, 0] = z;
			lastPos [n, 1] = x;
			lastPos [n, 2] = y;
			Instantiate (cube, new Vector3 (y * 5, z * 4, x * -5), Quaternion.identity);
			n++;
		}
	}
}
