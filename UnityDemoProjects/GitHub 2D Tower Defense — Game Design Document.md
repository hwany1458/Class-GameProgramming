# GitHub 2D Tower Defense — Game Design Document

Nov 17, 2026 · @YongHwan Lee

Unity Learn의 GDD 템플릿 순서를 따라, GitHub 공개 프로젝트 evanderchristian/Tower-Defense(MIT, Unity 2018.4)의 소스·씬·프리팹에서 읽은 사실만으로 쓴 "역설계" GDD다. 템플릿에 없는 것은 「확장 여지」로 표시했다. 스크립트 8개짜리 게임이므로 이 GDD는 네 편 중 가장 짧고, 「확장 여지」가 가장 많다 — Unity 공식 Tower Defense Template GDD와 나란히 놓고 같은 장르의 "최소"와 "완성"을 비교하는 용도다.

## 1. 기본 정보

| 항목 | 내용 |
| --- | --- |
| **게임 이름** | 2D Tower Defense Game (README 제목). 인게임 타이틀 없음 — 「확장 여지」 |
| **장르** | 2D 타워 디펜스, 고정 경로 + 지정 칸 배치. 웨이브·경제가 없어 "디펜스"보다 "터렛 배치 퍼즐"에 가깝다 |
| **게임 요소** | ① 오른쪽 패널에서 타워 아이콘을 집어 노란 칸에 놓는다 ② 5초마다 한 기씩 오는 적을 타워가 알아서 쇬 잡는 것을 본다 ③ 못 잡은 적이 끝에 닿으면 생명이 줄어든다 ④ 15기를 다 막으면 이기고 다음 레벨로 |
| **플레이어 수** | 1인 |

## 2. 기술 사양

| 항목 | 내용 |
| --- | --- |
| **기술 형태** | 2D. Built-in 렌더 파이프라인, SpriteRenderer(정렬 순서로 앞뒤), 2D 물리(BoxCollider2D/CircleCollider2D 트리거 + 키네마틱 Rigidbody2D), uGUI. Unity 2018.4.36f1 (Unity 6에서 열려면 업그레이드 필요) |
| **시점** | 정면 고정 카메라(orthographic 5). 이동·줄 없음. 가로 16:9 기준 배치 |
| **플랫폼** | PC. uGUI 드래그는 터치에서도 동작하지만 R키 재시작은 키보드 전용. 이 강의에서는 WebGL 배포(가벼워 가장 쉽다) |
| **언어** | C#. 데이터는 프리팹 변형(Prefab Variant)과 씬 인스펙터뿐 — ScriptableObject · JSON · 저장 없음 |
| **기기** | 마우스(드래그) + 키보드 R. 게임패드 없음 |

코드는 클래스 8개 748줄. LevelManager 하나가 스폰·이동·조준·발사·승패·풀을 매 프레임 직접 구동하는 중앙집중형. 확장은 프리팹 변형 추가(값)와 LevelManager 수정(규칙) 두 길뿐이다.

## 3. 게임플레이

**처음 2분 (Level1)**

앱을 켜면 메뉴 없이 바로 풀밭 위에 갈색 길이 U자로 놓인 화면이다. 왼쪽 위에 Lives: 3, Total Enemy: 15. 오른쪽 패널에 포탑 그림 세 개. 길 옆 노란 판 네 개가 타워 자리다. 첫 번째 아이콘을 누른 채 끌면 타워가 마우스를 따라 나오고, 노란 판 위에서 놓으면 그 자리에 붙는다. 판 밖에 놓으면 사라진다. 비용이 없으므로 네 칸을 바로 다 채운다.

그 사이 왼쪽 끝에서 첫 적이 나와 길을 따라 오고 카운트는 14가 된다. 사거리에 들어오자 제일 가까운 타워의 포탑이 돌아가고, 정면을 향하면 탄환이 날아가 맞는다 — 헤드 위 체력 바가 줄고 피격음. 빠른 적(체력 3)은 두세 발, 느린 적(체력 6)은 세네 발에 터진다. 5초마다 한 기씩, 종류는 랜덤. 타워가 네 개면 거의 다 잡히지만 길 가운데 구간에서 빠른 적이 한두 기 빠져나가 오른쪽 끝에 닿으면 Lives: 2. 75초째 마지막 적이 나오고, 그게 터지면 몇 초 뒤 "You Win!" 패널과 Play Again? / Next Level ›. 생명 세을 먼저 잃으면 "You Lose!" — R키로 언제든 다시.

**게임플레이 아웃라인**

| 항목 | 내용 |
| --- | --- |
| 앱 실행 | 빌드 0 = Level1. 타이틀·메뉴·로딩 없음 — 「확장 여지」 |
| 옵션 | 없음 — 「확장 여지」 |
| 스토리 | 없음 |
| 모드 | 레벨 2개, 같은 규칙. 난이도 없음 |
| 게임 요소 | 타워 3종(기본·저격·범위), 적 2종(빠름·둔함), 생명 3, 적 15 |
| 레벨 | Level1 U자 25유닛 / Level2 S자 30유닛. 배치 칸 4개씩. 잠금 없음(Next/Prev 버튼) |
| 조작 | 마우스 드래그 앞드롭, R 재시작 |
| 승리 | 15기가 모두 스폰된 뒤 활성 적 0 |
| 패배 | 생명 3 → 0 (적 3기 통과) |
| 종료 | 패널 + Play Again / Next(Prev) Level. 엔딩·점수·저장 없음 |
| 왜 재미있나 | 사거리와 길의 기하 — "어느 칸에 어떤 타워를 놓아야 길 전체가 덮이나"를 고르는 한 번의 결정. 그 뒤는 관람 |

**핵심 기능**

- 아이콘을 집어 놓는 드래그 배치 — 버튼 클릭·모드 전환 없이 한 동작
- 역할이 다른 타워 3종: 연사(DPS 0.67) · 저격(사거리 8) · 범위(반경 2)
- 포탑이 적을 따라 돌고 조준이 맞아야 발사(10°)
- 적 머리 위 체력 바, 피격·사망 효과음
- 적·탄환 오브젝트 풀링

## 4. 디자인 문서

**디자인 지침** (프로젝트에서 읽어낼 수 있는 제약과 목표)

- 전연령, 단순 도형 스프라이트(Kenney 계열 탑다운 에셋). 피·폭발 없음.
- 한 판 약 90초(마지막 스폰 75초 + 통과). 턴·건설 단계 없이 처음부터 실시간.
- 플레이어의 개입은 **배치 한 가지**. 업그레이드·판매·스킬·웨이브 시작 없음.
- 새 타워·적은 프리팹 변형으로, 새 레벨은 씬 복제로. 코드는 8개 안에서.
- 한 줄 목표: "타워 디펜스의 핵심 루프를 가장 적은 코드로 보여 준다" — 입문 교재 프로젝트의 목표다.

**게임 디자인 정의**

| 항목 | 정의 | 구현 |
| --- | --- | --- |
| **승리** | 적 카운터가 0 아래로 내려간 뒤(15기 전부 스폰) 활성 적이 없을 때. 검사는 5초 스폰 틱에서만 — 최대 5초 지연 | LevelManager.SpawnEnemy → SetGameOver(true) |
| **패배** | 생명 3이 0. 적이 마지막 경로 노드에 닿을 때마다 −1 (종류 무관) | LevelManager.Update → ReduceLives → SetGameOver(false) |
| **레벨 전환** | 패널 버튼으로 씬 로드. 레벨 사이에 이어지는 상태 없음(영속 객체 없음) | PlayAgain.playAgain / nextScene / prevScene |
| **핵심 초점** | "네 칸에 무엇을 놓을가" — 한 번 놓으면 끝. 게임의 의사결정이 시작 직후 몇 초에 모두 끝난다 | TowerUI, TowerPlacement |
| **경제** | 없음. 타워 무료, 처치 보상 없음. 유일한 자원은 배치 칸 4개 | — |
| **적의 행동** | Transform 배열을 순서대로 MoveTowards, 도달 판정 0.1. 타워를 공격하지 않고 서로 밀지 않는다 | LevelManager.Update, Enemy.MoveToTarget |
| **타워의 행동** | 사거리 안 가장 가까운 적을 잡고(사거리 밖이면 버림), 180°/s로 회전, 10° 이내면 지연마다 발사 | Tower.CheckNearestEnemy / SeekTarget / ShootTarget |
| **피해** | 단일: 목표만. 범위: 충돌점 반경 안 활성 적 전부, 전부 같은 피해 | Bullet.OnTriggerEnter2D, LevelManager.ExplodeAt |

네 GDD를 나란히 놓으면 이 프로젝트가 가장 비어 있다: 승패는 있지만 경제·진행·메뉴·저장·웨이브가 모두 없다. 그래서 학생 과제로는 오히려 좋다 — "어떤 기능을 더해야 게임이 되는가"를 GDD의 빈칸이 그대로 보여 준다.

## 5. 게임 플로우차트

메뉴와 스토리 개요가 없으므로 게임플레이 · 플레이어 조작 · 게임 오버 세 영역뿐이다. 각 상자의 번호 ①\~⑫은 8절 참조표의 번호와 같다.

&#91;embedded content: 2D 타워 디펜스 게임 플로우 · ①\~⑫ · 판정 4, 종료 2\]

플레이어의 행동은 윗줄(③→④)에만 있고 그것도 언제든 반복할 수 있을 뿐 루프와 연결되지 않는다(비용·보상이 없어서). 루프 안의 판정 네 개는 전부 LevelManager.Update가 매 프레임 한다. "15기 끝 · 적 0?" 판정만은 5초 스폰 틱에서만 일어나 승리가 늦게 뜨는 원인이 된다.

## 6. 플레이어 정의

**플레이어 정의**: 아바타 없는 지휘관. 플레이어 = 생명 3 + 배치 칸 4개의 소유주. 할 수 있는 행동은 하나 — 타워 아이콘을 끌어 빈 칸에 놓기 — 와 재시작(R). 템플릿의 Health / Weapons / Actions는 각각 생명 3 / 타워 3종 / 배치로 대응한다.

**플레이어 속성** (각 속성이 바뀔 때의 피드백 포함)

| 속성 | 범위·초기값 | 바뀜 때 | 피드백 |
| --- | --- | --- | --- |
| 생명 | 3 → 0 | 적이 마지막 노드에 도착할 때마다 −1 (종류 무관) | "Lives: n" 텍스트만. 효과음·연출 없음 — 「확장 여지」 |
| 남은 스폰 | 15 → 0 | 스폰마다 −1 | "Total Enemy: n" — 화면 위 적 수가 아니라 아직 안 나온 수 |
| 배치된 타워 | 0\~4 (칸당 1, 버그로 2 가능) | 드롭 성공 | 타워가 칸에 붙고 정렬 순서가 내려감. 실패 시 그냥 사라짐 |
| 드래그 중 타워 | 없음 / 1 | 아이콘 누름\~놓음 | 타워가 마우스를 따라다님. 사거리·배치 가능 표시 없음 — 「확장 여지」 |
| IsOver | false / true | 승패 | 패널 + "You Win!"/"You Lose!" 전부 정지 |

타워 속성(피해·사거리·지연·탄속·범위)은 변형 프리팹에 고정되어 게임 중 바뀌지 않는다. 타워는 체력이 없고 적에게 공격받지 않는다.

**플레이어 보상** (긍정적 영향을 주는 것)

| 보상 | 효과 | 얻는 방법 |
| --- | --- | --- |
| 타워 3종 | 길을 덮는다 | 드래그 — 무료, 제한 없음(칸만 있으면) |
| 범위 피해 (V3) | 반경 2 안 적 전부에 피해 1 | 세 번째 타워 배치 |
| 다음 레벨 | Level2 열림 | 승리 후 Next Level —단, 잠금이 없어 Level2에서 Prev로도 갈 수 있고 빌드 설정으로 바로 열 수도 있다 |
| 코인 · 점수 · 업그레이드 · 판매 · 별 · 파워업 · 생명 회복 | 없음 | 「확장 여지」 — 보상 체계가 통째로 비어 있다 |

## 7. 사용자 인터페이스

**조작**

| 행동 | 입력 | 비고 |
| --- | --- | --- |
| 타워 배치 | 오른쪽 패널 아이콘을 누른 채 끌어 노란 칸 위에서 놓기 | uGUI IDragHandler — 터치에서도 동작. 칸 밖에 놓으면 사라진다 |
| 재시작 | R 키 (언제나) / 패널 Play Again? | R은 키보드 전용 |
| 레벨 이동 | 패널 Next Level › (L1) / ‹ Prev (L2) | 승패 무관하게 같은 버튼 |
| 카메라 · 일시정지 · 선택 · 취소 · 게임패드 | 없음 | 「확장 여지」 |

**HUD** (uGUI Canvas, 씬마다 복제): 왼쪽 위 Lives / Total Enemy 텍스트, 오른쪽 세로 패널에 타워 아이콘 3개(포탑 스프라이트 그대로), 가운데 결과 패널(비활성 시작). 월드에는 적 처치 피드백으로 체력 바와 효과음 2종뿐.

**화면에 없는 정보**: 타워의 사거리·피해·역할(아이콘만 보고 추측), 배치 가능 여부(놓아 봐야 안다), 현재 화면 위 적 수, 다음 적 종류, 승리까지 남은 시간. "Total Enemy"는 직관과 반대로 아직 안 나온 수다.

**최악의 레이아웃 검토**: 마우스(또는 터치)만으로 배치는 가능하지만 패배 후 재시작은 패널 버튼으로 가능하므로 터치도 통과. 키보드만으로는 플레이 불가. 세로 화면에서는 16:9 기준으로 놓인 길이 양쪽이 잘린다(카메라 ortho 5 고정) — 세로 길이 없는 가로 전용 레이아웃. 사거리가 보이지 않아 처음 플레이에서는 어느 칸이 좋은지 알 수 없다 — 사거리 원 표시가 첫 UI 과제로 적당하다.

## 8. 부록 — 게임 오브젝트·속성·동작 참조표

플로우차트의 번호와 같다. "구현" 열은 Doxygen 분석자료·씬 구조도와 연결되는 실제 이름이다.

| # | 오브젝트 | 속성 | 동작 | 구현 |
| --- | --- | --- | --- | --- |
| ① | 앱 실행 | 빌드 0 = Level1 | 메뉴 없이 바로 레벨 | EditorBuildSettings |
| ② | 타워 버튼 | \_towerPrefabs 3, 아이콘 = 포탑 스프라이트 | Start에서 종류마다 Tower UI Instantiate | LevelManager.InstantiateAllTowerUI, TowerUI.SetTowerPrefab |
| ③ | 아이콘 드래그 | \_currentSpawnedTower | 시작 시 월드에 타워 생성, 마우스 따라감, 정렬 순서 2 | TowerUI.OnBeginDrag / OnDrag, Tower.ToggleOrderInLayer |
| ④ | 배치 칸 | tower\_placement ×4, BoxCollider2D trigger, \_placedTower | 타워가 들어오면 PlacePosition 주기, 놓으면 고정·등록 / 밖이면 Destroy | TowerPlacement, Tower.LockPlacement, TowerUI.OnEndDrag, LevelManager.RegisterSpawnedTower |
| ⑤ | 적 스폰 | \_spawnDelay 5(unscaled), \_totalEnemy 15, \_enemyPrefabs 2 | 카운터 −1, 랜덤 종류, 이름 "1"/"2"로 풀 재사용, paths\[0\]에 배치 | LevelManager.SpawnEnemy |
| ⑥ | 경로 이동 | \_enemyPaths 11/13 (road Transform), \_moveSpeed 2/1, 도달 판정 0.1 | MoveTowards, 노드마다 4방향 회전, 마지막 노드면 ⑩ | LevelManager.Update, Enemy.MoveToTarget / SetTargetPosition |
| ⑦ | 조준·발사 | \_shootDistance 5/8/4, \_shootDelay 3/4/5, 회전 180°/s, 조준 허용 10° | 사거리 안 최근접 적(비활성 포함) → 포탑 회전 → 풀에서 탄환 | Tower.CheckNearestEnemy / SeekTarget / ShootTarget, LevelManager.GetBulletFromPool |
| — | 탄환 | \_bulletPower 2/1/1, \_bulletSpeed 4, \_bulletSplashRadius 0/0/2 | FixedUpdate 추적, 충돌 시 단일 피해 또는 ExplodeAt | Bullet, LevelManager.ExplodeAt |
| ⑧ | 체력 0? | \_maxHealth 3/6, \_currentHealth | ReduceEnemyHealth → 피격음, 체력 바 폭 | Enemy.ReduceEnemyHealth, AudioPlayer.PlaySFX |
| ⑨ | 적 사망 | — | 비활성(풀 반환), 사망음. 보상 없음 | Enemy.ReduceEnemyHealth |
| ⑩ | 생명 −1 | \_maxLives 3, \_currentLives | ReduceLives → 텍스트, 0이면 ⑫ | LevelManager.ReduceLives / SetCurrentLives |
| ⑪ | 승리 | 카운터 < 0 ∧ 활성 적 0 | SetGameOver(true): IsOver, "You Win!", 패널 | LevelManager.SpawnEnemy / SetGameOver |
| ⑫ | 패배 | \_currentLives 0 | SetGameOver(false): "You Lose!" | LevelManager.SetGameOver |
| — | 결과 패널 | Panel, Status · PlayAgain · LoadScene 버튼 | OnClick(UnityEvent) → playAgain / nextScene / prevScene; R키도 같은 효과 | PlayAgain, LevelManager.Update |
| — | 오디오 | \_audioClips 2 | PlaySFX(이름) | AudioPlayer |

**「확장 여지」 모음** (학생이 자기 GDD에서 채울 자리): 게임 이름·세계관, 메인 메뉴·옵션, 경제(비용·처치 보상·판매), 웨이브와 건설 단계, 업그레이드, 타워 사거리 표시, 생명 피드백, 즉시 승리 판정, 점수·별·저장·레벨 잠금, 일시정지(deltaTime 통일 필요), 모바일 재시작 버튼, 세로 화면 레이아웃, 적·타워 종류 추가(변형), 데이터의 ScriptableObject화, 버그 수정 5건(죽은 적 조준 · 탄환 풀 혼선 · 한 칸 두 개 · 늦은 승리 · 이름 기반 풀).

**출처**: 업로드된 Tower-Defense-main.zip (GitHub evanderchristian/Tower-Defense, MIT, Unity 2018.4.36f1)의 소스 · 씬 · 프리팹 변형 직접 분석, GitHub 2D Tower Defense Doxygen 분석자료, 씬 동작 분석 · 씬 구조도, 첨부된 Game Design Document Template(PDF).
