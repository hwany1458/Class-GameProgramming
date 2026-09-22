/**
 * @file    CsSky.cs
 * @brief   배경 하늘의 텍스처를 흘려 구름이 움직이는 효과를 내는 스크립트
 * @details 스테이지의 Sky(Plane) 오브젝트에 부착한다.
 * @author  게임콘텐츠학과 게임프로그래밍
 * @date    2026
 * @version 1.0
 */

using UnityEngine;
using System.Collections;

/// <summary>
/// 배경 하늘을 스크롤하는 클래스.
/// </summary>
/// <remarks>
/// 배경을 스크롤하는 방법은 두 가지가 있다.
/// -# 배경 오브젝트 자체를 이동 — 화면을 벗어나면 원위치로 되돌리는 처리가 필요하다.
/// -# 오브젝트는 고정하고 머티리얼의 텍스처 오프셋을 이동 — 구현이 간단하다.
///
/// 이 스크립트는 2번 방식을 사용한다. 텍스처의 Wrap Mode가 Repeat이면
/// 오프셋이 1을 넘어도 이미지가 끊김 없이 반복된다.
/// </remarks>
public class CsSky : MonoBehaviour {

	float speed = 0.05f;		///< 스크롤 속도 (초당 텍스처 오프셋 증가량)
	
	/// <summary>
	/// 경과 시간에 비례해 텍스처의 가로 오프셋을 바꿔 하늘을 흐르게 한다.
	/// </summary>
	/// <remarks>
	/// 오프셋 = speed × Time.time 으로 계산하므로 누적 오차 없이 일정한 속도로 움직인다.
	/// 구름은 오른쪽에서 왼쪽으로 이동한다.
	/// </remarks>
	void Update () {
		float ofs = speed * Time.time;
		transform.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(ofs, 0);
	}
} // end of class 
