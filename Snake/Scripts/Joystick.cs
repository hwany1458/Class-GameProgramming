/**
 * @file    Joystick.cs
 * @brief   모바일용 가상 조이스틱 UI 스크립트
 * @details Canvas 위의 Background 이미지에 부착하며, 자식(첫 번째 자식)의 Stick 이미지를
 *          드래그하여 -1.0 ~ 1.0 범위의 입력값을 만든다.
 *          플레이어 스크립트는 Horizontal(), Vertical()로 그 값을 읽어 쓴다.
 * @author  게임콘텐츠학과 게임프로그래밍
 * @date    2026
 * @version 1.0
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 화면 터치(드래그)로 조작하는 가상 조이스틱.
/// </summary>
/// <remarks>
/// <b>구성</b>
/// - 이 스크립트를 부착한 오브젝트 : 조이스틱 Background (Image)
/// - 그 아래 <b>첫 번째 자식</b> : 손잡이 Stick (Image)
///
/// <b>동작 원리</b>
/// -# 터치(드래그) 지점의 화면 좌표를 Background 기준의 로컬 좌표로 변환한다.
/// -# 로컬 좌표를 Background 크기로 나누어 -1.0 ~ 1.0 범위로 정규화한다.
/// -# 그 값을 input에 저장하고, Stick 이미지를 같은 방향으로 이동시킨다.
/// -# 손을 떼면 input과 Stick 위치를 0으로 되돌린다.
///
/// <b>인터페이스</b>
/// EventSystem이 호출하는 세 가지 인터페이스를 상속받아 구현한다.
/// - IPointerDownHandler : 누르는 순간
/// - IDragHandler        : 누른 채 움직이는 동안
/// - IPointerUpHandler   : 떼는 순간
/// </remarks>
/// @note 씬에 EventSystem 오브젝트가 있어야 하며, Background와 Stick 이미지 모두
///       Raycast Target이 켜져 있어야 터치를 인식한다.
/// @see Snake
public class Joystick : MonoBehaviour,  IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    Image backImg;  ///< Background 이미지 (조이스틱의 바탕이자 입력 범위의 기준)
    Image stick;    ///< Stick 이미지 (손잡이, 첫 번째 자식 오브젝트)
    Vector3 input;  ///< 정규화된 입력값 (x, y 각각 -1.0 ~ 1.0, 크기는 최대 1)

    /// <summary>
    /// 시작할 때 Background와 Stick 이미지를 찾아 둔다.
    /// </summary>
    /// <remarks>
    /// Stick은 <c>transform.GetChild(0)</c>으로 찾으므로, 계층 창에서 반드시
    /// Background의 <b>첫 번째 자식</b>이어야 한다.
    /// </remarks>
    void Start()
    {
        backImg = GetComponent<Image>();
        stick = transform.GetChild(0).GetComponent<Image>();
    }

    /// <summary>
    /// 매 프레임 호출되지만 처리할 내용이 없다. 입력 처리는 모두 이벤트 함수에서 한다.
    /// </summary>
    /// @todo 사용하지 않는 Update()는 지우는 편이 성능에 유리하다.
    ///       (빈 Update()도 유니티가 매 프레임 호출한다.)
    void Update()
    {
        
    }

    /// <summary>
    /// 조이스틱을 누르는 순간 호출된다.
    /// </summary>
    /// <param name="data">터치·클릭 정보 (화면 좌표, 카메라 등)</param>
    /// <remarks>
    /// 누른 지점으로 Stick이 곧바로 따라가도록 OnDrag()를 그대로 호출한다.
    /// 드래그하지 않고 한 번 누르기만 해도 입력이 들어간다.
    /// </remarks>
    public void OnPointerDown(PointerEventData data)
    {
        OnDrag(data);
    }

    /// <summary>
    /// 조이스틱에서 손을 떼는 순간 호출된다. 입력값과 Stick을 중앙으로 되돌린다.
    /// </summary>
    /// <param name="data">터치·클릭 정보 (여기서는 사용하지 않음)</param>
    public void OnPointerUp(PointerEventData data)
    {
        input = Vector3.zero;
        stick.rectTransform.anchoredPosition = input;
    }

    /// <summary>
    /// 조이스틱을 누른 채 움직이는 동안 호출된다. 입력값을 계산하고 Stick을 이동시킨다.
    /// </summary>
    /// <param name="data">터치·클릭 정보. position(화면 좌표)과 pressEventCamera를 사용한다.</param>
    /// <remarks>
    /// -# <c>RectTransformUtility.ScreenPointToLocalPointInRectangle()</c>으로
    ///    화면 좌표를 Background 기준의 로컬 좌표로 바꾼다.
    ///    (Pivot이 중앙이면 로컬 좌표의 범위는 -크기/2 ~ +크기/2)
    /// -# 크기로 나눈 뒤 2를 곱해 -1.0 ~ 1.0으로 정규화한다.
    /// -# 대각선 방향에서 크기가 1을 넘을 수 있으므로,
    ///    magnitude가 1보다 크면 normalized로 원 안에 가둔다.
    ///    (그래야 대각선으로 움직일 때만 빨라지는 현상이 없다.)
    /// -# Stick은 Background 크기의 0.4배 범위 안에서만 움직이게 한다.
    /// </remarks>
    /// @note Canvas의 Render Mode가 Screen Space - Overlay이면 pressEventCamera가 null이지만,
    ///       변환 함수가 그 경우를 처리하므로 문제 없다.
    ///       Screen Space - Camera 모드라면 Canvas에 Render Camera가 지정되어 있어야 한다.
    /// @warning 계산의 기준이 되는 sizeDelta는 앵커가 늘어난(Stretch) 상태에서는 실제 크기와
    ///          달라진다. Background의 앵커는 늘리지 말고 크기를 고정해서 쓴다.
    public void OnDrag(PointerEventData data)
    {
        var rect = backImg.rectTransform;
        var camera = data.pressEventCamera;
        var dataPos = data.position;
        
        Vector2 pos;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, dataPos, camera, out pos))
        {
            // pos를 Background의 크기에 대한 비율로 설정
            pos.x = pos.x / backImg.rectTransform.sizeDelta.x * 2;
            pos.y = pos.y / backImg.rectTransform.sizeDelta.y * 2;

            // Vector 정규화 (-1.0 ~ 1.0으로 제한)
            input = new Vector3(pos.x, pos.y, 0);
            input = (input.magnitude > 1) ? input.normalized : input;

            // Stick 이동 범위 설정
            float x = input.x * rect.sizeDelta.x * 0.4f;
            float y = input.y * rect.sizeDelta.y * 0.4f;

            stick.rectTransform.anchoredPosition = new Vector3(x, y, 0);
        }
    }

    /// <summary>
    /// 좌우 입력값을 돌려준다. <c>Input.GetAxis("Horizontal")</c>을 대신한다.
    /// </summary>
    /// <returns>-1.0(왼쪽) ~ 1.0(오른쪽). 누르지 않았으면 0</returns>
    public float Horizontal()
    {
        return input.x;
    }

    /// <summary>
    /// 상하 입력값을 돌려준다. <c>Input.GetAxis("Vertical")</c>을 대신한다.
    /// </summary>
    /// <returns>-1.0(아래) ~ 1.0(위). 누르지 않았으면 0</returns>
    /// @note Snake 게임은 좌우 회전만 사용하므로 이 함수는 호출되지 않는다.
    ///       상하 입력이 필요한 게임에서 그대로 쓸 수 있도록 남겨 둔 것이다.
    public float Vertical()
    {
        return input.y;
    }
}
