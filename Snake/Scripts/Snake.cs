/**
 * @file    Snake.cs
 * @brief   스네이크(지렁이) 게임의 플레이어 머리 제어 및 게임 전체 관리 스크립트
 * @details 머리(Head) 오브젝트에 부착한다. 머리는 항상 앞으로 나아가며 좌우 입력으로 방향만 바꾼다.
 *          코인을 먹으면 꼬리가 하나 늘어나고, 벽이나 자신의 꼬리에 부딪히면 게임이 끝난다.
 *          플레이어 조작, 꼬리 관리, UI 표시, 게임 오버 처리를 한 클래스에서 담당한다.
 * @author  게임콘텐츠학과 게임프로그래밍
 * @date    2026
 * @version 1.0
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro; // TextMeshPro를 사용하려면 이 네임스페이스를 추가해야 한다

/// <summary>
/// 스네이크 게임의 플레이어(머리)와 게임 진행을 담당하는 클래스.
/// </summary>
/// <remarks>
/// <b>게임 규칙</b>
/// - 머리는 멈추지 않고 계속 앞으로 나아간다. 플레이어는 좌우 회전만 조작한다.
/// - 코인(Coin)을 먹으면 점수가 오르고 꼬리가 하나 늘어난다. 코인은 다른 곳에 다시 나타난다.
/// - 벽(Wall)이나 자신의 꼬리(Tail)에 부딪히면 게임 오버.
///
/// <b>충돌 태그</b>
/// | 태그  | 대상   | 처리                          |
/// |-------|--------|-------------------------------|
/// | Coin  | 코인   | 점수 +1, 코인 이동, 꼬리 추가 |
/// | Tail  | 꼬리   | 게임 오버 (Wall과 같은 처리)  |
/// | Wall  | 벽     | 게임 오버                     |
///
/// <b>조작</b>
/// - PC : 키보드 좌우 (Input.GetAxis("Horizontal"))
/// - 모바일 : 화면의 가상 조이스틱 (Joystick)
///
/// <b>씬에 있어야 하는 오브젝트</b> (이름으로 찾으므로 이름이 정확해야 함)
/// Coin, PanelOver, TxtCoin, TxtTime, PanelStick
/// </remarks>
/// @see Joystick
public class Snake : MonoBehaviour
{
    float speedMove = 3f;   ///< 전진 속도 (유닛/초)
    float speedRot = 120f;  ///< 회전 속도 (도/초)

    bool isDead = false;    ///< 게임 오버 여부. true가 되면 이동·꼬리·시간 갱신이 모두 멈춘다

    Transform coin;         ///< 코인 오브젝트 (먹을 때마다 위치만 옮겨 재사용)

    List<Transform> tails = new List<Transform>();   ///< 꼬리 목록. 앞에 있을수록 머리에 가깝다

    // UI
    GameObject panelOver;   ///< 게임 오버 패널 (평소에는 숨겨 둠)
    TMP_Text txtCoin;           ///< 먹은 코인 수를 표시할 텍스트
    TMP_Text txtTime;           ///< 경과 시간을 표시할 텍스트
    int coinCnt = 0;        ///< 먹은 코인 수 (점수)
    float startTime;        ///< 게임 시작 시각 (Time.time 기준)

    // Joystick
    GameObject panelStick;  ///< 가상 조이스틱 패널 (모바일에서만 활성화)
    Joystick stick;         ///< 가상 조이스틱 스크립트 (PanelStick의 첫 번째 자식)
    bool isMobile;          ///< 모바일 기기에서 실행 중인가? 조작 방식을 결정한다

    // ------------------ methods

    /// <summary>
    /// 씬이 시작될 때 가장 먼저 호출되어 게임을 초기화하고 시작 시각을 기록한다.
    /// </summary>
    /// <remarks>
    /// Start()가 아닌 Awake()에서 초기화하는 이유는, 다른 스크립트의 Start()보다
    /// 먼저 UI와 참조를 준비해 두기 위해서다.
    /// </remarks>
    private void Awake() 
    { 
        InitGame();
        startTime = Time.time;
    }

    /// <summary>
    /// 사용하지 않는다. 초기화는 Awake()에서 모두 끝낸다.
    /// </summary>
    void Start()
    {
        
    }

    /// <summary>
    /// 매 프레임 호출되는 게임 루프. 게임 오버 전까지만 동작한다.
    /// </summary>
    /// <remarks>
    /// 호출 순서가 중요하다. 머리를 먼저 옮긴 뒤 꼬리가 그 자리를 따라가야
    /// 꼬리가 머리 뒤를 자연스럽게 쫓아온다.
    /// -# MoveHead() : 머리 전진·회전
    /// -# MoveTail() : 꼬리들이 앞의 것을 따라감
    /// -# SetTime()  : 점수와 시간 표시 갱신
    /// </remarks>
    void Update()
    {
        if (!isDead) 
        { 
            MoveHead();
            MoveTail();
            SetTime();
        }

    }

    //-------------------- user defined function

    /// <summary>
    /// 머리를 앞으로 전진시키고, 입력값만큼 좌우로 회전시킨다.
    /// </summary>
    /// <remarks>
    /// - <b>이동</b> : 자기 자신의 앞 방향(Vector3.forward, 로컬 기준)으로 speedMove만큼 나아간다.
    ///   회전하면 나아가는 방향도 함께 바뀐다.
    /// - <b>회전</b> : y축(Vector3.up)을 중심으로 좌우 회전한다.
    ///   입력값(-1 ~ 1) × speedRot이 초당 회전 각도이고, 여기에 Time.deltaTime을 곱해
    ///   프레임레이트와 무관하게 같은 속도로 돌게 만든다.
    /// - 조작 장치는 isMobile 값에 따라 키보드와 가상 조이스틱 중에서 선택한다.
    /// </remarks>
    void MoveHead()
    {
        // 이동
        float amount = speedMove * Time.deltaTime;
        transform.Translate(Vector3.forward * amount);

        // 회전
        //amount = Input.GetAxis("Horizontal") * speedRot;
        if (!isMobile)
        {
            amount = Input.GetAxis("Horizontal") * speedRot;
        }
        else
        {
            amount = stick.Horizontal() * speedRot;
        }
        transform.Rotate(Vector3.up * amount * Time.deltaTime);
    }

    /// <summary>
    /// 머리가 다른 오브젝트와 충돌했을 때 태그에 따라 처리한다.
    /// </summary>
    /// <param name="collision">충돌 정보. <c>collision.gameObject.tag</c>로 대상을 구분한다.</param>
    /// <remarks>
    /// - <b>Coin</b> : 코인을 다른 자리로 옮기고(MoveCoin) 꼬리를 하나 늘린다(AddTail).
    /// - <b>Tail, Wall</b> : 게임 오버. 게임 오버 패널을 화면에 표시한다.
    ///
    /// "Tail"에 실행문 없이 break만 주석 처리해 두어 "Wall"의 처리로 넘어가게 했다.
    /// C#에서는 이처럼 <b>내용이 비어 있는 case</b>만 아래로 이어질 수 있다.
    /// (내용이 있으면 break나 goto가 없을 때 컴파일 오류가 난다.)
    /// </remarks>
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("OnCollisionEnter 충돌물체=" + collision.gameObject.name);
        switch (collision.gameObject.tag)
        {
            case "Coin":
                MoveCoin();
                AddTail();
                break;
            case "Tail":
                //break;
            case "Wall":
                isDead = true;
                panelOver.SetActive(isDead);
                break;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter 충돌물체=" + other.gameObject.name);
        /* 
         * switch (other.gameObject.tag)
        {
            case "Coin":
                MoveCoin();
                AddTail();
                break;
            case "Tail":
            //break;
            case "Wall":
                isDead = true;
                panelOver.SetActive(isDead);
                break;
        }
        */
    }

    /// <summary>
    /// 코인을 먹은 것으로 처리하고, 벽 안쪽의 임의의 위치로 코인을 옮긴다.
    /// </summary>
    /// <remarks>
    /// 코인을 없앴다가 새로 만들지 않고 <b>위치만 옮겨 재사용</b>한다.
    /// 오브젝트를 만들고 지우는 비용이 없어 가볍다.
    ///
    /// 이동 범위는 x −9 ~ 9, z −4 ~ 4로, 바깥 벽보다 안쪽이라 코인이 벽에 겹치지 않는다.
    /// y는 0으로 고정해 바닥 위에 놓는다.
    /// </remarks>
    /// @note 코인이 뱀의 몸 위에 겹쳐 나타날 수 있다. 이를 막으려면 꼬리들과의 거리를
    ///       확인한 뒤 위치를 다시 뽑는 처리가 필요하다.
    void MoveCoin()
    {
        coinCnt++;

        // Coin을 이동할 범위 - 벽의 안쪽
        float x = Random.Range(-9f, 9f);
        float z = Random.Range(-4f, 4f);

        coin.position = new Vector3(x, 0, z);
    }

    /// <summary>
    /// 씬에서 필요한 오브젝트를 찾아 두고, 실행 기기에 맞게 조작 UI를 준비한다.
    /// </summary>
    /// <remarks>
    /// -# 코인과 UI 위젯을 이름으로 찾는다.
    /// -# 게임 오버 패널을 숨긴다.
    /// -# 실행 중인 플랫폼이 Android나 iOS이면 모바일로 판단한다.
    /// -# 모바일일 때만 가상 조이스틱 패널을 켜고, 그 자식에서 Joystick 스크립트를 가져온다.
    ///
    /// 에디터에서 조이스틱을 시험해 보려면 주석 처리된
    /// <c>isMobile = true;</c> 줄을 잠시 살리면 된다.
    /// </remarks>
    /// @warning <c>GameObject.Find()</c>는 <b>비활성 상태인 오브젝트를 찾지 못한다</b>.
    ///          PanelOver와 PanelStick은 씬에서 켜 둔 채로 저장하고, 코드에서 SetActive()로
    ///          끄는 지금 방식을 유지해야 한다.
    /// @note 이름으로 찾기 때문에 계층 창에서 오브젝트 이름을 바꾸면 곧바로 오류가 난다.
    ///       public 변수로 만들어 인스펙터에서 연결하면 더 안전하고 빠르다.
    void InitGame()
    {
        coin = GameObject.Find("Coin").transform;

        // UI  위젯
        panelOver = GameObject.Find("PanelOver");
        panelOver.SetActive(false);

        txtCoin = GameObject.Find("TextCoin").GetComponent<TMP_Text>();
        txtTime = GameObject.Find("TextTime").GetComponent<TMP_Text>();

        // Mobile Device인가?
        //isMobile = Application.platform == RuntimePlatform.Android ||
        //           Application.platform == RuntimePlatform.IPhonePlayer;
        // For Testing
        //isMobile = true;

        //panelStick = GameObject.Find("PanelStick");
        //panelStick.SetActive(isMobile);
        //stick = panelStick.transform.GetChild(0).GetComponent<Joystick>();
    }

    /// <summary>
    /// 꼬리를 하나 만들어 뱀의 맨 끝에 붙인다.
    /// </summary>
    /// <remarks>
    /// -# Resources 폴더의 "Tail" 프리팹을 복제한다.
    /// -# <b>첫 번째 꼬리</b>는 머리 위치에 놓고 태그를 "Untagged"로 바꾼다.
    ///    머리에 바로 붙어 있어 태그가 "Tail"이면 곧바로 충돌해 게임이 끝나기 때문이다.
    /// -# <b>두 번째부터</b>는 마지막 꼬리의 위치에 놓는다. 이후 MoveTail()이 제자리를 찾아 준다.
    /// -# 꼬리 3개마다 색을 번갈아 칠해(초록 → 파랑) 길이를 눈으로 셀 수 있게 한다.
    ///    <c>n = cnt / 3 % 2</c>에서 정수 나눗셈이 3개 묶음을, 나머지 연산이 두 색의 반복을 만든다.
    /// </remarks>
    /// @warning "Tail" 프리팹은 반드시 <b>Assets/Resources</b> 폴더 안에 있어야 한다.
    ///          Resources.Load()는 그 폴더만 찾는다.
    /// @note 꼬리마다 material.color를 바꾸면 유니티가 머티리얼 사본을 새로 만든다.
    ///       꼬리가 아주 많아지면 드로우 콜이 늘어나므로, 색이 두 가지뿐이니
    ///       미리 만든 머티리얼 2개를 나눠 쓰는 방식이 더 가볍다.
    void AddTail()
    {
        GameObject tail = Instantiate(Resources.Load("Tail")) as GameObject;
        Vector3 pos = transform.position;

        // 꼬리의 개수 구하기
        int cnt = tails.Count;

        if (cnt == 0) 
        { 
            tail.tag = "Untagged"; 
        }
        else 
        { 
            pos = tails[cnt - 1].position; 
        }

        tail.transform.position = pos;

        // 꼬리 색
        Color[] colors = { new Color(0, 0.5f, 0, 1), new Color(0, 0.5f, 1, 1) };
        int n = cnt / 3 % 2;
        tail.GetComponent<Renderer>().material.color = colors[n];

        tails.Add(tail.transform);
    }

    /// <summary>
    /// 모든 꼬리가 바로 앞의 것(첫 꼬리는 머리)을 부드럽게 따라가게 한다.
    /// </summary>
    /// <remarks>
    /// 목록의 앞에서부터 차례로 처리하며, 방금 옮긴 꼬리를 다음 꼬리의 목표(target)로 삼는다.
    /// 이 연쇄 구조가 뱀처럼 이어지는 움직임을 만든다.
    ///
    /// 위치와 회전 모두 Lerp로 목표값에 조금씩 다가가게 해서(보간 계수 4 × deltaTime)
    /// 딱딱하게 끊기지 않고 자연스럽게 휘어진다. 계수가 클수록 앞을 바짝 따라붙는다.
    /// </remarks>
    /// @note <c>Lerp(현재, 목표, 4 * deltaTime)</c> 방식은 프레임레이트에 따라 따라붙는
    ///       정도가 미세하게 달라진다. 정확히 맞추려면
    ///       <c>1 - Mathf.Exp(-4 * Time.deltaTime)</c>를 보간 계수로 쓴다.
    void MoveTail()
    {
        Transform target = transform;
        foreach (Transform tail in tails)
        {
            Vector3 pos = target.position;
            Quaternion rot = target.rotation;

            tail.position = Vector3.Lerp(tail.position, pos, 4 * Time.deltaTime);
            tail.rotation = Quaternion.Lerp(tail.rotation, rot, 4 * Time.deltaTime);

            target = tail;
        }
    }

    /// <summary>
    /// 점수와 경과 시간을 UI 텍스트에 표시한다.
    /// </summary>
    /// <remarks>
    /// - 점수 : <c>ToString("Coin : 0")</c>은 숫자 서식 문자열로, 따옴표 안의 글자는 그대로 쓰이고
    ///   <c>0</c> 자리에 숫자가 들어간다. 결과는 "Coin : 5" 같은 형태가 된다.
    /// - 시간 : 시작 시각과의 차이를 시·분·초로 나눈다.
    ///   시와 분은 FloorToInt로 버림하고, 초는 나머지 연산으로 구해 소수 첫째 자리까지 보여 준다.
    ///   <c>{2:00.0}</c>은 "05.3"처럼 두 자리로 맞춰 표시하라는 뜻이다.
    /// </remarks>
    /// @note 게임 오버가 되면 Update()에서 이 함수를 더 이상 호출하지 않으므로
    ///       화면의 시간이 그 자리에 멈춘다.
    void SetTime()
    {
        txtCoin.text = coinCnt.ToString("Coin : 0");

        float span = Time.time - startTime;
        int h = Mathf.FloorToInt(span / 3600);
        int m = Mathf.FloorToInt(span / 60 % 60);
        float s = span % 60;

        txtTime.text = string.Format("Time: {0:0}:{1:0}:{2:00.0}", h, m, s);
    }

    /// <summary>
    /// 게임 오버 패널의 버튼을 눌렀을 때 호출된다.
    /// </summary>
    /// <param name="button">눌린 버튼. 이름(name)으로 어떤 버튼인지 구분한다.</param>
    /// <remarks>
    /// - <b>BtnYes</b> : 현재 씬을 다시 불러와 게임을 처음부터 시작한다.
    ///   씬 이름을 직접 적지 않고 <c>GetActiveScene().name</c>으로 가져오므로,
    ///   씬 이름을 바꾸어도 코드를 고칠 필요가 없다.
    /// - <b>BtnNo</b> : 애플리케이션을 종료한다.
    /// </remarks>
    /// @note 인스펙터의 Button → On Click ()에 이 함수를 연결하고,
    ///       인자 칸에 <b>그 버튼 자신</b>을 끌어다 놓아야 한다.
    ///       함수가 public이어야 On Click 목록에 나타난다.
    /// @note <c>Application.Quit()</c>은 에디터와 웹에서는 동작하지 않는다.
    ///       빌드한 실행 파일에서만 종료된다.
    public void OnButtonClick(Button button)
    {
        switch (button.name)
        {
            case "BtnYes" :
                SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
                break;
            case "BtnNo":
                Application.Quit();
                break;
        }
    }

}
