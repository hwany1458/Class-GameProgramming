# Tower Defense Template 씬 구조도

씬 파일과 프리팹을 직접 파싱해 얻은, 씬 안의 중요 객체와 거기에 붙은 스크립트, 스크립트 사이의 연결. Unity 6000.3.10f1 갱신판 기준. 레벨 씬 5개는 같은 틀이므로 Level1(기본형)과 Level5(두 갈래 경로)만 그렸다.

[읽는 법](#read)[레벨 비교](#levels)[Level1](#Level1)[Level5](#Level5)[MainMenu](#MainMenu)[타워·적 프리팹](#prefabs)[발견](#findings)

## 읽는 법

씬에 직접 배치된 객체 (같은 종류는 ×n으로 묶음) 프리팹 인스턴스 노드 안 파란 행 = 인스펙터 값 요약 구독: C# 이벤트(levelCompleted, currencyChanged …)를 듣는다 호출: 코드에서 다른 시스템의 메서드를 부름 참조: 인스펙터 필드로 다른 객체를 가리킴

Happy Harvest가 "모두 GameManager에 등록"하는 구조였다면, 이 템플릿은 **씬마다 LevelManager·GameUI·PoolManager 싱글턴**이 있고 HUD 프리팹들이 그 싱글턴의 C# 이벤트를 구독한다. 영속 GameManager는 레벨 목록·저장·오디오만 맡는다. 레벨 씬 하나는 **LevelNConfiguration**(LevelManager + WaveManager + 웨이브 10개 + 경로 노드 + 홈베이스), **LevelNPlacementAreas**(배치 그리드), **LevelNPoolManager**, 그리고 카메라·HUD 프리팹 12개로 이루어진다. 웨이브 구성은 데이터 에셋이 아니라 씬 안의 Wave 컴포넌트 필드에 있다.

## 레벨 5개 비교 (씬에서 읽은 값)

| 레벨 | 시작 에너지 | 타워 라이브러리 | 경로 노드 | 그리드 / 단일 칸 | 웨이브 / 적 수 | 적 구성 |
| --- | --- | --- | --- | --- | --- | --- |
| **Level1** Don't allow enemies to rush through | 10 | 기관총·로켓·레이저 | 6 | 4 / 5 | 10 / 174 | Hoverbuggy 79, Hovertank 62, Hovercopter 32, Hoverboss 1 |
| **Level2** Create mazes to slow enemies | 15 | + EMP | 3 | 2 / 0 | 10 / 141 | Hoverbuggy 71, Hovertank 50, Hovercopter 15, Super Hovertank 5 |
| **Level3** Take advantage of choke points | 20 | + EMP · 슈퍼타워 | 4 (랜덤 분기) | 3 / 0 | 10 / 209 | Hoverbuggy 59, Super Hoverbuggy 42, Hovertank 31, Super Hovercopter 28, Super Hovertank 25, Hovercopter 24 |
| **Level4** Protect the long mountain pass | 15 | 기관총·로켓·레이저·슈퍼타워 | 5 | 11 / 9 | 10 / 181 | Hoverbuggy 57, Super Hoverbuggy 39, Super Hovertank 37, Super Hovercopter 18, Hovercopter 14, Hovertank 13, Hoverboss 3 |
| **Level5** Guard both paths from enemies | 20 | 전부 (파일런·EMP·슈퍼타워 포함) | 6 (랜덤 분기) | 9 / 2 | 10 / 200 | Super Hoverbuggy 58, Hoverbuggy 46, Hovertank 24, Super Hovercopter 22, Super Hovertank 19, Hovercopter 16, Hoverboss 10, Super Hoverboss 5 |

모든 레벨: 홈베이스 체력 10, 상시 수입 0(alwaysGainCurrency 꺼짐, 수입은 적 처치 LootDrop과 파일런만), 마지막 10번째 웨이브는 TimedWave가 아닌 Wave(다음 웨이브 없음). Prototype 씬은 Level1과 동일 구성의 그레이박스.

## Level1

첫 레벨. 외길 경로(Start→1→2→3→4→End), 배치 그리드 4 + 단일 칸 5, 시작 에너지 10, 타워 3종. 웨이브 10개·적 174기. 아래 구조는 Level2\~5도 동일하며 노드 수·배치 영역·웨이브 구성·타워 라이브러리만 다르다. GameObject 153개, 프리팹 인스턴스 14개.

### 객체 → 스크립트 → 설정값·이벤트

| 객체 (계층 경로) | 스크립트 | 인스펙터 값 · UnityEvent 연결 |
| --- | --- | --- |
| Level1Configuration/Level1WaveManager 씬 직접 배치 | `TimedLevelIntro` 시작 인트로(1초) | **time** 1 |
| Level1Configuration/Level1WaveManager 씬 직접 배치 | `LevelManager` 레벨 상태 기계·통화·홈베이스·승패 | **intro** Level1WaveManager(TimedLevelIntro) **towerLibrary** Level_1_TowerLibrary **startingCurrency** 10 **alwaysGainCurrency** 0 **homeBases** End(PlayerHomeBase) **environmentColliders** ?(MeshCollider) |
| Level1Configuration/Level1WaveManager 씬 직접 배치 | `WaveManager` 웨이브 순서 진행 | **startWavesOnAwake** 0 **waves** 1(TimedWave), 2(TimedWave), 3(TimedWave), 4(TimedWave), 5(TimedWave), 6(TimedWave), 7(TimedWave), 8(TimedWave), 9(TimedWave), 10(Wave) |
| Level1Configuration/Nodes/1 씬 직접 배치 | `FixedNodeSelector` 다음 노드 고정 | **linkedNodes** 2(Node) |
| Level1Configuration/… ×6 씬 직접 배치 | `Node` 경로 지점 | **areaMesh** AreaMeshCreator(AreaMeshCreator) **weight** 1 |
| Level1Configuration/Nodes/2 씬 직접 배치 | `FixedNodeSelector` 다음 노드 고정 | **linkedNodes** 3(Node) |
| Level1Configuration/Nodes/3 씬 직접 배치 | `FixedNodeSelector` 다음 노드 고정 | **linkedNodes** 4(Node) |
| Level1Configuration/Nodes/4 씬 직접 배치 | `FixedNodeSelector` 다음 노드 고정 | **linkedNodes** End(Node) |
| (프리팹 내부 객체) 씬 직접 배치 | `PauseMenu` 일시정지 메뉴 | — |
| (프리팹 내부 객체) 씬 직접 배치 | `TowerUI` 선택 타워 패널(업그레이드·판매) | — |
| (프리팹 내부 객체) ×2 씬 직접 배치 | `MovingCanvas` 월드 위치 따라가는 UI | — |
| (프리팹 내부 객체) 씬 직접 배치 | `TowerDefenseTouchInput` 터치 스킴 | — |
| (프리팹 내부 객체) 씬 직접 배치 | `BuildInfoUI` 타워 정보 패널 열고 닫기 | — |
| Level1Configuration/… ×6 씬 직접 배치 | `AreaMeshCreator` 노드 도착 영역 메시 | **outSidePointsParent** Points(Transform) |
| Level1Configuration/Nodes/End 씬 직접 배치 | `PlayerHomeBase` 지켜야 할 기지(Damageable) | **chargePfx** ChargeEffect(ParticleSystem) **chargeSound** ChargeSound(RandomAudioSource) **attackPfx** AttackPfx(ParticleSystem) **attackSound** AttackSound(RandomAudioSource) |
| Level1Configuration/… ×3 씬 직접 배치 | `RandomAudioSource` 랜덤 효과음 | **playOnEnabled** 0 |
| Level1Configuration/… ×2 씬 직접 배치 | `MobileParticleSelector` 모바일/데스크톱 파티클 선택 | **defaultParticles** Desktop(ParticleSystem) **mobileParticles** Mobile(ParticleSystem) |
| Level1Configuration/Nodes/Start 씬 직접 배치 | `FixedNodeSelector` 다음 노드 고정 | **linkedNodes** 1(Node) |
| Level1PlacementAreas/… ×4 씬 직접 배치 | `TowerPlacementGrid` 격자 배치 영역 | **placementTilePrefab** PlacementTile **placementTilePrefabMobile** PlacementTileMobile **gridSize** 1 |
| Level1PlacementAreas/… ×5 씬 직접 배치 | `SingleTowerPlacementArea` 단일 칸 배치 영역 | **placementTilePrefab** SinglePlacementTile **placementTilePrefabMobile** SinglePlacementTileMobile |
| Level1PoolManager 씬 직접 배치 | `PoolManager` 풀링(적·투사체·이펙트) | **poolables** \[21개\] Hovercopter, Hoverboss, Hoverbuggy, Hovertank, LaserProjectile_0, LaserProjectile_1 … |
| Level1Configuration/Level1WaveManager/1 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[16개\] Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy … **timeToNextWave** 42 |
| Level1Configuration/Level1WaveManager/2 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[14개\] Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy … **timeToNextWave** 35 |
| Level1Configuration/Level1WaveManager/3 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[11개\] Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy … **timeToNextWave** 25 |
| Level1Configuration/Level1WaveManager/4 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[12개\] Hovertank, Hovertank, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy … **timeToNextWave** 36 |
| Level1Configuration/Level1WaveManager/5 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[15개\] Hovercopter, Hovercopter, Hovercopter, Hovercopter, Hovercopter, Hovercopter … **timeToNextWave** 40 |
| Level1Configuration/Level1WaveManager/6 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[20개\] Hoverbuggy, Hoverbuggy, Hovertank, Hoverbuggy, Hovertank, Hovertank … **timeToNextWave** 42 |
| Level1Configuration/Level1WaveManager/7 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[20개\] Hoverbuggy, Hovercopter, Hoverbuggy, Hovercopter, Hovercopter, Hovertank … **timeToNextWave** 30 |
| Level1Configuration/Level1WaveManager/8 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[40개\] Hoverbuggy, Hoverbuggy, Hoverbuggy, Hovertank, Hovertank, Hovertank … **timeToNextWave** 47 |
| Level1Configuration/Level1WaveManager/9 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[25개\] Hovertank, Hovertank, Hovertank, Hovertank, Hovertank, Hovertank … **timeToNextWave** 50 |
| Level1Configuration/Level1WaveManager/10 씬 직접 배치 | `Wave` 스폰 명령 목록(마지막 웨이브) | **spawnInstructions** {agentConfiguration=Hoverboss, delayToSpawn=0, startingNode=Start(Node)} |
| Build Menu/Sidebar 프리팹 Build Menu | `BuildSidebar` 타워 구매 버튼 생성 | **towerSpawnButton** TowerBuildButton |
| Build Menu/BuildInfoMask/BuildInfo 프리팹 Build Menu | `BuildInfoUI` 타워 정보 패널 열고 닫기 | **anim** BuildInfo(Animation) **showClipName** Show **hideClipName** Hide |
| Build Menu/BuildInfoMask/BuildInfo 프리팹 Build Menu | `TowerUI` 선택 타워 패널(업그레이드·판매) | **towerName** Name(?) **description** Description (?) **upgradeDescription** None **sellButton** None **upgradeButton** None **towerInfoDisplay** BuildInfo(TowerInfoDisplay) **panelRectTransform** BuildInfo(RectTransform) **confirmationButtons** |
| Build Menu/BuildInfoMask/BuildInfo 프리팹 Build Menu | `TowerInfoDisplay` 타워 이름·설명·DPS 표시 | **towerName** Name(?) **description** Description (?) **dps** Dps(?) **level** None **health** None **dimensions** None **upgradeCost** None **sellPrice** None |
| PauseMenu/PauseMenu/LearnMore 프리팹 PauseMenu | `UrlOpen` 외부 링크 | — |
| PauseMenu/PauseMenu/Container/Buttons/Main Menu/MainMenuConfirm 프리팹 PauseMenu | `SceneLoader` 씬 로드 버튼 | **sceneToLoadName** MainMenu |
| PauseMenu/PauseMenu/Container/Buttons/Restart 프리팹 PauseMenu | `SceneLoader` 씬 로드 버튼 | **sceneToLoadName** LevelSelect |
| PauseMenu 프리팹 PauseMenu | `PauseMenu` 일시정지 메뉴 | **pauseMenuCanvas** PauseMenu(Canvas) **titleText** LevelTitleText(?) **descriptionText** LevelDescriptionText(?) **levelSelectConfirmButton** MainMenuConfirm(?) **restartConfirmButton** RestartConfirm(?) **levelSelectButton** Main Menu(?) **restartButton** Restart(?) **topPanel** Top(?) |
| TowerControllerUI 프리팹 TowerControllerUI | `TowerInfoDisplay` 타워 이름·설명·DPS 표시 | **towerName** Name(?) **description** Description(?) **dps** Dps(?) **level** None **health** None **dimensions** None **upgradeCost** EnergyText(?) **sellPrice** EnergyText(?) |
| TowerControllerUI 프리팹 TowerControllerUI | `TowerUI` 선택 타워 패널(업그레이드·판매) | **towerName** Name(?) **description** None **upgradeDescription** UpgradeDescriptionText(?) **sellButton** SellButton(?) **upgradeButton** UpgradeButton(?) **towerInfoDisplay** TowerControllerUI(TowerInfoDisplay) **panelRectTransform** Panel(RectTransform) **confirmationButtons** SellConfirmButton |
| TowerPlaceConfirmationUI 프리팹 TowerPlaceConfirmationUI | `MovingCanvas` 월드 위치 따라가는 UI | **content** Content(RectTransform) |
| WaveContainer 프리팹 WaveContainer | `WaveUI` 웨이브 번호·진행 바 | **display** WaveNumber(?) **waveFillImage** SliderForeground(?) |
| TowerPlaceInvalid 프리팹 TowerPlaceInvalid | `MovingCanvas` 월드 위치 따라가는 UI | **content** Content(RectTransform) |
| GameManager 프리팹 GameManager | `GameManager` 영속 싱글턴·저장·레벨 목록 | **gameMixer** GameMix.mixer **masterVolumeParameter** MasterVolume **sfxVolumeParameter** SFXVolume **musicVolumeParameter** MusicVolume **levelList** LevelList |
| GameCamera 프리팹 GameCamera | `TowerDefenseKeyboardMouseInput` 키보드·마우스 스킴 | **cameraRig** GameCamera(CameraRig) **nearZoomPanSpeedModifier** 0.5 **screenPanThreshold** 40 **mouseEdgePanSpeed** 30 **mouseRmbPanSpeed** 15 |
| GameCamera 프리팹 GameCamera | `CameraInitialState` 카메라 초기 줌·주시 | **startZoomMode** 1 **initialLookAt** None |
| GameCamera 프리팹 GameCamera | `CameraRig` 카메라 이동·줌 | **lookDampFactor** 0 **movementDampFactor** 0 **nearestZoom** 10 **furthestZoom** 20 **maxZoom** 60 **zoomLogFactor** 10 **zoomRecoverSpeed** 20 **floorY** 0 **zoomedCamAngle** ZoomAngle(Transform) **springyZoom** 0 |
| GameCamera 프리팹 GameCamera | `InputController` 터치/마우스 공통 입력 해석 | **dragThresholdTouch** 5 **dragThresholdMouse** 1 **tapTime** 0.2 **holdTime** 0.8 **mouseWheelSensitivity** 1 **trackMouseButtons** 2 **flickThreshold** 2 |
| GameCamera 프리팹 GameCamera | `TowerDefenseInputSchemeSwitcher` 입력 스킴 자동 전환 | — |
| GameCamera 프리팹 GameCamera | `TowerDefenseTouchInput` 터치 스킴 | **cameraRig** GameCamera(CameraRig) **nearZoomPanSpeedModifier** 0.5 **panSpeed** 15 **flickDecayFactor** 0.2 **panAreaScreenPercentage** 0.2 **confirmationButtons** None **invalidButtons** None |
| GameCamera 프리팹 GameCamera | `GameUI` 입력 → 배치·선택·업그레이드·판매 상태 기계 | **sphereCastRadius** 1 **radiusVisualizerController** RadiusVisualiser(RadiusVisualizerController) **towerUI** None **buildInfoUI** None |
| GameCamera/RadiusVisualiser 프리팹 GameCamera | `RadiusVisualizerController` 사거리 원 표시 | **radiusVisualizerPrefab** RadiusVisualiser **radiusVisualizerHeight** 0.02 |
| GameCamera 프리팹 GameCamera | `PostProcessorConfigurationSelector` 화질별 포스트프로세싱 | **highQualityProfile** HighQPostprocessing **lowQualityProfile** LowQPostprocessing |
| Game Over 프리팹 Game Over | `EndGameScreen` 승패 화면·별점·저장 | **victorySound** TD Victory.wav **defeatSound** TD Defeat .wav **audioSource** Game Over(AudioSource) **endGameCanvas** Game Over(Canvas) **endGameMessageText** TitleText(?) **scorePanel** Score Panel(ScorePanel) **menuSceneName** MainMenu **levelCompleteText** {0} COMPLETE! **levelFailedText** {0} FAILED! **background** Background(?) **nextLevelButton** Next Level(Canvas) |
| Game Over/LearnMore 프리팹 Game Over | `UrlOpen` 외부 링크 | — |
| Game Over/Container/Top/Score Panel 프리팹 Game Over | `ScorePanel` 별 3개 표시 | **starImages** 1(?), 2(?), 3(?) **achievedStarSprite** icon_star_small_gold.png |
| PlayerBaseLife 프리팹 PlayerBaseLife | `PlayerBaseHealth` 홈베이스 체력 표시 | **display** LifeCounter(?) |
| PlayerBaseLife/Panel/CurrencyContainer 프리팹 PlayerBaseLife | `CurrencyUI` 통화(에너지) 표시 | **display** CurrencyAmount(?) **currencySymbol** Energy: |

그 밖의 프리팹 인스턴스: Enviro_Level_1_COL.fbx ×1, HeadQuarters.fbx ×1, Enviro_Level_1.fbx ×1

## Level5

마지막 레벨. Start에서 RandomNodeSelector로 Left/Right 두 갈래로 갈라지며, 파일런·EMP·슈퍼타워까지 모든 타워가 열린다. 적 200기, 보스 15기. GameObject 142개, 프리팹 인스턴스 14개.

### 객체 → 스크립트 → 설정값·이벤트

| 객체 (계층 경로) | 스크립트 | 인스펙터 값 · UnityEvent 연결 |
| --- | --- | --- |
| Level5Configuration/Level5WaveManager 씬 직접 배치 | `TimedLevelIntro` 시작 인트로(1초) | **time** 1 |
| Level5Configuration/Level5WaveManager 씬 직접 배치 | `LevelManager` 레벨 상태 기계·통화·홈베이스·승패 | **intro** Level5WaveManager(TimedLevelIntro) **towerLibrary** Level_5_TowerLibrary **startingCurrency** 20 **alwaysGainCurrency** 0 **homeBases** End(PlayerHomeBase) **environmentColliders** ?(MeshCollider) |
| Level5Configuration/Level5WaveManager 씬 직접 배치 | `WaveManager` 웨이브 순서 진행 | **startWavesOnAwake** 0 **waves** 1(TimedWave), 2(TimedWave), 3(TimedWave), 4(TimedWave), 5(TimedWave), 6(TimedWave), 7(TimedWave), 8(TimedWave), 9(TimedWave), 10(Wave) |
| (프리팹 내부 객체) 씬 직접 배치 | `BuildInfoUI` 타워 정보 패널 열고 닫기 | — |
| (프리팹 내부 객체) 씬 직접 배치 | `PauseMenu` 일시정지 메뉴 | — |
| (프리팹 내부 객체) 씬 직접 배치 | `TowerUI` 선택 타워 패널(업그레이드·판매) | — |
| (프리팹 내부 객체) ×2 씬 직접 배치 | `MovingCanvas` 월드 위치 따라가는 UI | — |
| (프리팹 내부 객체) 씬 직접 배치 | `TowerDefenseTouchInput` 터치 스킴 | — |
| Level5Configuration/Nodes/End 씬 직접 배치 | `PlayerHomeBase` 지켜야 할 기지(Damageable) | **chargePfx** ChargeEffect(ParticleSystem) **chargeSound** ChargeSound(RandomAudioSource) **attackPfx** AttackPfx(ParticleSystem) **attackSound** AttackSound(RandomAudioSource) |
| Level5Configuration/… ×6 씬 직접 배치 | `Node` 경로 지점 | **areaMesh** AreaMeshCreator(AreaMeshCreator) **weight** 1 |
| Level5Configuration/… ×6 씬 직접 배치 | `AreaMeshCreator` 노드 도착 영역 메시 | **outSidePointsParent** Points(Transform) |
| Level5Configuration/… ×3 씬 직접 배치 | `RandomAudioSource` 랜덤 효과음 | **playOnEnabled** 0 |
| Level5Configuration/… ×2 씬 직접 배치 | `MobileParticleSelector` 모바일/데스크톱 파티클 선택 | **defaultParticles** Desktop(ParticleSystem) **mobileParticles** Mobile(ParticleSystem) |
| Level5Configuration/Nodes/Left1 씬 직접 배치 | `FixedNodeSelector` 다음 노드 고정 | **linkedNodes** Left2(Node) |
| Level5Configuration/Nodes/Left2 씬 직접 배치 | `FixedNodeSelector` 다음 노드 고정 | **linkedNodes** End(Node) |
| Level5Configuration/Nodes/Right1 씬 직접 배치 | `FixedNodeSelector` 다음 노드 고정 | **linkedNodes** Right2(Node) |
| Level5Configuration/Nodes/Right2 씬 직접 배치 | `FixedNodeSelector` 다음 노드 고정 | **linkedNodes** End(Node) |
| Level5Configuration/Nodes/Start 씬 직접 배치 | `RandomNodeSelector` 다음 노드 랜덤 | **linkedNodes** Left1(Node), Right1(Node) |
| Level5PlacementAreas/… ×9 씬 직접 배치 | `TowerPlacementGrid` 격자 배치 영역 | **placementTilePrefab** PlacementTile **placementTilePrefabMobile** PlacementTileMobile **gridSize** 1 |
| Level5PlacementAreas/… ×2 씬 직접 배치 | `SingleTowerPlacementArea` 단일 칸 배치 영역 | **placementTilePrefab** SinglePlacementTile **placementTilePrefabMobile** SinglePlacementTileMobile |
| Level5PoolManager 씬 직접 배치 | `PoolManager` 풀링(적·투사체·이펙트) | **poolables** \[28개\] Hovercopter, Hoverboss, Hoverbuggy, Hovertank, Enemy Projectile, LaserProjectile_0 … |
| Level5Configuration/Level5WaveManager/1 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[20개\] Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy … **timeToNextWave** 55 |
| Level5Configuration/Level5WaveManager/2 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[24개\] Hovertank, Hovertank, Hovertank, Hoverbuggy, Hoverbuggy, Hoverbuggy … **timeToNextWave** 52 |
| Level5Configuration/Level5WaveManager/3 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[31개\] Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy … **timeToNextWave** 45 |
| Level5Configuration/Level5WaveManager/4 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[13개\] Hoverboss, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hovertank, Hoverbuggy … **timeToNextWave** 45 |
| Level5Configuration/Level5WaveManager/5 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[12개\] Hoverboss, Super Hovertank, Super Hovertank, Super Hoverbuggy, Super Hoverbuggy, Hoverboss … **timeToNextWave** 55 |
| Level5Configuration/Level5WaveManager/6 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[16개\] Hoverboss, Hoverboss, Super Hovertank, Super Hovertank, Super Hoverbuggy, Super Hoverbuggy … **timeToNextWave** 35 |
| Level5Configuration/Level5WaveManager/7 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[27개\] Hoverboss, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy, Hoverbuggy … **timeToNextWave** 50 |
| Level5Configuration/Level5WaveManager/8 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** {agentConfiguration=Super Hoverboss, delayToSpawn=0, startingNode=Start(Node)} **timeToNextWave** 60 |
| Level5Configuration/Level5WaveManager/9 씬 직접 배치 | `TimedWave` 스폰 명령 + 다음 웨이브까지 시간 | **spawnInstructions** \[20개\] Hoverboss, Super Hovertank, Super Hovertank, Hoverboss, Super Hovertank, Super Hovertank … **timeToNextWave** 45 |
| Level5Configuration/Level5WaveManager/10 씬 직접 배치 | `Wave` 스폰 명령 목록(마지막 웨이브) | **spawnInstructions** \[36개\] Hoverboss, Hoverboss, Hovertank, Hovertank, Hovertank, Hovertank … |
| PlayerBaseLife 프리팹 PlayerBaseLife | `PlayerBaseHealth` 홈베이스 체력 표시 | **display** LifeCounter(?) |
| PlayerBaseLife/Panel/CurrencyContainer 프리팹 PlayerBaseLife | `CurrencyUI` 통화(에너지) 표시 | **display** CurrencyAmount(?) **currencySymbol** Energy: |
| Build Menu/Sidebar 프리팹 Build Menu | `BuildSidebar` 타워 구매 버튼 생성 | **towerSpawnButton** TowerBuildButton |
| Build Menu/BuildInfoMask/BuildInfo 프리팹 Build Menu | `BuildInfoUI` 타워 정보 패널 열고 닫기 | **anim** BuildInfo(Animation) **showClipName** Show **hideClipName** Hide |
| Build Menu/BuildInfoMask/BuildInfo 프리팹 Build Menu | `TowerUI` 선택 타워 패널(업그레이드·판매) | **towerName** Name(?) **description** Description (?) **upgradeDescription** None **sellButton** None **upgradeButton** None **towerInfoDisplay** BuildInfo(TowerInfoDisplay) **panelRectTransform** BuildInfo(RectTransform) **confirmationButtons** |
| Build Menu/BuildInfoMask/BuildInfo 프리팹 Build Menu | `TowerInfoDisplay` 타워 이름·설명·DPS 표시 | **towerName** Name(?) **description** Description (?) **dps** Dps(?) **level** None **health** None **dimensions** None **upgradeCost** None **sellPrice** None |
| TowerControllerUI 프리팹 TowerControllerUI | `TowerInfoDisplay` 타워 이름·설명·DPS 표시 | **towerName** Name(?) **description** Description(?) **dps** Dps(?) **level** None **health** None **dimensions** None **upgradeCost** EnergyText(?) **sellPrice** EnergyText(?) |
| TowerControllerUI 프리팹 TowerControllerUI | `TowerUI` 선택 타워 패널(업그레이드·판매) | **towerName** Name(?) **description** None **upgradeDescription** UpgradeDescriptionText(?) **sellButton** SellButton(?) **upgradeButton** UpgradeButton(?) **towerInfoDisplay** TowerControllerUI(TowerInfoDisplay) **panelRectTransform** Panel(RectTransform) **confirmationButtons** SellConfirmButton |
| PauseMenu/PauseMenu/LearnMore 프리팹 PauseMenu | `UrlOpen` 외부 링크 | — |
| PauseMenu/PauseMenu/Container/Buttons/Main Menu/MainMenuConfirm 프리팹 PauseMenu | `SceneLoader` 씬 로드 버튼 | **sceneToLoadName** MainMenu |
| PauseMenu/PauseMenu/Container/Buttons/Restart 프리팹 PauseMenu | `SceneLoader` 씬 로드 버튼 | **sceneToLoadName** LevelSelect |
| PauseMenu 프리팹 PauseMenu | `PauseMenu` 일시정지 메뉴 | **pauseMenuCanvas** PauseMenu(Canvas) **titleText** LevelTitleText(?) **descriptionText** LevelDescriptionText(?) **levelSelectConfirmButton** MainMenuConfirm(?) **restartConfirmButton** RestartConfirm(?) **levelSelectButton** Main Menu(?) **restartButton** Restart(?) **topPanel** Top(?) |
| TowerPlaceConfirmationUI 프리팹 TowerPlaceConfirmationUI | `MovingCanvas` 월드 위치 따라가는 UI | **content** Content(RectTransform) |
| WaveContainer 프리팹 WaveContainer | `WaveUI` 웨이브 번호·진행 바 | **display** WaveNumber(?) **waveFillImage** SliderForeground(?) |
| TowerPlaceInvalid 프리팹 TowerPlaceInvalid | `MovingCanvas` 월드 위치 따라가는 UI | **content** Content(RectTransform) |
| GameManager 프리팹 GameManager | `GameManager` 영속 싱글턴·저장·레벨 목록 | **gameMixer** GameMix.mixer **masterVolumeParameter** MasterVolume **sfxVolumeParameter** SFXVolume **musicVolumeParameter** MusicVolume **levelList** LevelList |
| GameCamera 프리팹 GameCamera | `TowerDefenseKeyboardMouseInput` 키보드·마우스 스킴 | **cameraRig** GameCamera(CameraRig) **nearZoomPanSpeedModifier** 0.5 **screenPanThreshold** 40 **mouseEdgePanSpeed** 30 **mouseRmbPanSpeed** 15 |
| GameCamera 프리팹 GameCamera | `CameraInitialState` 카메라 초기 줌·주시 | **startZoomMode** 1 **initialLookAt** None |
| GameCamera 프리팹 GameCamera | `CameraRig` 카메라 이동·줌 | **lookDampFactor** 0 **movementDampFactor** 0 **nearestZoom** 10 **furthestZoom** 20 **maxZoom** 60 **zoomLogFactor** 10 **zoomRecoverSpeed** 20 **floorY** 0 **zoomedCamAngle** ZoomAngle(Transform) **springyZoom** 0 |
| GameCamera 프리팹 GameCamera | `InputController` 터치/마우스 공통 입력 해석 | **dragThresholdTouch** 5 **dragThresholdMouse** 1 **tapTime** 0.2 **holdTime** 0.8 **mouseWheelSensitivity** 1 **trackMouseButtons** 2 **flickThreshold** 2 |
| GameCamera 프리팹 GameCamera | `TowerDefenseInputSchemeSwitcher` 입력 스킴 자동 전환 | — |
| GameCamera 프리팹 GameCamera | `TowerDefenseTouchInput` 터치 스킴 | **cameraRig** GameCamera(CameraRig) **nearZoomPanSpeedModifier** 0.5 **panSpeed** 15 **flickDecayFactor** 0.2 **panAreaScreenPercentage** 0.2 **confirmationButtons** None **invalidButtons** None |
| GameCamera 프리팹 GameCamera | `GameUI` 입력 → 배치·선택·업그레이드·판매 상태 기계 | **sphereCastRadius** 1 **radiusVisualizerController** RadiusVisualiser(RadiusVisualizerController) **towerUI** None **buildInfoUI** None |
| GameCamera/RadiusVisualiser 프리팹 GameCamera | `RadiusVisualizerController` 사거리 원 표시 | **radiusVisualizerPrefab** RadiusVisualiser **radiusVisualizerHeight** 0.02 |
| GameCamera 프리팹 GameCamera | `PostProcessorConfigurationSelector` 화질별 포스트프로세싱 | **highQualityProfile** HighQPostprocessing **lowQualityProfile** LowQPostprocessing |
| Game Over 프리팹 Game Over | `EndGameScreen` 승패 화면·별점·저장 | **victorySound** TD Victory.wav **defeatSound** TD Defeat .wav **audioSource** Game Over(AudioSource) **endGameCanvas** Game Over(Canvas) **endGameMessageText** TitleText(?) **scorePanel** Score Panel(ScorePanel) **menuSceneName** MainMenu **levelCompleteText** {0} COMPLETE! **levelFailedText** {0} FAILED! **background** Background(?) **nextLevelButton** Next Level(Canvas) |
| Game Over/LearnMore 프리팹 Game Over | `UrlOpen` 외부 링크 | — |
| Game Over/Container/Top/Score Panel 프리팹 Game Over | `ScorePanel` 별 3개 표시 | **starImages** 1(?), 2(?), 3(?) **achievedStarSprite** icon_star_small_gold.png |

그 밖의 프리팹 인스턴스: Enviro_Level_5.fbx ×1, HeadQuarters.fbx ×1, Enviro_Level_5_COL.fbx ×1

## MainMenu

타이틀 → 레벨 선택 → 옵션의 세 페이지를 TowerDefenseMainMenu가 전환한다. 레벨 선택은 GameManager에 저장된 별점으로 버튼을 채운다. GameObject 108개, 프리팹 인스턴스 3개.

### 객체 → 스크립트 → 설정값·이벤트

| 객체 (계층 경로) | 스크립트 | 인스펙터 값 · UnityEvent 연결 |
| --- | --- | --- |
| (프리팹 내부 객체) 씬 직접 배치 | `Rotator` 배경 포탑 회전 | — |
| Canvas 씬 직접 배치 | `TowerDefenseMainMenu` 메뉴 페이지 전환 | **optionsMenu** Options(OptionsMenu) **titleMenu** Main(SimpleMainMenuPage) **levelSelectMenu** LevelSelect(LevelSelectScreen) |
| Canvas 씬 직접 배치 | `SceneLoader` 씬 로드 버튼 | **sceneToLoadName** LevelSelect |
| Canvas/LevelSelect 씬 직접 배치 | `LevelSelectScreen` 레벨 선택(별·잠금) | **canvas** LevelSelect(Canvas) **selectionPrefab** LevelSelectionButton **layout** Layout(?) **rightBuffer** RightBuffer(RectTransform) **backButton** Back(?) **mouseScroll** Scroll(MouseScroll) **cameraAnimator** Main Camera(Animation) **enterCameraAnim** CameraMove0 **exitCameraAnim** CameraMove1 |
| Canvas/LevelSelect/Levels/Scroll 씬 직접 배치 | `MouseScroll` 레벨 목록 가로 스크롤 | **clampScroll** 1 **scrollXBuffer** 0.15 **scrollYBuffer** 0 |
| Canvas/Main 씬 직접 배치 | `SimpleMainMenuPage` 타이틀 페이지 | **canvas** Main(Canvas) |
| Canvas/Main/LearnMore 씬 직접 배치 | `UrlOpen` 외부 링크 | — |
| Canvas/Main/Menu Buttons/Quit 씬 직접 배치 | `ExitButton` 종료 버튼 | —  `m_OnClick` → Quit(AudioSource).**PlayOneShot** |
| Canvas/Options 씬 직접 배치 | `OptionsMenu` 볼륨 설정 | **canvas** Options(Canvas) **masterSlider** Slider(?) **sfxSlider** Slider(?) **musicSlider** Slider(?) |
| Canvas/Options/Back 씬 직접 배치 | `SceneLoader` 씬 로드 버튼 | **sceneToLoadName** MainMenu |
| Canvas/Options/Panel/Credits/Left 씬 직접 배치 | `UrlOpen` 외부 링크 | — |
| Canvas/Options/Panel/Credits/Right 씬 직접 배치 | `UrlOpen` 외부 링크 | — |
| Main Camera 씬 직접 배치 | `PostProcessorConfigurationSelector` 화질별 포스트프로세싱 | **highQualityProfile** HighQPostprocessingMenu **lowQualityProfile** LowQPostprocessingMenu |
| WorldObjects/EnergyPylon_2/EneryPylon_PFX 씬 직접 배치 | `IntervalParticleSystemPlayer` 주기적 파티클 | **particleSystemToPlay** EneryPylon_PFX(ParticleSystem) **interval** 4 |
| WorldObjects/LaserTower_2/LaserTower_TURRET_L03 씬 직접 배치 | `Rotator` 배경 포탑 회전 | — |
| WorldObjects/LaserTower_2/LaserTower_TURRET_L03/LaserFireEffect 씬 직접 배치 | `IntervalParticleSystemPlayer` 주기적 파티클 | **particleSystemToPlay** LaserFireEffect(ParticleSystem) **interval** 6 |
| WorldObjects/MachineGunTower_1/Turret_MachineGun_L02 씬 직접 배치 | `Rotator` 배경 포탑 회전 | — |
| WorldObjects/SuperTower/SuperTower_Turret 씬 직접 배치 | `Rotator` 배경 포탑 회전 | — |
| GameManager 프리팹 GameManager | `GameManager` 영속 싱글턴·저장·레벨 목록 | **gameMixer** GameMix.mixer **masterVolumeParameter** MasterVolume **sfxVolumeParameter** SFXVolume **musicVolumeParameter** MusicVolume **levelList** LevelList |

그 밖의 프리팹 인스턴스: Turret_RocketTower_L03.fbx ×1

## 타워·적 프리팹 내부 구조

씬에는 없고 배치·스폰 시 생성되는 것들. 타워 루트 프리팹이 레벨 프리팹 3개를 자식으로 바꿔 끼우는 구조, 적은 Agent + Damageable + Targetter + Affector의 조합이라는 점이 핵심이다. 기관총 타워를 예로 들었고 로켓·레이저·EMP·파일런·슈퍼타워도 같은 틀이다.

### 타워 6종 수치 (TowerLevelData + 레벨 프리팹에서 읽음)

| 타워 | 역할 | Lv1 → Lv2 → Lv3 비용 | 판매가 | 공격 (피해 × 발사율) | 특이점 |
| --- | --- | --- | --- | --- | --- |
| **Assault Cannon** (MachineGun) | 고속 저피해 히트스캔 | 4 → 8 → 14 | 2 / 6 / 13 | 0.25 × 4 / 6 / 8 = DPS 1 / 1.5 / 2 | 세 레벨 모두 Projectile_0을 쓴다 (\_1,\_2는 미사용) |
| **Rocket** | 지상 전용 범위 피해(탄도) | 12 → 24 → 32 | 6 / 18 / 34 | 3 / 3 / 5 × 0.6 / 0.7 / 0.8 | 비행 적(Hovercopter)을 맞히지 못함 |
| **Laser** | 장거리 고피해 저속 | 15 → 30 → 40 | 7 / 22 / 42 | 12 / 16 / 20 × 0.3 / 0.35 / 0.4 | Lv3 판매가가 Lv3 비용보다 높다(42 > 40) |
| **EMP** | 범위 감속(패시브) | 6 → 10 → 10 | 3 / 8 / 13 | slowFactor 0.75 / 0.65 / 0.65 | Lv2와 Lv3의 감속이 같다 — 사거리만 차이 |
| **Energy Pylon** | 에너지 생산 | 8 → 8 → 8 | 4 / 8 / 12 | +1 / 10s → 8s → 5s | Level5에서만 구매 가능 |
| **Super Tower** | 미사일 일제사격 후 자폭 | 20 (단일 레벨) | −1(판매 불가) | fireRate 20, 체력 20 | Level3·4·5에서 구매 가능 |

### 적 8종 (AgentConfiguration + 프리팹 LootDrop)

| 적 | 체력 | 속도 | 피해 | 처치 보상 | 특징 |
| --- | --- | --- | --- | --- | --- |
| Hoverbuggy / Super | 4 / 14 | 4.8 | 1 | 1 / 2 | 가볍고 빠름 — 물량 |
| Hovertank / Super | 35 / 100 | 2 | 1 | 4 / 6 | 느리고 단단함 |
| Hovercopter / Super | 16 / 32 | 3 | 1 | 3 / 4 | FlyingAgent — 타워 위로 통과, 로켓 면역 |
| Hoverboss / Super | 600 / 1400 | 1.2 | 2 | 8 / 16 | 보스. Level1은 마지막 웨이브에 1기 |

모든 적: AttackingAgent(비행 제외)는 길이 타워로 막히면(NavMesh PathPartial) 그 타워를 공격한다. 홈베이스 도착 시 HomeBaseAttacker가 2초 충전 후 피해를 주고 자신은 제거된다.

## 이 구조도가 말해 주는 것

- **웨이브는 씬 데이터다.** 5개 레벨 × 10웨이브의 스폰 목록이 전부 Wave 컴포넌트 인스펙터에 들어 있다(Level1 W8은 40기). 난이도 조정·무한 모드·레벨 에디터를 만들려면 이것을 ScriptableObject로 빼는 작업이 먼저다.
- **수입원은 처치 보상뿐.** 모든 레벨에서 상시 수입이 0이고 파일런은 Level5에서만 살 수 있다. 시작 에너지 10\~20으로 기관총(4)을 2\~3개 짓는 것이 첫 선택이 된다 — 경제 밸런스를 바꾸려면 LevelManager.currencyGainer와 LootDrop 두 곳.
- **별점 계산에 구멍이 있다.** EndGameScreen.CalculateScore는 체력 비율이 정확히 1이면 3, 0.5\~0.9면 2, 0\~0.5면 1을 주는데 **0.9 초과 1 미만은 0별**이다. 홈베이스 체력 10·적 피해 정수라 실제로는 닿지 않는 값이지만, 체력을 20으로 올리는 순간 드러나는 잠재 결함이다.
- **기관총 Projectile_1/\_2는 어디서도 쓰이지 않는다.** 세 레벨 프리팹이 모두 Projectile_0을 가리키고 피해도 0.25로 같다 — 레벨 차이는 발사율뿐. 레이저 Lv3은 판매가(42)가 비용(40)보다 높아 "짓고 팔면 이익"이 된다.
- **Level3·5만 RandomNodeSelector를 쓴다.** Level3은 노드가 하나뿐이라 사실상 고정이고, 실제 분기는 Level5의 Start(Left1/Right1)뿐이다. 가중치 분기(weight 필드는 있으나 전부 1)가 확장 지점.
- **보스 프리팹의 AttackAffector는 기본 비활성**이고 AttackingAgent가 길이 막힐 때 켠다. 즉 적은 평소 타워를 공격하지 않는다 — "타워를 노리는 적"은 이 플래그만 바꿔도 만들 수 있다.
- **HUD 프리팹 12개가 레벨 씬마다 복제**되어 있다. 한 레벨에서 UI를 고치면 다른 레벨엔 반영되지 않으므로, UI 변경은 반드시 프리팹에서 한다.