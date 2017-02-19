using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class EnemyAI : MonoBehaviour {
	public GameObject winText, loseText, buttons;

	private int searchDepth = 0;
	private int[,,] field = new int[5, 5, 5];
	private int[,] winPos = new int[5, 3];
	private int height = 0, canSet = 0;
	private float value = 0;
	private bool initiative = false, attack = true;

	// Use this for initialization
	void Start () {
		searchDepth = Head.depth;
		initiative = !Head.initiative;
		winText.SetActive (false);
		loseText.SetActive (false);
		buttons.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		if (Head.phase % 2 == 0 && initiative && attack) {
			StartCoroutine (wait ());
		} else if (Head.phase % 2 == 1 && !initiative && attack) {
			StartCoroutine (wait ());
		}
	}

	float Search(bool test, bool isAI, int depth, float alpha, float beta){

		//ノードの評価値
		float value;

		//子ノードの評価値
		float childValue;

		int bestX = 0, bestY = 0;

		if (depth == 0) {
			return eval (false);
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
					childValue = Search (false, !isAI, depth - 1, alpha, beta);

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

		if (depth == 1 && test) {
			if (value == 500000) {
				Debug.Log ("オケーイ");
				finish (-1, bestX, bestY);
				return 0;
			} else {
				return 100;
			}
			return 100;
		}

		if (depth == searchDepth) {

			finish (-1, bestX, bestY);

			return 0;

		} else {

			//子ノードから継承した値を返す
			return value;
		}
	}

	//count : 白と黒の石を数える    overlap : 白と黒が重複しているかどうか    exit : ループを抜けるか    down : 下に石があるか
	float eval(bool isResult){
		value = 0;

		int setPos = 0;
		int x_limit = 5, y_limit = 5, z_limit = 5;

		//[Todo]高さによるループの回数制限かける
		for (int i = 0; i < 3; i++) {
			switch (i) {
			case 0:
				x_limit = height + 1;
				break;
			case 1:
				x_limit = 5;
				y_limit = height + 1;
				break;
			case 2:
				y_limit = 5;
				z_limit = height + 1;
				break;
			default:
				break;
			}

			for (int x = 0; x < x_limit; x++) {
				for (int y = 0; y < y_limit; y++) {

					int v = 0;
					if (i == 2) {
						v = slant (x, y, i, true, isResult);
						if (v != 0) {
							return returnValue (v);
						}
						v = slant (x, y, 1, false, isResult);
						if (v != 0) {
							return returnValue (v);
						}
						v = slant (x, y, 2, false, isResult);
						if (v != 0) {
							return returnValue (v);
						}
					} else if (i == 1) {
						v = slant (x, y, 0, false, isResult);
						if (v != 0) {
							return returnValue (v);
						}
					}
				
					int count = 0, overlap = 0;
					bool exit = false;

					for (int z = 0; z < z_limit; z++) {
						switch (i) {
						case 0:
							setPos = field [x, z, y];
							if (isResult) {
								setWinPos (z, x, z, y);
							}
							break;
						case 1:
							setPos = field [y, x, z];
							if (isResult) {
								setWinPos (z, y, x, z);
							}
							break;
						case 2:
							setPos = field [z, y, x];
							if (isResult) {
								setWinPos (z, z, y, x);
							}
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

					if (returnValue (count) != 0) {
						return returnValue (count);
					}

					value += cal (count);
				}
			}
		}

		return value;
	}

	int slant(int x, int y, int judge, bool diagonal, bool isResult){
		int count = 0, overlap = 0;
		bool exit = false;

		if (diagonal) {
			if (x % 4 == 0 && y % 4 == 0) {
				int n = (x == 0) ? 1 : -1;
				int m = (y == 0) ? 1 : -1;

				for (int i = 0; i < height + 1; i++) {
					if (isResult) {
						setWinPos (i, i, y, x);
					}
					if (check (field [i, y, x], overlap, exit) >= 100) {
						count = 0;
						exit = true;
					} else {
						overlap = check (field [i, y, x], overlap, exit);
						count += overlap;
					}

					if (exit) {
						break;
					}

					x += n;
					y += m;
				}

				if (count == 5) {
					return 5;
				} else if (count == -5) {
					return -5;
				}

				value += cal (count);
			}
		} else {
			if (x % 4 == 0 || y % 4 == 0) {
				int n = 0, m = 0;
				if (judge < 2) {
					if (x % 4 != 0) {
						return 0;
					}
					n = (x == 0) ? 1 : -1;
				} else {
					if (y % 4 != 0) {
						return 0;
					}
					m = (y == 0) ? 1 : -1;
				}
				int setPos = 0;

				for (int i = 0; i < 5; i++) {
					switch (judge) {
					case 0:
						setPos = field [y, x, i];
						if (isResult) {
							setWinPos (i, y, x, i);
						}
						break;
					case 1:
						setPos = field [i, x, y];
						if (isResult) {
							setWinPos (i, i, x, y);
						}
						break;
					case 2:
						setPos = field [i, x, y];
						if (isResult) {
							setWinPos (i, i, x, y);
						}
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
					y += m;
				}

				if (count == 5) {
					return 5;
				} else if (count == -5) {
					return -5;
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

	void setWinPos(int n, int x, int y, int z){
		winPos [n, 0] = x;
		winPos [n, 1] = y;
		winPos [n, 2] = z;
	}

	float returnValue(int n){
		if (n == 5) {
			return -500000;
		} else if (n == -5) {
			return 500000;
		}
		return 0;
	}

	//プレイヤーが置いてから少しして石を置く
	IEnumerator wait(){
		finish (1, 0, 0);
		yield return new WaitForSeconds (2.0f);
		float a = Search (true, true, 1, Mathf.NegativeInfinity, Mathf.Infinity);
		if (a != 0) {
			a = Search (false, true, searchDepth, Mathf.NegativeInfinity, Mathf.Infinity);
		}
	}

	void finish(int isPlayer, int x, int y){
		if (isPlayer < 0) {
			Head.SetStone (x, y);
		}
		int stoneHeight = Head.stonePos [0];
		height = (height < stoneHeight) ? stoneHeight : height;
		canSet += (stoneHeight == 4) ? 1 : 0;
		if (canSet >= 11) {
			searchDepth += 1;
			canSet -= 5;
		}
		if (Head.phase > 0) {
			field [stoneHeight, Head.stonePos [1], Head.stonePos [2]] = isPlayer;
			if (eval (true) == -500000 * isPlayer) {
				for (int i = 0; i < 5; i++) {
					Head.indicate (winPos [i, 0], winPos [i, 1], winPos [i, 2]);
				}
				Head.end = true;
				if (isPlayer > 0) {
					winText.SetActive (true);
				} else {
					loseText.SetActive (true);
				}
				buttons.SetActive (true);
				this.gameObject.SetActive (false);
			}
		}
		attack = (isPlayer > 0) ? false : true;
	}
}
