using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class EnemyAI : MonoBehaviour {
	public int searchDepth;

	private int[,,] field = new int[5, 5, 5];
	private int[,] winPos = new int[5, 3];
	private int height = 0, canSet = 0;
	private float value = 0;
	private bool initiative = false, attack = true;

	// Use this for initialization
	void Start () {
		initiative = !Head.initiative;
	}
	
	// Update is called once per frame
	void Update () {
		if (Head.phase % 2 == 0 && initiative && attack) {
			StartCoroutine (wait ());
		} else if (Head.phase % 2 == 1 && !initiative && attack) {
			StartCoroutine (wait ());
		}
	}

	float Search(bool isAI, int depth, float alpha, float beta){

		//ノードの評価値
		float value;

		//子ノードの評価値
		float childValue;

		int bestX = 0, bestY = 0;

		if (depth == 0) {
			return eval ();
		}

		//プレイヤーが打つときは自分のてくると仮定した上で、AIが打つときは自らの利益を最大にしたいので、
		//AIの手番では最小値、プレイヤーの手番では最大値をはじめに代入しておく
		value = (isAI) ? Mathf.NegativeInfinity : Mathf.Infinity;

		for (int x = 0; x < 5; x++) {
			for (int y = 0; y < 5; y++) {
				if (Head.stage [x, y] < 5) {
					int h = height;

					//石を置く
					field [Head.stage [x, y], x, y] = (isAI) ? -1 : 1;
					if (height < Head.stage [x, y]) {
						height = Head.stage [x, y];
					}

					//石を置ける高さを追加
					Head.stage [x, y] += 1;

					//再帰する
					childValue = Search (!isAI, depth - 1, alpha, beta);

					//
					if (isAI) {
						if (childValue > value) {
							value = childValue;
							alpha = value;
							bestX = x;
							bestY = y;
						}

						//βカット
						if (value > beta) {
							Head.stage [x, y] -= 1;
							height = h;
							field [Head.stage [x, y], x, y] = 0;
							return value;
						}
					} else {
						if (childValue < value) {
							value = childValue;
							beta = value;
							bestX = x;
							bestY = y;
						}

						//αカット
						if (value < alpha) {
							Head.stage [x, y] -= 1;
							height = h;
							field [Head.stage [x, y], x, y] = 0;
							return value;
						}
					}
					//打った石を元に戻す
					Head.stage [x, y] -= 1;
					height = h;
					field [Head.stage [x, y], x, y] = 0;
				}
			}
		}

		if (depth == searchDepth) {

			//ルートノードの時、石を打つ
			field [Head.stage [bestX, bestY], bestX, bestY] = -1;
			Head.SetStone (bestX, bestY);
			Debug.Log ("点数:" + eval () + " 高さ:" + Head.stonePos[0] + " 奥行き:" + Head.stonePos[1] + " 横:" + Head.stonePos[2] 
				+ " 石:" + field [Head.stage [bestX, bestY], bestX, bestY]);
			if (eval () == 500000) {
				Debug.Log ("YOU LOSE");
				return 0;
			}
			canSet += (Head.stage [bestX, bestY] == 4) ? 1 : 0;
			attack = true;

			return 0;

		} else {

			//子ノードから継承した値を返す
			return value;
		}
	}

	//count : 白と黒の石を数える    overlap : 白と黒が重複しているかどうか    exit : ループを抜けるか    down : 下に石があるか
	float eval(){
		value = 0;

		int setPos = 0;

		//[Todo]高さによるループの回数制限かける
		for (int i = 0; i < 3; i++) {
			for (int x = 0; x < 5; x++) {
				for (int y = 0; y < 5; y++) {

					if (i == 0 && x % 4 == 0 && y % 4 == 0) {
						if (slant (x, y, i, true) > 0) {
							return 500000;
						} else if (slant (x, y, i, true) < 0) {
							return -500000;
						}
					} else if(x % 4 == 0){
						if (slant (x, y, i, false) > 0) {
							return 500000;
						} else if (slant (x, y, i, false) < 0) {
							return -500000;
						}
					}
				
					int count = 0, overlap = 0;
					bool exit = false;

					for (int z = 0; z < 5; z++) {
						switch (i) {
						case 0:
							setPos = field [x, z, y];
							break;
						case 1:
							setPos = field [y, x, z];
							break;
						case 2:
							setPos = field [z, y, x];
							break;
						default:
							break;
						}

						if (check (setPos, overlap, exit) >= 100) {
							count = 0;
							exit = true;
						} else {
							overlap = check (setPos, overlap, exit);
							count += overlap;
						}

						if (exit) {
							break;
						}
					}

					if (count == 5) {
						return -500000;
					} else if (count == -5) {
						return 500000;
					}

					value += cal (count);
				}
			}
		}

		return value;
	}

	int slant(int x, int y, int judge, bool diagonal){
		int count = 0, overlap = 0;
		bool exit = false;

		if (diagonal) {
			if (x % 4 == 0 && y % 4 == 0) {
				int n = (x == 0) ? 1 : -1;
				int m = (y == 0) ? 1 : -1;

				for (int i = 0; i < 5; i++) {
					if (check (field [x, i, y], overlap, exit) >= 100) {
						count = 0;
						exit = true;
					} else {
						overlap = check (field [x, i, y], overlap, exit);
						count += overlap;
					}

					if (exit) {
						break;
					}

					x += n;
					y += m;
				}

				if (count == 5) {
					return -1;
				} else if (count == -5) {
					return 1;
				}

				value += cal (count);
			}
		} else {
			if (x % 4 == 0) {
				int n = (x == 0) ? 1 : -1;
				int setPos = 0;

				for (int i = 0; i < 5; i++) {
					switch (judge) {
					case 0:
						setPos = field [x, i, y];
						break;
					case 1:
						setPos = field [y, x, i];
						break;
					case 2:
						setPos = field [i, y, x];
						break;
					default:
						break;
					}

					if (check (setPos, overlap, exit) >= 100) {
						count = 0;
						exit = true;
					} else {
						overlap = check (setPos, overlap, exit);
						count += overlap;
					}

					if (exit) {
						break;
					}

					x += n;
				}

				if (count == 5) {
					return -1;
				} else if (count == -5) {
					return 1;
				}

				value += cal (count);
			}
		}

		return 0;
	}

	int check(int value, int overlap, bool exit){
		if (exit) {
			return 1000;
		} else {
			if (overlap == 0 || overlap == value) {
				return value;
			} else if (overlap == value * -1) {
				return 1000;
			}
		}
		return 0;
	}

	int cal(int value){
		if (value == 0)
			return 0;
		int n = (value > 0) ? -1 : 1;
		switch (Mathf.Abs (value) % 4) {
		case 0:
			value = 5000;
			break;
		case 1:
			value = 5;
			break;
		case 2:
			value = 50;
			break;
		case 3:
			value = 500;
			break;
		default:
			value = 5;
			break;
		}
		value *= n;
		return value;
	}

	//プレイヤーが置いてから少しして石を置く
	IEnumerator wait(){
		int stoneHeight = Head.stonePos [0];
		attack = false;
		height = (height < stoneHeight) ? stoneHeight : height;
		canSet += (stoneHeight == 4) ? 1 : 0;
		searchDepth = (canSet >= 10) ? 5 : 4;
		if (Head.phase > 0) {
			field [stoneHeight, Head.stonePos [1], Head.stonePos [2]] = 1;
			Debug.Log ("点数:" + eval () + " 高さ:" + stoneHeight + " 奥行き:" + Head.stonePos[1] + " 横:" + Head.stonePos[2] 
				+ " 石:" + field [stoneHeight, Head.stonePos [1], Head.stonePos [2]]);
			if (eval () == -500000) {
				Debug.Log ("YOU WIN");
			}
		}
		yield return new WaitForSeconds (2.0f);
		float a = Search (true, searchDepth, Mathf.NegativeInfinity, Mathf.Infinity);
	}
}
