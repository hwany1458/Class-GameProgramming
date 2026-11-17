# GitHub 2D Tower Defense 씬 동작 분석

Nov 17, 2026 · @YongHwan Lee

## 1. 개요

업로드된 압축파일은 GitHub 공개 저장소 **evanderchristian / Tower-Defense**(MIT)다. Unity 2018.4.36f1, 스크립트 8개 748줄, 씬 2개, 프리팹 12개(기본 4 + 변형 8). 주석이 인도네시아어인 것으로 보아 Dicoding의 Unity 입문 과정 교재("Belajar Membuat Game untuk Pemula")를 따라 만든 프로젝트다. README가 밝힌 기능: 적·투사체, 레벨 2개, UI(적 수·생명·승패), 효과음, 연속 플레이.

앞의 세 샘플과 같은 파서로 씬·프리팹 YAML을 읽었다. Unity 2018은 uGUI가 내장 DLL이라 Button의 OnClick을 잡기 위해 guid 매핑을 더했고, 레벨 지도는 경로·배치 칸의 Transform 좌표를 그대로 그렸다.

| 씬 | GameObject | 게임플레이 컴포넌트 | 프리팹 인스턴스 | 경로 노드 | 배치 칸 |
| --- | --- | --- | --- | --- | --- |
| Level1 (빌드 0) | 33 | 8 (LevelManager 1, TowerPlacement 4, AudioPlayer 1, PlayAgain 2) | **0** | 11 | 4 |
| Level2 (빌드 1) | 35 | 8 | 0 | 13 | 4 |

이 프로젝트의 가치는 **Unity 공식 Tower Defense Template(175개 스크립트)과 같은 장르를 8개로 구현**했다는 데 있다. 배치·조준·발사·경로·승패라는 핵심은 다 있고, 경제·웨이브·업그레이드·저장·메뉴가 없다. 두 프로젝트를 나란히 놓고 "무엇이 빠졌기에 20배 작은가"를 찾는 것이 강의에서의 쓰임새다. 클래스 단위 설명은 Doxygen 분석자료, 그림은 씬 구조도 페이지에 있다.

## 2. 레벨 씬의 뼈대

프리팹 인스턴스가 하나도 없다. 씬에는 관리자·길·배치 칸·캨버스·오디오만 있고, 타워 UI 버튼·타워·적·탄환은 전부 런타임에 만들어진다.

| 씬 객체 | 컴포넌트 | 역할 | 인스펙터 값 |
| --- | --- | --- | --- |
| **Level 1** | LevelManager | 싱글턴(FindObjectOfType). 스폰 타이머, 모든 타워·적 구동, 생명·적 수·승패, 풀, R키 재시작 | \_maxLives 3 · \_totalEnemy 15 · \_spawnDelay 5 · \_towerPrefabs 3 · \_enemyPrefabs 2 · \_enemyPaths 11 · UI 참조 5개 |
| Level 1/**Road**/road\_1·road\_2 ×11 | SpriteRenderer만 | 길 그림이자 경로 노드 — Transform을 그대로 배열에 넣었다 | — |
| Level 1/**Tower Placement**/tower\_placement ×4 | TowerPlacement, BoxCollider2D(trigger) | 들어온 타워에 자기 위치를 줄 뿐. 점유 여부는 \_placedTower | — |
| **Canvas** | Tower Selection/Panel, Lives, Total Enemy, Panel(Status · PlayAgain · LoadScene) | HUD와 결과 패널. Panel은 시작 시 비활성 | 버튼 OnClick → PlayAgain.playAgain / nextScene(L1) · prevScene(L2) |
| **Audio Player** | AudioPlayer, AudioSource | 효과음 2종 | hit-enemy · enemy-die |
| Main Camera · EventSystem | Camera(ortho 5), StandaloneInputModule | — | — |

**런타임에 생기는 것**: LevelManager.Start가 타워 종류마다 Tower UI 버튼을 Instantiate(3개). TowerUI가 드래그 시작에 Tower를 월드에 Instantiate. LevelManager가 5초마다 Enemy를 풀에서 꺼내거나 Instantiate. Tower가 발사 시 Bullet을 풀에서 꺼낸다.

**연결 방식**: 인스펙터 참조는 LevelManager에만(프리팹 배열, 경로 배열, UI 텍스트). 그 밖은 전부 `LevelManager.Instance` / `AudioPlayer.Instance` 직접 호출. C# 이벤트 0개, 상태 기계 0개, UnityEvent는 결과 패널 버튼 2개. Tower·Enemy·Bullet의 Update()는 비어 있고 **LevelManager.Update가 모든 타워와 적의 메서드를 매 프레임 직접 부른다** — 세 공식 샘플 어디에도 없던 "중앙 Update" 패턴이다.

## 3. 레벨 2개 비교

| 레벨 | 생명 | 적 수 | 스폰 간격 | 경로 노드 | 경로 길이 | 통과 시간 (적 V1 속도 2 / V2 속도 1) | 배치 칸 | 모양 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- |
| Level1 | 3 | 15 | 5s | 11 | 25 유닛 | 12.5s / 25s | 4 | U자 (한 번 내려갔다 올라옴) |
| Level2 | 3 | 15 | 5s | 13 | 30 유닛 | 15s / 30s | 4 | S자 (두 번 꾸임, 가운데 구간이 깊음) |

LevelManager의 값은 두 씬이 완전히 같다. 달라지는 것은 길 스프라이트의 배치와 그것을 순서대로 넣은 `_enemyPaths` 배열뿐이다. 그래서 난이도 = **경로 길이 × 배치 칸에서 길까지의 거리**. Level2의 가운데 구간(y = −1.5)은 가장 가까운 배치 칸(−2.5, 3.4)에서 4.9 유닛 떨어져 있어 사거리 4짜리 범위 타워(V3)는 닿지 않고, 사거리 8짜리 V2만 전 구간을 덮는다 — 의도한 설계인지 우연인지는 코드에 단서가 없다.

적 15기는 두 종류 중 랜덤이므로 총 체력은 45(전부 V1)\~90(전부 V2) 사이. 배치 칸 4개에 V1(DPS 0.67)을 채우면 DPS 2.7, 5초마다 오는 적 한 기당 13.3 피해 — 사거리 안에 머무는 시간이 짧아도 수치상은 다 잡힌다. 타워가 무료라 레벨 시작 직후 4칸을 다 채우는 것이 유일한 전략이고, 그 다음에는 할 일이 없다. 경제가 없으면 타워 디펜스의 의사결정이 사라진다는 것을 보여 주는 사례.

## 4. 한 판의 흐름 — 어느 객체가 무엇을 하는가

| 단계 | 일어나는 일 | 담당 |
| --- | --- | --- |
| 씬 로드 | Level1이 빌드 0 — 메뉴 없이 바로 게임. Start: 생명 3 · 적 15 표시, 타워 종류마다 Tower UI 버튼 생성 | LevelManager.Start, InstantiateAllTowerUI |
| 스폰 | \_runningSpawnDelay가 unscaledDeltaTime으로 0이 될 때마다(5초) SpawnEnemy: 카운터 −1 → 랜덤 종류 → 이름에 "1"/"2"가 들어간 비활성 적 재사용, 없으면 Instantiate → paths\[0\]에 놓고 목표 paths\[1\]. 첫 적은 시작 즉시 | LevelManager.SpawnEnemy |
| 이동 | 모든 활성 적: 목표까지 0.1 이내면 다음 노드로, 마지막 노드면 ReduceLives(1) + 비활성. 아니면 MoveTowards(속도 × deltaTime). 방향이 바뀔 때 4방향 회전(체력 바는 제외) | LevelManager.Update, Enemy.MoveToTarget / SetTargetPosition |
| 조준 | 모든 타워: 현재 목표가 비활성이거나 사거리 밖이면 버리고, 사거리 안 가장 가까운 적(비활성 포함!)을 새 목표로. 포탑을 180°/s로 돌린다 | Tower.CheckNearestEnemy / SeekTarget |
| 발사 | 지연 타이머 ≤ 0 이고 포탑이 목표 각도 10° 이내면 풀에서 탄환 → 위치 = 타워, SetProperties(피해·속도·범위), SetTargetEnemy, 활성 | Tower.ShootTarget, LevelManager.GetBulletFromPool |
| 탄환 비행 | FixedUpdate: 목표를 향해 MoveTowards + 회전. 목표가 비활성이면 자신도 비활성. IsOver면 정지 | Bullet.FixedUpdate |
| 피격 | 목표 콜라이더에 닿으면: 범위 > 0 → ExplodeAt(반경 안 활성 적 전부) / 아니면 목표만 ReduceEnemyHealth. 피격음, 0이면 비활성 + 사망음, 체력 바 갱신 | Bullet.OnTriggerEnter2D, Enemy.ReduceEnemyHealth, LevelManager.ExplodeAt |
| 패배 | 생명 0 → SetGameOver(false): IsOver = true, "You Lose!", 패널 켜기. Update는 이후 R키만 본다 | LevelManager.ReduceLives / SetGameOver |
| 승리 | 카운터가 0 미만이 된 스폰 틱에서 활성 적이 없으면 SetGameOver(true) — 검사가 5초마다만 일어난다 | LevelManager.SpawnEnemy |
| 재시작 | R키(언제나) 또는 패널의 Play Again → 같은 씬 로드. Next Level(L1) / Prev(L2) | LevelManager.Update, PlayAgain |

이 게임에는 상태가 두 개뿐이다: IsOver가 false인 동안은 "스폰·이동·조준·발사가 동시에 계속", true가 되면 전부 정지. 건설 단계도, 웨이브 사이 휴지도 없다. 타워를 짓는 동안에도 적은 온다.

## 5. 입력과 UI

**타워 배치 = uGUI 드래그 → 월드.** Tower UI 버튼(아이콘 = 포탑 스프라이트)에 IBeginDragHandler / IDragHandler / IEndDragHandler가 붙어 있다.

| 순간 | 일어나는 일 |
| --- | --- |
| 드래그 시작 | 타워 프리팹을 **월드에 바로 Instantiate**, 정렬 순서를 2로 올려 앞에 보이게 |
| 드래그 중 | 마우스 스크린 좌표 → ScreenToWorldPoint → 타워 위치. 타워의 키네마틱 Rigidbody2D가 배치 칸 트리거에 들어가면 TowerPlacement가 PlacePosition을 주고, 나가면 null |
| 놓기 | PlacePosition이 null이면 Destroy. 있으면 그 위치에 고정, 정렬 순서 0, LevelManager.RegisterSpawnedTower |

비용 검사가 없으므로 드래그 시작을 막는 조건이 없고, 배치된 타워를 다시 집거나 팔거나 업그레이드하는 입력도 없다. 설치된 타워는 레벨이 끝날 때까지 그대로다.

**그 밖의 입력**: R키 = 언제나 현재 씬 재로드(IsOver와 무관). 카메라 이동·줄 없음. 일시정지 없음. 터치: uGUI 드래그는 터치로도 동작하므로 모바일에서 배치는 가능하지만 R키 재시작은 불가(패널 버튼으로 대체).

**HUD** (uGUI Canvas): 왼쪽 위 "Lives: 3", "Total Enemy: 15"(스폰될 때마다 감소 — "남은 적"이 아니라 "아직 안 나온 적"), 오른쪽 Tower Selection 패널에 버튼 3개. 적 머리 위 체력 바(SpriteRenderer, 피해 비율만큼 폭 축소). 결과 패널: "You Win!" / "You Lose!", Play Again?, Next Level › (L1) / ‹ Prev Level (L2).

**화면에 없는 정보**: 타워의 사거리(놓기 전에도 후에도), 타워 수치, 화면 위의 적 수, 다음 적 종류, 남은 시간, 점수. 에디터에서만 OnDrawGizmos가 경로를 청록 선으로 그린다.

## 6. 프리팹 변형과 수치

데이터는 전부 **Prefab Variant**의 덮어쓴 값이다. ScriptableObject도 JSON도 없다. 기본 프리팹 값은 코드의 필드 기본값과 다르다(예: Tower 코드 기본 사거리 1, 프리팹 5).

| 프리팹 | 변형 | 덮어쓴 값 | 결과 |
| --- | --- | --- | --- |
| Tower (기본 피해 1 · 사거리 5 · 지연 5s · 탄속 4 · 범위 0) | Variant 1 | 피해 2, 지연 3, 탄환 Bullet Variant 1 | DPS 0.67 — 기본형 |
|  | Variant 2 | 사거리 8, 지연 4, 탄환 Bullet Variant 1 | DPS 0.25 — 저격 |
|  | Variant 3 | 사거리 4, 범위 2, 탄환 Bullet Variant 2 | DPS 0.2 × 범위 안 전부 — 박격 |
| Enemy (기본 체력 1 · 속도 1) | Variant 1 | 체력 3, 속도 2 | 빠름. 현재 V1 한 발 + 추가 한 발 |
|  | Variant 2 | 체력 6 | 느림. V1 세 발 |
| Bullet | Variant 1 | 이름 "Bullet", bullet\_1 스프라이트 | 수치는 발사하는 타워가 매번 주입 |
|  | Variant 2 | bullet\_2 스프라이트 | 범위탄 전용이지만 코드상 구분 없음 |

타워·적 프리팹은 모두 BoxCollider2D + Rigidbody2D(키네마틱)를 가진다 — 타워는 배치 칸 트리거를 위해, 적은 탄환 트리거를 위해. 탄환은 CircleCollider2D(trigger). 적의 체력 바는 자식 스프라이트 두 장이고, 방향 전환 시 부모를 잠시 떼어 회전을 피한다.

새 타워/적 = 변형 프리팹 하나 + LevelManager 배열에 등록(두 씬 모두). 풀 검색이 이름 문자열 Contains이므로 새 적 프리팹 이름에는 반드시 "3" 같은 인덱스 숫자가 들어가야 한다 — 이름에 의존하는 설계의 위험을 보여 주는 지점.

## 7. Unity 공식 Tower Defense Template과의 비교

같은 장르, 같은 핵심 루프. 크기가 20배 다른 이유를 항목별로 놓으면 "어디까지가 게임이고 어디부터가 구조인가"가 보인다.

| 항목 | GitHub 2D TD (8 스크립트) | Unity Tower Defense Template (175) |
| --- | --- | --- |
| 매 프레임 구동 | LevelManager.Update가 타워·적 메서드를 직접 호출 | 각 컴포넌트가 자기 Update, 이벤트로 통신 |
| 레벨 상태 | IsOver bool 하나 | LevelState 6단계 상태 기계 |
| 경로 | Transform 배열, 0.1 거리 도달 판정 | Node + NodeSelector, NavMesh, 랜덤 분기 |
| 타워 기능 | Tower 클래스 하나, 변형으로 수치만 | Tower → TowerLevel → Affector/Targetter/Launcher 조합, 업그레이드 = 자식 교체 |
| 피해 | Bullet이 Enemy.ReduceEnemyHealth 직접 호출 | Damager ↔ DamageCollider → Damageable, 진영(Alignment) |
| 적 종류 | 변형 2 (체력·속도) | AgentConfiguration SO 8, 비행/공격 파생 |
| 스폰 | 5초마다 랜덤 1기 × 15 | 웨이브 10개 ×스폰 명령 목록 |
| 경제 | 없음 | 통화, 비용, 판매가, 처치 보상, 파일런 |
| 배치 | 트리거 칸 4개, uGUI 드래그 | 그리드/단일 영역, 고스트, GameUI 상태 기계, 터치 확인 UI |
| 풀링 | List + 이름 Contains | Poolable 컴포넌트 + PoolManager |
| 종료·진행 | 패널 + 씬 재로드 | 별점, JSON 저장, 레벨 선택, 옵션 |
| 입력 | 마우스 드래그 + R키 | 키보드·마우스·터치 스킴 2종, 카메라 리그 |
| 데이터 | 프리팹 변형 + 씬 인스펙터 | ScriptableObject 30여 개 + 씬 |

강의에서의 쓰임새: 같은 기능을 "작게 만든 쪽"과 "확장할 수 있게 만든 쪽"을 나란히 읽고, 학생이 이 프로젝트에 공식 템플릿의 구조 하나(예: 웨이브, 또는 Damageable)를 이식해 보는 과제가 가능하다. 748줄은 한 주 안에 전부 읽히는 분량이다.

## 8. 발견 사항과 확장 지점

버그 후보는 코드를 읽어 추론한 것이고 실행으로 확인하지는 않았다 — 학생이 재현해 보는 것 자체가 과제다.

1. **승리가 늦게 뜨는다.** 승리 검사가 SpawnEnemy 안에만 있어 마지막 적을 잡아도 다음 스폰 틱(최대 5초)까지 패널이 안 뜨고, 적 수 표시도 검사마다 −1이 돼 음수로 가는 것을 Max(0)으로 가린다. 수정: ReduceEnemyHealth의 사망 쪽에서 검사.
2. **죽은 적을 조준한다.** Tower.CheckNearestEnemy가 비활성 적도 후보에 넣는다. 풀에 남은 시체 위치로 포탑이 돌고 탄환은 바로 꺼진다 — 발사 지연 한 번 낭비. `activeSelf` 필터 한 줄.
3. **탄환 풀 혼선.** Bullet Variant 1의 이름이 "Bullet"으로 덮어써져 있어 Contains("Bullet") 검색이 "Bullet Variant 2(Clone)"도 집어 온다. 수치는 매번 덮어쓰므로 피해는 맞지만 스프라이트가 섮인다. 풀을 타입/프리팹 참조 기반으로.
4. **한 칸에 두 개.** TowerPlacement.OnTriggerExit2D가 어떤 콜라이더가 나가든 \_placedTower를 비운다. 다른 타워를 끌고 스치면 점유된 칸이 빈 칸이 된다. 나간 콜라이더가 \_placedTower인지 확인해야 한다.
5. **timeScale이 반만 먹는다.** 스폰·발사는 unscaledDeltaTime, 적 이동·포탑 회전은 deltaTime. timeScale 0이면 적은 멈추는데 타워는 계속 쇬서 일시정지가 치트가 된다. 주석은 "독립적인 타이머"라고 설명하지만 이 게임엔 맞지 않는 선택.
6. **빈 Start/Update 6개.** 템플릿 잔재. 지우는 습관의 예제.
7. **적 풀 검색도 이름 숫자.** `name.Contains("1")` — 세 번째 적을 "Boss"라고 이름 지으면 풀이 깨진다.

**확장 지점** (2단계 입력)

- 값만: 타워·적 변형 추가, 씬 복제로 레벨 추가(경로 배열 재연결), 생명·적 수·간격.
- 작은 코드: 코인·비용(LevelManager 필드 + TowerUI 드래그 시작 조건 + 처치 보상), 즉시 승리 판정, 활성 적 필터, 일시정지(deltaTime 통일), 타워 판매(클릭 → 제거 + 칸 해제), 사거리 원 표시.
- 구조: 웨이브 목록(SpawnEnemy를 데이터 기반으로), ScriptableObject로 타워·적·레벨 분리, 타입 기반 풀, 메인 메뉴 씬, 진행 저장, 업그레이드(변형 교체).
- 공식 템플릿에서 이식해 보기: Damageable/Damager 분리, LevelState 상태 기계, Wave/SpawnInstruction.

출처: 업로드된 Tower-Defense-main.zip (evanderchristian/Tower-Defense, MIT)의 소스·씬·프리팹 직접 파싱, GitHub 2D Tower Defense Doxygen 분석자료, 씬 구조도 페이지.
