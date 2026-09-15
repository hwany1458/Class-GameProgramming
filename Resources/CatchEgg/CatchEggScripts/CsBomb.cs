using UnityEngine;
using System.Collections;

public class CsBomb : MonoBehaviour {
	
	public Transform expSmall;		// 작은 불꽃 - 프리팹 
	public Transform expBig;		// 큰 불꽃 
	
	public AudioClip sndMiss;		// 작은 사운드 
	public AudioClip sndBomb;		// 폭파 사운드 
	
	float speed;					// 속도	
	
	//------------------------------
	// 폭탄 위치 초기화 
	//------------------------------
	void Start ()
	{
		SetPosition();
	}
	
	//------------------------------
	// 게임 루프 - 폭탄 이동 
	//------------------------------
	void Update ()
	{
		float amtMove = speed * Time.smoothDeltaTime;
		transform.Translate(Vector3.down * amtMove, Space.World);
	}
	
	//------------------------------
	// 폭탄의 충돌 처리
	//------------------------------
	void OnCollisionEnter (Collision coll)
	{
		// 바닥, 계란과의 충돌 
		if (coll.transform.tag == "GROUND" || coll.transform.tag == "EGG") {
			Instantiate(expSmall, transform.position, Quaternion.identity);
			AudioSource.PlayClipAtPoint(sndMiss, transform.position);

			if (coll.transform.tag == "EGG") {			// 계란과 충돌시 계란 제거 
				Destroy(coll.gameObject);
			}
		}
		
		// 바구니, 바구니 안쪽과의 충돌 
		if (coll.transform.tag == "BUCKET" || coll.transform.tag == "SAFE") {
			Instantiate(expBig, transform.position, Quaternion.identity);
			AudioSource.PlayClipAtPoint(sndBomb, transform.position);
			CsManager.isDead = true;
			
			Destroy(coll.transform.root.gameObject);	// 바구니 전체 제거 
		}
		Destroy(gameObject);							// 폭탄 제거 
	}
		
	//------------------------------
	// 폭탄 초기화 
	//------------------------------
	void SetPosition ()
	{
		speed = Random.Range(3, 5f);				// 속도 
		float x = Random.Range(-8.5f, 8.5f);		// 수평 위치 
		float y = Random.Range(12, 14f);			// 수직 위치 
		
		transform.position = new Vector3(x, y, -2);	// 폭탄의 초기 위치 
	}
} // end of class 
