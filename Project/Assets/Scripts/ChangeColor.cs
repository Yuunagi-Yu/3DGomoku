using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Enums;

public class ChangeColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public int vertical, horizontal;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	//カーソルが乗った時
	public void OnPointerEnter(PointerEventData eventData){
		Enums.Head.VERTICAL = vertical;
		Enums.Head.HORIZONTAL = horizontal;
	}

	//カーソルが外れた時
	public void OnPointerExit(PointerEventData eventData){
		Enums.Head.VERTICAL = -1;
		Enums.Head.HORIZONTAL = -1;
	}
}
