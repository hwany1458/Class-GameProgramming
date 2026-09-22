/**
 * @file    CsManager.cs
 * @brief   계란받기 게임의 전체 흐름을 제어하는 게임 매니저 스크립트
 * @details 게임 초기화, 계란·폭탄·새의 주기적 생성, 난이도(생성 간격) 조절,
 *          배경음악 전환, 점수 표시 GUI, 게임 오버 후 재시작·종료를 담당한다.
 *          씬의 빈 오브젝트 <b>GameManager</b>에 부착한다.
 * @author  게임콘텐츠학과 게임프로그래밍
 * @date    2026
 * @version 1.0
 */

using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// 게임 전체를 관리하는 게임 매니저 클래스.
/// </summary>
/// <remarks>
/// "게임매니저" 개념에 따라 이 클래스는 오브젝트의 <b>생성</b>만 담당하고,
/// 생성된 계란·폭탄·새는 각자의 스크립트(CsEgg, CsBomb, CsBird)에서
/// 위치·속도 초기화, 이동, 충돌을 스스로 처리한다.
///
/// 게임 상태(hit, miss, isDead)는 <c>static</c> 변수로 선언되어
/// 다른 스크립트에서 <c>CsManager.변수</c> 형식으로 직접 참조·변경한다.
///
/// <b>게임 흐름</b>
/// -# Start() → InitStage() : 변수 초기화, 배경음악 재생
/// -# Update() : 게임 오버 전까지 계란·폭탄 생성, 새는 항상 생성
/// -# OnGUI() : 점수·시간 표시, 게임 오버 시 음악 교체 및 버튼 표시
/// </remarks>
/// @see CsEgg, CsBomb, CsBird, CsBucket
public class CsManager : MonoBehaviour 
{
    //---- Variables ----------------------

    public Transform egg;				///< 계란 프리팹
	public Transform bomb;				///< 폭탄 프리팹
	public Transform bird1;				///< 새1 프리팹 (6프레임 스프라이트)
	public Transform bird2;				///< 새2 프리팹 (4프레임 스프라이트)
	public GUISkin skin;				///< 점수·버튼 표시에 사용할 GUI Skin (글꼴 포함)
	
	public AudioClip sndBack;			///< 플레이 중 반복 재생되는 배경 음악
	public AudioClip sndOver;			///< 게임 오버 시 1회 재생되는 음악
	
	/// <summary>받은(성공한) 계란 수.</summary>
	/// <remarks>CsEgg에서 바구니 안쪽(SAFE) 충돌 시 증가한다. InitStage()에서 0으로 초기화.</remarks>
	static public int hit = 0;
	
	/// <summary>놓친(실패한) 계란 수.</summary>
	/// <remarks>CsEgg에서 바닥(GROUND) 충돌 시 증가하며, 5 이상이면 게임 오버.</remarks>
	static public int miss = 0;
	
	/// <summary>게임 오버 여부.</summary>
	/// <remarks>
	/// 폭탄이 바구니에 닿거나(CsBomb) 계란을 5개 놓치면(CsEgg) true가 된다.
	/// true가 되면 계란·폭탄 생성과 바구니 이동이 멈추고 게임 오버 화면이 표시된다.
	/// </remarks>
	static public bool isDead = false;
	
	float EGG_DELAY = 2.5f;				///< 계란 생성 간격(초). 생성할 때마다 ×0.98, 범위 0.5 ~ 2.5
	float BOMB_DELAY = 5;				///< 폭탄 생성 간격(초). 생성할 때마다 ×0.99, 범위 0.9 ~ 5
	float BIRD1_DELAY = 15;				///< 새1 생성 간격(초), 고정값
	float BIRD2_DELAY = 20;				///< 새2 생성 간격(초), 고정값
	
	float eggDelay;						///< 다음 계란 생성까지 남은 시간(초)
	float bombDelay;					///< 다음 폭탄 생성까지 남은 시간(초)
	float bird1Delay;					///< 다음 새1 생성까지 남은 시간(초)
	float bird2Delay;					///< 다음 새2 생성까지 남은 시간(초)
	
	float startTime;					///< 게임 시작 시각 (Time.time 기준)
	float stageTime;                    ///< 마지막으로 갱신된 현재 시각. 게임 오버 시 갱신이 멈춰 경과 시간이 고정된다

    //--- Methods ----------------------------

    /// <summary>
    /// 게임 시작 시 한 번 호출되어 화면 설정과 게임 초기화를 수행한다.
    /// </summary>
    /// <remarks>
    /// - 화면을 가로 방향(LandscapeLeft)으로 고정한다. (모바일 대응)
    /// - 플레이 중 화면이 꺼지지 않도록 설정한다.
    /// - InitStage()를 호출해 게임 변수를 초기화한다.
    /// </remarks>
    void Start ()
	{
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		InitStage();
	}
	
	/// <summary>
	/// 매 프레임 호출되는 게임 루프.
	/// </summary>
	/// <remarks>
	/// - ESC 키를 누르면 언제든 애플리케이션을 종료한다.
	/// - 게임 오버 전(isDead == false)에는 현재 시각을 갱신하고 계란·폭탄을 생성한다.
	/// - 새는 게임에 영향을 주지 않으므로 게임 오버 후에도 계속 생성한다.
	/// </remarks>
	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
        {  // ESC 키를 누르면 게임 종료
            Application.Quit();
		}
		
		if (!isDead) 
		{
			stageTime = Time.time;		// 현재 시각 읽기 
			MakeEgg();					// 계란 만들기	
			MakeBomb();					// 폭탄 만들기 
		}
		
		MakeBirds();					// 새만들기 
	}

    //--- User Defined Methods ----------------------------

    /// <summary>
    /// 생성 간격이 지나면 계란을 하나 생성하고, 다음 생성 간격을 줄인다.
    /// </summary>
    /// <remarks>
    /// 생성할 때마다 간격에 0.98을 곱해 게임이 진행될수록 계란이 자주 나온다.
    /// 간격은 최소 0.5초에서 멈추며, 계산상 약 80개 생성(약 100초) 후 최소값에 도달한다.
    /// 계란의 위치와 속도는 생성된 계란이 CsEgg.SetPosition()에서 스스로 정한다.
    /// </remarks>
    void MakeEgg ()
	{
		eggDelay -= Time.deltaTime;		// 지연시간 계산 
		
		if (eggDelay <= 0) {
			Instantiate(egg);			// 계란 만들기 
			
			// 지연시간 감소 
			EGG_DELAY = Mathf.Clamp(EGG_DELAY * 0.98f, 0.5f, 2.5f);
			eggDelay = EGG_DELAY;
		}
	}
	
	/// <summary>
	/// 생성 간격이 지나면 폭탄을 하나 생성하고, 다음 생성 간격을 줄인다.
	/// </summary>
	/// <remarks>
	/// 생성할 때마다 간격에 0.99를 곱하며 최소 0.9초에서 멈춘다.
	/// 계란보다 천천히 빨라져 약 170개 생성(약 410초) 후 최소값에 도달한다.
	/// </remarks>
	void MakeBomb ()
	{
		bombDelay -= Time.deltaTime;	// 지연시간 계산 
		
		if (bombDelay <= 0) {
			Instantiate(bomb);			// 폭탄 만들기 
			
			// 지연시간 감소 
			BOMB_DELAY = Mathf.Clamp(BOMB_DELAY * 0.99f, 0.9f, 5);
			bombDelay = BOMB_DELAY;
		}
	}
	
	/// <summary>
	/// 정해진 간격마다 새1(15초), 새2(20초)를 생성한다.
	/// </summary>
	/// <remarks>
	/// 새는 배경 연출용으로 난이도와 무관하게 고정 간격으로 생성되며,
	/// 게임 오버 후에도 계속 생성된다.
	/// </remarks>
	void MakeBirds ()
	{
		bird1Delay -= Time.deltaTime;
		if (bird1Delay <= 0) {
			Instantiate(bird1);
			bird1Delay = BIRD1_DELAY;
		}
		
		bird2Delay -= Time.deltaTime;
		if (bird2Delay <= 0) {
			Instantiate(bird2);
			bird2Delay = BIRD2_DELAY;
		}
	}
	
	
	/// <summary>
	/// 게임 상태 변수와 생성 타이머를 초기화하고 배경음악을 재생한다.
	/// </summary>
	/// <remarks>
	/// static 변수(hit, miss, isDead)는 씬을 다시 불러와도 값이 유지되므로,
	/// 재시작 시 반드시 여기에서 초기화해야 한다.
	/// 배경음악은 Main Camera의 AudioSource를 통해 반복 재생한다.
	/// </remarks>
	void InitStage ()
	{
		// 변수 초기화 
		startTime = Time.time;
		isDead = false;
		hit = 0;
		miss = 0;
		
		// 프리팹 표시 시간 설정 
		eggDelay = EGG_DELAY;
		bombDelay = BOMB_DELAY;
		bird1Delay = BIRD1_DELAY;
		bird2Delay = BIRD2_DELAY;
		
		// 배경음악 설정 
		Camera.main.GetComponent<AudioSource>().clip = sndBack;
		Camera.main.GetComponent<AudioSource>().loop = true;
		Camera.main.GetComponent<AudioSource>().Play();
	}
	
	/// <summary>
	/// 점수·시간을 화면에 표시하고, 게임 오버 시 음악 교체와 버튼 처리를 한다.
	/// </summary>
	/// <remarks>
	/// <b>화면 배치</b>
	/// - 좌상단: 계란 아이콘 + 받은 계란 수(남색)
	/// - 중앙 상단: 경과 시간(녹색, 초 단위 정수)
	/// - 우상단: 깨진 계란 아이콘 + 놓친 계란 수(빨강)
	///
	/// <b>게임 오버 시</b>
	/// - 배경음악을 게임 오버 음악으로 한 번만 교체한다(현재 클립 비교로 중복 재생 방지).
	/// - "Play Game" 버튼: 씬 MakingCatchEggScene을 다시 불러와 재시작한다.
	/// - "Quit Game" 버튼: 애플리케이션을 종료한다.
	///
	/// 글자 색과 크기는 리치 텍스트 태그(&lt;color&gt;, &lt;size&gt;)로 지정한다.
	/// </remarks>
	/// @note OnGUI()는 한 프레임에 여러 번 호출될 수 있다. 저장·효과음처럼 한 번만
	///       실행해야 하는 처리를 여기에 넣을 때는 플래그 변수로 중복을 막아야 한다.
	/// @todo 아이콘을 매 호출마다 Resources.Load()로 불러오므로, Start()에서 한 번 로드해 캐싱하도록 개선.
	/// @todo 레거시 IMGUI(OnGUI) 대신 uGUI·TextMeshPro 기반 UI로 전환 검토.
	void OnGUI ()
	{
		GUI.skin = skin;						// GUI Skin 설정 
		
		int w = Screen.width / 2;				// 화면의 중심 
		int h = Screen.height / 2;				// 화면의 중심 
		float time = stageTime - startTime;		// 게임 진행시간 계산 
		
		// 글자색과 크기 설정 
		string sHit = "<color='navy'><size='40'>" + hit + "</size></color>";
		string sMiss = "<color='red'><size='40'>" + miss + "</size></color>";
		string sTime = "<color='#006600'><size='32'>Time " + (int)time + "</size></color>";
		
		// 계란 이미지와 개수 표시 
		GUI.DrawTexture(new Rect(20, 25, 40, 32), Resources.Load("egg_icon") as Texture);
		GUI.Label(new Rect(75, 20, 60, 40), sHit);
		
		GUI.Label(new Rect(w - 40, 20, 160, 40), sTime);
		
		// 깨진계란 이미지와 개수 표시 
		GUI.DrawTexture(new Rect(w * 2 - 100, 25, 40, 32), Resources.Load("egg_broken") as Texture);
		GUI.Label(new Rect(w * 2 - 45, 20, 60, 40), sMiss);
		
		if (isDead) {
			// 게임오버 음악 설정 
			if (Camera.main.GetComponent<AudioSource>().clip != sndOver) {
				Camera.main.GetComponent<AudioSource>().clip = sndOver;
				Camera.main.GetComponent<AudioSource>().loop = false;
				Camera.main.GetComponent<AudioSource>().Play();
			}	
		
			// 버튼 표시 및 처리 
			if (GUI.Button(new Rect(w - 70, h - 50, 140, 60), "Play Game")) {
				//Application.LoadLevel("MakingCatchEggScene");	
				SceneManager.LoadScene("MakingCatchEggScene");
            }	
			
			if (GUI.Button(new Rect(w - 70, h + 50, 140, 60), "Quit Game")) {
				Application.Quit();	
			}	
		}
	}

}
