Happy Harvest 소스 분석 {#mainpage}
======================

이 문서는 Unity 공식 2D URP 샘플 **Happy Harvest**(Asset Store 1.6x, Unity 6000.3.10f1 기준)의
게임플레이 스크립트 52개를 Doxygen으로 분석한 자료다. 「유니티 샘플 기반 게임 제작」 강의의
1단계(분석) 교재이며, 2단계(기능 도출)에서 확장 포인트를 찾는 근거로 쓴다.

분석 범위는 `Assets/HappyHarvest/Scripts`와 `Assets/HappyHarvest/Common`이다.
VFX Graph 샘플 헬퍼, ReadMe·Tutorial 에디터 스크립트, 셰이더 그래프, 아트 자산은 제외했다.

@section sec_overview 한눈에 보는 구조

게임은 **영속 GameManager**(씬이 바뀌어도 살아남는 단 하나의 객체) 아래에 씬마다 등록되는
시스템들이 매달리는 구조다. 씬 오브젝트는 `Awake`/`OnEnable`에서 `GameManager.Instance`의
프로퍼티에 자기 자신을 등록하고, 다른 시스템은 그 프로퍼티를 통해서만 서로를 찾는다.

| 역할 | 클래스 | 수명 |
|---|---|---|
| 진입점·시간·씬 전환·전역 참조 | HappyHarvest.GameManager | 영속 (DontDestroyOnLoad) |
| 플레이어 이동·입력·인벤토리·코인 | HappyHarvest.PlayerController | 영속 (DontDestroyOnLoad) |
| 땅·물·작물 상태 (타일맵) | HappyHarvest.TerrainManager | 실외 씬 전용 |
| 낮/밤 조명 | HappyHarvest.DayCycleHandler | 씬마다 |
| 날씨 요소 on/off | HappyHarvest.WeatherSystem | 씬마다 |
| HUD·상점·창고·페이드 | HappyHarvest.UIHandler | 씬마다 |
| 아이템·작물 정의 | HappyHarvest.Item, HappyHarvest.Crop + 데이터베이스 | 에셋 (ScriptableObject) |
| 저장/불러오기 (F5/F9) | HappyHarvest.SaveSystem | static |

씬 구성: `Loader`(GameManager 생성 보장) → `MainMenu` → `Farm_Outdoor` ⇄ `House_Interior`.
실내 씬에는 TerrainManager가 없으므로 `GameManager.Instance.Terrain`이 null일 수 있다 — 도구의 `CanUse`가 이를 검사한다.

@section sec_loop 핵심 루프 1회: 클릭이 타일을 바꾸기까지

```
[입력] Use 액션 (마우스 클릭)
  └─ PlayerController.UseObject()
       ├─ 마우스 아래에 InteractiveObject(레이어 31)가 있으면 → InteractedWith()  (상점/창고 UI)
       └─ 없으면 → PlayerController.UseItem()
            └─ InventorySystem.UseEquippedObject(targetCell)
                 ├─ Item.CanUse(cell)  ← 도구별로 TerrainManager 상태 질의
                 └─ Item.Use(cell)     ← 도구별로 TerrainManager 함수 호출
                      Hoe      → TerrainManager.TillAt    (Tillable 타일 → Tilled 타일, GroundData 생성)
                      WaterCan → TerrainManager.WaterAt   (WaterTimer = 60초, Water 타일맵에 Watered 타일)
                      SeedBag  → TerrainManager.PlantAt   (CropData 생성, Crop 타일맵에 0단계 타일)
                      Basket   → TerrainManager.HarvestAt (GrowthRatio == 1이면 Produce 지급, 수확 횟수 증가)
                 └─ Consumable이면 스택 1 감소 → UIHandler.UpdateInventory
[매 프레임] TerrainManager.Update()
  └─ 모든 GroundData: WaterTimer 감소 → 0이면 Watered 타일 제거
     물이 있으면 CropData.GrowthTimer 증가 → 단계 바뀌면 Crop 타일 교체
     물이 없으면 CropData.DyingTimer 증가 → DryDeathTimer 초과 시 작물 제거
```

타깃 셀은 플레이어가 선 셀에서 마우스 방향으로 **최대 1칸** 떨어진 셀로 제한된다
(PlayerController.Update의 `toTarget` 클램프). 이것이 "바로 옆 칸만 경작할 수 있다"는 규칙의 전부다.

@section sec_data 데이터와 로직의 분리

- 작물·아이템은 전부 ScriptableObject 에셋이다 (`Assets/HappyHarvest/Data`). 새 작물 1종은 Product + Crop + SeedBag
  에셋 3개를 만들고 데이터베이스에 등록하면 코드 수정 없이 추가된다 → @ref manual_crops "새 작물 만들기".
- 새 도구는 HappyHarvest.Item을 상속해 `CanUse`/`Use`만 구현한다 → @ref manual_items "새 아이템 만들기".
- 밸런스 수치(성장 시간, 수확 횟수, 가격, 마르는 시간)는 모두 에셋 필드다. 코드에 박힌 상수는
  `InventorySystem.InventorySize = 9`, `GroundData.WaterDuration = 60초`, 시작 코인 `m_Coins = 10` 정도다.
- 저장 데이터는 ID 문자열(`UniqueID`)로 에셋을 참조하고 HappyHarvest.BaseDatabase 로 되찾는다.

@section sec_status 현황 판정 요약 (강의용)

| 기능 | 현황 | 근거 |
|---|---|---|
| 경작·물·심기·성장·수확 | ● 완성 | TerrainManager, 도구 4종 |
| 9칸 스택형 인벤토리 + 장착 전환 | ● 완성 | InventorySystem (칸 수 상수) |
| 코인·구매·판매 | ● 완성 | PlayerController.BuyItem / SellItem, MarketStall → UIHandler.OpenMarket |
| 창고 보관 | ● 완성 | Storage, Warehouse, WarehouseUI — 단, Storage는 저장되지 않음 |
| 낮/밤 사이클 + 시간대 이벤트 | ● 완성 | GameManager 시간 틱 → DayCycleHandler, DayEventHandler (UnityEvent) |
| 날씨 | ◐ 부분 | WeatherSystem은 요소 on/off만 하고 전환 로직 없음 (StartingWeather 고정) |
| 저장/불러오기 | ◐ 부분 | F5/F9 디버그 키, 플레이어·시간·지형만 저장, Storage·날씨 미저장, UI 없음 |
| 게임 목표·종료·날짜 | ○ 없음 | 하루가 반복될 뿐 날짜 카운트 없음 |
| 터치 입력 | ○ 없음 | `Mouse.current`, `Input.mousePosition` 직접 사용 |
| 성장 틱 | ▲ 주의 | TerrainManager.Update가 매 프레임 모든 셀을 순회 (개발자도 인정한 비효율) |

@section sec_extension 확장 포인트 (2단계 입력)

1. **데이터만으로**: 새 작물·새 상품·상점 품목(GameManager.MarketEntries 배열).
2. **새 컴포넌트로**: 날짜 카운터·게임 목표(GameManager의 하루 랩어라운드 지점에 이벤트 추가), 오프라인 보상, 날씨 전환 타이머.
3. **상속으로**: 새 도구(Item 상속), 새 상호작용 오브젝트(InteractiveObject 상속 — 레이어 31 자동 설정).
4. **기존 수정이 필요한 것**: Storage 저장, 터치 입력, 성장 틱 최적화, 창고 UI 다중 슬롯 조작.

@section sec_modules 모듈 안내

[모듈](topics.html) 페이지에서 역할별로 클래스를 묶어 두었다. 각 클래스 페이지에는 상속도·협력도·호출/피호출 그래프와
소스 링크가 있다. 호출 그래프는 "이 함수를 고치면 누가 영향을 받는가"(피호출 그래프)를 볼 때 특히 쓸모가 있다.

@note 이 문서의 한국어 설명은 강의용으로 분석자가 덧붙인 것이며, 각 멤버의 영문 주석은 원본 소스의 XML 주석이다.
