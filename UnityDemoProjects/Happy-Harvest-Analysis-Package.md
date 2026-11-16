# Happy Harvest - Unity 초보 개발자 분석 패키지

> 대상: Unity를 이제 막 시작한 학생
> 프로젝트: Happy Harvest (Unity 2022 LTS / URP 2D, 2D 탑다운 농사 시뮬레이션)
> 목표: 플레이 → 소스보기 → 이론읽기 → 직접만들기
> 작성일: 2026-11-17

---

## 목차

1. [프로젝트 한눈에 보기](#1-프로젝트-한눈에-보기)
2. [소스 구조도](#2-소스-구조도)
3. [튜토리얼 링크](#3-튜토리얼-링크)
4. [보고서 목차](#4-보고서-목차-그대로-쓰면-됨)
5. [새 작물 추가 Step-by-Step 가이드](#5-새-작물-추가-step-by-step-가이드-예시-무radish)
6. [유니티 전체 프로젝트 분석 방법](#6-유니티-전체-프로젝트-분석-방법)
7. [플레이하면서 분석하기](#7-플레이하면서-분석하기)
8. [플레이 체크리스트 (30분)](#8-플레이-체크리스트-30분-프린트용)
9. [참고문헌 - IEEE 스타일](#9-참고문헌---ieee-스타일)

---

## 1. 프로젝트 한눈에 보기

**Happy Harvest**는 Unity 2022 LTS / URP 기반 2D 탑다운 농사 시뮬레이션 샘플이다. 농부가 집에서 나와 밀, 당근, 옥수수를 심고, 사과를 따고, 돼지와 소에게 먹이를 주는 하루를 보여주는 데모이다.

초보자 분석용으로 좋은 이유는 게임 로직이 복잡하지 않고, **Tilemap + 데이터 + 시간/빛** 이 3가지로 거의 다 설명되기 때문이다. 2D 조명, 그림자, 애니메이션, 특수효과를 URP로 구현하는 방법을 보여주는 공식 샘플이다.

- 다운로드: Unity Asset Store에서 `Happy Harvest - 2D Sample Project` 무료 / Unity 2022.3 이상, URP 전용
- 장르: 2D 탑다운 농사 시뮬레이션
- 핵심 루프: 갈다 → 심다 → 물주기 → 성장 → 수확 → 낮-밤 사이클

### 왜 이 프로젝트인가?

| 기준 | 평가 |
| --- | --- |
| 소스코드 공개 | O (Asset Store 전체 프로젝트) |
| 튜토리얼 / 학습자료 | O (공식 아티클 4편 + 영상 + 전자책) |
| 분석 자료 만들기 쉬움 | O (Tilemap 기반이라 구조가 눈에 보임) |
| 초보자가 따라 만들기 쉬움 | O (핵심 시스템이 3개로 단순함) |
| 보고서 쓰기 쉬움 | O (성장 시간 등 정량 데이터 측정 가능) |

### 피해야 할 데모 (비교용)

Fantasy Kingdom, Megacity Metro, Boss Room, Enemies, Time Ghost는 그래픽/멀티플레이 기술 자랑용이라 소스가 100개 이상, ECS/DOTS를 써서 초보자는 분석 불가.

---

## 2. 소스 구조도

### 핵심 흐름

```
플레이어 입력 → PlayerController가 도구 사용 → TerrainManager 호출
→ Tilemap API로 타일 변경 → CropData / GroundData 업데이트
→ 시간/빛/VFX가 피드백으로 표시
```

### 2-1. Player - 플레이어

- `PlayerController`: 4방향 이동, 도구 사용 트리거
- 도구 사용 시 `TerrainManager`의 함수를 호출하는 구조
- 애니메이션은 3가지 세트: Right(옆), Up, Down

### 2-2. World / Terrain - 게임의 심장

- `TerrainManager`: `Grid` GameObject에 붙어 있음. 타일 변경과 작물 추적을 담당
- 내부 클래스 2개가 핵심:
  - `GroundData`: 땅이 젖어있는 시간 등 땅 상태
  - `CropData`: 작물이 자라는 속도, 물 없이 버티는 시간, 수확 안 하면 죽는 시간 등
- 사용하는 Tilemap API: `Vector3int` 위치로 타일 정보를 읽고 업데이트

### 2-3. Tilemaps - 전부 하나의 Grid 안에 있음

모든 타일맵은 `Grid` GameObject 아래에 있고, 타일맵 개수를 최소로 해서 오버드로우를 줄인 설계이다.

| Tilemap | 역할 |
| --- | --- |
| GroundBase | 길, 잔디, 진흙, 돌 등 기본 땅 |
| TilledTilemap | 씨앗 심을 수 있는 땅인지 판정 |
| WateredTileLayer | 물 준 땅 표현. 마르면 제거됨 |
| Water / UnderwaterTiles | 물, 연못, 절벽 |
| Crop | 모든 작물 부모. TerrainManager가 관리 |
| ObjectsInTiles | 울타리 같은 GameObject를 GameObject Brush로 배치 |
| Warehouse, House | 건물도 타일맵으로 만들어 크기 조절 쉽게 함 |

### 2-4. Time / Light - 이 데모의 차별점

- `Interpolator` 스크립트로 낮-밤 사이클 제어
- Normal Map + Mask Map(Secondary Texture)로 실시간 2D 라이트/그림자 구현. 스프라이트에 그림자를 굽지 않고, 빛/볼륨 정보를 별도 텍스처로 분리한 게 핵심 베스트 프랙티스이다.
- Ambient + Spot Light로 태양 이동 느낌, Blob Shadow + Infinite Shadow로 시간대별 그림자

### 2-5. Art / Animation

- 스켈레탈 애니메이션 + 2D IK, PSD Importer로 포토샵 레이어 그대로 가져옴
- Sprite Library + Sprite Swap으로 표정/방향별 스프라이트 교체. 뼈 회전만으로 안 되는 표정 변화를 이렇게 해결함
- 얼굴 리깅으로 표정 제어, 반-3D 같은 효과도 만듦

### 2-6. VFX / Shader

- Built-in Particle System: 낙엽, 발자국, 램프 주변 나방(Flipbook)
- VFX Graph: 비, 물 튀김. 비 파티클은 2D Lit 셰이더를 써서 빛에 반응함
- Shader Graph: 물결, 모닥불/연기, 바람에 흔들리는 나무
- URP Volumes: Tint + Bloom으로 따뜻한 분위기

### 2-7. Hierarchy 구조도

```
Main Scene
├── Player (PlayerController)
├── Grid (TerrainManager)
│   ├── GroundBase
│   ├── TilledTilemap
│   ├── WateredTileLayer
│   ├── Water
│   ├── Crop
│   └── ObjectsInTiles
├── Lights / DayNight System
├── VFX (Rain, Fire, Leaves)
└── UI / Camera
```

---

## 3. 튜토리얼 링크

### 3-1. 프로젝트 본체 다운로드

- **Happy Harvest - 2D Sample Project (공식, 무료)**
  - https://assetstore.unity.com/packages/templates/packs/happy-harvest-2d-sample-project-259218
  - Unity 계정 로그인 → Add to My Assets → Unity Hub / Package Manager에서 Import
  - 주의: 구글에 떠도는 GitHub 복제본은 비공식. 버전이 옛날 거거나 파일이 빠져있는 경우가 많음.

- **Unity Play에서 바로 플레이해보기 (설치 없이)**
  - https://play-prod.struckd.com/en/games/9d7f1ab8-d1f9-4b5d-9a53-f15ef61cfa73/happy-harvest

- **Unity Demos / Samples 목록**
  - https://unity.com/demos

### 3-2. 개발 과정 / 공식 소개

- **공식 소개 블로그 - Happy Harvest demo: See the latest 2D techniques**
  - https://unity.com/blog/games/happy-harvest-demo-latest-2d-techniques
  - 왜 만들었는지, 어떤 기술을 보여주려고 했는지 요약. 보고서 1~2장에 인용 가능.

- **공식 포럼 공지**
  - https://forum.unity.com/threads/new-urp-2d-sample-project-happy-harvest-download-now.1456480/
  - 2023년 7월 5일 출시 공지. 4개 심화 아티클 링크 + 유저 질문/피드백.

### 3-3. 핵심 아티클 4편 (진짜 교재)

| # | 제목 | 내용 | 링크 |
| --- | --- | --- | --- |
| 1 | 2D light and shadow techniques in URP | Normal Map / Mask Map, 낮-밤 Interpolator, Blob/Infinite Shadow, 빛 최적화 | https://activation.unity3d.com/how-to/2d-light-shadow-techniques-in-the-universal-render-pipeline |
| 2 | How to Create Art and Gameplay with 2D Tilemaps | Grid 구조, TerrainManager + GroundData/CropData, Rule Tile / Palette_Tiles | https://unity.com/ja/how-to/create-art-and-gameplay-2d-tilemaps-unity |
| 3 | How to animate 2D characters in Unity 2022 LTS | 스켈레탈 리깅, Sprite Library / Sprite Swap, 2D IK, 4방향 애니메이션 | https://on.unity.com/3pJuP1M |
| 4 | Create 2D special effects with VFX Graph and Shader Graph | VFX Graph 비, Particle 낙엽/발자국/나방, Shader Graph 물결/불/바람 | https://unity.com/en/how-to/2d-special-effects-vfx-graph-shader-graph |

### 3-4. 영상 튜토리얼

- **Unity 2D Tech Art 영상 플레이리스트 (공식)**
  - https://unity.com/resources/2d-technical-art-video-tutorials
  - 2D 셰이더, Secondary Texture, 2D VFX, Happy Harvest 커스텀까지 묶인 공식 영상 모음

- **Happy Harvest 커스텀 튜토리얼 - 무(Radish) 작물 추가 영상**
  - YouTube에서 `Happy Harvest URP 2D sample game customization tutorial` 로 검색
  - 정리 페이지: https://www.classcentral.com/course/youtube-happy-harvest-urp-2d-sample-game-customization-tutorial-unity-288926
  - Blender에서 무 스프라이트 모델링 → 잎 추가 → Normal/Mask Map 만들기 → Unity에서 Secondary Texture 연결 → 새 프리팹 + 데이터 만들기 → UI Toolkit 노트 만들기 전 과정

### 3-5. 전자책 / 문서

- **전자책 - 2D game art, animation, and lighting for artists (Unity 6.3 LTS edition)**
  - https://unity.com/resources/2d-game-art-animation-lighting-unity-6-3-lts
  - 175페이지 공식 전자책. Happy Harvest, Dragon Crashers, Gem Hunter Match 예시.

- **Unity Docs - 2D game development 허브**
  - https://docs.unity.com

- **Unity 6 Resources Hub**
  - https://unity.com/campaign/unity-6-graphics-training-sessions

### 추천 다운로드 순서

| 순서 | 뭘 받을까 | 시간 |
| --- | --- | --- |
| 1 | Unity Play에서 5분 플레이 | 5분 |
| 2 | Asset Store에서 프로젝트 다운로드 | 10분 |
| 3 | 블로그 + 아티클 4개 북마크 | 10분 |
| 4 | 전자책 PDF 다운로드 | 5분 |
| 5 | 무 작물 영상 보면서 따라하기 | 1시간 |

---

## 4. 보고서 목차 (그대로 쓰면 됨)

학생용 12~15페이지 기준. 표지+목차 빼고 본문 10장.

**1. 서론**
- 1.1 연구 배경: 왜 2D인가, 왜 Happy Harvest인가
- 1.2 연구 목적: Tilemap 기반 농사 게임의 구조 분석 및 재현 가능성 검증
- 1.3 연구 범위 및 방법: Unity 2022 LTS, URP, Asset Store 샘플 분석
- 1.4 실행 환경: Unity 버전, 패키지 목록 (2D Animation, Tilemap Extras, URP)

**2. 프로젝트 개요**
- 2.1 게임 소개: 장르, 핵심 루프 (갈다 → 심다 → 물주기 → 성장 → 수확)
- 2.2 플레이 흐름도
- 2.3 소스 구조도

**3. 핵심 시스템 분석 1 - Tilemap과 TerrainManager**
- 3.1 Grid / Tilemap 계층 구조
- 3.2 TerrainManager 역할과 Tilemap API (`Vector3int`, GetTile/SetTile)
- 3.3 GroundData / CropData 데이터 설계 - 표로 정리
- 3.4 Rule Tile과 Palette_Tiles로 길 자동 완성하는 원리
- 3.5 분석 포인트: 타일맵을 게임 로직 저장소로 쓴 이유 (별도 배열 안 쓰고 타일을 데이터로 씀)

**4. 핵심 시스템 분석 2 - 플레이어와 도구 상호작용**
- 4.1 PlayerController 구조: 이동, 방향, 도구 전환
- 4.2 도구별 동작: 괭이(경작) / 씨앗(심기) / 물뿌리개 / 수확
- 4.3 상호작용 시퀀스 다이어그램 (플레이어 → TerrainManager → Tilemap)

**5. 핵심 시스템 분석 3 - 시간, 빛, 그림자**
- 5.1 낮-밤 Interpolator 스크립트 분석
- 5.2 Normal Map / Mask Map (Secondary Texture) 개념
- 5.3 Blob Shadow vs Infinite Shadow
- 5.4 최적화: Tilemap Chunk 모드, 오버드로우 줄이기

**6. 핵심 시스템 분석 4 - 캐릭터 애니메이션**
- 6.1 스켈레탈 애니메이션과 2D IK
- 6.2 Sprite Library / Sprite Swap으로 표정·방향 전환
- 6.3 4방향 애니메이션 세트 구성

**7. 핵심 시스템 분석 5 - VFX와 셰이더**
- 7.1 VFX Graph 비 / 물 튀김
- 7.2 Particle System 낙엽 / 발자국 / 나방
- 7.3 Shader Graph 물결 / 불 / 바람
- 7.4 URP Volumes Tint / Bloom

**8. 직접 재현 / 확장 실험 (이 장이 점수 좌우함)**
- 8.1 실험 1: 새로운 작물(무) 추가하기 - 스프라이트 → 프리팹 → CropData 값 설정
- 8.2 실험 2: 성장 시간 / 물 마르는 시간 바꿔보기 - 밸런스 변화 기록
- 8.3 실험 3: 새로운 도구나 비료 기능 추가해보기 (선택)
- 8.4 실험 결과: 스크린샷 Before / After + 코드 변경 부분 캡처

**9. 고찰 및 개선점**
- 9.1 장점: Tilemap = 맵 + 데이터, URP 2D의 확장성
- 9.2 단점 / 한계: 저장 기능 없음, 멀티플레이 없음, 모바일 최적화 필요
- 9.3 개선 아이디어: 인벤토리, 상점, 날씨 시스템, 세이브/로드

**10. 결론**
- 배운 점 3줄 요약 + 앞으로 만들고 싶은 게임에 어떻게 적용할지

**부록**
- A. 주요 스크립트 목록표 (클래스명 / 역할 / 핵심 함수)
- B. 참고문헌 (9장 IEEE 스타일)
- C. 실행 스크린샷 모음

---

## 5. 새 작물 추가 Step-by-Step 가이드 (예시: 무/Radish)

> 전제: Happy Harvest 프로젝트가 Unity 2022 LTS에서 열리고, Play 눌렀을 때 정상 플레이 되는 상태

### 전체 흐름

새 작물 1개 = **아트 1세트 + 프리팹 1개 + 데이터 1개 + 등록 1곳**

| 순서 | 할 일 | 결과물 |
| --- | --- | --- |
| 1 | 스프라이트 만들기 | 성장단계별 스프라이트 3~4장 |
| 2 | Secondary Texture 연결 | NormalMap + MaskTex |
| 3 | 기존 작물 프리팹 복제해서 수정 | `Crop_Radish` 프리팹 |
| 4 | CropData / 작물 데이터 만들기 | 성장시간, 물 필요량 등 |
| 5 | 씨앗/도구/ TerrainManager에 등록 | 게임 안에서 선택 가능하게 |
| 6 | 테스트 + 밸런스 조정 | 실제 플레이 확인 |

### 1단계. 기존 작물 구조 뜯어보기 (복제 전 필수)

1. Hierarchy에서 `Grid` 선택
2. `Grid > Crop` 안에 있는 작물들 확인
3. Project에서 기존 작물 프리팹 찾기 (보통 `Assets / Prefabs / Crop` 안에 `Crop_Corn`, `Crop_Wheat`, `Crop_Carrot`)
4. `Crop_Carrot` 클릭하고 Inspector 메모:
   - Sprite Renderer가 몇 개인가? 성장 단계별로 몇 개 오브젝트가 있는가?
   - 어떤 스크립트가 붙어 있는가?
   - BoxCollider2D 크기
   - Sorting Layer / Order in Layer

핵심: Happy Harvest 작물은 1개 프리팹 안에 성장 단계별 오브젝트가 들어있는 구조이다.
- Stage 0: 새싹
- Stage 1: 중간
- Stage 2: 다 자람 (수확 가능)

### 2단계. 스프라이트 준비하기

**방법 A - 제일 쉬움 (추천):** 기존 작물 스프라이트 복사해서 색만 바꾸기. 포토샵 / Aseprite / Krita에서 당근 스프라이트를 열어서 빨간 몸통을 하얀 무로 바꾸고, 잎은 그대로. 학생 과제용으로 제일 빠르고, 보고서에 "기존 에셋 재활용"이라고 쓰면 됨.

**방법 B - 공식 튜토리얼 방식:** Blender로 모델링. Unity 공식 커스텀 튜토리얼이 이 방식. Blender에서 무 모델 만들고, 잎 추가하고, 정면(Top-down) 렌더링해서 스프라이트로 저장.

**방법 C - 직접 그리기:** 256x256 또는 512x512, 투명 배경 PNG로 그리기.

#### 필요한 스프라이트 목록

| 파일명 | 용도 |
| --- | --- |
| `Radish_Stage0.png` | 심은 직후 - 새싹만 보임 |
| `Radish_Stage1.png` | 중간 - 잎이 큼, 뿌리 살짝 보임 |
| `Radish_Stage2.png` | 다 자람 - 하얀 무 + 잎, 수확 가능 표시 |
| `Radish_Icon.png` (선택) | 씨앗 선택 UI / 인벤토리 아이콘 |

주의: 모든 스프라이트 크기, 중심점(Pivot)을 기존 작물과 똑같이 맞추기. Pixels Per Unit도 기존과 동일하게 (보통 100).

#### Unity에 Import하기

1. `Assets / Art / Crops / Radish` 폴더 새로 만들기
2. PNG 3~4장 드래그
3. 각 텍스처 선택 → Inspector: Texture Type `Sprite (2D and UI)`, Filter Mode `Bilinear`, Compression `None` (테스트용)
4. Apply

### 3단계. Secondary Texture 연결하기

이 단계를 빼먹으면 무가 밤에 빛을 안 받거나, 너무 밝게 뜬다. Happy Harvest가 다른 2D 게임과 다른 이유가 바로 이거다.

#### Normal Map 만들기

빛이 어디서 반사되는지 알려주는 텍스처. 쉬운 방법은 기존 당근의 Normal Map을 복사해서 이름만 `Radish_Normal.png`로 바꾸고 색감만 무에 맞게 수정. 정석은 포토샵 `Filter > 3D > Generate Normal Map` 또는 Blender에서 베이크.

#### Mask Map 만들기

어디에 빛을 받을지, 림라이트를 줄지 정하는 텍스처. 검정 = 빛 안 받음, 흰색 = 빛 받음. 무의 몸통은 흰색에 가깝게, 그림자 부분은 검정에 가깝게. 기존 작물의 Mask Map을 복사해서 시작하는 걸 추천.

#### Unity에서 연결하기

1. `Radish_Stage2.png` 선택 → Sprite Editor 열기
2. 왼쪽 탭에서 Secondary Textures 클릭
3. 추가하기:
   - Name: `_NormalMap` → Texture: `Radish_Normal.png`
   - Name: `_MaskTex` → Texture: `Radish_Mask.png`
4. Stage0, Stage1도 똑같이 반복
5. Apply

> 이 이름 `_NormalMap`, `_MaskTex`는 URP 2D Lights 패키지가 정해둔 약속 이름이다. 철자 틀리면 적용 안 됨.

### 4단계. 프리팹 만들기 - 복제가 정답

1. Project에서 `Crop_Carrot` 우클릭 → Duplicate
2. 이름 `Crop_Radish`로 바꾸기
3. 더블클릭해서 Prefab 편집 모드로 들어가기

#### 스프라이트 교체

| 오브젝트 | 바꿀 것 |
| --- | --- |
| `Stage0` | Sprite Renderer → Sprite를 `Radish_Stage0`으로 |
| `Stage1` | Sprite Renderer → Sprite를 `Radish_Stage1`으로 |
| `Stage2` | Sprite Renderer → Sprite를 `Radish_Stage2`으로 |

#### 스크립트 값 확인

프리팹 루트에 붙은 작물 스크립트(버전마다 `Crop`, `CropData`, `CropBehaviour` 등으로 다름)를 클릭해서 확인:
- Growth Stages 배열에 Stage 오브젝트 3개가 연결되어 있는지
- 각 Stage별 지속 시간 필드가 있는지
- Harvestable 체크가 마지막 Stage에만 있는지

주의: 버전별 스크립트 이름이 바뀔 수 있다. Inspector에 보이는 필드명을 기준으로 따라가면 된다.

#### Collider 조정

- 무가 당근보다 크면 BoxCollider2D Size를 살짝 키우기. 너무 크면 플레이어가 수확할 때 옆 타일까지 건드림.

#### 저장

Prefab 편집 창에서 Save → Hierarchy로 돌아오기.

### 5단계. 작물 데이터 만들기

#### 데이터 에셋 찾기

Project에서 `Crop` 검색 → `CropData` 타입 에셋 찾기 (보통 `Assets / Data / Crops` 또는 `Assets / ScriptableObjects` 안에 `CarrotData`, `WheatData` 등)

#### 복제해서 무 데이터 만들기

1. `CarrotData` 우클릭 → Duplicate → `RadishData`
2. Inspector에서 값 수정 - 예시:

| 항목 | 당근(원본) | 무(새로) | 의미 |
| --- | --- | --- | --- |
| Crop Name | Carrot | Radish | 표시 이름 |
| Crop Prefab | Crop_Carrot | Crop_Radish | 4단계에서 만든 프리팹 연결 |
| Growth Time | 30초 | 45초 | 다 자라는 데 걸리는 시간 |
| Water Needed | 1회 | 2회 | 물 줘야 하는 횟수 |
| Time Without Water | 20초 | 15초 | 물 안 주면 시드는 시간 |
| Time To Die | 60초 | 50초 | 수확 안 하면 죽는 시간 |
| Yield / Sell Price | 10 | 15 | 수확 시 얻는 보상 |
| Growth Stages | 3 | 3 | 단계 수 |

팁: 무를 당근보다 "느리게 자라지만 비싸게" 설정하면 밸런스 분석할 거리가 생긴다. 보고서 8장에 "왜 이렇게 설정했는가"를 쓸 수 있음.

#### TerrainManager에 등록하기

이걸 안 하면 무 씨앗이 게임에 안 나온다.

1. Hierarchy → `Grid` 선택 → Inspector에서 `TerrainManager` 찾기
2. TerrainManager 안에 있는 목록 찾기: `Crops List` / `Available Crops` / `Crop Datas` 같은 배열
3. Size를 +1 늘리고, 마지막 칸에 `RadishData` 드래그
4. 만약 씨앗 선택 UI가 따로 있으면, 그 UI 스크립트에도 `RadishData` 추가

확인 방법: TerrainManager 스크립트를 더블클릭해서 코드 열기 → `CropData` 배열이 어디서 쓰이는지 검색.

### 6단계. 테스트하기

Play 누르기 전에 Save All (`Ctrl + S`) 먼저.

#### 테스트 체크리스트

- [ ] 1. 심기: 괭이로 땅 갈기 → 무 씨앗 선택 → 타일 클릭하면 Stage0 새싹이 보이는가?
- [ ] 2. 물주기: 물뿌리개로 물 주면 타일이 젖은 색(`WateredTileLayer`)으로 바뀌는가?
- [ ] 3. 성장: 시간이 지나면 Stage0 → Stage1 → Stage2로 바뀌는가?
- [ ] 4. 시들기: 물 안 주면 시드는가? (테스트용으로 Time Without Water를 5초로 줄여서 확인)
- [ ] 5. 수확: Stage2에서 수확 도구로 클릭하면 사라지고 보상이 들어오는가?
- [ ] 6. 밤/빛: 밤이 되면 무에 램프 빛이 정상적으로 비치는가? (Secondary Texture 확인)
- [ ] 7. 충돌: 플레이어가 무 위를 지나갈 때 이상하게 걸리는가?

#### 자주 나오는 오류 3가지

**1. 심었는데 아무것도 안 보여요**
→ 프리팹의 Stage 오브젝트가 비활성화되어 있거나, Sprite가 안 연결됨. 프리팹 다시 열어서 Sprite Renderer 확인.

**2. 심기는 되는데 성장이 안 돼요**
→ CropData의 Growth Time이 너무 길거나, TerrainManager에 RadishData가 등록 안 됨. Inspector에서 TerrainManager 목록 확인.

**3. 밤에 무만 하얗게 빛나요 / 검게 보여요**
→ Mask Map / Normal Map이 안 연결됨. Sprite Editor → Secondary Textures에서 `_NormalMap`, `_MaskTex` 이름 확인.

### 7단계. 보고서용으로 정리하기

보고서 8장에 넣을 사진:

1. Before / After: 당근 밭 vs 무 밭 스크린샷 나란히
2. 프리팹 구조: `Crop_Radish` Hierarchy 펼쳐서 Stage0/1/2 보이는 Inspector 캡처
3. 데이터 비교표: 당근 vs 무 CropData 값 비교표
4. Secondary Texture: Sprite Editor에서 Normal/Mask 연결된 화면
5. 테스트 영상 or GIF: 심기 → 성장 → 수확 30초짜리 화면 녹화

### 8단계. 심화 확장 아이디어

- 새 작물 2개 더 추가: 호박(느리지만 비쌈), 딸기(빠르지만 금방 죽음) → 밸런스 3종 비교표
- 비료 기능: 비료 뿌리면 Growth Time 50% 감소
- 계절 시스템: 무는 겨울에만 자람
- 상점: 수확한 무를 팔아서 씨앗 사기

### 최종 체크리스트 - 제출 전 확인

- [ ] 무가 심기고, 자라고, 수확되는가?
- [ ] CropData 값이 당근과 다르게 설정되어 있는가? (그냥 복사하면 감점)
- [ ] Secondary Texture 연결했는가?
- [ ] TerrainManager 등록했는가?
- [ ] 스크린샷 5장 + 비교표 1개 있는가?

가장 중요한 건 **4단계 프리팹 복제 → 5단계 데이터 복제 → TerrainManager 등록** 이 3단계이다.

---

## 6. 유니티 전체 프로젝트 분석 방법

### 전체 그림 - 분석은 4바퀴 돌리는 거야

| 바퀴 | 목적 | 도구 |
| --- | --- | --- |
| 1바퀴: 플레이 | 게임이 뭔지 몸으로 이해 | Play Mode, Game View |
| 2바퀴: 정적 분석 | 코드 안 보고 구조만 파악 | Hierarchy, Project, Inspector |
| 3바퀴: 동적 분석 | 실행 중에 값 변화를 추적 | Console, Breakpoint, Profiler, Frame Debugger |
| 4바퀴: 수정 실험 | 값 하나 바꿔서 가설 검증 | Inspector 값 변경, 코드 주석 |

이 4바퀴를 안 돌고 코드부터 보면 100% 헤맨다. 무조건 1바퀴부터.

### 1바퀴: 플레이 분석

**실행 전 준비:**
1. 프로젝트 열고 `File > Build Settings` 확인 - Main Scene이 뭔지
2. `Edit > Project Settings > Player` - 해상도, 회사명 확인 (보고서 실행환경에 씀)
3. `Window > Package Manager` - 설치된 패키지 목록 스크린샷. `2D Animation, 2D Tilemap Extras, URP` 있는지 확인

**플레이하면서 적을 관찰 노트:**

| 행동 | 관찰할 것 | 메모 예시 |
| --- | --- | --- |
| 이동 | 4방향 애니메이션 바뀌나? | 위/아래/옆 3세트 |
| 괭이질 | 타일이 어떻게 바뀌나? | 갈색 흙으로 변경 |
| 심기 | 새싹이 바로 보이나? | Stage0 |
| 물주기 | 타일 색이 바뀌나? | 젖은 색으로 변경 |
| 시간 경과 | 몇 초 후에 Stage1로 바뀌나? | 스톱워치로 재기 |
| 밤 | 빛이 어떻게 바뀌나? | 램프 켜짐, 그림자 길어짐 |
| 비 | 언제 오나? | 저녁에 VFX Rain |
| 수확 | 보상이 뭔가? | 사라지고 +점수? |

꿀팁: 휴대폰 스톱워치 켜고 `심기 → Stage1까지 몇 초, Stage1 → Stage2까지 몇 초` 재면 보고서에 쓸 정량 데이터가 생긴다.

**플레이 중 Pause 활용:** Play 중에 Pause 버튼 누르면 그 순간의 Hierarchy가 언다. 그 상태에서 Grid > Crop 안에 작물이 몇 개인지, 플레이어 위치가 어디인지, 어떤 오브젝트가 활성화되어 있는지 확인할 수 있다.

### 2바퀴: 정적 분석 - 코드 안 보고 구조부터

**Hierarchy 분석 - 3가지 질문:**
1. 누가 부모인가? `Grid` 아래에 뭐가 있나? `Player`는 어디 있나?
2. 뭐가 많나? Tilemap이 몇 개인가? Light가 몇 개인가?
3. 뭐가 꺼져있나? 비활성화된 오브젝트는 나중에 켜지는 이벤트용인 경우가 많음

**Project 분석 - 폴더가 곧 설계도:**

| 폴더 | 봐야 할 것 |
| --- | --- |
| `Scenes` | 씬이 몇 개인가? Main 하나만 있나? |
| `Scripts` | 스크립트가 몇 개인가? 이름만 보고 역할 추측해보기 |
| `Prefabs` | 작물, 도구, 플레이어 프리팹이 어떻게 나뉘어 있나? |
| `Art / Sprites` | 스프라이트 + Normal/Mask Map이 쌍으로 있나? |
| `Data / ScriptableObjects` | CropData 같은 데이터 에셋이 있나? |
| `Tilemaps / Palettes` | Palette_Tiles가 있나? Rule Tile이 있나? |

이 단계에서 할 일: 스크립트 파일을 더블클릭해서 여는 게 아니라, 파일 이름만 보고 엑셀에 정리하기. 예: `TerrainManager.cs - 아마 땅 관리`, `PlayerController.cs - 아마 이동`

**Inspector 분석 - 컴포넌트 연결도 그리기 (대표 오브젝트 3개만):**

- Grid 클릭: TerrainManager에 어떤 Tilemap들이 연결되어 있나? CropData 배열에 뭐가 들어있나?
- Player 클릭: PlayerController에 어떤 필드가 있나? Animator, Sprite Renderer, Rigidbody2D 설정은?
- Crop_Carrot 프리팹 클릭: Stage 오브젝트가 몇 개인가? 어떤 스크립트가 붙어있나?

연결선: `PlayerController -> TerrainManager -> Tilemap -> CropData`

### 3바퀴: 동적 분석

**Console + Debug.Log로 추적하기:**

```csharp
// TerrainManager.cs 안에 임시로 추가
Debug.Log($"Tile changed: {position} -> {tileName}");
Debug.Log($"Crop growth: {cropName} stage {currentStage}");
```

Play 하고 Console 창에서 로그가 언제 찍히는지 보면, `심기 → 물주기 → 성장` 순서가 눈으로 보인다. 분석 끝나면 Debug.Log는 지우기.

**Breakpoint로 멈춰서 보기 (Visual Studio):**
1. Visual Studio에서 TerrainManager.cs 열기
2. 작물이 자라는 함수 옆에 빨간 점(Breakpoint) 찍기
3. Unity에서 Play → 그 함수에 걸리면 게임이 멈춤
4. 마우스 올리면 변수 값이 보임: `growthTime = 12.5`, `waterLevel = 0` 같은 거

**Inspector에서 런타임 값 보기:** Play 중에 Inspector는 실시간 값을 보여준다. Play 중에 Grid 선택 → TerrainManager의 CropData 배열이 어떻게 바뀌는지, Crop 오브젝트 선택 → 현재 Stage가 뭔지, Timer가 몇 초 남았는지 확인. 이걸 영상 캡처하면 보고서에 "실행 중 값 변화" 사진으로 쓸 수 있다.

**Profiler / Frame Debugger - 가볍게만:**
- Profiler (Window > Analysis > Profiler): Play 중에 CPU / 렌더링이 얼마나 드는지. 비가 올 때 FPS가 떨어지나?
- Frame Debugger (Window > Analysis > Frame Debugger): 1프레임이 어떻게 그려지는지. Tilemap → 캐릭터 → 빛 → VFX 순서로 그려지는지 확인

### 4바퀴: 수정 실험

**실험 1: 숫자만 바꾸기 (5분)**
- RadishData의 Growth Time을 45초 → 5초로. 예측: "빨리 자랄 것이다" → Play해서 확인 → 스크린샷

**실험 2: Inspector에서 연결 바꾸기 (10분)**
- 작물 프리팹의 Stage1 스프라이트를 다른 작물 스프라이트로 바꿔보기

**실험 3: 코드 한 줄 바꾸기 (20분)**
- TerrainManager에서 물 주는 횟수를 1회 → 2회로, 또는 `if (waterLevel > 0)` 조건을 바꿔보기

**실험 4: 새 기능 추가 (1시간)**
- 새 작물 추가, 새 도구 추가, 비료 추가 중 하나

### 분석 노트 양식 (매일 5줄만 채우면 보고서가 쌓임)

1. 오늘 본 오브젝트: Grid, Player, Crop_Carrot
2. 오늘 본 스크립트: TerrainManager.cs - 타일 변경 담당
3. 오늘 발견한 연결: PlayerController → TerrainManager → Tilemap
4. 오늘 바꾼 값 / 결과: Growth Time 45→5초, 5초 만에 성장 확인
5. 아직 모르는 것: 밤에 빛이 어떻게 계산되는지 모름 → 내일 아티클 읽기

### Happy Harvest 분석 순서 - 추천 루트 (Day 1~5)

| 단계 | 할 일 | 시간 | 결과물 |
| --- | --- | --- | --- |
| Day 1 - 플레이 | 30분 플레이 + 관찰 노트 + 스톱워치 측정 | 1시간 | 핵심 루프 이해 |
| Day 2 - 소스보기 | Hierarchy / Project / Inspector 구조도 그리기 | 2시간 | 소스 구조도 |
| Day 3 - 이론읽기 | 아티클 4개 읽기 (빛, 타일맵, 캐릭터, VFX) | 2시간 | 이론 정리 노트 |
| Day 4 - 동적 분석 | Debug.Log + Breakpoint + Inspector 런타임 확인 | 2시간 | 값 변화 캡처 |
| Day 5 - 직접만들기 | 무 작물 추가 + 밸런스 실험 | 3시간 | 수정된 프로젝트 + 스크린샷 |

### 흔히 하는 실수 3가지

1. 코드부터 읽기: 코드 100줄 읽는 것보다 Play 10분이 더 많이 알려준다. 무조건 플레이부터.
2. 전체를 다 이해하려고 하기: VFX Graph, Shader Graph는 처음부터 다 볼 필요 없다. `TerrainManager + CropData` 2개만 파고 나머지는 나중에.
3. 수정을 안 하고 읽기만 하기: 값 하나 안 바꿔보면 이해한 게 아니다. 무조건 Growth Time 5초로 바꿔보는 실험부터 해.

---

## 7. 플레이하면서 분석하기

> 핵심 원칙: 코드를 처음부터 다 읽지 않는다. 게임을 하면서 뭘 보고, 뭘 적어야 하는지를 미션으로 따라가면 분석이 된다.

### 준비 - 플레이 전에 이 3개만 세팅

1. 노트 준비: 종이 노트나 메모장 열고 아래 표 복사해두기
2. 스톱워치 준비: 휴대폰 스톱워치 켜두기
3. Unity 화면 세팅: Game 탭 옆에 Hierarchy 보이게 하기, Inspector도 보이게 하기. Play 누르면 Game 화면이 커지니까, Play 전에 Game 탭 크기를 작게 해두면 옆에 Hierarchy도 같이 보여서 좋다.

Play를 누르면 Unity가 Play Mode로 바뀌면서 Hierarchy에 있는 값들이 실시간으로 바뀐다. 그게 분석의 핵심이다.

### 미션 1. 맵 한 바퀴 돌기 (5분 자유 플레이)

- 키보드로 농부 움직여보기 (보통 WASD / 방향키)
- 집, 밭, 연못, 축사, 창고가 어디 있는지 확인
- 마우스로 화면 클릭하면 뭔가 되나? 안 되나?

적어야 할 것:
```
- 집은 왼쪽 위에 있음
- 밭은 가운데에 있음
- 연못은 오른쪽에 있음
- 닭/돼지/소가 있음 / 없음
```
→ 보고서 2장 게임 소개 - 맵 구성

### 미션 2. 낮 → 밤 바뀌는지 보기

- 가만히 두고 1~2분 기다리기
- 하늘이 어두워지나? 램프에 불이 켜지나? 그림자가 길어지나?
- 비가 오나? 천둥이 치나?

적어야 할 것:
```
- 00초 후에 하늘이 어두워짐
- 램프가 켜짐
- 비가 옴 / 안 옴
```
→ 보고서 5장 낮-밤 사이클

### 미션 3. 땅 갈기 (Hoe)

1. 도구 선택에서 괭이/호미 선택
2. 밭 타일을 클릭
3. 보고 적기: 클릭한 타일 색이 어떻게 바뀌었나? (갈색 흙으로?) 소리가 나나? 클릭 안 한 타일은 그대로 있나?

배우는 것: TerrainManager가 클릭한 타일만 바꾼다는 것. 전체 맵이 아니라 Vector3int 위치 하나만 바꿈.

### 미션 4. 씨앗 심기 (Plant)

1. 씨앗 선택 (밀 / 당근 / 옥수수 중 하나)
2. 갈아놓은 땅 클릭
3. 보고 적기: 새싹이 바로 보이나? (Stage0) 갈지 않은 땅에 심으면 어떻게 되나? (아무 일도 안 생기면 그게 정답) 스톱워치 시작!

배우는 것: TilledTilemap에 있는 타일에서만 심을 수 있다는 규칙. 갈지 않은 땅은 TerrainManager가 막음.

### 미션 5. 물 주기 (Water)

1. 물뿌리개 선택
2. 심은 타일 클릭
3. 보고 적기: 타일 색이 젖은 색으로 바뀌었나? (WateredTileLayer) 물 안 주고 기다리면 어떻게 되나?

배우는 것: 땅에 젖음이라는 상태가 있다는 것. GroundData가 이걸 기억하고 있음.

### 미션 6. 성장 기다리기 - 스톱워치로 재는 게 제일 중요

스톱워치를 켜고 가만히 기다려.

| 시간 | 보이는 것 |
| --- | --- |
| 0초 | 새싹 (Stage0) |
| __초 후 | 중간 (Stage1) |
| __초 후 | 다 자람 (Stage2) |

이 표를 채우는 게 분석의 80%이다. 보고서에 "당근은 30초 만에 자란다" 같은 정량 데이터가 이거다.

물 안 준 작물과 물 준 작물을 2개 나란히 심어보면 더 좋다:
- 왼쪽: 물 준 당근 → 빨리 자람
- 오른쪽: 물 안 준 당근 → 안 자람 / 시듦
- 이 비교 사진 하나면 보고서 3장이 끝남.

### 미션 7. 수확하기 (Harvest)

1. 다 자란 작물 클릭 (수확 도구 선택)
2. 보고 적기: 작물이 사라지나? 땅이 원래대로 돌아오나? 점수나 보상이 있나?

배우는 것: 수확하면 Crop 오브젝트가 사라지고, 타일이 다시 빈 땅이 된다는 순환 구조.

### 미션 8. Play 중에 Hierarchy 보기

1. Play 누른 상태로 두기
2. Hierarchy에서 Grid > Crop 클릭
3. 확인할 것: 작물을 심기 전에는 Crop 아래에 뭐가 있나? (없음) 심은 후에는 Crop 아래에 뭐가 생기나? (Crop_Carrot 같은 오브젝트가 생김) 수확하면 그 오브젝트가 사라지나?

배우는 것: 작물은 미리 배치된 게 아니라, 심을 때 생성된다는 것. 코드 안 봐도 구조가 보인다.

### 미션 9. Play 중에 Inspector 값 보기

1. Play 상태에서 심은 작물 클릭
2. Inspector 보기
3. 확인할 것: Timer 같은 숫자가 실시간으로 줄어드나? 늘어나나? Stage가 0 → 1 → 2로 바뀌면서 체크박스가 바뀌나?

Play를 멈추면 이 값들이 원래대로 돌아가니까, Play 중에 스크린샷 찍어두기. 보고서에 "실행 중 값 변화"라고 쓰면 된다.

### 미션 10. Pause로 멈춰서 자세히 보기

1. Play 중에 상단에 Pause 버튼 누르기 ( || 모양)
2. 게임이 멈춘 상태에서 Hierarchy / Inspector를 천천히 보기
3. 다시 Play 누르면 이어서 진행됨

활용 예: 비 오기 전에 Pause → VFX Rain 오브젝트가 꺼져있나? 비 올 때 Pause → VFX Rain이 켜져있나?

### 비교 실험 3가지 (코드 안 건드려도 가능)

**실험 A. 작물 비교**

| 작물 | 심고 → Stage1까지 | Stage1 → Stage2까지 | 총 시간 |
| --- | --- | --- | --- |
| 밀 | __초 | __초 | __초 |
| 당근 | __초 | __초 | __초 |
| 옥수수 | __초 | __초 | __초 |

이 표만 채우면 CropData가 작물마다 다르다는 걸 증명할 수 있다.

**실험 B. 물 유무 비교**
- 같은 작물 2개를 나란히 심기. 왼쪽만 물 주기, 오른쪽은 안 주기. 결과: 왼쪽은 자람 / 오른쪽은 안 자람 or 시듦. 보고서에 들어갈 제어 실험.

**실험 C. 밤/낮 비교**
- 낮에 작물 스크린샷 1장, 밤에 같은 위치 스크린샷 1장. 비교: 빛, 그림자, 램프가 어떻게 다른가? → 5장 빛/그림자 분석 사진.

### 플레이 노트 양식

```
[플레이 날짜: 11/17]
1. 맵 구성: 집 __, 밭 __, 연못 __, 동물 __
2. 농사 사이클:
   - 갈기: 타일 색 __로 바뀜
   - 심기: 갈지 않은 땅은 __됨
   - 물주기: 타일 색 __로 바뀜
   - 성장: Stage0 __초 → Stage1 __초 → Stage2 __초
   - 수확: 작물 __되고, 땅은 __됨
3. 낮/밤: __초 후에 어두워짐, 램프 __, 비 __
4. Play 중 Hierarchy: 심으면 Crop 아래에 __가 생김
5. 궁금한 점: 왜 __는 이렇게 동작하지?
```

### 정리 - 플레이 분석은 이렇게 하면 된다

안 해도 되는 것: 코드를 처음부터 다 읽기, VFX Graph 열어서 분석하기, Shader 이해하기
꼭 해야 하는 것:
1. 1개 작물을 심어서 수확까지 해보기
2. 스톱워치로 성장 시간 재기
3. Play 중에 Hierarchy/Inspector 보기
4. 비교하기 (물 준 거 vs 안 준 거, 낮 vs 밤, 작물 vs 작물)

---

## 8. 플레이 체크리스트 (30분, 프린트용)

> 사용법: Unity를 켜고, 이 체크리스트를 옆에 두고 하나씩 체크하면서 플레이하기. 체크가 끝나면 8장 중 2~5장 자료가 완성됨.
> 준비물: 휴대폰 스톱워치, 메모장, 스크린샷 폴더

### [0분] 시작 전 준비

- [ ] Happy Harvest 프로젝트 열었는가?
- [ ] Main Scene이 열렸는가?
- [ ] Game 탭 + Hierarchy + Inspector가 한 화면에 보이는가?
- [ ] 휴대폰 스톱워치 켰는가?
- [ ] 메모장 / 종이 노트 준비했는가?
- [ ] 스크린샷 저장 폴더 만들었는가? (예: `HappyHarvest_스크린샷`)

### [0~5분] 미션 1 - 맵 둘러보기

- [ ] 농부 이동해보기 (WASD / 방향키)
- [ ] 집 위치 확인하고 적기: ____________________
- [ ] 밭 위치 확인하고 적기: ____________________
- [ ] 연못 위치 확인하고 적기: ____________________
- [ ] 축사 / 창고 위치 확인하고 적기: ____________________
- [ ] 동물 확인: 돼지 [ ] 소 [ ] 닭 [ ] 없음 [ ]
- [ ] 스크린샷 1: 전체 맵 사진 찍기

### [5~8분] 미션 2 - 낮/밤 확인

- [ ] 가만히 두고 1~2분 기다리기
- [ ] 하늘이 어두워졌는가? 예 [ ] 아니오 [ ]
- [ ] 어두워지기까지 걸린 시간: ______초
- [ ] 램프에 불이 켜졌는가? 예 [ ] 아니오 [ ]
- [ ] 그림자가 길어졌는가? 예 [ ] 아니오 [ ]
- [ ] 비가 왔는가? 예 [ ] 아니오 [ ] / 온 시간: ______
- [ ] 스크린샷 2: 낮 사진 / 스크린샷 3: 밤 사진 (같은 위치)

### [8~12분] 미션 3+4 - 땅 갈기 + 심기

- [ ] 괭이/호미 도구 선택했는가?
- [ ] 밭 타일 2곳 클릭해서 갈았는가?
- [ ] 갈기 전 타일 색: ____________ → 갈은 후 타일 색: ____________
- [ ] 씨앗 선택했는가? 작물 이름: ____________
- [ ] 갈아놓은 땅에 심었는가? → 새싹(Stage0) 보이나? 예 [ ] 아니오 [ ]
- [ ] 갈지 않은 땅에 심어봤는가? → 결과: ____________ (아무 일 없음이 정답)
- [ ] 스톱워치 시작했는가? 시작 시간: ______
- [ ] 스크린샷 4: 심은 직후 (Stage0) 사진

### [12~18분] 미션 5+6 - 물주기 + 성장 측정

- [ ] 왼쪽 작물에만 물 줬는가? (오른쪽은 물 안 주기 - 비교 실험)
- [ ] 물 준 타일 색: ____________ (젖은 색으로 바뀌었는가? 예 [ ] 아니오 [ ])
- [ ] 물 안 준 타일 색: ____________

**성장 시간 측정표 (가장 중요)**

| 작물 | Stage0 → Stage1 | Stage1 → Stage2 | 총 시간 |
| --- | --- | --- | --- |
| 물 준 작물: ______ | ______초 | ______초 | ______초 |
| 물 안 준 작물: ______ | ______초 | ______초 | ______초 |

- [ ] Stage1로 바뀐 순간 스톱워치 기록했는가?
- [ ] Stage2로 바뀐 순간 스톱워치 기록했는가?
- [ ] 스크린샷 5: Stage1 사진 / 스크린샷 6: Stage2 사진

### [18~22분] 미션 7 - 수확하기

- [ ] 다 자란 작물(Stage2) 클릭해서 수확했는가?
- [ ] 수확 후 작물이 사라졌는가? 예 [ ] 아니오 [ ]
- [ ] 수확 후 땅이 원래대로 돌아왔는가? 예 [ ] 아니오 [ ]
- [ ] 보상 / 점수 / 아이템이 있었는가? 내용: ____________
- [ ] 스크린샷 7: 수확 직전 / 스크린샷 8: 수확 직후

### [22~27분] 미션 8+9+10 - Play 중에 Unity 화면 보기

- [ ] Play 상태에서 Hierarchy → Grid → Crop 클릭했는가?
- [ ] 심기 전 Crop 아래: ____________ (없음이 정상)
- [ ] 심은 후 Crop 아래: ____________ (Crop_작물이름이 생김이 정상)
- [ ] 수확 후 Crop 아래: ____________ (사라짐이 정상)
- [ ] Play 상태에서 심은 작물 클릭 → Inspector에서 Timer / Stage 값이 실시간으로 바뀌는가? 예 [ ] 아니오 [ ]
- [ ] Play 중 Pause( || ) 눌러서 멈춰봤는가?
- [ ] 비 올 때 Pause → VFX Rain 오브젝트가 켜져있는가? 예 [ ] 아니오 [ ]
- [ ] 스크린샷 9: Play 중 Hierarchy (Crop 아래 작물 보이는 화면)
- [ ] 스크린샷 10: Play 중 Inspector (Timer / Stage 값 보이는 화면)

### [27~30분] 비교 실험 마무리 + 정리

- [ ] 작물 비교표 채웠는가? (밀 / 당근 / 옥수수 중 2개 이상)
- [ ] 물 유무 비교 결과 적었는가? 왼쪽: ______ / 오른쪽: ______
- [ ] 낮 vs 밤 비교 사진 2장 있는가?
- [ ] 오늘 발견한 것 3줄 요약:
  1. ________________________________________
  2. ________________________________________
  3. ________________________________________
- [ ] 아직 모르는 것 / 내일 확인할 것:
  ________________________________________
- [ ] 스크린샷 총 10장 있는지 확인: [ ] 10장 완료 / [ ] 부족 (부족한 번호: ______)

### 체크리스트 완료 후 - 보고서에 바로 들어갈 자료

| 체크리스트 결과 | 보고서 어디에 쓰나 |
| --- | --- |
| 맵 구성 메모 + 전체 맵 스크린샷 | 2장 프로젝트 개요 |
| 성장 시간 측정표 | 3장 Tilemap / CropData 분석 |
| 물 유무 비교 결과 | 3장 + 8장 실험 |
| 낮/밤 사진 2장 | 5장 시간/빛 분석 |
| Play 중 Hierarchy / Inspector 스크린샷 | 3~4장 동적 분석 증거 |
| 수확 전/후 사진 | 4장 플레이어 상호작용 |

### 다음 단계

- [ ] Day 2: Hierarchy / Project / Inspector 구조도 그리기
- [ ] Day 3: 아티클 4개 읽기 (빛, 타일맵, 캐릭터, VFX)
- [ ] Day 4: Debug.Log + Breakpoint로 동적 분석
- [ ] Day 5: 무 작물 추가 + 밸런스 실험 (5장 가이드 참고)

---

## 9. 참고문헌 - IEEE 스타일

> 작성 기준: IEEE Reference Guide 기준. 온라인 자료는 `[번호] 저자, "제목," 출처, 연도. [Online]. Available: URL` 형태. 열람일은 `Nov. 17, 2026` 으로 통일. 제출일에 맞춰 바꾸면 된다.

### 프로젝트 / 다운로드

[1] Unity Technologies, "Happy Harvest | 2D Sample Project," Unity Asset Store, 2023. [Online]. Available: https://assetstore.unity.com/packages/templates/packs/happy-harvest-2d-sample-project-259218

[2] Unity Technologies, "Happy Harvest," Unity Play, 2024. [Online]. Available: https://play-prod.struckd.com/en/games/9d7f1ab8-d1f9-4b5d-9a53-f15ef61cfa73/happy-harvest

[3] Unity Technologies, "Demos, Sample Games and Projects," Unity, 2025. [Online]. Available: https://unity.com/demos

### 공식 소개 / 개발 과정

[4] S. Zachariah, "Happy Harvest demo: See the latest 2D techniques," Unity Blog, Jul. 5, 2023. [Online]. Available: https://unity.com/blog/games/happy-harvest-demo-latest-2d-techniques

[5] Unity Technologies, "New URP 2D sample project: Happy Harvest - Download now," Unity Forum, Jul. 5, 2023. [Online]. Available: https://forum.unity.com/threads/new-urp-2d-sample-project-happy-harvest-download-now.1456480/

### 튜토리얼 - 핵심 아티클 4편

[6] Unity Technologies, "2D light and shadow techniques in the Universal Render Pipeline," Unity How-to, 2023. [Online]. Available: https://activation.unity3d.com/how-to/2d-light-shadow-techniques-in-the-universal-render-pipeline

[7] Unity Technologies, "How to Create Art and Gameplay with 2D Tilemaps," Unity How-to, 2023. [Online]. Available: https://unity.com/ja/how-to/create-art-and-gameplay-2d-tilemaps-unity

[8] Unity Technologies, "How to animate 2D characters in Unity 2022 LTS," Unity How-to, 2023. [Online]. Available: https://on.unity.com/3pJuP1M

[9] Unity Technologies, "Create 2D special effects with the VFX Graph and Shader Graph," Unity How-to, 2023. [Online]. Available: https://unity.com/en/how-to/2d-special-effects-vfx-graph-shader-graph

### 영상 튜토리얼

[10] Unity Technologies, "Unity 2D Tech Art Video Tutorial Playlist," Unity Resources, Jun. 13, 2025. [Online]. Available: https://unity.com/resources/2d-technical-art-video-tutorials

[11] Unity Technologies, "Happy Harvest URP 2D Sample Game Customization Tutorial," YouTube, 2024. [Online]. Available: https://www.classcentral.com/course/youtube-happy-harvest-urp-2d-sample-game-customization-tutorial-unity-288926

### 전자책 / 문서 허브

[12] Unity Technologies, "How to make a 2D game: art, animation, and lighting for artists (Unity 6.3 LTS edition)," Unity Resources, 2026. [Online]. Available: https://unity.com/resources/2d-game-art-animation-lighting-unity-6-3-lts

[13] Unity Technologies, "2D game development," Unity Docs, 2025. [Online]. Available: https://docs.unity.com

[14] Unity Technologies, "Unity 6 Hands-On, Graphics Tech Training Sessions," Unity, 2024. [Online]. Available: https://unity.com/campaign/unity-6-graphics-training-sessions

### 본문에 인용할 때

IEEE는 본문에 `[번호]`만 쓰면 된다. 예:

> Happy Harvest는 URP 2D 라이트와 그림자, 스켈레탈 애니메이션을 보여주는 공식 2D 샘플 프로젝트이다 [1].
>
> 타일맵은 `Grid` GameObject 아래에 모여 있으며, `TerrainManager`가 타일 변경과 작물 상태를 관리한다 [7].
>
> 실시간 2D 그림자는 스프라이트에 직접 굽지 않고 Normal Map과 Mask Map 같은 Secondary Texture로 분리하여 구현한다 [6].
>
> 캐릭터는 스켈레탈 애니메이션과 Sprite Library, Sprite Swap을 이용해 표정과 방향을 전환한다 [8].
>
> 비와 물 튀김 같은 효과는 VFX Graph로, 낙엽과 발자국은 Built-in Particle System으로 구현한다 [9].
>
> 새로운 작물을 추가하는 과정은 기존 작물 프리팹을 복제하고, Secondary Texture를 연결한 뒤, 작물 데이터를 등록하는 순서로 진행한다 [11].

### 보고서에 넣을 때 팁

1. 부록에 넣을 때: 위에 있는 References를 그대로 참고문헌 섹션에 붙여넣기
2. 번호 순서: 인용된 순서대로 번호 매기는 게 IEEE 원칙이라서, 본문에서 [1]부터 순서대로 나오는지 마지막에 한 번 확인하기
3. 교수님이 URL 클릭을 원하면: `[Online]. Available:` 뒤에 있는 링크는 그대로 두기. 줄바꿈 때문에 링크가 끊기지 않게 하기
4. 열람일 넣고 싶으면: 각 항목 끝에 `[Accessed: Nov. 17, 2026].` 추가하면 된다. 예:
   > [1] Unity Technologies, "Happy Harvest | 2D Sample Project," Unity Asset Store, 2023. [Online]. Available: https://assetstore.unity.com/packages/templates/packs/happy-harvest-2d-sample-project-259218 [Accessed: Nov. 17, 2026].
