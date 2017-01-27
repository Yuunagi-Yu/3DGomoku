using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enums;

public class Pillars : MonoBehaviour {
	public int vertical, horizontal;
	private int v, h;
	private GameObject child;

	// Use this for initialization
	void Start () {
		child = transform.FindChild ("PillarChild").gameObject;
		child.SetActive (false);
	}
	
	// Update is called once per frame
	void Update () {
		v = Enums.Head.VERTICAL;
		h = Enums.Head.HORIZONTAL;

		//Enumsの値と同値だと子オブジェクトをActiveに
		if (v >= 0 && h >= 0) {
			if (v == vertical && h == horizontal) {
				child.SetActive (true);
				return;
			} else {
				child.SetActive (false);
				return;
			}
		} else {
			child.SetActive (false);
		}
	}
}
