using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Enums;

public class ChangeColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public int vertical, horizontal;
	private bool initiative;
	private int limit = 0;

	// Use this for initialization
	void Start () {
		initiative = Head.initiative;
	}
	
	// Update is called once per frame
	void Update () {
		if (Head.end) {
			Button button = GetComponent<Button> ();
			if (button != null) {
				button.enabled = false;
			}
			this.gameObject.GetComponent<ChangeColor> ().enabled = false;
		}
	}

	public void OnClick(){
		if (Head.phase % 2 == 1 && initiative) {
			return;
		} else if (Head.phase % 2 == 0 && !initiative) {
			return;
		}
		limit = Head.stage [vertical, horizontal];
		if (limit < 5) {
			Head.SetStone (vertical, horizontal);
		}
	}

	//カーソルが乗った時
	public void OnPointerEnter(PointerEventData eventData){
		Head.VERTICAL = vertical;
		Head.HORIZONTAL = horizontal;
	}

	//カーソルが外れた時
	public void OnPointerExit(PointerEventData eventData){
		Head.VERTICAL = -1;
		Head.HORIZONTAL = -1;
	}
}
