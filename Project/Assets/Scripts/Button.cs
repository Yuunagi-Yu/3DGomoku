using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Enums;

public class Button : MonoBehaviour {
	public string sceneName;
	public int level;

	private GameObject toggleObj;

	// Use this for initialization
	void Start () {
		toggleObj = GameObject.Find ("Toggle");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnClick(){
		if (level != 0) {
			Head.depth = level;
			if (toggleObj != null) {
				Toggle toggle = toggleObj.GetComponent<Toggle> ();
				Head.initiative = toggle.isOn;
			}
		}
		Head.VERTICAL = -1;
		Head.HORIZONTAL = -1;
		Head.phase = 0;
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 5; j++) {
				Head.stage [i, j] = 0;
				if (j < 3) {
					Head.lastPos [i, j] = 0;
					Head.stonePos [j] = 0;
				}
			}
		}
		Head.end = false;
		SceneManager.LoadScene (sceneName);
	}
}
