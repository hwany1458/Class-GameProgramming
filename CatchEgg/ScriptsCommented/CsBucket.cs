/**
 * @file    CsBucket.cs
 * @brief   마우스(또는 터치) 입력으로 바구니를 좌우로 움직이는 스크립트
 * @details 바구니 오브젝트에 부착한다. PC에서는 마우스 왼쪽 버튼,
 *          모바일에서는 터치를 누르고 있는 동안 바구니가 포인터 위치를 따라간다.
 * @author  게임콘텐츠학과 게임프로그래밍
 * @date    2026
 * @version 1.0
 */

using UnityEngine;
using System.Collections;

/// <summary>
/// 플레이어가 조작하는 바구니의 이동을 담당하는 클래스.
/// </summary>
/// <remarks>
/// 바구니는 좌우(x축)로만 움직이며, y = 0, z = -2에 고정된다.
/// 바구니의 충돌 판정은 이 스크립트가 아니라 태그로 구분된 콜라이더가 담당한다.
/// - <b>BUCKET</b>: 바구니 본체(Mesh Collider) — 폭탄 충돌 대상
/// - <b>SAFE</b>: 바구니 안쪽 빈 오브젝트(Box Collider) — 계란이 들어왔는지 감지
/// </remarks>
/// @see CsEgg, CsBomb
public class CsBucket : MonoBehaviour {

	/// <summary>
	/// 매 프레임 입력을 확인해 바구니를 이동시킨다.
	/// </summary>
	/// <remarks>
	/// 게임 오버가 아니고 "Fire1" 버튼을 누르고 있을 때만 MoveBucket()을 호출한다.
	/// "Fire1"은 Input Manager 기본 설정에서 마우스 왼쪽 버튼과 왼쪽 Ctrl 키이며,
	/// 모바일에서는 화면 터치가 마우스 왼쪽 버튼으로 처리된다.
	/// </remarks>
	void Update ()
	{
		if (!CsManager.isDead && Input.GetButton("Fire1")) {
			MoveBucket();
		}
	}
	
	/// <summary>
	/// 마우스(터치) 화면 좌표를 월드 좌표로 바꿔 바구니를 그 위치로 옮긴다.
	/// </summary>
	/// <remarks>
	/// -# Input.mousePosition으로 화면 좌표(픽셀)를 얻는다.
	/// -# z에 카메라로부터의 거리 24를 넣는다.
	///    (Main Camera z = -26, 바구니 z = -2 이므로 거리 24)
	/// -# Camera.main.ScreenToWorldPoint()로 월드 좌표로 변환한다.
	/// -# x는 -8.5 ~ 8.5로 제한(Mathf.Clamp)해 화면 밖으로 나가지 않게 하고,
	///    y와 z는 고정값으로 덮어쓴다.
	/// </remarks>
	/// @note 가속·감속 없이 포인터 위치로 즉시 이동(순간이동)하므로 반응은 빠르지만,
	///       이동 경로상의 폭탄을 건너뛸 수 있다. 이동 속도를 두려면
	///       Vector3.MoveTowards() 등을 사용한다.
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
