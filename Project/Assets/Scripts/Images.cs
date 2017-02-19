using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;

public class Images : MonoBehaviour {
	public int vertical, horizontal, stack;

	private GameObject white, black;
	private int v, h;
	private Image image;

	// Use this for initialization
	void Start () {
		white = transform.FindChild ("White").gameObject;
		black = transform.FindChild ("Black").gameObject;
		image = this.GetComponent<Image> ();
	}
	
	// Update is called once per frame
	void Update () {
		v = Head.VERTICAL;
		h = Head.HORIZONTAL;

		if (Head.end) {
			image.color = new Color (0.8f, 0.8f, 0.8f, 0.1647f);
			for (int i = 0; i < 5; i++) {
				if (stack == Head.lastPos [i, 0] && vertical == Head.lastPos [i, 1] && horizontal == Head.lastPos [i, 2]) {
					image.color = new Color (0.22745f, 1, 0.22745f, 0.6f);
				}
			}
			GetComponent<Images> ().enabled = false;
		} else {
			//Enumsと同値だと色を変更
			if (v == vertical && h == horizontal) {
				image.color = new Color (1, 0.22745f, 0.22745f, 0.1647f);
			} else {
				image.color = new Color (0.8f, 0.8f, 0.8f, 0.1647f);
			}
		}

		if (Head.phase > 0) {
			if (vertical == Head.stonePos [1] && horizontal == Head.stonePos [2] && stack == Head.stonePos [0]) {
				if (Head.phase % 2 == 1) {
					black.SetActive (true);
				} else if (Head.phase % 2 == 0) {
					white.SetActive (true);
				}
				image.color = new Color (0.22745f, 0.22745f, 1, 0.6f);
			}
		}
	}
}
