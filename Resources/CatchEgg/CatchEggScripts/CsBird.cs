using UnityEngine;
using System.Collections;

public class CsBird : MonoBehaviour {
	
	public int cellCnt; 		// 전체 이미지 수 
	public int cellsPerSec;		// 1초에 표시할 이미지 수 
	
	float frmDelay;				// 이미지 표시 간격 
	int cellNum = 0; 			// 현재 표시할 이미지 번호 
	float cellOfs;				// 이미지의 Material Offset
	
	bool canNext = true;		// 다음 장면? 
	float speed;				// 이동 속도 
	int[] DIR = {1, -1};		// 이동 방향 (1: Left, -1: Right)
	int	dirX;					// dirX = 1 or -1
	
	//------------------------------
	// 새 위치 초기화
	//------------------------------
	void Start ()
	{
		SetPosition();
	}
	
	//------------------------------
	// 게임 루프 - 애니메이션 & 이동
	//------------------------------
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
	
	//------------------------------
	// 애니메이션 
	//------------------------------
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
	
	//------------------------------
	// 참새 위치 초기화
	//------------------------------
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
