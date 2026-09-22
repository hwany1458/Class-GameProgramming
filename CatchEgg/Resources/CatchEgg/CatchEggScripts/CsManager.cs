using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class CsManager : MonoBehaviour 
{
    //---- Variables ----------------------

    public Transform egg;				// 계란 - 프리팹 
	public Transform bomb;				// 폭탄 
	public Transform bird1;				// 새1 
	public Transform bird2;				// 새 
	public GUISkin skin;				// GUI Skin 
	
	public AudioClip sndBack;			// 배경 음악 
	public AudioClip sndOver;			// 게임오버 음악 
	
	static public int hit = 0;			// 성공한 개수 
	static public int miss = 0;			// 실패한 개수 
	static public bool isDead = false;	// 게임끝?
	
	float EGG_DELAY = 2.5f;				// 계란 표시 간격 초깃값 
	float BOMB_DELAY = 5;				// 폭탄 표시 간격	초깃값 
	float BIRD1_DELAY = 15;				// 새1 표시 간격 초깃값 
	float BIRD2_DELAY = 20;				// 새2 표시 간격 초깃값	 
	
	float eggDelay;						// 계란 표시 간격	
	float bombDelay;					// 폭탄 표시 간격 
	float bird1Delay;					// 새1 표시 간격 
	float bird2Delay;					// 새2 표시 간격 
	
	float startTime;					// 게임 시작 시각 
	float stageTime;                    // 게임 진행 시각 

    //--- Methods ----------------------------

    //------------------------------
    // 게임 초기화 
    //------------------------------
    void Start ()
	{
		Screen.orientation = ScreenOrientation.LandscapeLeft;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
		InitStage();
	}
	
	//------------------------------
	// 게임 루프 
	//------------------------------
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

    //------------------------------
    // 계란 만들기 
    //------------------------------
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
	
	//------------------------------
	// 폭탄 만들기 
	//------------------------------
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
	
	//------------------------------
	// 새 만들기
	//------------------------------
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
	
	
	//------------------------------
	// 게임 초기화 
	//------------------------------
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
	
	//------------------------------
	// Score 등 화면표시
	//------------------------------
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
