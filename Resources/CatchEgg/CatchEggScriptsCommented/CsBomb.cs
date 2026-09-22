/**
 * @file    CsBomb.cs
 * @brief   떨어지는 폭탄의 이동과 충돌·폭발을 처리하는 스크립트
 * @details 폭탄 프리팹에 부착한다. 폭탄은 CsManager.MakeBomb()에 의해 생성되며,
 *          무엇이든 닿으면 폭발 이펙트를 남기고 사라진다. 바구니에 닿으면 즉시 게임 오버.
 * @author  게임콘텐츠학과 게임프로그래밍
 * @date    2026
 * @version 1.0
 */

using UnityEngine;
using System.Collections;

/// <summary>
/// 폭탄 오브젝트의 동작을 담당하는 클래스.
/// </summary>
/// <remarks>
/// <b>충돌 규칙</b> (충돌 대상의 태그로 구분)
/// | 태그          | 대상                | 처리                                               |
/// |---------------|---------------------|----------------------------------------------------|
/// | GROUND        | 바닥                | 작은 폭발, 작은 폭발음                             |
/// | EGG           | 떨어지는 계란       | 작은 폭발, 작은 폭발음, 계란 제거 (miss로 세지 않음) |
/// | BUCKET / SAFE | 바구니 / 바구니 안쪽 | 큰 폭발, 폭파음, 바구니 전체 제거, 게임 오버       |
///
/// 어떤 대상과 충돌하든 마지막에 폭탄 자신은 제거된다.
///
/// <b>필요 컴포넌트</b>: Sphere Collider, Rigidbody(Use Gravity 해제),
/// 도화선의 Sparks 파티클(자식 오브젝트)
/// </remarks>
/// @see CsManager, CsEgg
public class CsBomb : MonoBehaviour {
	
	public Transform expSmall;		///< 작은 폭발 이펙트 프리팹 (바닥·계란과 충돌 시)
	public Transform expBig;		///< 큰 폭발 이펙트 프리팹 (바구니와 충돌 시)
	
	public AudioClip sndMiss;		///< 바닥·계란과 충돌할 때 재생할 작은 폭발음
	public AudioClip sndBomb;		///< 바구니와 충돌할 때 재생할 폭파음
	
	float speed;					///< 낙하 속도(유닛/초). SetPosition()에서 3 ~ 5 사이 무작위로 정해진다
	
	/// <summary>
	/// 폭탄이 생성될 때 한 번 호출되어 위치와 속도를 초기화한다.
	/// </summary>
	void Start ()
	{
		SetPosition();
	}
	
	/// <summary>
	/// 매 프레임 폭탄을 월드 좌표 기준 아래 방향으로 이동시킨다.
	/// </summary>
	void Update ()
	{
		float amtMove = speed * Time.smoothDeltaTime;
		transform.Translate(Vector3.down * amtMove, Space.World);
	}
	
	/// <summary>
	/// 폭탄이 다른 콜라이더와 충돌했을 때 대상에 따라 폭발을 처리한다.
	/// </summary>
	/// <param name="coll">충돌 정보. <c>coll.transform.tag</c>로 충돌 대상을 구분한다.</param>
	/// <remarks>
	/// - <b>GROUND, EGG</b>: 작은 폭발 이펙트를 생성하고 작은 폭발음을 재생한다.
	///   계란과 충돌한 경우 계란도 함께 제거한다.
	/// - <b>BUCKET, SAFE</b>: 큰 폭발 이펙트와 폭파음을 재생하고,
	///   CsManager.isDead를 true로 바꿔 게임 오버로 전환한다.
	///   <c>coll.transform.root</c>를 제거하므로 충돌한 부분이 자식 오브젝트여도
	///   바구니 전체가 제거된다.
	/// - 모든 경우 마지막에 폭탄 자신을 제거한다.
	///
	/// 효과음은 AudioSource.PlayClipAtPoint()로 재생하므로 폭탄이 바로 제거되어도
	/// 소리가 끊기지 않는다.
	/// </remarks>
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
		
	/// <summary>
	/// 폭탄의 낙하 속도와 시작 위치를 무작위로 설정한다.
	/// </summary>
	/// <remarks>
	/// | 항목 | 값 |
	/// |------|----|
	/// | 낙하 속도 | 3 ~ 5 유닛/초 |
	/// | x 위치 | -8.5 ~ 8.5 |
	/// | y 위치 | 12 ~ 14 |
	/// | z 위치 | -2 (계란·바구니와 같은 평면이라 서로 충돌할 수 있음) |
	/// </remarks>
	void SetPosition ()
	{
		speed = Random.Range(3, 5f);				// 속도 
		float x = Random.Range(-8.5f, 8.5f);		// 수평 위치 
		float y = Random.Range(12, 14f);			// 수직 위치 
		
		transform.position = new Vector3(x, y, -2);	// 폭탄의 초기 위치 
	}
} // end of class 
