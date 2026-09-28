using UnityEngine;
using System.Collections;

public class CsEgg : MonoBehaviour {
	
	public AudioClip sndCatch;		// 성공 사운드 
	public AudioClip sndMiss;		// 실패 사운드 
	
	float speed;
	
	//------------------------------
	// 계란 위치 초기화
	//------------------------------
	void Start ()
	{
		SetPosition();
	}
	
	//------------------------------
	// 게임 루프 - 계란 이동
	//------------------------------
	void Update ()
	{
		float amtMove = speed * Time.smoothDeltaTime;
		transform.Translate(Vector3.down * amtMove, Space.World);
	}
	
	//------------------------------
	// 계란의 충돌 처리
	//------------------------------
	void OnCollisionEnter (Collision coll)
	{
		switch (coll.transform.tag) {
		case "SAFE" :					// 바구니 안쪽 
			CsManager.hit++;
			AudioSource.PlayClipAtPoint(sndCatch, transform.position);
			Destroy(gameObject);		// 계란 제거 
			break;
		case "GROUND" : 				// 그라운드 
			CsManager.miss++;
			AudioSource.PlayClipAtPoint(sndMiss, transform.position);
			if (CsManager.miss >= 5) {
				CsManager.isDead = true;
				Destroy(GameObject.Find("Bucket"));		// 바구니 제거 
			}
			StartCoroutine("EggBroken");
			break;
		}
	}
		
	//------------------------------
	// 깨진 계란 표시
	//------------------------------
	IEnumerator EggBroken ()
	{
		// 계란 텍스쳐를 깨진 계란 이미지로 대치 
		transform.GetComponent<Renderer>().material.mainTexture = (Texture)Resources.Load("egg_broken");
		
		// 계란 텍스처의 투명도를 낮춰서 화면에서 사라지는 효과 처리 
		for (float i = 1f; i >= 0f; i -= 0.1f) {
			Color color = new Vector4(1, 1, 1, i);			// i = Alpha  
			transform.GetComponent<Renderer>().material.color = color;
			yield return 0;									// 1프레임 양보 
		}	
		
		Destroy(gameObject);								// 계란 제거 
	}
	
	//------------------------------
	// 계란 초기화
	//------------------------------
	void SetPosition ()
	{
		speed = Random.Range(3, 5f);			// 속도 
		
		float x = Random.Range(-8.5f, 8.5f);	// 수평 위치 
		float y = Random.Range(12, 14f);		// 수직 위치	 
		transform.position = new Vector3(x, y, -2);		// 초기 위치 
		
		// 계란을 z축으로 -30~+30도 회전 
		transform.eulerAngles = new Vector3(0, 0, Random.Range(-30, 30));
	}
} // end of class 
