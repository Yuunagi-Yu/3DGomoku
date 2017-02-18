using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class EnemyAI1 : MonoBehaviour {
	public int searchDepth;

	private int[,,] field = new int[5, 5, 5];
	private int[,] winPos = new int[5, 3];
	private int height = 0, canSet = 0;
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
			/*Debug.Log ("点数:" + eval () + " 高さ:" + Head.stonePos[0] + " 奥行き:" + Head.stonePos[1] + " 横:" + Head.stonePos[2] 
				+ " 石:" + field [Head.stage [bestX, bestY], bestX, bestY]);*/
			Head.SetStone (bestX, bestY);
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
		float value = 0;

		for (int z = 0; z <= height; z++) {

			int overlap3 = 0, overlap4 = 0;
			int count3 = 0, count4 = 0;
			int down3 = 0, down4 = 0;
			bool exit3 = false, exit4 = false;

			for (int x = 0; x < 5; x++) {

				int overlap1 = 0, overlap2 = 0;
				int count1 = 0, count2 = 0;
				int down1 = 0, down2 = 0;
				bool exit1 = false, exit2 = false;

				//1段ごとの横1列と縦1列の検証
				for (int y = 0; y < 5; y++) {

					//横1列の検証
					if (check (field [z, y, x], overlap1, exit1) >= 100) {
						count1 = 0;
						exit1 = true;
					} else {
						overlap1 = check (field [z, y, x], overlap1, exit1);
						if (z > 0 && overlap1 == 0 && field [z - 1, y, x] == 0) {
							down1++;
						}
						count1 += overlap1;
					}

					//奥行き1列の検証
					if (check (field [z, x, y], overlap2, exit2) >= 100) {
						count2 = 0;
						exit2 = true;
					} else {
						overlap2 = check (field [z, x, y], overlap2, exit2);
						if (z > 0 && overlap2 == 0 && field [z - 1, x, y] == 0) {
							down2++;
						}
						count2 += overlap2;
					}

					//白と黒の被りがあった時、ループを抜ける
					if (exit1 && exit2) {
						break;
					}
				}

				//斜めの検証
				if (check (field [z, x, x], overlap3, exit3) >= 100) {
					count3 = 0;
					exit3 = true;
				} else {
					overlap3 = check (field [z, x, x], overlap3, exit3);
					if (z > 0 && overlap3 == 0 && field [z - 1, x, x] == 0) {
						down3++;
					}
					count3 += overlap3;
				}
				if (check (field [z, 4 - x, x], overlap4, exit4) >= 100) {
					count4 = 0;
					exit4 = true;
				} else {
					overlap4 = check (field [z, 4 - x, x], overlap4, exit4);
					if (z > 0 && overlap4 == 0 && field [z - 1, 4 - x, x] == 0) {
						down4++;
					}
					count4 += overlap4;
				}

				//どちらかが勝利した時、ループを抜ける
				if (count1 == 5 || count2 == 5) {
					return -500000;
				} else if (count1 == -5 || count2 == -5) {
					return 500000;
				}


				if (exit3 && exit4) {
					break;
				}

				//評価値の追加
				value += cal(count1) / 5 * (5 - down1) + cal(count2) / 5 * (5 - down2);
			}

			//どちらかが勝利した時、ループを抜ける
			if (count3 == 5 || count4 == 5) {
				return -500000;
			} else if (count3 == -5 || count4 == -5) {
				return 500000;
			}

			value += cal (count3) / 5 * (5 - down3) + cal (count4) / 5 * (5 - down4);
		}

		for (int x = 0; x < 5; x++) {
			for (int y = 0; y < 5; y++) {

				int count = 0, overlap = 0;
				bool exit = false;

				for (int z = 0; z <= height; z++) {

					//縦1列の検証
					if (check (field [z, y, x], overlap, exit) >= 100) {
						count = 0;
						exit = true;
					} else {
						overlap = check (field [z, y, x], overlap, exit);
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

		for (int x = 0; x < 5; x++) {

			int count1 = 0, count2 = 0, count3 = 0, count4 = 0;
			int overlap1 = 0, overlap2 = 0, overlap3 = 0, overlap4 = 0;
			int down1 = 0, down2 = 0, down3 = 0, down4 = 0;
			bool exit1 = false, exit2 = false, exit3 = false, exit4 = false;

			for (int z = 0; z <= height; z++) {

				//縦横の斜め1列の検証
				if (check (field [z, x, z], overlap1, exit1) >= 100) {
					count1 = 0;
					exit1 = true;
				} else {
					overlap1 = check (field [z, x, z], overlap1, exit1);
					if (z > 0 && overlap1 == 0 && field [z - 1, x, z] == 0) {
						down1++;
					}
					count1 += overlap1;
				}
				if (check (field [z, x, 4 - z], overlap2, exit2) >= 100) {
					count2 = 0;
					exit2 = true;
				} else {
					overlap2 = check (field [z, x, 4 - z], overlap2, exit2);
					if (z > 0 && overlap2 == 0 && field [z - 1, x, 4 - z] == 0) {
						down2++;
					}
					count2 += overlap2;
				}

				//縦奥行きの斜め1列の検証
				if (check (field [z, z, x], overlap3, exit3) >= 100) {
					count3 = 0;
					exit3 = true;
				} else {
					overlap3 = check (field [z, z, x], overlap3, exit3);
					if (z > 0 && overlap3 == 0 && field [z - 1, z, x] == 0) {
						down3++;
					}
					count3 += overlap3;
				}
				if (check (field [z, 4 - z, x], overlap4, exit4) >= 100) {
					count4 = 0;
					exit4 = true;
				} else {
					overlap4 = check (field [z, 4 - z, x], overlap4, exit4);
					if (z > 0 && overlap4 == 0 && field [z - 1, 4 - z, x] == 0) {
						down4++;
					}
					count4 += overlap4;
				}

				if (exit1 && exit2 && exit3 && exit4) {
					break;
				}
			}

			if (count1 == 5 || count2 == 5 || count3 == 5 || count4 == 5) {
				return -500000;
			} else if (count1 == -5 || count2 == -5 || count3 == -5 || count4 == -5) {
				return 500000;
			}

			value += cal (count1) / 5 * (5 - down1) + cal (count2) / 5 * (5 - down2) + cal (count3) / 5 * (5 - down3) + cal (count4) / 5 * (5 - down4);
		}


		//対角線の1列の検証
		int _count1 = 0, _count2 = 0, _count3 = 0, _count4 = 0;
		int _overlap1 = 0, _overlap2 = 0, _overlap3 = 0, _overlap4 = 0;
		int _down1 = 0, _down2 = 0, _down3 = 0, _down4 = 0;
		bool _exit1 = false, _exit2 = false, _exit3 = false, _exit4 = false;

		for (int z = 0; z <= height; z++) {
			if (check (field [z, z, z], _overlap1, _exit1) >= 100) {
				_count1 = 0;
				_exit1 = true;
			} else {
				_overlap1 = check (field [z, z, z], _overlap1, _exit1);
				if (z > 0 && _overlap1 == 0 && field [z - 1, z, z] == 0) {
					_down1++;
				}
				_count1 += _overlap1;
			}
			if (check (field [z, 4 - z, z], _overlap2, _exit2) >= 100) {
				_count2 = 0;
				_exit2 = true;
			} else {
				_overlap2 = check (field [z, 4 - z, z], _overlap2, _exit2);
				if (z > 0 && _overlap2 == 0 && field [z - 1, 4 - z, z] == 0) {
					_down2++;
				}
				_count2 += _overlap2;
			}
			if (check (field [z, z, 4 - z], _overlap3, _exit3) >= 100) {
				_count3 = 0;
				_exit3 = true;
			} else {
				_overlap3 = check (field [z, z, 4 - z], _overlap3, _exit3);
				if (z > 0 && _overlap3 == 0 && field [z - 1, z, 4 - z] == 0) {
					_down3++;
				}
				_count3 += _overlap3;
			}
			if (check (field [z, 4 - z, 4 - z], _overlap4, _exit4) >= 100) {
				_count4 = 0;
				_exit4 = true;
			} else {
				_overlap4 = check (field [z, 4 - z, 4 - z], _overlap4, _exit4);
				if (z > 0 && _overlap4 == 0 && field [z - 1, 4 - z, 4 - z] == 0) {
					_down4++;
				}
				_count4 += _overlap4;
			}

			if (_exit1 && _exit2 && _exit3 && _exit4) {
				break;
			}
		}

		if (_count1 == 5 || _count2 == 5 || _count3 == 5 || _count4 == 5) {
			return -500000;
		} else if (_count1 == -5 || _count2 == -5 || _count3 == -5 || _count4 == -5) {
			return 500000;
		}

		value += cal (_count1) / 5 * (5 - _down1) + cal (_count2) / 5 * (5 - _down2) + cal (_count3) / 5 * (5 - _down3) + cal (_count4) / 5 * (5 - _down4);

		return value;
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
			/*Debug.Log ("点数:" + eval () + " 高さ:" + stoneHeight + " 奥行き:" + Head.stonePos[1] + " 横:" + Head.stonePos[2] 
				+ " 石:" + field [stoneHeight, Head.stonePos [1], Head.stonePos [2]]);*/
			if (eval () == -500000) {
				Debug.Log ("YOU WIN");
			}
		}
		yield return new WaitForSeconds (2.0f);
		float a = Search (true, searchDepth, Mathf.NegativeInfinity, Mathf.Infinity);
	}
}