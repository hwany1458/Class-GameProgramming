# Happy Harvest 씬 구조도

씬 파일과 프리팹을 직접 파싱해 얻은, 씬 안의 중요 객체와 거기에 붙은 스크립트, 스크립트 사이의 연결. Unity 6000.3.10f1, Asset Store 1.6x 기준.

[Farm_Outdoor](#Farm_Outdoor)[House_Interior](#House_Interior)[읽는 법](#read)

## 읽는 법

씬에 직접 배치된 객체 프리팹 인스턴스 (×n = 개수) 노드 안 붉은 행 = 인스펙터로 연결된 UnityEvent 등록: Awake/OnEnable에서 GameManager 프로퍼티에 자신을 넣음 호출: 코드에서 다른 시스템의 메서드를 부름 참조: 인스펙터 필드로 다른 객체를 가리킴

모든 씬 객체는 **GameManager**(Resources 프리팹, 영속)에 자신을 등록하고, 서로를 직접 찾지 않는다. 그래서 구조도의 선 대부분이 GameManager 한 곳으로 모인다. 반대로 GameManager는 씬을 모르므로, 새 시스템을 추가할 때도 "GameManager에 프로퍼티 하나 + 씬 객체에서 등록"이 이 프로젝트의 관례다.

## Farm_Outdoor

실외 농장. 게임플레이 컴포넌트 17개가 씬에 직접, 나머지는 프리팹 인스턴스 안에. GameObject 167개, 프리팹 인스턴스 476개.

### 객체 → 스크립트 → 설정값·이벤트

| 객체 (계층 경로) | 스크립트 | 인스펙터 값 · UnityEvent 연결 |
| --- | --- | --- |
| Grid/House/VFX_WaterDrop 씬 직접 배치 | `WeatherSystemElement` 특정 날씨에서만 활성 | **WeatherType** 6 (Rain\|Thunder) |
| Visual Effect Rain 씬 직접 배치 | `WeatherSystemElement` 특정 날씨에서만 활성 | **WeatherType** 6 (Rain\|Thunder) |
| SceneData 씬 직접 배치 | `SceneData` 씬 저장 키 | **UniqueSceneName** outside_space |
| Grid/House/VFX_WaterDrop (1) 씬 직접 배치 | `WeatherSystemElement` 특정 날씨에서만 활성 | **WeatherType** 6 (Rain\|Thunder) |
| CM vcam1 씬 직접 배치 | `CameraSetter` 카메라 등록 | — |
| HouseEntrance 씬 직접 배치 | `SceneTransition` 씬 이동 트리거 | **TargetSceneBuildIndex** 3 **TargetSpawnIndex** 1 |
| CropInitializer 씬 직접 배치 | `CropInitializer` 시작 시 작물 88개 배치 | **InitList** \[88개\] |
| HouseSpawn 씬 직접 배치 | `SpawnPoint` 플레이어 등장 위치 | **SpawnIndex** 0 |
| WeatherSystem 씬 직접 배치 | `WeatherSystem` 날씨 요소 on/off | **StartingWeather** 1 |
| (프리팹 내부 객체) 씬 직접 배치 | `DayEventHandler` 시간대별 UnityEvent | —  연결 없음 (빈 이벤트)  연결 없음 (빈 이벤트) |
| Grid 씬 직접 배치 | `TerrainManager` 경작 상태(타일맵) 관리 | **Grid** Grid(Grid) **GroundTilemap** TilledTilemap(Tilemap) **CropTilemap** Crop(Tilemap) **WaterTilemap** WateredTileLayer(Tilemap) **WateredTile** RuleTIle_Soil_Watered **TilleableTile** Tiles_Dirt1 **TilledTile** RuleTile_SoilDig **TillingEffectPrefab** TilingPuff_prefab |
| Visual Effect Rain Foreground 씬 직접 배치 | `WeatherSystemElement` 특정 날씨에서만 활성 | **WeatherType** 2 (Rain) |
| (프리팹 내부 객체) 씬 직접 배치 | `MarketStall` 클릭 → 상점 UI | — |
| Visual Effect Rain Foreground Thunder 씬 직접 배치 | `WeatherSystemElement` 특정 날씨에서만 활성 | **WeatherType** 4 (Thunder) |
| Grid/GroundBase 씬 직접 배치 | `WalkableSurface` 발소리 지면 등록 | — |
| RainSound 씬 직접 배치 | `WeatherSystemElement` 특정 날씨에서만 활성 | **WeatherType** 6 (Rain\|Thunder) |
| Grid/House/Roof/GameObject 씬 직접 배치 | `RendererFader` 가림 시 투명 처리 | **time** 0.7 **RendererToHide** None **finalAlpha** 0 **tilemap** Roof(Tilemap) |
| Prefab_Bush 프리팹 Prefab_Bush ×78 | `StartAnimation` 애니메이션 재생 | **Animation** Prefab_Bush(Animation) |
| Prefab_Bush 프리팹 Prefab_Bush ×78 | `TriggerEvent` 진입/이탈 UnityEvent | —  `OnEnter` → LeafParticle(ParticleSystem).**Stop**  `OnEnter` → LeafParticle(ParticleSystem).**Play**  `OnEnter` → Prefab_Bush(StartAnimation).**Trigger**  `OnEnter` → Audio Source(AudioSource).**Play**  `OnEnter` → Prefab_Bush(AudioSource).**Play** |
| Prefab_Pinetree/GameObject 프리팹 Prefab_Pinetree ×41 | `RendererFader` 가림 시 투명 처리 | **time** 0.5 **RendererToHide** Prefab_Pinetree(SpriteRenderer) **finalAlpha** 0.2 **tilemap** None |
| DayCycleHandler 프리팹 DayCycleHandler | `DayCycleHandler` 낮/밤 조명 보간 | **LightsRoot** None **DayLight** None **NightLight** None **AmbientLight** None **SunRimLight** None **MoonRimLight** None **DayDurationInSeconds** 120 **StartingTime** 30 **SunHeight** 0.8 |
| Prefab_piggy 프리팹 Prefab_piggy ×2 | `BasicAnimalMovement` 동물 배회 | **Area** None **MinIdleTime** 3 **MaxIdleTime** 5 **Speed** 3 **AnimalSound** Pig-001.wav, Pig-002.wav **MinRandomSoundTime** 4 **MaxRandomSoundTime** 8 |
| VFX_WaterDropRight 프리팹 Prefab_Streetlamp ×9 | `WeatherSystemElement` 특정 날씨에서만 활성 | **WeatherType** 1 (Sun) |
| VFX_WaterDropLeft 프리팹 Prefab_Streetlamp ×9 | `WeatherSystemElement` 특정 날씨에서만 활성 | **WeatherType** 1 (Sun) |
| Light_Point (1) 프리팹 Prefab_Streetlamp ×9 | `DayEventHandler` 시간대별 UnityEvent | —  `Events[0].OnEvents` → Light_Point (1)(Behaviour).**set_enabled**  `Events[0].OffEvent` → Light_Point (1)(Behaviour).**set_enabled** |
| (프리팹 내부 객체) 프리팹 Prefab_Streetlamp ×9 | `DayEventHandler` 시간대별 UnityEvent | —  `Events[0].OnEvents` → ?(ParticleSystemRenderer).**set_enabled**  `Events[0].OffEvent` → ?(ParticleSystemRenderer).**set_enabled** |
| (프리팹 내부 객체) 프리팹 Prefab_Streetlamp ×9 | `DayEventHandler` 시간대별 UnityEvent | —  `Events[0].OnEvents` → ?(Behaviour).**set_enabled**  `Events[0].OffEvent` → ?(Behaviour).**set_enabled** |
| (프리팹 내부 객체) 프리팹 Prefab_Streetlamp ×9 | `DayEventHandler` 시간대별 UnityEvent | —  `Events[0].OnEvents` → ?(Behaviour).**set_enabled**  `Events[0].OffEvent` → ?(Behaviour).**set_enabled** |
| Light 2D_Warehouse 프리팹 Light 2D_Warehouse ×2 | `LightInterpolator` 하루 중 조명 보간 | **TargetLight** Light 2D_Warehouse(?) **LightFrames** {ReferenceLight=Frame1(?), NormalizedTime=0.18}, {ReferenceLight=Frame2(?), NormalizedTime=0.3}, {ReferenceLight=Frame3(?), NormalizedTime=0.5}, {ReferenceLight=Frame4(?), NormalizedTime=0.76}, {ReferenceLight=Frame1(?), NormalizedTime=0.86} |
| Prefab_Sprite_HouseLamp 프리팹 Prefab_Sprite_HouseLamp ×2 | `DayEventHandler` 시간대별 UnityEvent | —  `Events[0].OnEvents` → Prefab_Sprite_HouseLamp(Behaviour).**set_enabled**  `Events[0].OffEvent` → Prefab_Sprite_HouseLamp(Behaviour).**set_enabled** |
| VFX_Fire/Light 2D 프리팹 VFX_Fire | `LightFlicker` 불 깜빡임 | — |
| VFX_Fire/Light 2D_Shadow 프리팹 VFX_Fire | `LightFlicker` 불 깜빡임 | — |
| VFX_Fire/Light 2D 프리팹 VFX_Fire | `LightFlicker` 불 깜빡임 | — |
| (프리팹 내부 객체) 프리팹 Prefab_Chicken ×3 | `BasicAnimalMovement` 동물 배회 | **Area** None **MinIdleTime** 3 **MaxIdleTime** 5 **Speed** 3 **AnimalSound** Chicken-001.wav, Chicken-002.wav **MinRandomSoundTime** 4 **MaxRandomSoundTime** 8 |
| MarketCollider 프리팹 Prefab_Market | `MarketStall` 클릭 → 상점 UI | — |
| Character/UI Target 프리팹 Character | `TargetMarker` 타깃 셀 표시 | — |
| Character 프리팹 Character | `PlayerController` 이동·도구 사용·코인·인벤토리 | **InputAction** Player.inputactions **Speed** 6 **Target** UI Target(SpriteRenderer) **ItemAttachBone** ?(Transform) **m_Coins** 10 |
| (프리팹 내부 객체) 프리팹 Character | `CharacterAnimationEventHandler` 애니메이션 이벤트 | — |
| (프리팹 내부 객체) 프리팹 Character | `StepSoundHandler` 타일별 발소리 | **DefaultStepSounds** Stepping on grass-001.wav, Stepping on grass-002.wav, Stepping on grass-003.wav, Stepping on grass-004.wav **SoundMappings** {Tiles=\[{'fileID': 11400000, 'guid': 'f4def2e4b1b365a419aa7b04bf1268b6', 'type': 2}, {'fileID': 11400000, 'guid': '847b2bcd75c2cda468a81f3a03b8f477', 'type': 2}\], StepSounds=\[{'fileID': 8300000, 'guid': '5124d120b8a1ce14187fc8ff1fc92478', 'type': 3}, {'fileID': 8300000, 'guid': '0b9335f5061f6bf4c96cdd2e81b13786', 'type': 3}, {'fileID': 8300000, 'guid': '5206a83338e09524a833deb509e3aa0e', 'type': 3}, {'fileID': 8300000, 'guid': 'ac1509efc37253f40aee0ce095d80d52', 'type': 3}\]}, {Tiles=\[{'fileID': 11400000, 'guid': 'c57890ef93fcac5418c851f4ff8b5dc5', 'type': 2}\], StepSounds=\[{'fileID': 8300000, 'guid': '267338feb0193f84193397f752406c44', 'type': 3}, {'fileID': 8300000, 'guid': '85d497df13819bb438f3b759a61b601f', 'type': 3}, {'fileID': 8300000, 'guid': '0b5867ffd4e70c844a7e834b37a444ff', 'type': 3}\]}, {Tiles=\[{'fileID': 11400000, 'guid': '9863fd0ec3a12e6438421dcb10a92e73', 'type': 2}, {'fileID': 11400000, 'guid': 'c8f392d7e42146b4fae4123647bdeefa', 'type': 2}, {'fileID': 11400000, 'guid': 'cc0de635fbcac0149a2019e544d367a3', 'type': 2}\], StepSounds=\[{'fileID': 8300000, 'guid': 'baeba4e984e572847b6fef28211111dd', 'type': 3}, {'fileID': 8300000, 'guid': '2e8cdd9e70213544c88a52fbf94822cd', 'type': 3}, {'fileID': 8300000, 'guid': '47835b8c2e1768841924ebea5e4efa12', 'type': 3}\]}, {Tiles=\[{'fileID': 11400000, 'guid': '3156adfc4a0046f4cb0dd932bcb3622f', 'type': 2}, {'fileID': 11400000, 'guid': 'e6b7499f946f1d94ab4db4e7c5a9e413', 'type': 2}, {'fileID': 11400000, 'guid': '0ed636eddcbb1b844a10d534e7d50331', 'type': 2}, {'fileID': 11400000, 'guid': '7fda88231bf9d4b429c76da7bf629221', 'type': 2}, {'fileID': 11400000, 'guid': '7a1bbcc15931f704cbad85f2a03e53aa', 'type': 2}, {'fileID': 11400000, 'guid': '7a2e6efbc4854aa4b8dc0371b2e11718', 'type': 2}, {'fileID': 11400000, 'guid': 'a16c2827cda1f654eae59e8bdc02a786', 'type': 2}, {'fileID': 11400000, 'guid': 'd979b948fbcbd0044b8381c07c576710', 'type': 2}, {'fileID': 11400000, 'guid': '4429021edc71f3849827a0a12e2d5ce6', 'type': 2}, {'fileID': 11400000, 'guid': 'e62b05b5e76e58c44b836f6479d5b0c7', 'type': 2}, {'fileID': 11400000, 'guid': 'fe5be83a6affac240b7bf2b06e6ca0d5', 'type': 2}, {'fileID': 11400000, 'guid': 'f15a61d5403b5c340abde89e31eef285', 'type': 2}, {'fileID': 11400000, 'guid': 'bec709dda9e66d04f9f845f96934ae7a', 'type': 2}, {'fileID': 11400000, 'guid': '75ced6d5050db374e83916496bcfb5b0', 'type': 2}\], StepSounds=\[{'fileID': 8300000, 'guid': '2e92c599efe20584e8609035dc64319d', 'type': 3}, {'fileID': 8300000, 'guid': '02d1f005accd1bb409b875bae594d7de', 'type': 3}, {'fileID': 8300000, 'guid': '5cf97734372b7bb4b9c6f4eea5a978e9', 'type': 3}, {'fileID': 8300000, 'guid': '7bbb4509e1fcb6a4d9115077af0f0c56', 'type': 3}\]} |
| (프리팹 내부 객체) 프리팹 Character | `EmitParticles` 발 먼지 파티클 | **particles** ?(ParticleSystem) **particleCount** 1 |
| SoundManager 프리팹 SoundManager | `SoundManager` 사운드 싱글턴 | **Mixer** MainMixer.mixer **UISource** UISource(AudioSource) **SFXReferenceSource** SFXReference(AudioSource) |
| SoundManager 프리팹 SoundManager | `AmbienceBlender` 낮/밤 환경음 | **DayAmbienceSource** DayAmbienceSource(AudioSource) **NightAmbienceSource** NightAmbienceSource(AudioSource) |
| SoundManager 프리팹 SoundManager | `DayEventHandler` 시간대별 UnityEvent | —  `Events[0].OnEvents` → SoundManager(AmbienceBlender).**BlendToDay**  `Events[0].OffEvent` → SoundManager(AmbienceBlender).**BlendToNight** |

장식 프리팹(게임플레이 스크립트 없음): Prefab_Flowers_white ×62, Prefab_Flowers_yellow ×59, Prefab_RocksSmall ×48, Prefab_Flowers_red ×39, Prefab_grass_clump_01 ×38, Prefab_Grass_clump02 ×22, Prefab_RockMedium ×20, Prefab_Barrel ×14, Prefab_RocksBig ×12, Prefab_WaterLilly ×8, Prefab_Sprite_Sign_right ×3, log_horizontal ×2, Prefab_Scarecrow ×2, log_vertical ×2, VFX_Cliffs ×1, Smoke ×1

## House_Interior

집 안. 게임이 시작되는 씬(Loader → 빌드 인덱스 3). TerrainManager가 없어 도구를 쓸 수 없다. GameObject 31개, 프리팹 인스턴스 36개.

### 객체 → 스크립트 → 설정값·이벤트

| 객체 (계층 경로) | 스크립트 | 인스펙터 값 · UnityEvent 연결 |
| --- | --- | --- |
| Outside 비활성 씬 직접 배치 | `DayEventHandler` 시간대별 UnityEvent | —  `Events[0].OnEvents` → Outside(SpriteRenderer).**set_enabled** *(에디터 전용)*  `Events[0].OffEvent` → Outside(SpriteRenderer).**set_enabled** |
| SceneData 씬 직접 배치 | `SceneData` 씬 저장 키 | **UniqueSceneName** house_interior |
| Exit_Trigger 씬 직접 배치 | `SceneTransition` 씬 이동 트리거 | **TargetSceneBuildIndex** 2 **TargetSpawnIndex** 0 |
| Spawn_Point_Entrance 씬 직접 배치 | `SpawnPoint` 플레이어 등장 위치 | **SpawnIndex** 1 |
| Spawn_Point_Default 씬 직접 배치 | `SpawnPoint` 플레이어 등장 위치 | **SpawnIndex** 0 |
| Grid/Tilemap 씬 직접 배치 | `WalkableSurface` 발소리 지면 등록 | — |
| DayCycleHandler_House 프리팹 DayCycleHandler_House | `DayCycleHandler` 낮/밤 조명 보간 | **LightsRoot** DayCycleHandler_House(Transform) **DayLight** DayLight(?) **NightLight** NightLight(?) **DayDurationInSeconds** 120 **StartingTime** 30 **SunHeight** 0.8 |
| Character/UI Target 프리팹 Character ×2 | `TargetMarker` 타깃 셀 표시 | — |
| Character 프리팹 Character ×2 | `PlayerController` 이동·도구 사용·코인·인벤토리 | **InputAction** Player.inputactions **Speed** 6 **Target** UI Target(SpriteRenderer) **ItemAttachBone** ?(Transform) **m_Coins** 10 |
| (프리팹 내부 객체) 프리팹 Character ×2 | `CharacterAnimationEventHandler` 애니메이션 이벤트 | — |
| (프리팹 내부 객체) 프리팹 Character ×2 | `StepSoundHandler` 타일별 발소리 | **DefaultStepSounds** Stepping on grass-001.wav, Stepping on grass-002.wav, Stepping on grass-003.wav, Stepping on grass-004.wav **SoundMappings** {Tiles=\[{'fileID': 11400000, 'guid': 'f4def2e4b1b365a419aa7b04bf1268b6', 'type': 2}, {'fileID': 11400000, 'guid': '847b2bcd75c2cda468a81f3a03b8f477', 'type': 2}\], StepSounds=\[{'fileID': 8300000, 'guid': '5124d120b8a1ce14187fc8ff1fc92478', 'type': 3}, {'fileID': 8300000, 'guid': '0b9335f5061f6bf4c96cdd2e81b13786', 'type': 3}, {'fileID': 8300000, 'guid': '5206a83338e09524a833deb509e3aa0e', 'type': 3}, {'fileID': 8300000, 'guid': 'ac1509efc37253f40aee0ce095d80d52', 'type': 3}\]}, {Tiles=\[{'fileID': 11400000, 'guid': 'c57890ef93fcac5418c851f4ff8b5dc5', 'type': 2}\], StepSounds=\[{'fileID': 8300000, 'guid': '267338feb0193f84193397f752406c44', 'type': 3}, {'fileID': 8300000, 'guid': '85d497df13819bb438f3b759a61b601f', 'type': 3}, {'fileID': 8300000, 'guid': '0b5867ffd4e70c844a7e834b37a444ff', 'type': 3}\]}, {Tiles=\[{'fileID': 11400000, 'guid': '9863fd0ec3a12e6438421dcb10a92e73', 'type': 2}, {'fileID': 11400000, 'guid': 'c8f392d7e42146b4fae4123647bdeefa', 'type': 2}, {'fileID': 11400000, 'guid': 'cc0de635fbcac0149a2019e544d367a3', 'type': 2}\], StepSounds=\[{'fileID': 8300000, 'guid': 'baeba4e984e572847b6fef28211111dd', 'type': 3}, {'fileID': 8300000, 'guid': '2e8cdd9e70213544c88a52fbf94822cd', 'type': 3}, {'fileID': 8300000, 'guid': '47835b8c2e1768841924ebea5e4efa12', 'type': 3}\]}, {Tiles=\[{'fileID': 11400000, 'guid': '3156adfc4a0046f4cb0dd932bcb3622f', 'type': 2}, {'fileID': 11400000, 'guid': 'e6b7499f946f1d94ab4db4e7c5a9e413', 'type': 2}, {'fileID': 11400000, 'guid': '0ed636eddcbb1b844a10d534e7d50331', 'type': 2}, {'fileID': 11400000, 'guid': '7fda88231bf9d4b429c76da7bf629221', 'type': 2}, {'fileID': 11400000, 'guid': '7a1bbcc15931f704cbad85f2a03e53aa', 'type': 2}, {'fileID': 11400000, 'guid': '7a2e6efbc4854aa4b8dc0371b2e11718', 'type': 2}, {'fileID': 11400000, 'guid': 'a16c2827cda1f654eae59e8bdc02a786', 'type': 2}, {'fileID': 11400000, 'guid': 'd979b948fbcbd0044b8381c07c576710', 'type': 2}, {'fileID': 11400000, 'guid': '4429021edc71f3849827a0a12e2d5ce6', 'type': 2}, {'fileID': 11400000, 'guid': 'e62b05b5e76e58c44b836f6479d5b0c7', 'type': 2}, {'fileID': 11400000, 'guid': 'fe5be83a6affac240b7bf2b06e6ca0d5', 'type': 2}, {'fileID': 11400000, 'guid': 'f15a61d5403b5c340abde89e31eef285', 'type': 2}, {'fileID': 11400000, 'guid': 'bec709dda9e66d04f9f845f96934ae7a', 'type': 2}, {'fileID': 11400000, 'guid': '75ced6d5050db374e83916496bcfb5b0', 'type': 2}\], StepSounds=\[{'fileID': 8300000, 'guid': '2e92c599efe20584e8609035dc64319d', 'type': 3}, {'fileID': 8300000, 'guid': '02d1f005accd1bb409b875bae594d7de', 'type': 3}, {'fileID': 8300000, 'guid': '5cf97734372b7bb4b9c6f4eea5a978e9', 'type': 3}, {'fileID': 8300000, 'guid': '7bbb4509e1fcb6a4d9115077af0f0c56', 'type': 3}\]} |
| (프리팹 내부 객체) 프리팹 Character ×2 | `EmitParticles` 발 먼지 파티클 | **particles** ?(ParticleSystem) **particleCount** 1 |
| SoundManager 프리팹 SoundManager | `SoundManager` 사운드 싱글턴 | **Mixer** MainMixer.mixer **UISource** UISource(AudioSource) **SFXReferenceSource** SFXReference(AudioSource) |
| SoundManager 프리팹 SoundManager | `AmbienceBlender` 낮/밤 환경음 | **DayAmbienceSource** DayAmbienceSource(AudioSource) **NightAmbienceSource** NightAmbienceSource(AudioSource) |
| SoundManager 프리팹 SoundManager | `DayEventHandler` 시간대별 UnityEvent | —  `Events[0].OnEvents` → SoundManager(AmbienceBlender).**BlendToDay**  `Events[0].OffEvent` → SoundManager(AmbienceBlender).**BlendToNight** |
| VFX_Fire/Light 2D 프리팹 VFX_Fire | `LightFlicker` 불 깜빡임 | — |
| VFX_Fire/Light 2D_Shadow 프리팹 VFX_Fire | `LightFlicker` 불 깜빡임 | — |
| VFX_Fire/Light 2D 프리팹 VFX_Fire | `LightFlicker` 불 깜빡임 | — |

장식 프리팹(게임플레이 스크립트 없음): Prefab_Barrel ×6, Prefab_Curtain ×4, Prefab_Plant ×3, Prefab_Chair ×2, Prefab_Sofa ×2, rug_02 ×2, Prefab_pillowA ×1, Prefab_Painting_1 ×1, Prefab_Fireplace ×1, Prefab_Painting_4 ×1, Prefab_Painting_2 ×1, Prefab_Painting_3 ×1, Prefab_Bed ×1

## 이 구조도가 말해 주는 것

- **Warehouse(창고)** 스크립트는 어느 씬·프리팹에도 붙어 있지 않다. 코드는 있지만 플레이어가 닿을 수 없는 기능이다.
- 낮/밤 시간대 이벤트는 가로등 9개·집 램프 2개·환경음에만 연결되어 있고, 게임 규칙(상점·작물·동물)에는 연결된 것이 없다.
- 실외 씬의 이름 없는 DayEventHandler는 빈 이벤트(연결 없음)이고, 실내 씬의 `Outside` 창밖 이벤트는 오브젝트가 꺼져 있다 — 작업 흔적.
- 실내 씬에 Character 프리팹이 2개 놓여 있다. PlayerController.Awake가 둘째를 파괴하므로 실제로는 하나만 산다.
- 하루 길이 120초가 GameManager와 DayCycleHandler 프리팹에 따로 적혀 있다. 시간 틱은 GameManager만 하므로 GameManager 값이 실제 값이다.