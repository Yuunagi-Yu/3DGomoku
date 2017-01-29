using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;

public class Images : MonoBehaviour {
	public int vertical, horizontal, stack;

	private GameObject white, black;
	private int v, h;
	private bool stone = false;
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

		//Enumsと同値だと色を変更
		if (v >= 0 && h >= 0) {
			if (v == vertical && h == horizontal) {
				image.color = new Color (1, 0.22745f, 0.22745f, 0.1647f);
			} else {
				image.color = new Color (0.8f, 0.8f, 0.8f, 0.1647f);
			}
		} else {
			image.color = new Color (0.8f, 0.8f, 0.8f, 0.1647f);
		}

		if (!stone && Head.phase > 0) {
			if (vertical == Head.stonePos [1] && horizontal == Head.stonePos [2] && stack == Head.stonePos [0]) {
				if (Head.phase % 2 == 1) {
					stone = true;
					black.SetActive (true);
				} else if (Head.phase % 2 == 0) {
					stone = true;
					white.SetActive (true);
				}
			}
		}
	}
}
