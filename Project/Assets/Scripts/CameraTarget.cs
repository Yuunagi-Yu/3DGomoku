using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraTarget : MonoBehaviour {
	public float speed, limit;
	private float move_x = 0.0f, move_y = 0.0f, angle;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton (1)) {
			move_x = Input.GetAxis ("Mouse X");
			move_y = Input.GetAxis ("Mouse Y");

			//このオブジェクトを回転
			if (move_x >= limit) {
				transform.Rotate (Vector3.up * Time.deltaTime * speed, Space.World);
			} else if (move_x <= -limit) {
				transform.Rotate (Vector3.up * Time.deltaTime * -speed, Space.World);
			}
			if (move_y >= limit) {
				transform.Rotate (Vector3.right * Time.deltaTime * -speed, Space.Self);
			} else if (move_y <= -limit) {
				transform.Rotate (Vector3.right * Time.deltaTime * speed, Space.Self);
			}
		}
	}

	void LateUpdate () {
		//角度制限
		angle = transform.eulerAngles.x;
		angle = Mathf.Clamp (angle, 10, 80);
		transform.eulerAngles = new Vector3 (angle, transform.eulerAngles.y, transform.eulerAngles.z);
	}
}
