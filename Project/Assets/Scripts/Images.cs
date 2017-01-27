using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Enums;

public class Images : MonoBehaviour {
	public int vertical, horizontal;
	private int v, h;
	private Image image;

	// Use this for initialization
	void Start () {
		image = this.GetComponent<Image> ();
	}
	
	// Update is called once per frame
	void Update () {
		v = Enums.Head.VERTICAL;
		h = Enums.Head.HORIZONTAL;

		//Enumsと同値だと色を変更
		if (v >= 0 && h >= 0) {
			if (v == vertical && h == horizontal) {
				image.color = new Color (1, 0.22745f, 0.22745f, 0.1647f);
				return;
			} else {
				image.color = new Color (0.8f, 0.8f, 0.8f, 0.1647f);
				return;
			}
		} else {
			image.color = new Color (0.8f, 0.8f, 0.8f, 0.1647f);
		}
	}
}
