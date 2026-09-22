/**
 * @file    CsBird.cs
 * @brief   화면을 가로질러 날아가는 새의 스프라이트 애니메이션과 이동을 처리하는 스크립트
 * @details Bird1, Bird2 프리팹에 부착한다. 새는 배경 연출용으로 충돌 판정이 없으며
 *          게임 결과에 영향을 주지 않는다.
 * @author  게임콘텐츠학과 게임프로그래밍
 * @date    2026
 * @version 1.0
 */

using UnityEngine;
using System.Collections;

/// <summary>
/// 새 오브젝트의 애니메이션과 비행을 담당하는 클래스.
/// </summary>
/// <remarks>
/// <b>스프라이트 시트 애니메이션</b>
/// 여러 프레임이 가로로 이어진 한 장의 이미지를 머티리얼에 매핑하고,
/// Tiling x를 (1 / 프레임 수)로 설정해 한 프레임만 보이게 한 뒤
/// 텍스처 오프셋을 한 칸씩 옮겨 날갯짓을 표현한다.
///
/// | 프리팹 | cellCnt | cellsPerSec | Tiling x |
/// |--------|---------|-------------|----------|
/// | Bird1  | 6       | 18          | 0.1667   |
/// | Bird2  | 4       | 6           | 0.25     |
///
/// 새는 CsManager.MakeBirds()에 의해 생성되며, 화면 왼쪽 또는 오른쪽 바깥에서
/// 나타나 반대편으로 날아간 뒤 화면을 벗어나면 스스로 제거된다.
/// </remarks>
/// @see CsManager
public class CsBird : MonoBehaviour {
	
	public int cellCnt; 		///< 스프라이트 시트의 전체 프레임(이미지) 수. 인스펙터에서 설정
	public int cellsPerSec;		///< 1초에 표시할 프레임 수(애니메이션 속도). 인스펙터에서 설정
	
	float frmDelay;				///< 프레임 표시 간격(초) = 1 / cellsPerSec
	int cellNum = 0; 			///< 현재 표시 중인 프레임 번호 (0 ~ cellCnt-1)
	float cellOfs;				///< 프레임 한 칸에 해당하는 텍스처 오프셋 = 1 / cellCnt
	
	bool canNext = true;		///< 다음 프레임으로 넘어갈 수 있는지 여부 (프레임 간격 대기 중이면 false)
	float speed;				///< 이동 속도(유닛/초). 2 ~ 4 사이 무작위
	int[] DIR = {1, -1};		///< 이동 방향 후보 (1: 왼쪽에서 나타나 오른쪽으로, -1: 오른쪽에서 나타나 왼쪽으로)
	int	dirX;					///< 선택된 이동 방향 (1 또는 -1)
	
	/// <summary>
	/// 새가 생성될 때 한 번 호출되어 애니메이션 값, 방향, 위치를 초기화한다.
	/// </summary>
	void Start ()
	{
		SetPosition();
	}
	
	/// <summary>
	/// 매 프레임 애니메이션을 진행하고 새를 이동시키며, 화면을 벗어나면 제거한다.
	/// </summary>
	/// <remarks>
	/// - Animation() 코루틴을 호출해 프레임 간격이 지났으면 다음 프레임을 표시한다.
	/// - dirX 방향으로 speed만큼 월드 x축 이동한다.
	/// - |x| > 15 이면 화면 밖으로 나간 것으로 보고 오브젝트를 제거한다.
	/// </remarks>
	/// @note Animation()을 매 프레임 StartCoroutine()으로 호출하고 canNext로 중복을 막는 구조다.
	///       Start()에서 무한 반복 코루틴을 한 번만 시작하는 방식으로 바꾸면 호출 비용을 줄일 수 있다.
	void Update ()
	{
		StartCoroutine("Animation");

		// 새 이동 
		float amtMove = speed * Time.smoothDeltaTime * dirX;
		transform.Translate(Vector3.right * amtMove, Space.World);
				
		// 새가 화면을 벗어나면 제거 
		if (Mathf.Abs(transform.position.x) > 15) {
			Destroy(gameObject);
		}
	}
	
	/// <summary>
	/// 스프라이트 시트의 다음 프레임을 표시하고 프레임 간격만큼 대기하는 코루틴.
	/// </summary>
	/// <returns>frmDelay초 대기하는 IEnumerator</returns>
	/// <remarks>
	/// -# canNext가 true일 때만 실행하고, 즉시 false로 바꿔 중복 실행을 막는다.
	/// -# 프레임 번호를 1 증가시키고 Mathf.Repeat()로 0 ~ cellCnt-1 범위를 순환시킨다.
	/// -# 프레임 번호 × cellOfs 를 텍스처 오프셋으로 적용한다.
	/// -# frmDelay초 대기한 뒤 canNext를 true로 되돌린다.
	/// </remarks>
	IEnumerator  Animation ()
	{
		if (canNext) {
			canNext = false;
			
			cellNum = (int) Mathf.Repeat(++cellNum, cellCnt);
			float ofs = cellOfs * cellNum;	
			transform.GetComponent<Renderer>().material.mainTextureOffset = new Vector2(ofs, 0);
			
			yield return new WaitForSeconds(frmDelay);
			canNext = true;
		}
	}		
	
	/// <summary>
	/// 애니메이션 값, 이동 속도·방향, 이미지 방향, 시작 위치를 설정한다.
	/// </summary>
	/// <remarks>
	/// -# 프레임 간격(frmDelay)과 프레임 한 칸의 오프셋(cellOfs)을 계산한다.
	/// -# 속도(2 ~ 4)와 방향(dirX = 1 또는 -1)을 무작위로 정한다.
	/// -# 이동 방향에 맞게 이미지를 좌우 반전한다.
	///    - 방법 1: Transform의 localScale.x 부호 반전 (현재 주석 처리됨)
	///    - 방법 2: 머티리얼의 Tiling(mainTextureScale) x 부호 반전 (현재 사용)
	/// -# 시작 위치를 x = -13 × dirX (진행 방향 반대편 화면 밖), y = 6 ~ 9, z = 4로 정한다.
	/// </remarks>
	void SetPosition ()
	{
		frmDelay = 1.0f / cellsPerSec;		// 이미지 표시 간격  
		cellOfs = 1.0f / cellCnt;			// 표시할 이미지 위치 
		
		speed = Random.Range(2, 4f);		// 새의 이동 속도 
		dirX = DIR[Random.Range(0, 2)];		// 새의 이동 방향 
		
		// 이미지를 이동 방향으로 뒤집기 
		Vector3 scale = transform.localScale;	
		scale.x *= dirX; 
		// transform.localScale = scale;
		
		// 매트리얼로 뒤집기 
		Vector2 tiling = transform.GetComponent<Renderer>().material.mainTextureScale;
		tiling.x *= dirX;
		transform.GetComponent<Renderer>().material.mainTextureScale = new Vector2(tiling.x, 1);
		
		// 새의 초기 위치 설정 
		transform.position = new Vector3(-13 * dirX, Random.Range(6, 9f), 4);
	}	
} // end of class 
