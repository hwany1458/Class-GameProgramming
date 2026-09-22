/**
 * @file    CsEgg.cs
 * @brief   떨어지는 계란의 이동, 충돌 판정, 깨짐 연출을 처리하는 스크립트
 * @details 계란 프리팹에 부착한다. 계란은 CsManager.MakeEgg()에 의해 생성되며,
 *          생성 직후 스스로 위치·속도·기울기를 무작위로 정한 뒤 아래로 떨어진다.
 * @author  게임콘텐츠학과 게임프로그래밍
 * @date    2026
 * @version 1.0
 */

using UnityEngine;
using System.Collections;

/// <summary>
/// 계란 오브젝트의 동작을 담당하는 클래스.
/// </summary>
/// <remarks>
/// <b>충돌 규칙</b> (충돌 대상의 태그로 구분)
/// | 태그   | 대상            | 처리                                              |
/// |--------|-----------------|---------------------------------------------------|
/// | SAFE   | 바구니 안쪽     | hit +1, 성공음, 계란 제거                         |
/// | GROUND | 바닥            | miss +1, 실패음, 깨진 계란 연출 후 제거, miss ≥ 5면 게임 오버 |
///
/// 바구니 테두리(BUCKET)와의 충돌은 별도 처리하지 않는다.
///
/// <b>필요 컴포넌트</b>: Collider, Rigidbody(Use Gravity 해제),
/// 투명 셰이더(Legacy Transparent/Diffuse) 머티리얼, 태그 "EGG"
/// </remarks>
/// @see CsManager, CsBomb
public class CsEgg : MonoBehaviour {
	
	public AudioClip sndCatch;		///< 계란을 받았을 때(성공) 재생할 효과음
	public AudioClip sndMiss;		///< 계란이 바닥에 떨어졌을 때(실패) 재생할 효과음
	
	float speed;					///< 낙하 속도(유닛/초). SetPosition()에서 3 ~ 5 사이 무작위로 정해진다
	
	/// <summary>
	/// 계란이 생성될 때 한 번 호출되어 위치와 속도를 초기화한다.
	/// </summary>
	void Start ()
	{
		SetPosition();
	}
	
	/// <summary>
	/// 매 프레임 계란을 월드 좌표 기준 아래 방향으로 이동시킨다.
	/// </summary>
	/// <remarks>
	/// 계란이 기울어져 있어도 똑바로 떨어지도록 Space.World 기준으로 이동한다.
	/// Time.smoothDeltaTime을 사용해 프레임 간 이동량 변동을 완화한다.
	/// </remarks>
	void Update ()
	{
		float amtMove = speed * Time.smoothDeltaTime;
		transform.Translate(Vector3.down * amtMove, Space.World);
	}
	
	/// <summary>
	/// 계란이 다른 콜라이더와 충돌했을 때 태그에 따라 성공·실패를 처리한다.
	/// </summary>
	/// <param name="coll">충돌 정보. <c>coll.transform.tag</c>로 충돌 대상을 구분한다.</param>
	/// <remarks>
	/// - <b>SAFE</b>: 받은 수(CsManager.hit)를 늘리고 성공음을 재생한 뒤 계란을 제거한다.
	/// - <b>GROUND</b>: 놓친 수(CsManager.miss)를 늘리고 실패음을 재생한다.
	///   놓친 수가 5 이상이면 게임 오버로 전환하고 바구니를 제거한다.
	///   이후 EggBroken() 코루틴으로 깨진 계란을 보여 주며 사라지게 한다.
	/// </remarks>
	/// @warning GameObject.Find("Bucket")은 계층 창의 이름으로 바구니를 찾는다.
	///          프리팹 루트 이름이 "Bucket"이 아니면(예: "바구니") 자식 오브젝트만 제거되거나
	///          찾지 못할 수 있다. CsBomb처럼 transform.root를 제거하는 방식과 비교해 볼 것.
	/// @note 게임 오버 후에도 이미 떨어지던 계란이 바닥에 닿으면 miss가 계속 증가한다.
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
		
	/// <summary>
	/// 계란을 깨진 모양으로 바꾸고 점점 투명하게 만든 뒤 제거하는 코루틴.
	/// </summary>
	/// <returns>프레임 단위로 실행을 양보하는 IEnumerator</returns>
	/// <remarks>
	/// -# Resources 폴더의 "egg_broken" 이미지로 텍스처를 교체한다.
	/// -# 머티리얼 색의 알파값을 1.0에서 0.1씩 낮추며 매 프레임 적용한다(약 10프레임).
	/// -# 완전히 투명해지면 계란 오브젝트를 제거한다.
	///
	/// 알파값이 화면에 반영되려면 머티리얼이 투명 셰이더를 사용해야 한다.
	/// 유니티 색상은 RGBA 형식이므로 Vector4(R, G, B, A)로 만들어 Color에 대입한다.
	/// </remarks>
	/// @note 사라지는 속도가 프레임 수 기준이므로 기기의 프레임레이트에 따라 달라진다.
	///       일정한 시간으로 만들려면 Time.deltaTime 기반으로 알파값을 줄인다.
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
	
	/// <summary>
	/// 계란의 낙하 속도, 시작 위치, 기울기를 무작위로 설정한다.
	/// </summary>
	/// <remarks>
	/// | 항목 | 값 |
	/// |------|----|
	/// | 낙하 속도 | 3 ~ 5 유닛/초 |
	/// | x 위치 | -8.5 ~ 8.5 (바구니 이동 범위와 동일) |
	/// | y 위치 | 12 ~ 14 (화면 위쪽 바깥) |
	/// | z 위치 | -2 (바구니와 같은 평면) |
	/// | z축 회전 | -30° ~ 30° (시각적 다양성, 판정에는 영향 없음) |
	/// </remarks>
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
