using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class EnemyAI : MonoBehaviour {
	public int searchDepth;

	private int[,,] field = new int[5,5,5];
	private int height = 0;
	private long p = 0;
	private bool initiative = false, attack = true;

	// Use this for initialization
	void Start () {
		initiative = !Head.initiative;
	}
	
	// Update is called once per frame
	void Update () {
		if (Head.phase % 2 == 0 && initiative && attack) {
			attack = false;
			if (height < Head.stonePos [0]) {
				height = Head.stonePos [0];
			}
			field [Head.stonePos [0], Head.stonePos [1], Head.stonePos [2]] = -1;
			StartCoroutine (wait ());
		} else if (Head.phase % 2 == 1 && !initiative && attack) {
			attack = false;
			if (height < Head.stonePos [0]) {
				height = Head.stonePos [0];
			}
			field [Head.stonePos [0], Head.stonePos [1], Head.stonePos [2]] = 1;
			Debug.Log (eval ());
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
						if (value >= beta) {
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
						if (value <= alpha) {
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
			Debug.Log (eval ());
			attack = true;

			return 0;

		} else {

			//子ノードから継承した値を返す
			return value;
		}
	}

	float eval(){
		float value = 0;

		for (int z = 0; z <= height; z++) {
			//白と黒が重複しているか
			int overlap3 = 0, overlap4 = 0;

			//白と黒の数を数える
			int count3 = 0, count4 = 0;

			//数えるか
			bool exit3 = false, exit4 = false;

			for (int x = 0; x < 5; x++) {

				//白と黒が重複しているか
				int overlap1 = 0, overlap2 = 0;

				//白と黒の数を数える
				int count1 = 0, count2 = 0;

				//ループを抜けるか
				bool exit1 = false, exit2 = false;

				//1段ごとの横1列と縦1列の検証
				for (int y = 0; y < 5; y++) {

					//横1列の検証
					if (field [z, y, x] != 0 && !exit1) {
						int result = check (field [z, y, x], overlap1);
						if (result == 0) {
							count1 = 0;
							exit1 = true;
						} else {
							overlap1 = result;
							count1 += result;
						}
					}

					//奥行き1列の検証
					if (field [z, x, y] != 0 && !exit2) {
						int result = check (field [z, x, y], overlap2);
						if (result == 0) {
							count2 = 0;
							exit2 = true;
						} else {
							overlap2 = result;
							count2 += result;
						}
					}

					//白と黒の被りがあった時、ループを抜ける
					if (exit1 && exit2) {
						break;
					}
				}

				//斜めの検証
				if (field [z, x, x] != 0 && !exit3) {
					int result = check (field [z, x, x], overlap3);
					if (result == 0) {
						count3 = 0;
						exit3 = true;
					} else {
						overlap3 = result;
						count3 += result;
					}
				}
					
				if (field [z, 4 - x, x] != 0 && !exit4) {
					int result = check (field [z, 4 - x, x], overlap4);
					if (result == 0) {
						count4 = 0;
						exit4 = true;
					} else {
						overlap4 = result;
						count4 += result;
					}
				}

				//どちらかが勝利した時、ループを抜ける
				if (count1 == 5 || count2 == 5) {
					value += -10000;
				} else if (count1 == -5 || count2 == -5) {
					value += 10000;
				}

				//評価値の追加
				value += cal(count1) + cal(count2);
			}

			//どちらかが勝利した時、ループを抜ける
			if (count3 == 5 || count4 == 5) {
				value += -10000;
			} else if (count3 == -5 || count4 == -5) {
				value += 10000;
			}

			value += cal (count3) + cal (count4);
		}

		for (int x = 0; x < 5; x++) {
			for (int y = 0; y < 5; y++) {
				int count = 0, overlap = 0;
				bool exit = false;
				for (int z = 0; z <= height; z++) {

					//縦1列の検証
					if (field [z, y, x] != 0 && !exit) {
						int result = check (field [z, y, x], overlap);
						if (result == 0) {
							count = 0;
							exit = true;
						} else {
							overlap = result;
							count += result;
						}
					}

					if (exit) {
						break;
					}
				}

				if (count == 5) {
					value += -10000;
				} else if (count == -5) {
					value += 10000;
				}

				value += cal (count);
			}
		}

		for (int x = 0; x < 5; x++) {
			int count1 = 0, count2 = 0, count3 = 0, count4 = 0;
			int overlap1 = 0, overlap2 = 0, overlap3 = 0, overlap4 = 0;
			bool exit1 = false, exit2 = false, exit3 = false, exit4 = false;
			for (int z = 0; z <= height; z++) {
				//縦横の斜め1列の検証
				if (field [z, x, z] != 0 && !exit1) {
					int result = check (field [z, x, z], overlap1);
					if (result == 0) {
						count1 = 0;
						exit1 = true;
					} else {
						overlap1 = result;
						count1 += result;
					}
				}
				if (field [z, x, 4 - z] != 0 && !exit2) {
					int result = check (field [z, x, 4 - z], overlap2);
					if (result == 0) {
						count2 = 0;
						exit2 = true;
					} else {
						overlap2 = result;
						count2 += result;
					}
				}

				//縦奥行きの斜め1列の検証
				if (field [z, z, x] != 0 && !exit3) {
					int result = check (field [z, z, x], overlap3);
					if (result == 0) {
						count3 = 0;
						exit3 = true;
					} else {
						overlap3 = result;
						count3 += result;
					}
				}
				if (field [z, 4 - z, x] != 0 && !exit4) {
					int result = check (field [z, 4 - z, x], overlap4);
					if (result == 0) {
						count4 = 0;
						exit4 = true;
					} else {
						overlap4 = result;
						count4 += result;
					}
				}

				if (exit1 && exit2 && exit3 && exit4) {
					break;
				}
			}

			if (count1 == 5 || count2 == 5 || count3 == 5 || count4 == 5) {
				value += -10000;
			} else if (count1 == -5 || count2 == -5 || count3 == -5 || count4 == -5) {
				value += 10000;
			}

			value += cal (count1) + cal (count2) + cal (count3) + cal (count4);
		}


		int _count1 = 0, _count2 = 0, _count3 = 0, _count4 = 0;
		int _overlap1 = 0, _overlap2 = 0, _overlap3 = 0, _overlap4 = 0;
		bool _exit1 = false, _exit2 = false, _exit3 = false, _exit4 = false;
		for (int z = 0; z <= height; z++) {
			
			if (field [z, z, z] != 0 && !_exit1) {
				int result = check (field [z, z, z], _overlap1);
				if (result == 0) {
					_count1 = 0;
					_exit1 = true;
				} else {
					_overlap1 = result;
					_count1 += result;
				}
			}
			if (field [z, 4 - z, z] != 0 && !_exit2) {
				int result = check (field [z, 4 - z, z], _overlap2);
				if (result == 0) {
					_count2 = 0;
					_exit2 = true;
				} else {
					_overlap2 = result;
					_count2 += result;
				}
			}
			if (field [z, z, 4 - z] != 0 && !_exit3) {
				int result = check (field [z, z, 4 - z], _overlap3);
				if (result == 0) {
					_count3 = 0;
					_exit3 = true;
				} else {
					_overlap3 = result;
					_count3 += result;
				}
			}
			if (field [z, 4 - z, 4 - z] != 0 && !_exit4) {
				int result = check (field [z, 4 - z, 4 - z], _overlap4);
				if (result == 0) {
					_count4 = 0;
					_exit4 = true;
				} else {
					_overlap4 = result;
					_count4 += result;
				}
			}

			if (_exit1 && _exit2 && _exit3 && _exit4) {
				break;
			}
		}

		if (_count1 == 5 || _count2 == 5 || _count3 == 5 || _count4 == 5) {
			value += -10000;
		} else if (_count1 == -5 || _count2 == -5 || _count3 == -5 || _count4 == -5) {
			value += 10000;
		}

		value += cal (_count1) + cal (_count2) + cal (_count3) + cal (_count4);

		return value;
	}

	int check(int value, int overlap){
		if (value == 1) {
			if (overlap == 0 || overlap == 1) {
				return 1;
			} else if (overlap == -1) {
				return 0;
			}
		} else if (value == -1) {
			if (overlap == 0 || overlap == -1) {
				return -1;
			} else if (overlap == 1) {
				return 0;
			}
		}
		return 0;
	}

	int cal(int value){
		if (value > 0) {
			switch (value % 4) {
			case 0:
				value = -100;
				break;
			case 1:
				value = -1;
				break;
			case 2:
				value = -2;
				break;
			case 3:
				value = -10;
				break;
			default:
				value = -1;
				break;
			}
		} else if (value < 0) {
			value *= -1;
			switch (value % 4) {
			case 0:
				value = 100;
				break;
			case 1:
				value = 1;
				break;
			case 2:
				value = 2;
				break;
			case 3:
				value = 10;
				break;
			default:
				value = 1;
				break;
			}
		}
		return value;
	}

	//プレイヤーが置いてから少しして石を置く
	IEnumerator wait(){
		yield return new WaitForSeconds (2.0f);
		float a = Search (true, searchDepth, Mathf.NegativeInfinity, Mathf.Infinity);
	}
}
