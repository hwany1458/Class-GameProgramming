# Tower Defense Template — Game Design Document

Nov 17, 2026 · @YongHwan Lee

Unity Learn의 GDD 템플릿 순서를 따라, 업로드된 Tower Defense Template 프로젝트의 소스·씬·데이터 에셋에서 읽은 사실만으로 쓴 "역설계" GDD다. 템플릿에 없는 것은 「확장 여지」로 표시했다. Happy Harvest GDD와 같은 구성이므로 두 문서를 나란히 놓고 "승패가 있는 게임"과 "없는 게임"의 GDD가 어떻게 달라지는지 비교할 수 있다.

## 1. 기본 정보

| 항목 | 내용 |
| --- | --- |
| **게임 이름** | Tower Defense Template (Unity 공식 템플릿, 고유 제목 없음 — 「확장 여지」: 학생 팀이 이름을 붙인다) |
| **장르** | 3D 타워 디펜스 (고정 경로 + 제한 배치 영역). 비교 대상: Kingdom Rush, Bloons TD의 "길 위에는 못 짓고 정해진 칸에만 짓는" 계열. 단, Level2는 넓은 그리드로 "미로 만들기(배치로 경로 바꾸기)" 요소도 있다 |
| **게임 요소** | ① 타워를 고르고 에너지를 써 배치 영역에 짓는다 ② 웨이브를 시작하고 적이 경로를 따라 오는 것을 본다 ③ 타워가 자동으로 쇬 적을 잡고 에너지를 얻는다 ④ 그 에너지로 타워를 업그레이드하거나 팔고 다시 짓는다 ⑤ 10웨이브를 버티고 홈베이스 잔여 체력로 별을 받는다 ⑥ 다음 레벨로 |
| **플레이어 수** | 1인. 네트워크·로컬 멀티 없음 (상태가 LevelManager 싱글턴 하나에 모여 있어 협동 디펜스로 확장할 때는 여기서부터 나눠야 한다) |

## 2. 기술 사양

| 항목 | 내용 |
| --- | --- |
| **기술 형태** | 3D. Built-in 렌더 파이프라인 + 동봉 PostProcessing 스택(고/저 화질 프로필 2종), NavMesh 경로 탐색, 오브젝트 풀링. Unity 6000.3.10f1 갱신판 |
| **시점** | 위에서 내려다보는 고정 각도 카메라(CameraRig). 팬·줄(10\~20, 최대 60) 가능, 회전 없음. 레벨 선택 화면은 카메라 애니메이션으로 진입 |
| **플랫폼** | PC(Windows/Mac) 및 모바일(iOS/Android) — 입력 스킴 2종과 모바일용 배치 타일·파티클이 따로 있다. 이 강의에서는 WebGL 배포가 목표(포스트프로세싱 부하 확인 필요) |
| **언어** | C#. 데이터는 ScriptableObject(TowerLevelData 16, AgentConfiguration 8, TowerLibrary 5, LevelList, SimpleAlignment 2)와 씬 컴포넌트(웨이브). 저장은 JSON(persistentDataPath) |
| **기기** | PC: 마우스 + 키보드(숫자키·Esc·WASD). 모바일: 터치(탭·드래그·핀치·플릭). 게임패드 없음 |

코드 구조는 세 계층(Core → ActionGameFramework → TowerDefense)으로, 아래 계층은 타워 디펜스를 모른다. 새 타워·새 적은 데이터 에셋 + 프리팹만으로 추가할 수 있도록 설계되어 있다 (공식 문서 "Creating a Tower" 절).

## 3. 게임플레이

**처음 3분 (Level1)**

레벨 선택에서 "Level 1 — Don't allow enemies to rush through"를 고르면 카메라가 협곡 위로 내려앉는다. 구불구불한 외길이 왼쪽 적 출현 지점에서 오른쪽 헤드쿼터까지 이어져 있고, 길가에 파란 격자 타일이 깜빡인다 — 거기가 타워를 지을 수 있는 곧이다. 왼쪽 아래 Energy: 10. 오른쪽 사이드바에 Assault Cannon(4), Rocket(12), Laser(15) 세 버튼이 있고, 1을 누르거나 버튼을 클릭하면 반투명 기관총 고스트가 마우스를 따라온다. 격자 위에서 초록으로 변할 때 클릭하면 건설 이펙트와 함께 타워가 서고 에너지가 6으로 줄어든다. 하나 더 짓고(2 남음) 위쪽 웨이브 패널의 시작 버튼을 누른다.

Hoverbuggy 열여섯 대가 잘게 떨며 길을 따라 달려오고, 기관총 포탑이 휘돌아 초당 네 발씩 쇠다. 한 대가 터질 때마다 에너지가 1씩 오른다. 두 대가 빠져나가 헤드쿼터 앞에 서서 2초 충전하더니 터지고, 기지 체력이 10에서 8로 떨어진다. 42초 뒤 다음 웨이브 — 그 사이에 에너지 16이 모였다. 기관총을 클릭하면 위에 패널이 뜨고 Upgrade(8)와 Sell(2)가 보인다. 업그레이드하면 포탑이 큰 모델로 바뀜고 발사율이 오른다. 4웨이브에 느리고 둔한 Hovertank가 섮이기 시작하고, 여기서 로켓(12)을 살지 레이저(15)를 살지 고민하게 된다. 6웨이브부터 타워 위로 날아 넘어오는 Hovercopter가 나오는데, 로켓은 맞히지 못한다. 10웨이브는 Hoverboss 한 대 — 체력 600, 느리지만 타워를 쇜며 온다. 보스가 터지면 "LEVEL 1 COMPLETE!"와 별 세 개, Next Level 버튼.

**게임플레이 아웃라인**

| 항목 | 내용 |
| --- | --- |
| 앱 실행 | MainMenu 씬: 타이틀 + 배경에 타워 모델 회전. Play / Options / Quit / Learn More |
| 옵션 | Master · SFX · Music 볼륨 (저장됨). 그 외 없음 |
| 스토리 | 없음. 레벨 설명 한 줄뿐 ("Create mazes to slow enemies") — 「확장 여지」 |
| 모드 | 레벨 5개뿐. 난이도·무한·협동 없음 — 「확장 여지」 |
| 게임 요소 | 타워 6종(기관총 · 로켓 · 레이저 · EMP · 파일런 · 슈퍼타워) × 3레벨, 적 4종 × 일반/Super, 에너지 경제 |
| 레벨 | 5개, 각 10웨이브, 적 141\~209기. 레벨이 오를수록 타워 종류가 열리고 경로가 복잡해진다(Level5는 두 갈래). 잠금 없음 — 모두 처음부터 열림 |
| 조작 | 마우스 클릭/드래그, 숫자키 1\~9, Esc, 휠, WASD / 터치 탭·드래그·핀치 (7절) |
| 승리 | 10웨이브가 모두 스폰된 뒤 살아 있는 적이 0 |
| 패배 | 홈베이스 체력 10 → 0 |
| 종료 | 승패 화면(별 0\~3, 저장) → Next Level / Main Menu. 엔딩·크레딧 없음 |
| 왜 재미있나 | "어디에 무엇을 언제 짓느냐"의 결정이 연쇄적으로 값을 치르고, 적 종류(비행·보스·물량)가 그 결정을 계속 흔든다. 에너지가 처치에서만 나오므로 "잘 지으면 더 지을 수 있다"는 양의 순환이 있다 |

**핵심 기능**

- 건설 중 마우스를 따라오는 고스트와 사거리 원 — 짓기 전에 닿는 범위를 본다
- 타워 3단계 업그레이드(모델이 바뀜)과 판매, 스탯·DPS를 보여주는 정보 패널
- 역할이 다른 타워: 지상 범위(로켓) · 단일 고피해(레이저) · 감속(EMP) · 생산(파일런) · 일회용 자폭(슈퍼타워)
- 비행 적과 보스 — 로켓만으로는 못 막는다
- 막히면 타워를 부수고 지나가는 적 (미로를 완전히 막을 수 없음)
- 레벨별 별점과 저장, PC·모바일 입력 둘 다

## 4. 디자인 문서

**디자인 지침** (템플릿에서 읽어낼 수 있는 제약과 목표)

- 전연령 SF 호버 차량 — 피·인명 없이 기계만 터진다.
- 한 레벨 5\~8분. 일시정지는 있지만 중간 저장은 없다 — 레벨은 한 번에 끝내는 단위다.
- 플레이어의 개입은 **배치·업그레이드·판매·웨이브 시작** 넷뿐. 조준·발사·스킬 사용은 없다. 새 기능을 더할 때도 이 네 동사 안에서(또는 다섯 번째 동사를 의식적으로 추가해서) 설계한다.
- 타워·적은 데이터 + 프리팹으로 늘린다. Core/ActionGameFramework 계층은 고치지 않는다 (학생 코드 리뷰 기준).
- PC와 모바일을 동시에 지원한다. UI·입력을 바꿀 때는 두 스킴 모두에서 확인한다.
- 한 줄 목표: "제한된 에너지로 어디에 무엇을 지을지 고민하게 만들고, 그 선택의 결과를 즉시 보여 준다."

**게임 디자인 정의**

| 항목 | 정의 | 구현 |
| --- | --- | --- |
| **승리** | 마지막(10번째) 웨이브의 스폰이 끝난 후(AllEnemiesSpawned) 살아 있는 적이 0이 되는 순간. 홈베이스가 1이라도 남아 있으면 승리 | LevelManager: 적 카운터 + LevelState |
| **패배** | 홈베이스 체력 10이 0. 적이 도착할 때마다 1(보스 2)씩 깎인다 — 즉 보통 적 10기 통과가 한계 | PlayerHomeBase(Damageable).died → homeBaseDestroyed |
| **별점** | 승패 시 홈베이스 잔여 체력 비율: 100% → 3별, 50\~90% → 2별, 0\~50% → 1별, 패배 0별. 레벨별 최고 별이 저장된다 | EndGameScreen.CalculateScore → GameManager.CompleteLevel |
| **레벨 전환** | 승리 화면의 Next Level → LevelList의 다음 씬. 실패 시 Retry 없이 메인 메뉴로. 레벨 사이에 이어지는 상태는 별·볼륨뿐이고 에너지·타워는 레벨마다 초기화 | SceneLoader, GameDataStore |
| **핵심 초점** | "지금 가진 에너지로 무엇을 지을 것인가". 웨이브 사이 15\~60초의 휴지가 생각할 시간이고, 웨이브 중에도 지을 수 있다 | TimedWave.timeToNextWave |
| **경제** | 시작 10\~20. 수입은 처치 보상(1\~16)과 파일런(Level5)뿐, 상시 수입 0. 판매가는 누적 투자의 약 절반(레이저 Lv3 예외) | LevelManager.currency, LootDrop, TowerLevelData.sell |
| **적의 행동** | 경로 노드를 따라 홈베이스로. 평소엔 타워를 공격하지 않고, 길이 타워로 막혔을 때만 그 타워를 부수고 지나간다. 비행 적은 타워 위로 통과 | AttackingAgent / FlyingAgent, NavMesh PathPartial |

이 표의 네 번째 줄이 Happy Harvest와 가장 다른 점이다. 농장은 "끝이 없는 게임"이어서 승패 정의가 「확장 여지」였지만, 이 템플릿은 승패·별점·저장이 완성되어 있어 GDD의 빈칸이 "규칙"이 아니라 "내용(스토리·모드·변화)"에 있다.

## 5. 게임 플로우차트

템플릿이 요구하는 다섯 영역(메뉴 · 개요 · 게임플레이 · 플레이어 조작 · 게임 오버) 중 스토리 개요는 없으므로 네 영역이다. 각 상자의 번호 ①\~⑫은 8절 참조표의 번호와 같다.

&#91;embedded content: 타워 디펜스 게임 플로우 · ①\~⑫ · 판정 3, 종료 2\]

이 흐름의 특징은 나가는 길이 두 개라는 점이다. 위쪽 루프(⑤→⑥→⑦→⑧→⑨)는 플레이어가 돌리고, 아래쪽 두 판정(마지막 웨이브·체력 0)은 LevelManager가 매 프레임 감시한다. Happy Harvest의 플로우차트에는 없던 종료 상자 두 개가 여기에는 있다.

## 6. 플레이어 정의

**플레이어 정의**: 화면에 보이는 아바타가 없는 "지휘관"이다. 플레이어 = 카메라(CameraRig) + 에너지 잔고 + 홈베이스 체력의 세 가지로 정의된다. 할 수 있는 행동은 다섯: 카메라 팬·줄, 타워 선택·배치, 업그레이드, 판매, 웨이브 시작. 무기는 없고 타워가 그 자리를 대신한다 — 템플릿의 "Health / Weapons / Actions" 권장 항목은 각각 홈베이스 체력 / 타워 6종 / 위 다섯 행동으로 대응한다.

**플레이어 속성** (각 속성이 바뀔 때의 피드백 포함)

| 속성 | 범위·초기값 | 바뀜 때 | 피드백 |
| --- | --- | --- | --- |
| 홈베이스 체력 | 0\~10, 시작 10 | 적 도착 −1(보스 −2). 회복 없음 | 헤드쿼터 위 체력 바(항상 표시), HUD 숫자, 충전 파티클·소리 → 타격 파티클·소리. 0이면 폭발 후 패배 화면 |
| 에너지(통화) | 0 이상, 시작 10/15/20/15/20 | 처치 +1\~16, 파일런 +1, 배치·업그레이드 −, 판매 + | HUD "Energy:" 숫자 갱신. 부족하면 구매 버튼 비활성(회색) — 별도 안내 없음 |
| 현재 웨이브 | 1\~10 | 웨이브 시작 시 | HUD 웨이브 번호 · 다음 웨이브까지 채워지는 바 |
| GameUI 상태 | Normal / Building / BuildingWithDrag / Paused / GameOver | 버튼·키·Esc | Building: 고스트 + 사거리 원 + 배치 가능 타일 강조(녹/적). Paused: 메뉴 오버레이, 시간 정지 |
| 선택 타워 | 없음 / 타워 1개 | 타워 탭 | 타워 위 패널(이름·설명·DPS·업그레이드 비용·판매가) + 사거리 원 |
| 카메라 | 줄 10\~20 | 휠·핀치, 팬 | 즉시 이동(댐핑 0) |
| 레벨별 별 (영속) | 0\~3 × 5레벨 | 승패 시 최고값 갱신 | 종료 화면 별 3개, 레벨 선택 버튼의 별 |

타워 자체도 체력 5(슈퍼타워 20)를 가진 Damageable이다. 피해를 입으면 체력 바가 나타나고(가득 찼을 때는 숨김), 0이면 폭발하며 사라진다 — 환불 없음.

**플레이어 보상** (긍정적 영향을 주는 것)

| 보상 | 효과 | 얻는 방법 |
| --- | --- | --- |
| 처치 보상 (LootDrop) | 에너지 +1 / 2 / 3 / 4 / 4 / 6 / 8 / 16 | 버기 · 슈퍼버기 · 콥터 · 탱크 · 슈퍼콥터 · 슈퍼탱크 · 보스 · 슈퍼보스 처치 |
| 파일런 생산 | 에너지 +1 / 10s → 8s → 5s | Energy Pylon 배치(Level5만, 8 · 업그레이드 8·8) |
| 판매 환급 | 누적 투자의 약 50%(레이저 Lv3 105%) | 선택 타워 → Sell → 확인 |
| 웨이브 사이 휴지 | 15.5\~60초의 계획 시간 | 자동 (TimedWave.timeToNextWave) |
| 별 | 다음 방문 시 표시만. 잠금 해제·보너스 없음 | 「확장 여지」 |
| 파워업 · 일회용 스킬 · 홈베이스 회복 · 이자 수입 | 없음 | 「확장 여지」 (슈퍼타워가 유일한 일회용 요소) |

## 7. 사용자 인터페이스

**조작** (InputController → 스킴 2종, 플랫폼에 따라 자동 선택)

| 행동 | 키보드 · 마우스 | 터치 | 비고 |
| --- | --- | --- | --- |
| 타워 선택(건설 모드) | 사이드바 버튼 클릭 / 숫자키 1\~9 | 버튼 탭, 또는 버튼에서 바로 드래그 | 숫자키는 버튼 순서 = TowerLibrary 순서 |
| 배치 | 배치 영역 위 왼클릭 | 드래그 후 놓기 → 확인 버튼(TowerPlaceConfirmationUI) | 터치는 확인 단계가 하나 더 |
| 건설 취소 | Esc / 우클릭 / 빈 곳 클릭 | 취소 버튼 / 안드로이드 백 | — |
| 타워 선택(정보) | 타워 왼클릭 | 타워 탭 | 패널에서 업그레이드 / 판매(확인 한 번 더) |
| 웨이브 시작 | HUD 버튼 | HUD 버튼 | 건설 단계에서만 |
| 카메라 팬 | WASD/방향키, 화면 가장자리(40px), 우클릭·가운데 드래그 | 한 손가락 드래그, 플릭 관성 | — |
| 줄 | 휠 | 핀치 | 10\~20 |
| 일시정지 | Esc (건설 중이 아닐 때) | HUD 버튼 | 캨버스 오버레이, timeScale 0 |
| 게임패드 | 없음 | — | 「확장 여지」 |

**HUD** (uGUI, 레벨 씬마다 프리팹 8개): 왼쪽 위 홈베이스 체력·Energy(PlayerBaseLife), 위 가운데 웨이브 번호·진행 바·시작 버튼(WaveContainer), 오른쪽 타워 구매 사이드바와 호버 정보 패널(Build Menu), 선택 타워 위의 업그레이드/판매 패널(TowerControllerUI), 일시정지 버튼·메뉴(PauseMenu), 종료 화면(Game Over). 월드 공간에는 타워·적·홈베이스 체력 바, 사거리 원, 배치 타일 강조.

**화면에 없는 정보**: 남은 적 수, 다음 웨이브의 적 구성(비행 여부 포함), 타워가 지금 무엇을 노리는지, 마지막 웨이브 표시, 에너지 부족 안내. 타워 디펜스 관습(다음 웨이브 미리보기)에 비추어 「확장 여지」.

**최악의 레이아웃 검토**: 마우스만 있어도 전부 할 수 있다(키보드는 단축키뿐) — Happy Harvest와 달리 통과. 터치 전용 확인 UI와 큰 배치 타일이 따로 있어 모바일도 통과. 약점은 작은 화면에서 사이드바 버튼 6개(Level5)와 정보 패널이 월드를 가리는 것, 그리고 판매 버튼과 업그레이드 버튼이 같은 패널에 나란히 있어 오탭 위험이 있다는 것(그래서 판매에만 확인이 붙어 있다). 웨이브 중 드래그 배치는 적 탭과 겹칠 수 있으나 적은 탭 대상이 아니므로 문제가 없다.

## 8. 부록 — 게임 오브젝트·속성·동작 참조표

플로우차트의 번호와 같다. "구현" 열은 Doxygen 분석자료·씬 구조도와 연결되는 실제 이름이다.

| # | 오브젝트 | 속성 | 동작 | 구현 |
| --- | --- | --- | --- | --- |
| ① | 메인 메뉴 | 페이지 3(Main·LevelSelect·Options), 볼륨 3 | 페이지 전환, 종료, 외부 링크 | MainMenu 씬, TowerDefenseMainMenu, OptionsMenu |
| ② | 레벨 선택 | LevelList 5항목(id·이름·설명·씬), 저장된 별 | 버튼 생성, 클릭 → 씬 로드 | LevelSelectScreen, LevelSelectionButton, GameManager.levelList |
| ③ | 레벨 로드 | startingCurrency 10\~20, towerLibrary, homeBases, 인트로 1초 | 싱글턴 초기화, 풀 채움, Intro → Building | LevelNConfiguration → LevelManager, TimedLevelIntro, PoolManager |
| ④ | 건설 단계 | LevelState.Building, startWavesOnAwake 0 | 웨이브 대기, 자유 배치 | LevelManager, WaveManager |
| ⑤ | 타워 선택 | TowerLibrary의 타워 목록, 버튼별 비용·아이콘 | SetToBuildMode → 고스트 생성, 사거리 원 | BuildSidebar, TowerSpawnButton, GameUI, TowerPlacementGhost |
| ⑥ | 배치 영역 클릭 | 그리드 타일(gridSize 1) / 단일 칸, 점유 여부 | 배치 가능 검사 → 비용 차감 → Tower.Initialize | TowerPlacementGrid, SingleTowerPlacementArea, GameUI.TryPlaceTower |
| ⑦ | 웨이브 시작 | waves\[10\], 현재 인덱스 | BuildingCompleted → StartWaves, waveChanged 이벤트 | WaveManager, WaveUI |
| ⑧ | 적 스폰·이동 | SpawnInstruction(어떤 적, 몇 초 후, 어느 노드), AgentConfiguration(체력·속도·피해), Node 체인 | 풀에서 꺼내 Start에 배치, NavMesh로 다음 노드로, 막히면 타워 공격 | TimedWave/Wave, Agent → AttackingAgent/FlyingAgent, Node, Fixed/RandomNodeSelector |
| ⑨ | 타워 교전 | 사거리(Targetter 콜라이더), fireRate, 투사체 피해, slowFactor | 타겟 획득 → Launcher.Launch → Damager → TakeDamage | Targetter, AttackAffector/SlowAffector, Hitscan/Ballistic/HomingLauncher, Damageable |
| ⑩ | 에너지 획득 | lootDropped 1\~16, 파일런 gainRate | died → currency.AddCurrency, currencyChanged 이벤트 | LootDrop, CurrencyAffector, Currency, CurrencyUI |
| ⑪ | 업그레이드·판매 | TowerLevelData cost/sell, levels\[\] 인덱스 | 자식 레벨 프리팹 교체 / Destroy + 환급 | Tower.UpgradeTower, Tower.Sell, TowerUI, TowerInfoDisplay |
| ⑫ | 홈베이스 피해 | 체력 10, 충전 2초, 적 피해 1\~2 | HomeBaseAttacker → TakeDamage → healthChanged / died → homeBaseDestroyed → Lose | PlayerHomeBase, HomeBaseAttacker, PlayerBaseHealth |
| — | 승패 판정 | numberOfEnemies, LevelState | AllEnemiesSpawned ∧ 적 0 → Win; 홈베이스 0 → Lose | LevelManager.ChangeLevelState |
| — | 종료 화면 | 별 0\~3 (잔여 체력 비율), Next Level | 캔버스 켜기, CompleteLevel 저장, 씬 이동 | EndGameScreen, ScorePanel, GameManager, GameDataStore |
| — | 카메라·입력 | 줄 10\~20, 팬 속도, 탭/드래그 임계 | 스킴 전환, GameUI에 포인터 이벤트 전달 | CameraRig, InputController, TowerDefenseKeyboardMouseInput/TouchInput |

**「확장 여지」 모음** (학생이 자기 GDD에서 채울 자리): 게임 이름·세계관·스토리, 난이도·무한·협동 모드, 레벨 잠금과 별의 쓰임새, 웨이브 데이터화(SO)와 다음 웨이브 미리보기, 상시 수입·이자·파워업, 타워 능력 다양화(범위 버프·체인·독·수리), 적 다양화(힐러·분열·타워를 노리는 적), 가중치 분기 경로, 게임패드, 별점 계산의 0.9\~1 구멍 수정, 안 쓰이는 투사체 에셋 정리, 레이저 Lv3 판매가 밸런스.

**출처**: 업로드된 TowerDefenseTemplateUnityTechProject(소스 · 씬 · 프리팹 · Data 에셋 직접 분석), Tower Defense Doxygen 분석자료, Tower Defense Template 씬 동작 분석 · 씬 구조도, 템플릿 동봉 TowerDefenseDocumentation.pdf, 첨부된 Game Design Document Template(PDF).
