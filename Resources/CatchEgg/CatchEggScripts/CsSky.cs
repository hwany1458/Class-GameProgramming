using UnityEngine;
using System.Collections;

public class CsSky : MonoBehaviour {

	float speed = 0.05f;		// 스크롤 속도
	
	//------------------------------
	// 배경화면 스크롤
	//------------------------------
	void Update () {
		float ofs = speed * Time.time;
		transform.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(ofs, 0);
	}
} // end of class 
