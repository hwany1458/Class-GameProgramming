# Happy Harvest 데모: Unity 2D 제작 기술 정리

> 출처: Shanti Zachariah, “Happy Harvest demo: See the latest 2D techniques,” Unity Blog, 2023-07-05.  
> 원문: https://unity.com/blog/games/happy-harvest-demo-latest-2d-techniques  
> 이 문서는 원문을 복제하거나 번역한 자료가 아니라 강의 활용을 위해 핵심 내용을 재구성한 요약이다. 자세한 절차와 이미지는 원문 및 아래의 공식 기술 자료를 참조한다.

## 1. 데모 개요

**Happy Harvest**는 농장 생활을 소재로 한 탑다운 2D 예제다. Unity 2022 LTS의 Universal Render Pipeline(URP) 기반 2D 조명과 그림자, Tilemap, 캐릭터 리깅·스프라이트 교체, 파티클·셰이더 효과를 함께 보여준다. 농부가 농작물을 돌보고 동물을 먹이며, 낮에서 밤으로 바뀌는 장면을 통해 시각 요소들이 하나의 월드에서 어떻게 결합되는지 살펴볼 수 있다.

| 기술 영역 | 데모에서 확인할 요소 | 학습 목적 |
|---|---|---|
| 2D 조명과 그림자 | 시간대 변화, 랜턴, 캐릭터 윤곽광, 나무·소품 그림자 | 평면 스프라이트에 깊이와 분위기 표현 |
| Tilemap | 길, 연못, 잔디, 숲 | 반복 타일로 넓은 맵과 충돌 구성 |
| 2D 애니메이션 | 농부의 방향·표정·동작 | 본 애니메이션과 Sprite Swap 비교 |
| 시각효과 | 낙엽, 발자국, 비, 물결, 불·연기 | 효과의 규모에 맞는 Particle System·VFX Graph·Shader Graph 선택 |

## 2. 조명과 그림자

스프라이트 그림에 광원 방향이 고정된 강한 그림자를 미리 그려 넣으면 동적 조명과 충돌할 수 있다. 데모는 색상 그림을 비교적 평면적으로 유지하고, 표면 방향과 조명 적용 영역을 **Secondary Textures**로 분리한다.

- **Normal map**: 픽셀의 표면 방향을 표현해 2D 그림에 부피감이 있는 듯한 조명 반응을 만든다.
- **Mask map**: 빛을 받거나 윤곽광을 강조할 영역을 채널별로 조절한다.
- **2D Light**: 주변 밝기와 국소 광원을 결합해 낮·밤, 랜턴, 실내 분위기를 표현한다.
- **Shadow Caster 2D**: 광원과 물체의 관계에 맞춰 그림자를 만든다. 모양에 따라 단순한 블롭 그림자와 길게 드리운 그림자도 사용할 수 있다.
- **시간 변화 스크립트**: 조명의 색·강도 등을 점차 바꾸어 낮에서 밤으로 전환한다.

**실습 제안:** 하나의 스프라이트에 Global Light 2D와 Spot Light 2D를 설정하고, Normal map의 유무에 따른 차이를 캡처해 비교한다. 이후 간단한 시간 변수를 사용해 조명 색과 강도를 보간한다.

## 3. Tilemap으로 월드 구성

Tilemap은 작은 스프라이트를 격자에 반복 배치한다. 데모의 길·잔디·연못·숲은 큰 배경 그림 한 장이 아니라 타일 계층으로 구성된다. Rule Tile을 활용하면 이웃 타일에 따라 길의 가장자리나 모서리 모양을 자동으로 고를 수 있다.

- 기본 색상 타일에 대응하는 Normal/Mask map도 같은 배치로 관리할 수 있다.
- Grid 하위 Tilemap은 역할별로 정리하되 불필요한 겹침과 오버드로우를 줄인다.
- Tilemap Collider 등을 이용하면 맵 편집과 충돌 설정을 연결할 수 있다.
- Tilemap API로 타일 정보에 따른 게임 로직을 구현할 수 있다.

**실습 제안:** 길과 물 타일을 Rule Tile로 칠하고, 물에는 충돌을 적용한다. 플레이어가 물 타일로 들어가려 할 때 이동을 막거나 별도의 반응을 만든다.

## 4. 캐릭터 리깅과 애니메이션

탑다운 캐릭터는 정면만 잘 그려서는 자연스럽게 보이지 않는다. 데모는 여러 방향의 외형과 본 기반 애니메이션을 함께 사용한다.

- **2D Animation / PSD Importer**: 여러 레이어로 그린 캐릭터를 불러와 본, 메시, 가중치를 설정한다.
- **Sprite Library / Sprite Swap**: 같은 본에 붙은 얼굴·표정·방향별 부품을 교체한다.
- **2D IK**: 목표 지점을 기준으로 본의 회전을 계산하여 사지의 동작을 조절한다.
- 방향별 그림과 표정 교체를 결합하면 탑다운 화면에서 캐릭터의 행동을 더 읽기 쉽게 만든다.

**실습 제안:** 좌·우·앞·뒤 네 방향의 대기/걷기 상태를 먼저 구현한다. 표정 한 부분을 Sprite Swap으로 바꾸고, 프레임 교체와 본 애니메이션의 차이를 관찰한다.

## 5. 비·불·연기 등 시각효과

효과마다 같은 도구를 쓸 필요는 없다. 적은 수의 발자국이나 낙엽은 Built-in Particle System으로, 많은 빗방울은 VFX Graph로, 물결이나 바람에 흔들리는 식물은 Shader Graph로 구현하는 식이다. 장면 전체의 색조와 Bloom은 URP Volume을 통해 조절할 수 있다.

| 표현 | 데모에서 언급한 접근 |
|---|---|
| 발자국·낙엽·나방 | Built-in Particle System, 필요 시 플립북 파티클 |
| 비·천둥·빗물 튀김 | VFX Graph와 2D Lit 셰이더 |
| 물결·불·연기·식물 흔들림 | 셰이더 기반 움직임과 파티클의 조합 |
| 장면 분위기 | 색조 및 Bloom 후처리 |

**실습 제안:** 낮/밤 토글에 따라 랜턴을 켜고, 밤에는 나방 파티클을 표시한다. 비가 올 때는 빗방울 효과와 화면 색조를 함께 바꾼다.

## 6. 강의 구성 예시

1. **관찰(15분):** 데모 플레이 화면에서 빛·맵·캐릭터·날씨 효과를 구별한다.
2. **월드(30분):** Tilemap과 Rule Tile로 길·물·장애물을 만든다.
3. **조명(30분):** Global/Spot Light 2D, Normal map, 그림자를 비교한다.
4. **캐릭터(30분):** 네 방향 이동과 간단한 Sprite Swap을 구현한다.
5. **효과(25분):** 낙엽 또는 비 효과를 추가하고 낮/밤 장면을 비교한다.
6. **정리(10분):** 효과별로 사용한 컴포넌트와 성능 고려 사항을 설명하게 한다.

### 학습 점검 질문

- 스프라이트에 강한 그림자를 미리 그려 넣으면 동적 2D 조명에서 어떤 문제가 생길 수 있는가?
- Normal map과 Mask map은 각각 어떤 정보를 담는가?
- Rule Tile을 직접 타일을 한 칸씩 배치하는 방식과 비교하면 어떤 이점이 있는가?
- 캐릭터의 방향 전환과 표정 변경에 Sprite Swap을 쓰는 이유는 무엇인가?
- 낙엽·비·물결 효과에 각각 어떤 도구를 선택하겠는가? 이유는 무엇인가?

## 원문 및 공식 자료

- [Unity 블로그 원문](https://unity.com/blog/games/happy-harvest-demo-latest-2d-techniques)
- [2D light and shadow techniques with URP](https://unity.com/how-to/2d-light-shadow-techniques-in-the-universal-render-pipeline)
- [How to create art and gameplay with 2D Tilemaps](https://unity.com/how-to/create-art-and-gameplay-2d-tilemaps-unity)
- [How to animate 2D characters](https://unity.com/how-to/2d-characters-and-animation-unity-2022-lts)
- [Create 2D special effects with VFX Graph and Shader Graph](https://unity.com/how-to/2d-special-effects-vfx-graph-shader-graph)
- [Happy Harvest Asset Store 검색](https://assetstore.unity.com/?q=Happy%20Harvest)

> **버전 참고:** 원문은 2023년에 작성되어 Unity 2022 LTS를 기준으로 설명한다. 현재 사용하는 Unity 버전에서는 패키지·메뉴 이름과 설정 위치를 프로젝트에서 다시 확인한다.
