# Tower Defense Template 씬 동작 분석

Nov 17, 2026 · @YongHwan Lee

## 1. 개요

Happy Harvest 때와 같은 방법이다. 씬 파일(.unity)과 프리팹(.prefab)의 Unity YAML을 직접 파싱해, 게임플레이 스크립트가 붙은 객체마다 **인스펙터 값과 UnityEvent 연결**을 뽑았다. 다른 점은 게임플레이 스크립트 집합을 손으로 적지 않고 `Scripts/` 폴더의 175개 .cs 파일에서 자동으로 만든 것, 그리고 웨이브 스폰 목록처럼 긴 리스트도 생략하지 않고 담은 것이다.

| 씬 | GameObject | 게임플레이 컴포넌트 | 프리팹 인스턴스 | 역할 |
| --- | --- | --- | --- | --- |
| MainMenu | 108 | 18 | 3 | 타이틀 · 레벨 선택 · 옵션 |
| Level1 | 153 | 52 | 14 | 외길 경로, 타워 3종 |
| Level2 | 110 | 36 | 14 | 미로 만들기(상·하단 그리드), EMP 추가 |
| Level3 | 122 | 38 | 14 | 병목 지점, 슈퍼타워 추가 |
| Level4 | 146 | 60 | 14 | 긴 산길, 배치 영역 20개 |
| Level5 | 142 | 54 | 14 | 두 갈래 경로, 타워 전부 |
| Prototype | 153 | 48 | 12 | Level1과 동일 구성의 그레이박스 |

생성물: `scenes.json`(씬 7개), `prefabs.json`(프리팹 74개 중 스크립트 있는 것), 구조도 HTML(Level1 · Level5 · MainMenu · 타워/적 프리팹). 이 문서는 그 데이터를 읽어 "씬 안에서 실제로 무엇이 어떻게 동작하는가"를 정리한 것이다. 클래스 단위의 설명은 Doxygen 분석자료에 있다.

## 2. 레벨 씬의 공통 뼈대

레벨 씬 5개는 모두 같은 네 덩어리로 되어 있다. Happy Harvest가 "모든 객체가 영속 GameManager에 등록"하는 구조였다면, 여기는 **씬마다 살고 죽는 싱글턴 세 개**(LevelManager · GameUI · PoolManager)가 있고 HUD 프리팹들이 그 싱글턴의 C# 이벤트를 구독한다. 영속 GameManager는 레벨 목록·저장·오디오만 맡는다.

| 덩어리 (씬 시에 따라 이름이 붙음) | 안에 있는 것 | 스크립트 |
| --- | --- | --- |
| **LevelNConfiguration** | WaveManager 객체(= LevelManager + WaveManager + TimedLevelIntro 세 컴포넌트), 그 아래 웨이브 1\~10, Nodes 아래 경로 노드들과 End(홈베이스) | LevelManager, WaveManager, TimedWave×9 + Wave×1, Node + Fixed/RandomNodeSelector + AreaMeshCreator, PlayerHomeBase |
| **LevelNPlacementAreas** | 타워를 놓을 수 있는 격자와 단일 칸들 | TowerPlacementGrid(gridSize 1), SingleTowerPlacementArea |
| **LevelNPoolManager** | 적·투사체·이펙트 풀 21\~28종 | PoolManager |
| **프리팹 인스턴스 12개** | GameCamera(카메라+입력+GameUI), GameManager, HUD 8종(Build Menu, TowerControllerUI, PlayerBaseLife, WaveContainer, Game Over, PauseMenu, TowerPlaceConfirmationUI, TowerPlaceInvalid), LevelMusic, 환경 메시 2개(Enviro, Enviro\_COL), HeadQuarters | 아래 5절 |

연결 방식 세 가지:

- **인스펙터 참조** — LevelManager.homeBases → End, LevelManager.towerLibrary → Level\_N\_TowerLibrary(SO), WaveManager.waves → 웨이브 10개, Wave.spawnInstructions\[\].startingNode → Start, NodeSelector.linkedNodes → 다음 노드.
- **C# 이벤트 구독** — PlayerBaseLife는 currency.currencyChanged와 홈베이스 healthChanged, WaveContainer는 waveChanged, Game Over는 levelCompleted/levelFailed. 모두 코드에서 `LevelManager.instance`를 찾아 구독한다.
- **UnityEvent(인스펙터 연결)** — 거의 쓰지 않는다. 씬 전체에서 발견된 것은 MainMenu의 Quit 버튼 효과음 하나뿐이다. Happy Harvest(가로등 9개·낮밤 이벤트)와 대조적인 점으로, 이 템플릿은 무엇이든 코드 이벤트로 연결한다.

주의할 점: HUD 프리팹 12개가 레벨 씬마다 따로 놓여 있다. 한 씬에서 UI를 고치면 다른 씬에는 반영되지 않으므로 UI 변경은 반드시 프리팹에서 한다.

## 3. 레벨 5개 비교 (씬에서 읽은 값)

| 레벨 | LevelList 설명 | 시작 에너지 | 구매 가능 타워 | 경로 노드 | 그리드 / 단일 칸 | 적 총수 | 적 구성 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Level1 | Don't allow enemies to rush through | 10 | 기관총 · 로켓 · 레이저 | 6 (외길) | 4 / 5 | 174 | 버기 79, 탱크 62, 콥터 32, 보스 1 |
| Level2 | Create mazes to slow enemies | 15 | + EMP | 3 | 2 / 0 (상·하단 넓은 그리드) | 141 | 버기 71, 탱크 50, 콥터 15, 슈퍼탱크 5 |
| Level3 | Take advantage of choke points | 20 | + EMP · 슈퍼타워 | 4 | 3 / 0 | 209 | 슈퍼버기 42, 슈퍼콥터 28, 슈퍼탱크 25 등 |
| Level4 | Protect the long mountain pass | 15 | 기관총 · 로켓 · 레이저 · 슈퍼타워 (EMP 없음) | 5 | 11 / 9 | 181 | 보스 3 포함 |
| Level5 | Guard both paths from enemies | 20 | 전부 (파일런 · EMP · 슈퍼타워) | 6 (Start에서 Left/Right 랜덤 분기) | 9 / 2 | 200 | 보스 10, 슈퍼보스 5 |

모든 레벨 공통:

- 웨이브 10개. 1\~9는 TimedWave(다음 웨이브까지 15.5\~60초), 10번째는 Wave(타이머 없음). 가장 큰 웨이브는 Level3 W8의 48기.
- 홈베이스 하나, 체력 10.
- 상시 수입 0 (`alwaysGainCurrency` 꺼짐, `currencyGainer.constantCurrencyGainRate 0`). 수입은 적 처치 LootDrop과 파일런(Level5)뿐.
- 인트로: Level1·4·5는 TimedLevelIntro 1초, Level2·3은 없음(바로 Building 상태).
- 배치 타일 프리팹은 데스크톱/모바일 두 벌(PlacementTile / PlacementTileMobile)을 들고 있고 런타임에 고른다.

레벨 사이의 난이도는 **적 수가 아니라 적 종류와 배치 제약**으로 만들어져 있다. Level2는 적이 가장 적지만 그리드가 넓어 "타워로 미로를 만드는" 레벨이고, Level4는 배치 영역이 20개로 짧게 쪼개져 있다.

## 4. 한 판의 흐름 — 어느 객체가 무엇을 하는가

| 단계 | 일어나는 일 | 씬 객체 → 스크립트 | 상태 (LevelState) |
| --- | --- | --- | --- |
| 씬 로드 | GameManager 프리팹이 없으면 생성(영속). LevelManager.Awake가 홈베이스와 통화(시작 에너지) 초기화. PoolManager가 풀 미리 채움 | LevelNConfiguration → LevelManager, LevelNPoolManager → PoolManager | Intro |
| 인트로 | TimedLevelIntro 1초 뒤 introCompleted → Building. 없는 레벨은 바로 Building | WaveManager 객체 → TimedLevelIntro | Intro → Building |
| 건설 | 플레이어가 Build Menu에서 타워를 고르면 GameUI가 Building 상태로 들어가 고스트를 마우스에 붙이고, 배치 영역 위에서 클릭하면 비용을 빼고 Tower를 Instantiate. 웨이브는 아직 시작되지 않음(`startWavesOnAwake 0`) | GameCamera → GameUI · PlacementAreas → TowerPlacementGrid | Building |
| 웨이브 시작 | WaveContainer의 버튼(또는 첩 배치 후 자동)으로 LevelManager.BuildingCompleted → WaveManager.StartWaves | WaveContainer → WaveUI | SpawningEnemies |
| 스폰 | TimedWave가 spawnInstructions를 순서대로 실행: delayToSpawn만큼 기다렸다가 PoolManager에서 agentPrefab을 꺼내 startingNode(Start)에 놓고 LevelManager.IncrementNumberOfEnemies. 웨이브가 끝나면 timeToNextWave 후 다음 웨이브 | 웨이브 1\~10 → TimedWave/Wave | SpawningEnemies |
| 이동 | Agent가 NavMesh로 현재 노드로 이동, 노드의 트리거(AreaMeshCreator 영역)에 들어가면 NodeSelector.GetNextNode로 다음 노드. Fixed는 linkedNodes\[0\], Random은 무작위 | Nodes/Start…1…2…End → Node + Selector | — |
| 교전 | 타워 레벨 프리팹의 Targetter(트리거 콜라이더)에 적이 들어오면 AttackAffector가 fireRate로 Launcher.Launch → 투사체(풀) → Damager가 적의 DamageCollider에 닿으면 Damageable.TakeDamage. 적은 길이 타워로 막혔을 때만(PathPartial) 자기 AttackAffector를 켜서 반격 | (프리팹 내부, 6절) | — |
| 처치 | Damageable.died → DeathEffect(파티클) · LootDrop(통화 +1\~16) · Poolable 반환 · LevelManager.DecrementNumberOfEnemies | — | — |
| 도착 | 적이 End 노드에 닿으면 HomeBaseAttacker가 2초 충전 후 PlayerHomeBase에 피해(1\~2) 주고 자신은 제거. 홈베이스 체력 0 → homeBaseDestroyed | Nodes/End → PlayerHomeBase | → Lose |
| 마지막 웨이브 | WaveManager.spawningCompleted → AllEnemiesSpawned. 이후 적 수가 0이 되는 순간 Win | — | AllEnemiesSpawned → Win |
| 종료 | levelCompleted/levelFailed → Game Over 프리팹의 EndGameScreen이 캨버스를 켜고, 홈베이스 잔여 체력 비율로 별 0\~3 계산, GameManager.CompleteLevel(id, 별)로 저장. "Next Level" 버튼은 승리 시에만 | Game Over → EndGameScreen · ScorePanel | Win / Lose |

승패가 **두 곳에서 감시**된다는 점이 핵심이다: 적 수 카운터(Increment/Decrement)와 홈베이스 체력. 적 수를 세는 것은 웨이브와 Agent.Remove이므로, 새 적 종류를 만들 때 Agent를 상속하지 않으면 승리 판정이 깨진다.

## 5. 입력과 UI 동작

입력과 UI는 전부 **GameCamera 프리팹 하나**에 모여 있다. 카메라 리그·입력 해석·GameUI 상태 기계가 한 객체의 컴포넌트 9개다.

| 컴포넌트 (GameCamera) | 역할 | 인스펙터 값 |
| --- | --- | --- |
| InputController | 터치·마우스를 탭/드래그/홈드/플릭/핀치/휠 이벤트로 바꿈 | dragThreshold 터치 5 · 마우스 1, tapTime 0.2s, holdTime 0.8s |
| TowerDefenseInputSchemeSwitcher | 플랫폼에 따라 아래 두 스킴 중 하나 활성 | — |
| TowerDefenseKeyboardMouseInput | 숫자 1\~9 = Build Menu의 n번째 타워, Esc = 취소/일시정지, 휠 = 줄, 우클릭·가운데 드래그·화면 가장자리·WASD = 팬 | edgePan 30, rmbPan 15, 가장자리 40px |
| TowerDefenseTouchInput | 드래그로 고스트 끌기(BuildingWithDrag), 핀치 줄, 플릭 팬, Esc(안드로이드 백) | panSpeed 15, flickDecay 0.2 |
| CameraRig | 이동·줄 보간 | 줄 10\~20 (최대 60), floorY 0 |
| CameraInitialState | 시작 줄 모드 | startZoomMode 1 |
| **GameUI** | 상태 기계. 탭을 받아 배치 영역 레이캐스트(sphereCast 1) → 배치/선택/해제 | 상태 5개: Normal · Building · BuildingWithDrag · Paused · GameOver |
| RadiusVisualizerController | 선택·고스트 타워의 사거리 원 | 높이 0.02 |
| PostProcessorConfigurationSelector | 화질별 포스트프로세싱 프로필 | HighQ / LowQ |

**GameUI 상태 전이**: Normal ─(Build Menu 버튼, 숫자키)→ Building ─(배치 성공 / Esc / 우클릭)→ Normal. Normal에서 타워 탭 → 선택(상태는 Normal 유지, TowerControllerUI 표시). 터치는 버튼에서 바로 드래그하면 BuildingWithDrag. PauseMenu → Paused(timeScale 0). levelCompleted/levelFailed → GameOver(입력 무시).

**HUD 프리팹 8개** (레벨 씬마다 인스턴스)

| 프리팹 | 스크립트 | 동작 |
| --- | --- | --- |
| Build Menu | BuildSidebar, BuildInfoUI, TowerUI, TowerInfoDisplay | 시작 시 LevelManager.towerLibrary를 읽어 타워마다 TowerBuildButton(TowerSpawnButton) 생성. 버튼 호버/탭 → BuildInfo 패널(이름·설명·DPS) Show/Hide 애니메이션 |
| TowerControllerUI | TowerUI, TowerInfoDisplay | 선택 타워 위에 뜨는 패널. 업그레이드(다음 레벨 비용) · 판매(확인 버튼 SellConfirmButton 한 번 더) |
| PlayerBaseLife | PlayerBaseHealth, CurrencyUI | 홈베이스 체력 숫자 · "Energy:" 통화 |
| WaveContainer | WaveUI | 웨이브 번호 · 다음 웨이브까지 진행 바(SliderForeground). 건설 단계에서 웨이브 시작 버튼 |
| Game Over | EndGameScreen, ScorePanel, UrlOpen | "{0} COMPLETE!" / "{0} FAILED!", 별 3개, Next Level(승리 시), 승리/패배 효과음 |
| PauseMenu | PauseMenu, SceneLoader×2, UrlOpen | Esc 토글. 레벨 이름·설명 표시, 재시작 → LevelSelect 씬(!), 메인 메뉴 → MainMenu. 둘 다 확인 버튼 거침 |
| TowerPlaceConfirmationUI | MovingCanvas | 터치 전용: 고스트 위치에 확인/취소 버튼 |
| TowerPlaceInvalid | MovingCanvas | 터치 전용: 배치 불가 표시 |

화면에 **없는** 정보: 적의 남은 수, 다음 웨이브의 적 구성, 타워의 현재 타겟, 마지막 웨이브임을 알리는 표시. 타워 체력 바는 피해를 입었을 때만 보인다(showWhenFull 0).

## 6. 타워·적 프리팹 동작

씬에는 없고 배치·스폰 시 생기는 것들. 프리팹 74개 중 타워 37개(루트 6 + 레벨 16 + 투사체 10 + 고스트 6), 적 11개.

**타워 = 루트 프리팹 + 레벨 프리팹 3개.** 루트(예: MachineGunTower)에는 Tower · DamageCollider · HealthVisualizer · DeathEffect가 있고, `levels[]`에 레벨 프리팹 3개를 든다. 배치하면 levels\[0\]을 자식으로 만들고, 업그레이드는 **자식을 파괴하고 levels\[1\]을 다시 만드는 것**이다(스탯 증가가 아니라 교체). 레벨 프리팹에는 TowerLevel(levelData SO, 고스트, 건설 이펙트) + 자식 Targetter + 자식 Affector(기능)가 있다.

| 타워 (towerName) | Affector | Launcher | Lv1→Lv2→Lv3 비용 / 판매 | 공격 수치 | 비고 |
| --- | --- | --- | --- | --- | --- |
| Assault Cannon (MachineGun) | AttackAffector | HitscanLauncher | 4→8→14 / 2·6·13 | 피해 0.25 × 발사율 4·6·8 = DPS 1·1.5·2 | 세 레벨 모두 Projectile\_0 사용 |
| Rocket | AttackAffector | BallisticLauncher | 12→24→32 / 6·18·34 | 피해 3·3·5 × 0.6·0.7·0.8, 범위 | 지상만 (비행 적 못 맞힘) |
| Laser | AttackAffector | HomingLauncher | 15→30→40 / 7·22·42 | 피해 12·16·20 × 0.3·0.35·0.4 | Lv3 판매가 > 비용 |
| EMP | SlowAffector (패시브) | — | 6→10→10 / 3·8·13 | slowFactor 0.75·0.65·0.65 | 범위 안 적에 AgentSlower 부착 |
| Energy Pylon | CurrencyAffector | — | 8→8→8 / 4·8·12 | +1 / 10s·8s·5s | Level5에서만 |
| Super Tower | AttackAffector | BallisticLauncher | 20 (단일) / 판매 불가 | 발사율 20, 체력 20 | 일제사격 후 자폭 |

모든 타워 체력 5 (슈퍼타워 20). Targetter는 searchRate 0.25초마다 타겟을 찾고, 타겟이 없으면 idleRotationSpeed 30으로 포탑을 돌린다.

**적 = Agent + Damageable + Targetter + Affector.** 루트에 Poolable · AttackingAgent(또는 FlyingAgent) · DamageCollider · Damager · HealthVisualizer · DeathEffect · LootDrop · HomeBaseAttacker · HitscanLauncher, 자식 Targetter. 수치는 프리팹이 아니라 **AgentConfiguration(SO)** 에서 스폰 시 주입된다.

| 적 | 체력 | 속도 | 피해 | LootDrop | 특징 |
| --- | --- | --- | --- | --- | --- |
| Hoverbuggy / Super | 4 / 14 | 4.8 | 1 | 1 / 2 | 물량 |
| Hovertank / Super | 35 / 100 | 2 | 1 | 4 / 6 | 단단함 |
| Hovercopter / Super | 16 / 32 | 3 | 1 | 3 / 4 | FlyingAgent — 타워 위로 통과, 로켓 면역 |
| Hoverboss / Super | 600 / 1400 | 1.2 | 2 | 8 / 16 | 보스 |

적의 AttackAffector는 **기본 비활성**이다. AttackingAgent가 NavMesh 경로가 타워로 막혔을 때(PathPartial)만 켜서 그 타워를 공격하고, 길이 뚫리면 다시 끈다. 즉 "적은 평소에 타워를 공격하지 않는다"가 이 게임의 규칙이고, 미로를 완전히 막으면 뿌고 지나간다. 피해 전달은 타워·적 가릴 것 없이 같은 경로다: Damager(투사체) ↔ DamageCollider(피격자) → Damageable.TakeDamage → healthChanged / died. 어느 쪽이 어느 쪽을 때릴 수 있는지는 SimpleAlignment(SO) Player/Enemy가 정한다.

## 7. 메인 메뉴 씬

Canvas 하나에 페이지 세 개가 있고 TowerDefenseMainMenu가 전환한다. 배경은 타워 모델 4개가 Rotator로 돌고 IntervalParticleSystemPlayer가 4\~6초마다 발사 이펙트를 트는 연출이다.

| 페이지 | 스크립트 | 동작 |
| --- | --- | --- |
| Main | SimpleMainMenuPage, ExitButton, UrlOpen | Play → LevelSelect 페이지, Options, Quit(효과음 은 씬에서 유일한 UnityEvent 연결), Learn More(외부 URL) |
| LevelSelect | LevelSelectScreen, MouseScroll, LevelSelectionButton(프리팹) | GameManager.levelList의 5개 레벨마다 버튼 생성, 저장된 별(ScorePanel) 표시. 카메라 애니메이션 CameraMove0/1로 진입·퇴장. 가로 스크롤 clamp |
| Options | OptionsMenu | Master/SFX/Music 슬라이더 → GameManager가 오디오 믹서에 적용하고 저장. Credits에 UrlOpen 2개 |

저장은 GameManager가 GameDataStore를 JsonSaver로 persistentDataPath에 기록한다: 레벨별 별 수·클리어 여부, 볼륨 3개. 레벨 잠금은 없다 — 모든 레벨이 처음부터 열려 있고 별만 표시된다(LevelSelectButton은 잠금 필드를 갖고 있지 않다).

## 8. 발견 사항과 확장 지점

씬 데이터를 읽어야만 보이는 것들이다. 강의 2단계(추가 기능 도출)의 보기로 쓸 수 있다.

1. **웨이브는 데이터가 아니라 씬이다.** 5레벨 × 10웨이브의 스폰 목록(최대 48기)이 전부 Wave 컴포넌트 인스펙터에 있다. 난이도 선택·무한 모드·랜덤 웨이브·레벨 에디터는 먼저 이것을 ScriptableObject로 빼는 리팩터링이 필요하다.
2. **수입원은 처치 보상뿐.** 상시 수입 0, 파일런은 Level5 전용. 경제를 바꾸려면 LevelManager.currencyGainer(씬 값)와 각 적 프리팹의 LootDrop 두 곳.
3. **별점 계산의 구멍.** EndGameScreen.CalculateScore: 체력 비율 1 → 3별, 0.5\~0.9 → 2별, 0\~0.5 → 1별, 그 외 0별. **0.9 초과 1 미만이 0별**이다. 체력 10·정수 피해라 지금은 닿지 않지만 홈베이스 체력을 바꾸는 순간 드러나는 잠재 결함 — "수치 하나 바꿀 때 무엇을 확인해야 하는가"의 예제.
4. **안 쓰이는 에셋.** MachineGunProjectile\_1/\_2는 어느 레벨도 참조하지 않는다(세 레벨 모두 \_0). 기관총의 레벨 차이는 발사율뿐. 레이저 Lv3은 판매가 42 > 비용 40으로 "짓고 팔면 이익" — 밸런스 표 검토 과제로 적당.
5. **분기 경로는 Level5뿐.** RandomNodeSelector는 Level3·5에 있지만 Level3은 연결 노드가 하나라 사실상 고정. Node.weight 필드는 전부 1이고 읽는 코드가 없다 — 가중치 분기가 자연스러운 확장.
6. **적은 평소 타워를 공격하지 않는다.** AttackAffector가 기본 비활성이고 길이 막힐 때만 켜진다. "타워를 노리는 적"은 플래그 하나로, "타워 수리"는 Damageable 위에 바로 얹을 수 있다.
7. **UnityEvent를 안 쓴다.** 씬 전체에서 인스펙터 연결은 Quit 버튼 하나. 나머지는 C# 이벤트 + 싱글턴 instance 참조다. Happy Harvest와 비교하면 "디자이너가 연결하는 구조"와 "프로그래머가 연결하는 구조"의 차이를 보여 주는 사례.
8. **HUD 프리팹은 레벨마다 12개씩 복제.** UI 수정은 프리팹에서. PauseMenu의 "재시작"은 현재 레벨이 아니라 LevelSelect 씬을 로드한다(sceneToLoadName 필드) — 이름과 동작이 다른 예.
9. **레벨 잠금·진행 게이트가 없다.** 별은 저장되지만 어디에도 쓰이지 않는다. "이전 레벨 1별 이상이면 열림" 같은 규칙은 LevelSelectButton + GameDataStore만으로 가능.

출처: 업로드된 TowerDefenseTemplateUnityTechProject의 씬·프리팹·Data 에셋 직접 파싱(scenes.json, prefabs.json), Scripts/ 소스, Tower Defense Doxygen 분석자료, 템플릿 동봉 TowerDefenseDocumentation.pdf.
