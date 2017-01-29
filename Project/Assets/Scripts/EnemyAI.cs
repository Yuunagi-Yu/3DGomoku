using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class EnemyAI : MonoBehaviour {
	public int searchDepth;

	private int[,,] field = new int[5,5,5];
	private int height = 0, n = 0;
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
		/*if (n != Head.phase) {
			if (n % 2 == 0) {
				field [Head.stonePos [0], Head.stonePos [1], Head.stonePos [2]] = 1;
			} else {
				field [Head.stonePos [0], Head.stonePos [1], Head.stonePos [2]] = -1;
			}
			if (height < Head.stonePos [0]) {
				height = Head.stonePos [0];
			}
			n = Head.phase;
			StartCoroutine (wait ());
		}*/
	}

	int Search(bool isAI, int depth, int alpha, int beta){

		//ノードの評価値
		int value;

		//子ノードの評価値
		int childValue;

		int bestX = 0, bestY = 0;

		if (depth == 0) {
			return eval ();
		}

		//プレイヤーが打つときは自分のてくると仮定した上で、AIが打つときは自らの利益を最大にしたいので、
		//AIの手番では最小値、プレイヤーの手番では最大値をはじめに代入しておく
		value = (isAI) ? -1000 : 1000;

		for (int x = 0; x < 5; x++) {
			for (int y = 0; y < 5; y++) {
				if (Head.stage [x, y] < 5) {

					//石を返す
					field [Head.stage [x, y], x, y] = (isAI) ? -1 : 1;

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
							field [Head.stage [x, y], x, y] = 0;
							return value;
						}
					} else if (!isAI) {
						if (childValue < value) {
							value = childValue;
							beta = value;
							bestX = x;
							bestY = y;
						}

						//αカット
						if (childValue < alpha) {
							Head.stage [x, y] -= 1;
							field [Head.stage [x, y], x, y] = 0;
							return value;
						}
					}

					//打った石を元に戻す
					Head.stage [x, y] -= 1;
					field [Head.stage [x, y], x, y] = 0;
				}
			}
		}
			

		if (depth == searchDepth) {

			//ルートノードの時、石を打つ
			field [Head.stage [bestX, bestY], bestX, bestY] = -1;
			Head.SetStone (bestX, bestY);
			attack = true;
			return 0;

		} else {

			//子ノードから継承した値を返す
			return value;
		}
	}

	int eval(){
		int value = 0;
		bool win = false;
		bool lose = false;

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

					//縦1列の検証
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
					lose = true;
					break;
				} else if (count1 == -5 || count2 == -5) {
					win = true;
					break;
				}

				//評価値の追加
				value += cal(count1) + cal(count2);
			}

			//どちらかが勝利した時、ループを抜ける
			if (count3 == 5 || count4 == 5) {
				lose = true;
			} else if (count3 == -5 || count4 == -5) {
				win = true;
			}

			//どちらかが勝利している時、ループを抜ける
			if (win || lose) {
				break;
			}

			value += cal (count3) + cal (count4);
		}

		for (int x = 0; x < 5; x++) {
			for (int y = 0; y < 5; y++) {
				int count = 0, overlap = 0;
				bool exit = false;
				for (int z = 0; z <= height; z++) {
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
				}

				value += cal (count);
			}
		}

		if (win) {
			return 1000;
		} else if (lose) {
			return -1000;
		} else {
			return value;
		}
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
			int n = 1;
			for(int i = 1; i <= value; i++){
				n *= i;
			}
			value = -n;
		} else if (value < 0) {
			int n = 1;
			for(int i = 1; i <= -value; i++){
				n *= i;
			}
			value = n;
		}
		return value;
	}

	//プレイヤーが置いてから少しして石を置く
	IEnumerator wait(){
		yield return new WaitForSeconds (1.8f);
		int a = Search (true, searchDepth, -1000, 1000);
		Debug.Log (eval ());
	}
}
