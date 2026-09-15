using UnityEngine;
using System.Collections;

public class CsBucket : MonoBehaviour {

	//------------------------------
	// 게임 루프 
	//------------------------------
	void Update ()
	{
		if (!CsManager.isDead && Input.GetButton("Fire1")) {
			MoveBucket();
		}
	}
	
	//------------------------------
	// 바구니 이동 
	//------------------------------
	void MoveBucket () {
		
		Vector3 mousePos = Input.mousePosition;		// 마우스 위치 
		mousePos.z = 24;							// 카메라로부터의 거리 
		
		// 마우스 좌표를 월드(Global) 좌표로 변환 
		Vector3 bucketPos = Camera.main.ScreenToWorldPoint(mousePos);
		
		// 바구니의 이동 범위 제한 
		bucketPos.x = Mathf.Clamp(bucketPos.x, -8.5f, 8.5f);
		bucketPos.y = 0;
		bucketPos.z = -2;
		
		transform.position = bucketPos;		// 바구니 이동 
		
		
	}
} // end of class 
