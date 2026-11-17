# GitHub 2D Tower Defense 씬 구조도

evanderchristian/Tower-Defense (Unity 2018.4, MIT). 씬 두 개의 YAML을 파싱해 얻은 구조와, 씬 Transform 좌표로 다시 그린 레벨 지도. 스크립트 8개·748줄이라 Unity 공식 Tower Defense Template(175개)과 같은 장르의 "최소 구현"으로 나란히 읽기 좋다.

[레벨 지도](#maps)[씬 구조](#structure)[수치표](#values)[발견](#findings)

## 레벨 지도 — 씬 좌표에서 그림

파란 번호 = LevelManager.\_enemyPaths 배열 순서(0 스폰 → 마지막 도착). 노란 칸 = TowerPlacement 트리거(칸당 타워 1개). 점선 = 카메라가 보여주는 영역(orthographic 5, 16:9 기준). 경로 노드와 배치 칸은 스크립트가 아니라 **씬에 손으로 놓은 Transform**이다.

**Level1** — 한 번 내려갔다 올라오는 U자. 배치 칸 4개가 모두 길에서 한 칸 거리.

**Level2** — 두 번 꺾이는 S자, 노드 13개. 가운데 구간이 배치 칸에서 멀어 사거리 4짜리 범위 타워(V3)는 닿지 않는다.

| 레벨 | 생명 | 적 수 | 스폰 간격 | 경로 노드 | 경로 길이 | 통과 시간 (빠른 적 / 느린 적) | 배치 칸 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| **Level1** | 3 | 15 | 5s | 11 | 25.0 유닛 | 12s / 25s | 4 |
| **Level2** | 3 | 15 | 5s | 13 | 30.0 유닛 | 15s / 30s | 4 |

두 씬의 LevelManager 값은 완전히 같다(생명 3 · 적 15 · 5초). 차이는 경로뿐이므로 난이도 = 경로 길이와 배치 칸 거리. 적 15기 × 5초 = 마지막 스폰까지 75초.

## 씬 구조 — 객체와 스크립트 연결

프리팹 인스턴스가 하나도 없다. 씬에는 LevelManager·배치 칸·경로·캔버스·오디오만 있고, 타워 UI 버튼·타워·적·탄환은 전부 런타임에 LevelManager와 TowerUI가 Instantiate한다. 연결은 거의 전부 LevelManager.Instance 직접 호출이며, UnityEvent는 결과 패널의 버튼 두 개뿐이다.

Unity 공식 템플릿과 가장 다른 점: **LevelManager.Update가 모든 타워의 CheckNearestEnemy → SeekTarget → ShootTarget과 모든 적의 MoveToTarget을 매 프레임 직접 호출**한다(중앙 Update 패턴). Tower·Enemy·Bullet의 Update()는 비어 있다. 이벤트도 상태 기계도 없고, IsOver 플래그 하나가 전체를 멈춘다.

## 수치표 — 프리팹 변형(Prefab Variant)에서 읽음

| 프리팹 | 변형 | 값 | 비고 |
| --- | --- | --- | --- |
| **Tower** 기본: 피해 1 · 사거리 5 · 지연 5s · 탄속 4 | Variant 1 | 피해 2 · 사거리 5 · 지연 3s · 단일 | DPS 0.67 — 가장 효율적 |
| Variant 2 | 피해 1 · 사거리 8 · 지연 4s · 단일 | DPS 0.25 — 장거리 |  |
| Variant 3 | 피해 1 · 사거리 4 · 지연 5s · **범위 2** | Bullet Variant 2 사용, ExplodeAt로 반경 안 전부 |  |
| **Enemy** 기본: 체력 1 · 속도 1 | Variant 1 | 체력 3 · 속도 2 | 빠름. 체력 바는 자식이지만 회전에서 제외 |
| Variant 2 | 체력 6 · 속도 1 | 느리고 단단함 |  |
| **Bullet** | Variant 1 | 스프라이트 bullet_1, 이름 "Bullet" | 피해·속도·범위는 타워가 SetProperties로 주입. 풀 검색은 이름 Contains |
| Variant 2 | 스프라이트 bullet_2 |  |  |

경제 없음(타워 무료·보상 없음), 웨이브 없음(5초마다 랜덤 1기), 저장 없음. 적 15기 전부 느린 적이면 총 체력 90, 빠른 적이면 45 — 배치 칸 4개에 V1을 채우면 DPS 2.7.

## 이 구조도가 말해 주는 것

- **레벨 = 씬에 놓은 Transform.** 경로는 road 스프라이트 객체 자체를 순서대로 배열에 끌어다 놓은 것이고, 배치 칸은 트리거 콜라이더 4개. 새 레벨은 씬 복제 → 길 스프라이트 옮기기 → 배열 다시 채우기. 데이터 에셋이 전혀 없으므로 "ScriptableObject로 빼기"가 좋은 첫 리팩터링 과제.
- **승리가 늦게 뜬다.** 승리 검사가 SpawnEnemy 안에만 있어 마지막 적을 잡아도 다음 스폰 틱(최대 5초)까지 패널이 안 뜬다.
- **죽은 적을 조준한다.** Tower.CheckNearestEnemy가 비활성 적도 후보에 넣는다. 풀에 남은 시체 위치로 포탑이 돌아가고, 탄환은 목표가 비활성이라 바로 꺼진다 — 발사 지연만 낭비. activeSelf 필터 한 줄이 과제.
- **탄환 풀 혼선.** Bullet Variant 1의 이름이 "Bullet"으로 덮어써져 있어 Contains 검색이 Variant 2(범위탄, 다른 스프라이트)도 집어 온다. 수치는 매번 덮어쓰므로 피해는 맞지만 그림이 섞인다.
- **한 칸에 두 개.** TowerPlacement.OnTriggerExit2D가 어떤 콜라이더가 나가든 칸을 비운다. 다른 타워를 끌고 지나가면 이미 놓인 칸이 "빈 칸"이 된다.
- **timeScale이 반만 먹는다.** 스폰·발사는 unscaledDeltaTime, 적 이동은 deltaTime. timeScale 0으로 일시정지하면 적은 멈추는데 타워는 계속 쏜다 — 일시정지 기능을 넣을 때 반드시 만나는 버그.
- **빈 Start/Update 6개.** 템플릿 잔재. 호출 비용은 작지만 "비어 있으면 지운다"는 습관의 예제.
- **터치 호환.** uGUI 드래그 핸들러라 모바일에서도 배치는 되지만, 재시작은 R키뿐이다(결과 패널 버튼은 있음).