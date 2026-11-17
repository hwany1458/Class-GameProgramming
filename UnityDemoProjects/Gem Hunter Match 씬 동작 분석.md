# Gem Hunter Match 씬 동작 분석

Nov 17, 2026 · @YongHwan Lee

## 1. 개요

Happy Harvest · Tower Defense와 같은 방법으로 씬과 프리팹의 Unity YAML을 파싱했다. 다만 이 샘플은 씬에 게임플레이 컴포넌트가 **단 두 개**(Board, LevelData)뿐이고, 보드의 모양·시작 보석·장애물·스포너가 전부 **타일맵의 타일 에셋**으로 그려져 있어서, 타일맵의 칸별 타일을 읽는 파서(`parse_tilemaps.py`)를 하나 더 만들었다. 그 결과가 이 샘플 분석의 핵심이다.

| 씬 | GameObject | 게임플레이 컴포넌트 | 프리팹 인스턴스 | 보드 칸 | 역할 |
| --- | --- | --- | --- | --- | --- |
| Init | 1 | 1 (InitLoader) | 1 (GameManager) | — | GameManager 생성 후 Main으로 |
| Main | 4 | 1 (MainMenu) | 10 (장식 보석 6, 초상 카메라, 배경·조명) | — | 레벨 선택 |
| Level1 | 8 | 2 | 3 (BG, Lights, LightRotator) | 85 | 기본 보드, 보석 목표 4종 |
| Level2 | 8 | 2 | 3 | 60 | 상자 6개 부수기 |
| Level3 | 8 | 2 | 3 | 96 | 하트 모양, 전부 고정 배치 |

스크립트는 30개(에디터·Readme 제외 약 27개), Board.cs 하나가 1,565줄로 전체의 절반이다. 프리팹 49개 중 스크립트가 붙은 것은 19개(보석 6, 보너스 젠 5, 장애물 2, GameManager, UIDocument, LightRotator 등).

생성물: `scenes.json`, `prefabs.json`, `tilemaps.json`(레벨별 칸 → 타일 에셋, ASCII 보드), 구조도 HTML(레벨 보드 지도 3장 · 씬 구조 · 보석/보너스 프리팹). 클래스 단위 설명은 Doxygen 분석자료에 있다.

## 2. 레벨 씬의 뼈대

레벨 씬은 GameObject 8개가 전부다. Happy Harvest(등록형)도 Tower Defense(씬 싱글턴 세 개)도 아닌, **영속 GameManager 하나가 거의 모든 것을 들고 있고 씬은 보드만** 제공하는 구조다.

| 씬 객체 | 컴포넌트 | 역할 | 인스펙터 값 (Level1) |
| --- | --- | --- | --- |
| **Grid** | Grid, Board | 보드 싱글턴. 입력·스와프·매치 판정·낙하·보너스·힌트·종료 전부 | ExistingGems 6종(랜덤 칸에 쓸 보석), GemHoldPrefab VFX\_CellHold, HoldTrailPrefab VFX\_CellTrail. SpawnerPosition은 비어 있음(타일이 채움) |
| Grid/**Tilemap** | Tilemap + Renderer | 보드 바닥 그림. 로직 없음 | board\_tiles\_22 ×85 |
| Grid/**Logic** | Tilemap (렌더러 없음) | **보드 정의.** 칸마다 타일 에셋 하나: 랜덤 보석 / 고정 보석 / 상자 / 밧줄 / 스포너 | 랜덤 72 · 스포너 9 · 상자 3 · 밧줄 2 · 고정 보석 8 |
| Grid/**Tilemap\_Frame** | Tilemap + Renderer | 테두리 장식 | 44개 |
| **LevelData** | LevelData | 레벨 싱글턴. 목표·이동 수·음악 | MaxMove 20, LowMoveTrigger 3, Goals 파랑·보라·빨강·하양 각 8 |
| Main Camera | Camera, VFX\_Bubbles | URP 2D | — |
| BG / Lights / LightRotator | (없음) / Light2D / LightManager | 배경·조명 연출 | — |

**씬에 없지만 항상 있는 것 — GameManager (Resources, 영속)**: Init 씬이 만들고(에디터에서 레벨을 바로 열면 첫 접근 시 Resources에서 자동 생성) 씬을 넘어 살아남는다. 안에 있는 것: Coins · Lives 5 · Stars(메모리만, 저장 없음), GameSettings(힌트 8초, 낙하 속도 10, 커브 5종, 보너스 젠 5종 목록, 상점 품목 6, 사운드), 시작 부스터 4종×2, VFX 풀 시스템, 음악·효과음 소스, 승/패 VFX, 그리고 자식 프리팹 **UIDocument(UIHandler)** — 즉 HUD·상점·설정·종료 화면 전부.

연결 방식: 인스펙터 참조는 Board.ExistingGems와 LevelData.Goals 정도가 전부이고, 나머지는 모두 싱글턴 직접 호출(`Board.Instance`, `LevelData.Instance`, `GameManager.Instance`, `UIHandler.Instance`)과 C# 이벤트(OnAllGoalFinished, OnNoMoveLeft, OnGoalChanged, OnMoveHappened)다. **UnityEvent는 씬 전체에 하나도 없다.** GameManager는 레벨이 로드되면 `FindFirstObjectByType<Board>`로 보드를 찾아 Init을 부른다.

## 3. 보드는 타일로 그린다

`Tiles/` 폴더의 타일 에셋은 Unity TileBase를 상속한 세 종류의 **저작(authoring) 타일**이다. 실행 시 그림을 그리지 않고, 씬이 로드될 때 `StartUp(position)`에서 Board에 자기 칸을 등록한다.

| 타일 에셋 | 스크립트 | StartUp이 하는 일 | 보드 지도 기호 |
| --- | --- | --- | --- |
| RandomGemPlacer | GemPlacerTile (PlacedGem 비움) | RegisterCell — 빈 칸, GenerateBoard가 나중에 ExistingGems 중 하나로 채움 | ? |
| BlueGem · RedGem · GreenGem · YellowGem · WhiteGem · PinkGem | GemPlacerTile (PlacedGem = 해당 Gem 프리팹) | RegisterCell + NewGemAt(고정 색) | 색 칸 |
| Crate | GemPlacerTile (PlacedGem = WoodenCrate) | RegisterCell + NewGemAt(상자) — 상자는 Gem의 파생 | # |
| TileBlockerPlacer | ObstaclePlacer (ObstaclePrefab = TieBlock) | Instantiate(밧줄) — 그 칸의 보석을 묶는다 | ≈ |
| GemSpawner | GemSpawner | RegisterSpawner — 위쪽에서 내려오는 보석의 출발점 | S |

그래서 레벨 디자인 = 타일 팔레트로 Logic 타일맵 칠하기 + LevelData에 목표 입력. 코드도 데이터 에셋도 건드리지 않는다.

| 레벨 | 모양 | 칸 | 랜덤 | 고정 보석 | 상자 | 밧줄 | 스포너 | 이동 | 목표 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Level1 | 9×10 사각, 아래 가운데 3칸 비움 | 85 | 72 | 8 (가운데 십자 모양) | 3 | 2 | 9 (맨 윗줄) | 20 | 파랑·보라·빨강·하양 각 8 |
| Level2 | 육각형 | 60 | 54 | 0 | 6 | 0 | 10 (윗줄 4 + 어깨 6) | 20 | 상자 6 |
| Level3 | 하트 | 96 | **0** | 96 (빨강 24 · 보라 23 · 노랑 18 · 파랑 11 · 초록 10 · 하양 10) | 0 | 0 | 28 (윗줄 + 하트 안쪽 빈 공간 아래) | 20 | Level1과 동일 |

읽을 점: 세 레벨의 이동 수가 같고, 난이도는 오직 **보드 모양과 초기 배치**로만 조절된다. Level3은 랜덤 칸이 하나도 없어 시작 장면이 항상 같고(디자이너가 색으로 하트를 그렸다), 스포너가 보드 안쪽에도 있어 위에 칸이 없는 고립 영역에도 보석이 공급된다. 스포너가 없는 열은 보석이 보충되지 않으므로, 스포너 배치가 곧 레벨 규칙이다.

초기 생성 규칙(Board.GenerateBoard): 고정 보석은 칠한 그대로 두고, 빈 칸만 왼쪽/아래 두 칸이 같은 색이면 그 색을 빼고(2×2 정사각도 검사) 랜덤으로 채운다. 즉 "시작하자마자 터지는 매치"는 랜덤 칸에서만 막히고, 고정 보석을 잘못 칠하면 막지 못한다.

## 4. 한 판의 흐름 — 어느 객체가 무엇을 하는가

| 단계 | 일어나는 일 | 담당 |
| --- | --- | --- |
| 씬 로드 | Logic 타일맵의 타일들이 StartUp으로 Board에 칸·고정 보석·스포너·장애물 등록. GameManager가 Board를 찾아 Init: UIHandler.Init → GenerateBoard(랜덤 칸 채우기) → FindAllPossibleMatch → 페이드인 후 입력 허용 | GemPlacerTile/GemSpawner/ObstaclePlacer, Board.Init, LevelData.Awake(RemainingMove = 20) |
| 입력 | 칸을 누르면 Hold VFX, 끌다가 놓을 때 이동 거리가 0.5 이상이면 그 방향(상하좌우 중 큰 쪽)의 이웃과 스와프 예약. 두 번 탭이 아니라 **드래그** | Board.CheckInput, GameManager.ClickAction/ClickPosition (Input System) |
| 스와프 | Forward 단계로 두 보석이 자리를 바꾼다. 매치가 생기면 확정하고 LevelData.Moved()로 이동 1 차감; 없으면 Return 단계로 되돌리고 이동은 소모하지 않는다. 밧줄 묶인 보석은 못 움직인다 | Board.Update (SwapStage) |
| 매치 판정 | 가로·세로 3개 이상 일렬 + 보너스 젠의 Shapes(4일렬, 5일렬, L자, 2×2) 검사. Shapes가 맞으면 그 매치의 원점에 보너스 젠을 남긴다 | Board.DoMatchCheck, Match, BonusGem.Shapes |
| 삭제 | 매치된 보석은 DeletionTimer가 1을 넘을 때(보너스 발동은 즉시) 사라진다. 사라질 때마다 LevelData.Matched(gem) → 목표 카운트 감소, UI로 날아가는 연출. 한 매치에서 4번째 보석부터 코인 +1 | Board.Update 매치 틱, LevelData.Matched, GameManager.ChangeCoins |
| 장애물 | 매치 인접 칸의 상자는 체력 −1 (3 → 0에서 파괴, 목표에 셌 수 있음). 밧줄 칸의 보석이 매치되면 LockState 한 단계 해제(2단계) | Crate.Damage, TieBlocker |
| 낙하·보충 | 빈 칸 위의 보석이 내려오고(FallSpeed 10, 가속·바운스 커브), 스포너 칸은 새 보석을 위에서 떨구다. 멈추면 다시 매치 판정 → 연쇄 | Board (m\_TickingCells), GemSpawner 등록 위치 |
| 보너스 발동 | 보너스 젠이 매치에 포함되거나 두 보너스를 스와프하면 Use(): 로켓은 한 줄, 폭탄은 범위, 컴러는 같은 색 전부 — ForcedDeletion으로 즉시 삭제(코인 없음) | LineRocket / SmallBomb / LargeBomb / ColorClean |
| 부스터 | 하단 부스터 버튼 → 보드가 어두워지고 대상 칸 클릭 → 그 칸에 보너스 젠 생성 후 발동, 보유 수 −1 | UIHandler → Board.ActivateBonusItem → GameManager.UseBonusItem |
| 힌트 | 아무 일도 없는 8초가 지나면 가능한 스와프 하나를 골라 HintLight가 그 방향으로 왕복 | Board.Update, FindAllPossibleMatch |
| 종료 예약 | 목표가 모두 0 → OnAllGoalFinished(+WinStar) / 이동 0 → OnNoMoveLeft. 둘 다 입력을 끄고 FinalStretch 플래그만 세운다 | LevelData, GameManager |
| 종료 | 보드의 낙하·연쇄가 모두 멈춘 프레임에 ShowEnd: GoalLeft==0이면 승리(마지막 수로 달성해도 승리), 아니면 패배. 승/패 VFX·보이스·캐릭터 애니메이션, Replay / Select Level 버튼 | Board.Update, UIHandler.ShowEnd |

이 게임에는 "턴"이 없다. 모든 것이 Board.Update 한 곳에서 매 프레임 틱되고(입력 → 스와프 보간 → 매치 타이머 → 낙하 → 보드 액션 → 힌트/종료), 상태는 플래그 여러 개(m\_SwapStage, m\_FinalStretch, m\_BoardChanged, m\_TickingCells)로 표현된다. Tower Defense의 LevelState 같은 명시적 상태 기계가 없다는 것이 학생이 기능을 더할 때 가장 먼저 부딪히는 벝이다.

## 5. 입력과 UI 동작

**입력**: Input System의 액션 두 개(ClickAction = 마우스 왼쪽 / 터치, ClickPosition)를 GameManager가 들고 Board.CheckInput이 매 프레임 읽는다. 키보드 없음. 마우스와 터치가 같은 코드 경로라 **모바일에서 추가 작업 없이 동작**한다(세 샘플 중 유일). UIHandler.ApplySafeArea가 노치 영역을 피한다.

**UI = UI Toolkit, UIDocument 프리팹 하나** (GameManager 자식, 영속). uGUI 캨버스가 없고 UXML 템플릿 4종(GoalGem, ShopEntry, ButtonBooster, LevelEntry)을 코드에서 Instantiate한다.

| 영역 (UXML 이름) | 내용 | 갱신 계기 |
| --- | --- | --- |
| 상단 바: CoinLabel · LiveLabel · StarLabel | 코인 · 생명 · 별 | UpdateTopBarData (시작·상점·종료 시) |
| GoalContainer + GoalGemTemplate × n | 목표 보석 아이콘과 남은 수, 달성 시 Checkmark | OnGoalChanged; 매치된 보석이 아이콘으로 날아가는 연출(MatchFlyCurve) |
| MoveCounter · LevelName | 남은 이동 수, 레벨 이름("Level") | OnMoveHappened; 3 이하에서 캐릭터 LowMove 애니메이션 |
| MiddleTopSection / RenderTarget | 인어 캐릭터 초상 (PortraitCamera 렌더 텍스처) | Idle / LowMove / Win / Lose 애니메이션 |
| BoosterZone + ButtonBooster × 4 | 부스터 아이콘과 보유 수(LabelBoosterNumber) | 클릭 → Board.ActivateBonusItem, 보드 어둡게, 칸 선택 대기 |
| ButtonMenu → Settings | Main/Music/SFX 슬라이더(저장됨), ReturnButton(레벨 선택으로), CloseButton | — |
| ShopButton → Shop | ShopContentScroll에 ShopEntry × 6: 아이콘·이름·가격·BuyButton(코인 부족 시 비활성) | 구매 → ChangeCoins(−가격) + 효과(부스터 +1 / 생명 +1 / 코인 +100) |
| EndScreen: WinTitle / LoseTitle, ReplayButton, SelectLevelButton | 승패 화면 | ShowEnd → 효과음 후 버튼 표시. Replay = 같은 씬 다시 로드, Select = 씬 1(Main) |
| Cover | 씬 전환 페이드 | — |
| (개발 빌드만) Debug 메뉴 | 보석·보너스를 골라 칸에 직접 놓기 | UNITY\_EDITOR / DEVELOPMENT\_BUILD |

화면에 **없는** 정보: 점수(점수 개념 자체가 없다), 레벨 진행도·별 조건, 다음 보석 미리보기, 일시정지 표시(설정 메뉴를 열어도 시간은 흐른다 — 단, 턴제이라 문제는 없다).

## 6. 보석 · 보너스 젠 · 장애물 프리팹과 경제

**보석 프리팹 6종**: Gem 컴포넌트 하나(GemType 0 파랑 · 1 노랑 · 2 하양 · 3 보라 · 4 초록 · 5 빨강) + 스프라이트 + 매치 VFX\_CellCrash. 낙하·바운스·찌그러짐 커브는 프리팹이 아니라 GameManager의 VisualSettings에 공통으로 있다.

**보너스 젠 5종** — BonusGem : Gem. 각각 `Shapes`(어떤 매치 모양이 나를 만드는가)와 `Use()`(발동하면 무엇을 지우는가)를 가진다.

| 프리팹 | 스크립트 | 만드는 모양 (Shapes) | 발동 효과 | GemType |
| --- | --- | --- | --- | --- |
| HorizontalBonus | LineRocket (Vertical 0) | **세로** 4개 일렬 | 가로 한 줄 전부 | −1 |
| VerticalBonus | LineRocket (Vertical 1) | **가로** 4개 일렬 | 세로 한 줄 전부 | −5 |
| SmallBomb | SmallBomb | 2×2 정사각 | 주변 작은 범위 | −4 |
| LargeBomb | LargeBomb | L자 (3+3, 회전·대칭 허용) | 주변 큰 범위 | −2 |
| ColorBonus | ColorClean | 5개 일렬 (회전 허용) | 스와프한 색 전부 제거 | −3 |

모양은 프리팹 인스펙터의 셀 좌표 목록이다(BonusGemEditor가 격자로 편집). 한 매치가 여러 모양에 맞으면 그중 하나를 랜덤으로 고른다. 보너스끼리 스와프하면 둘 다 발동. 새 보너스(예: 십자 폭탄)는 BonusGem 상속 + 프리팹 + GameManager.BonusSettings 등록으로 추가한다.

**장애물 2종**

| 프리팹 | 스크립트 | 동작 |
| --- | --- | --- |
| WoodenCrate | Crate : Gem (GemType 99) | 칸을 차지하는 "보석". 움직이지 않고 매치되지 않으며, 인접 매치마다 체력 3 → 2 → 1 → 파괴(스프라이트 3단계). 파괴되면 Matched로 목표에 셌다(Level2) |
| TieBlock | TieBlocker : Obstacle | 칸 위에 얹힌 밧줄. 그 칸의 보석은 스와프 불가, 그 보석이 매치되면 LockState 2 → 1 → 해제 |

**경제 수치** (전부 에셋·프리팹에서 읽음)

| 항목 | 값 |
| --- | --- |
| 시작 자원 | 코인 0 · 생명 5 · 별 0 · 부스터 소형폭탄·세로로켓·가로로켓·컴러 각 2 |
| 코인 수입 | 한 매치에서 4번째 보석부터 개당 +1 (4매치 1, 5매치 2). 보너스 발동으로 지운 것은 0 |
| 상점 | 소형 폭탄 20 · 가로/세로 로켓 30 · 대형 폭탄 50 · 컴러 150 · 생명 50 · **코인 +100 = 0원** |
| 별 | 승리마다 +1, 저장 안 됨 |
| 생명 | LoseLife()를 부르는 코드가 없음 — 절대 줄지 않는다 |

상점 품목 중 세 개의 이름이 전부 "Bomb"이고, 대형 폭탄은 상점에서만 살 수 있고 시작 부스터에는 없다.

## 7. Init · Main 씬

**Init** (빌드 인덱스 0): GameObject 하나, InitLoader. GameManager 프리팹을 만들고(DontDestroyOnLoad) Main을 로드한다. Happy Harvest의 Loader 씬과 같은 역할.

**Main** (빌드 인덱스 1): MainMenu 컴포넌트 + UIDocument(MainMenu.uxml). `LevelList`(ScriptableObject, 씬 3개)의 개수만큼 LevelEntry 버튼을 만들고 번호를 붙인다. 클릭 → Cover 페이드아웃 → 트랜지션 끝 이벤트에서 `LevelList.LoadLevel(i)`. 배경에 보석 프리팹 6개와 인어 초상 카메라가 장식으로 놓여 있다. GameManager.MainMenuOpened()가 레벨 HUD를 숨기고 메뉴 음악으로 바꾼다.

메뉴에 없는 것: 옵션(볼륨은 레벨 안 설정 메뉴에만), 레벨 잠금·별 표시·진행 저장, 종료 버튼. 모든 레벨이 처음부터 열려 있고 버튼은 번호뿐이다. 새 레벨을 더하면 LevelList에 씬을 넣는 것만으로 버튼이 늘어난다(빌드 설정에도 씬을 추가해야 한다 — 에디터에서는 null 검사만).

## 8. 발견 사항과 확장 지점

1. **레벨 = 씬 + 타일 칠하기.** 코드도 데이터 에셋도 아닌 타일맵이 보드를 정의한다. 새 레벨은 씬 복제 → Logic 타일맵 다시 칠하기 → LevelData 목표 → LevelList 등록. 레벨 에디터·절차적 보드를 만들려면 TileBase.StartUp이 하는 네 가지 등록(RegisterCell / NewGemAt / RegisterSpawner / 장애물)을 코드에서 대신 부르면 된다.
2. **생명은 줄지 않는다.** GameManager.LoseLife()를 부르는 곳이 없다. 패배해도 Lives 5 그대로이고, 상점의 "Live 50"은 쓸 데가 없는 상품이다. 별은 승리마다 +1 되지만 저장되지 않아 앱을 끄면 0. 저장되는 것은 볼륨뿐 — "진행 저장"이 세 샘플 중 가장 비어 있다.
3. **코인은 4매치부터, 그것도 4번째 보석부터 1개.** 3매치 0, 보너스 발동 0. 상점 가격 20\~150에 비해 수입이 매우 느리고, 그 대신 "Coin x100 — 0원" 테스트 상품이 남아 있다. 경제 밸런스 과제의 출발점.
4. **Level3은 랜덤 칸 0개.** 96칸 전부 색 지정. GenerateBoard는 빈 칸만 매치 없이 채우고 고정 보석은 손대지 않으므로, 디자이너가 3개를 나란히 칠하면 시작하자마자 터진다 — 고정 배치 검증 에디터 도구가 확장 과제.
5. **스포너 배치가 규칙이다.** 스포너 없는 열은 보충되지 않는다. Level3은 하트 안쪽 빈 공간 아래에 스포너를 두어 고립 영역을 살렸다 — "보드 모양 디자인" 수업의 예제.
6. **막힌 보드 처리가 없다.** FindAllPossibleMatch가 0개를 돌려도 셔플하지 않고, 그 상태에서 힌트 코드가 빈 목록의 0번을 읽어 예외가 날 수 있다. 막힘 감지 + 셔플이 가장 실용적인 상속 없는 추가 기능.
7. **힌트 8초.** 코드 기본값 2초를 프리팹에서 8초로 덮어썼다 — "코드의 기본값과 인스펙터 값이 다를 때 무엇이 진짜인가"의 예제(Happy Harvest의 하루 120초와 같은 유형).
8. **UnityEvent 0개, 씬 간 참조 0개, Board 1,565줄.** 세 샘플 중 가장 "코드 중심"이고, Board가 입력·스와프·매치·낙하·보너스·힌트·종료를 혼자 한다. 기능을 더하기 전에 입력·매치 판정·상태 기계를 분리하는 리팩터링이 좋은 연습이다.
9. **모바일 준비는 된 샘플.** 터치와 마우스가 같은 경로, SafeArea 처리, UI Toolkit — 온라인(WebGL) 배포 후 모바일 브라우저에서도 바로 된다.

출처: 업로드된 GemHunterMatchUntyTechProject의 씬·타일맵·프리팹·Data 에셋 직접 파싱(scenes.json, prefabs.json, tilemaps.json), Scripts/ 소스, Gem Hunter Match Doxygen 분석자료.
