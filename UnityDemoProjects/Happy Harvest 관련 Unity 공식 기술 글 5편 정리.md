# Happy Harvest 관련 Unity 공식 기술 글 5편 정리

Nov 17, 2026 · @YongHwan Lee

Unity가 Happy Harvest 샘플과 함께 공개한 기술 글 5편의 내용 정리. 강의 1단계(분석) 사전 읽기 자료와 강사 참고용이다.

## 0. 5편 한눈에 보기

1번은 샘플 전체 소개 글(2023)이고, 2\~5번은 그 글이 가리키는 기술 해설 4편(Unity 6 기준으로 2025년 갱신)이다. 네 편 모두 "아티스트·테크니컬 아티스트용"이라 게임플레이 코드는 거의 다루지 않고, 유일한 예외가 4번 타일맵 글의 Terrain Manager 설명이다.

| # | 글 | 한 줄 요약 | 다루는 Unity 기능 | 강의 활용 |
| --- | --- | --- | --- | --- |
| 1 | [Happy Harvest demo: See the latest 2D techniques](https://unity.com/blog/games/happy-harvest-demo-latest-2d-techniques) (2023-07) | 샘플이 보여주는 것 전체 목록과 4가지 모범 원칙 | URP 2D 라이트·그림자, 보조 텍스처, 타일맵, 2D 애니메이션, VFX | 0주차 사전 읽기 — "이 샘플은 무엇을 보여주려고 만들었나"(제작 의도) |
| 2 | [How to animate 2D characters in Unity 6](https://discussions.unity.com/t/how-to-animate-2d-characters-in-unity-6/1642950) | PSD 임포트 → 본 리깅 → 스프라이트 스와프 → 애니메이터 3레이어 → 2D IK | 2D Animation, PSD Importer, Sprite Library/Resolver, Sprite Mask, 2D IK | 1단계 "분석 제외 영역"의 참고서; 애니메이터 파라미터(DirX/DirY/Speed)는 PlayerController 분석과 직결 |
| 3 | [2D light and shadow techniques with URP](https://discussions.unity.com/t/2d-light-and-shadow-techniques-with-the-universal-render-pipeline/1641848) | 라이트 4종·블렌드 스타일, 노멀/마스크 맵, 그림자 3가지 방식, 낮/밤 스크립트 | Light 2D, Shadow Caster 2D, Secondary Textures, DayCycleHandler, LightInterpolator | 씬 구조도의 조명 프리팽(DayCycleHandler, Light 2D\_Warehouse, shadow)가 왜 그렇게 생겼는지 설명 |
| 4 | [How to create art and gameplay with 2D tilemaps](https://discussions.unity.com/t/how-to-create-art-and-gameplay-with-2d-tilemaps/1643416) (2025-05) | 타일맵 레이어 8개의 역할과 Terrain Manager가 타일을 게임 상태로 쓰는 방법 | Tilemap, Rule Tile, Rule Override Tile, GameObject Brush, Chunk 모드, Tilemap API | **1단계 핵심 교재** — 기능분해표의 "담당 씬/프리팽" 열을 채울 때 그대로 쓴다 |
| 5 | [Create 2D special effects with VFX Graph and Shader Graph](https://discussions.unity.com/t/create-2d-special-effects-in-unity-with-the-vfx-graph-and-shader-graph/1646746) | 효과별로 어느 시스템(내장 파티클 / VFX Graph / Shader Graph)을 썼는지와 이유 | VFX Graph 2D 출력, Camera Sorting Layer Texture, 카메라 타일링, VFX 풀링 | 3단계 "새 작물 추가" 시범에서 수확 VFX 참조 방식(Crop 에셋 → TerrainManager 풀) 설명에 사용 |

공통 메시지: **스프라이트에 그림자·입체감을 그리지 말고 보조 텍스처(노멀 맵·마스크 맵)으로 넘겨서 런타임 라이트가 만들게 하라** — Happy Harvest의 아트 파이프라인 전체가 이 한 문장에서 나온다.

## 1. Happy Harvest demo: See the latest 2D techniques

Unity 블로그, Shanti Zachariah, 2023-07-05. 샘플 공개 글이자 나머지 4편의 목차 역할. Unity 2022 LTS + URP 2D Renderer 기준.

**네 가지 모범 원칙**: ① 그림자를 스프라이트에 굽지 말 것 ② 스프라이트는 평평하게 ③ 그림자·부피 정보는 보조 텍스처로 ④ 타일맵 고급 기능을 쓸 것.

**내용 구성**

| 주제 | 핵심 내용 |
| --- | --- |
| 배경과 과제 | 탑다운 농장 "농부의 하루". 해결한 문제: 탑다운에서 캐릭터 투영, 격치는 오브젝트 정렬, 그림자 표현 |
| 깊이감·그림자·낮/밤 | 노멀 맵·마스크 맵으로 림 라이트와 부피, 블롭 그림자와 무한 투영 그림자(Shadow Caster), 보간 스크립트가 시간 경과와 조명 변화를 구동 |
| 타일맵 | 길·연못·풀·숲 모두 타일맵. 전용 렌더러, 화면 밖 타일 비활성화, 브러시·규칙 페인팅·자동 충돌·GameObject 배치·Tilemap API. 모든 타일은 Grid 하나 아래, 오버드로를 줄이려고 타일맵 수를 최소로 |
| 캐릭터 | 얼굴 리깅(준 3D 느낌), 4방향 설계, PSD Importer, 2D Animation 패키지, Sprite Library·Sprite Swap(방향 바뀔 때 표정 교체), 2D IK |
| VFX·셰이더·포스트 | CPU 파티클: 냙잎·발자국·나방. GPU VFX Graph: 비·천둥·물트김(2D Lit 셰이더로 조명 반응). Shader Graph: 물결·불·연기·식물 흔들림. URP Volume: 틴트·블룸 |
| 배포 | 개인·상업 프로젝트에 무료, 수정 가능. Asset Store + Unity Samples(Plastic SCM) |

**강의 연결**: "이 샘플은 그래픽 데모"라는 제작 의도가 여기 적혀 있다. 1단계 현황 판정에서 "게임 목표·진행이 없는 것은 미완성이 아니라 범위 밖"이었음을 학생에게 설명하는 근거. 버전 주의: 글은 2022 LTS 기준이고 현재 패키지는 Unity 6용으로 갱신되어 있다(소프트 그림자 추가 등).

## 2. How to animate 2D characters in Unity 6

Unity Technical Content Team. 주인공 캐릭터의 리깅·스켈레탈 애니메이션·스프라이트 스와프·Sprite Mask를 작업 순서대로 설명.

**작업 흐름 (글의 순서)**

1. **캠릭터 설계** — 4방향 제한, 애니메이션 세트는 3개(옆·위·아래, 왼쪽은 옆을 반전). 나무 없는 중립 포즈로 그리고, PPU보다 약간 높은 해상도로, 방향성 없는 암비언트 오클루전만 칠하고 빛은 런타임에 맡긴다. 교체할 부위(입 등)는 PSD 레이어 그룹으로 묶는다.
2. **PSD 임포트** — PSD Importer 설정: Import Hidden Layers, Individual Sprites(Mosaic), Use as Rig(레이어 계층 그대로 프리팽 생성), Use Layer Group(스와프용 그룹 유지). 샘플 파일: PSD\_Character\_Boy, PSD\_character\_base.
3. **리깅(Skinning Editor)** — Create Bone/Edit/Split Bone, 본 이름 정리, Preview Pose. 얼굴에 face\_bone(고개 살짝 돌림)과 눈써벌·눈꺼풀 본.
4. **지오메트리·가중치** — Auto Geometry 슬라이더 0 + Weights 자동 바인딩, 정점은 최소로(본 회전마다 재계산). 관절(팔꿈치·무릎)에 신경. Bone Influence로 부위별 영향 본만 남김 — hand\_l\_bone 하나가 손 변형 여러 개를 담당.
5. **Sprite Library** — Category/Label 구조. 규칙: 모든 라이브러리가 같은 Label 집합을 가져야 교체 가능. LIBRARY\_PSD\_character\_base(더미) / \_boy / 여성 변형이 같은 스켈레톤을 공유.
6. **Sprite Resolver와 방향** — 각 애니메이션 첫 프레임에서 Resolver 값(옆 버전 스프라이트)·본 시작 위치·IK 타깃을 설정. 비주얼은 Prefab\_character\_base의 자식, 충돌·로직은 루트. ANIM\_character\_face\_left/right는 부모 X 스케일 ±1만 바꿈.
7. **Sprite Mask (Unity 6)** — 모든 2D 렌더러가 마스크가 될 수 있게 변경. 눈: 눈알에 Sprite Mask(Mask Source = Supported Renderer), 눈꺼풀·동공은 Visible Inside Mask.
8. **Animator Controller** — CONTROLLER\_PSD\_Character\_base\_prefab, 레이어 3개: Head(Additive, 눈 깜박임·시선) / Base(걷기·대기·도구 사용 서브 상태기계) / Flip(DirX로 X 스케일). 파라미터: **DirX, DirY, Speed, ToolSwing, Use**.
9. **새 클립 만들기** — Animation 창 → Create New Clip → Record, root\_bone 아래 본을 인형처럼 움직임.
10. **2D IK** — IK Manager 2D + Limb 솔버(본 2개 + 이펙터)로 팔·다리. LimbSolver2D 오브젝트는 계층 맨 아래.
11. **기타** — 돼지 프리팽은 IK 없는 단순 리그, 가로등은 회전 클립만, 물트김·배경 캐릭터는 플립북.

**소스 분석과의 연결**: PlayerController.FixedUpdate가 `DirX`/`DirY`/`Speed`를 설정하고 UseItem이 도구별 트리거(Item.PlayerAnimatorTriggerUse: ToolSwing/Watering/Planting/Picking/Eating)를 쪼는 이유가 이 글의 8번 항목이다. 도구 프리팽의 방향별 클립과 동기화된 ToolAnimationEventHandler도 여기서 나온다. **포럼 댓글**: 옆 방향은 좋으나 앞·뒤·45° 걷기는 본 회전축 때문에 일그러진다는 지적 — 학생이 새 애니메이션을 만들 때 같은 문제를 만난다.

## 3. 2D light and shadow techniques with the Universal Render Pipeline

Unity Technical Article. 2D 라이트·그림자·보조 텍스처·낮/밤 사이클을 Happy Harvest의 실제 오브젝트 이름으로 설명.

**라이트 기초**

| 항목 | 내용 |
| --- | --- |
| Light 2D 4종 | Freeform(다각형 — 큰 영역·창문 투영), Sprite(스프라이트를 빛 모양으로 — 렌즈플레어·쿠키), Spot(원/부채꼴 — 횟불·가로등), Global(모양 없음, 정렬 레이어 전체 틴트 — 블렌드 스타일·레이어당 1개) |
| Blend Style | Additive / Modulate / Subtractive(어둡게) / Custom. Light Order·Overlap Operation으로 겹침 처리, Target Sorting Layers로 영향 레이어 선택 |
| 노멀 맵 | RGB = 픽셀별 표면 방향. Spot·Point·Freeform에 작용, 라이트에서 Normal Maps 켜야 함(Fast/Accurate). 만드는 법 4가지: 기존 노멀 변형 / 생성 앱(SpriteIlluminator, Laigter, Krita 등, Unity의 Create from Grayscale) / 노멀 팔레트에서 색 샘플링 / 3방향 빛을 채널별로 그리기(위=G, 오른쪽=R) |
| 마스크 맵 | 라이트가 닿는 부위를 채널별로 제한. Texture Type은 Default로(스프라이트 아틀라스에 잘못 들어가지 않게). **림 라이트: 캐릭터는 R 채널, 소품은 G 채널** — 라이트 세트를 분리해 따로 제어 |

**Happy Harvest의 조명 구성**

- Ambient Light: Global Light, 씨 전체 틴트. 밤은 낮은 강도 + 보라 틴트.
- 태양: 2D에는 방향광이 없으므로 카메라에 붙인 큰 Spot 라이트를 스크립트로 회전. LightsRotator 아래 **DayLight / NightLight / DayLightRim / NightLightRim** 4개(Rim 2개는 마스크 채널만 닿음). 큰 라이트는 저사양 기기에서 비싸다는 경고.
- 그림자 3가지 방식: ① **네거티브 라이트**(Subtractive 블렌드) — 창고·집의 큰 그림자 ② **무한 투영 그림자** — Shadow Caster 2D, 캐릭터는 발 본(foot\_r/l\_bone)에만 캐스터(실루엣 전체는 탑다운에서 부자연) ③ **블롭 그림자** — 나무·덤불은 늘여 흐린 Sprite 라이트, ShadowInstance 스크립트가 RotationHandle을 시간에 따라 회전·신축.
- 소프트 그림자: Unity 6 갱신에서 추가(URP Asset Shadows 설정), 캐스터에서 멀어질수록 흐려짐.
- 건물 그림자: **LightInterpolator** — Light\_2D\_Warehouse에 태양 위치별(상·하·좌·우) Freeform 라이트 4개를 두고 정점을 보간. 정점 수가 같아야 하고, 각 프레임에 Normalized 가중치, Preview Time으로 에디터 미리보기.
- **DayCycleHandler**: Lights Root 회전, Night/Ambient/Rim 라이트별 색 그라디언트, Day Duration·시작 시간, Test Time 슬라이더, Shadow Angle/Length 애니메이션 커브(일몰 때 길고 정오에 짧게).

**성능 지침** (수치 없이 정성적): 최저 사양에서 테스트, 필레이트 낮게(큰 라이트 1개 > 작은 여러 개), 인접 레이어는 같은 라이트 설정으로 배칭, Render Scale 낮게, 그림자 라이트와 블렌드 스타일 수 최소화(전환 비용 큼), Max Light Render Textures / Max Shadow Render 조정.

**소스·씬 분석과의 연결**: 씬 구조도의 DayCycleHandler 프리팽(DayLight·NightLight·AmbientLight·SunRimLight·MoonRimLight 필드, ShadowAngle/Length 커브), Light 2D\_Warehouse의 LightInterpolator(프레임 4개, NormalizedTime 0.18/0.3/0.5/…), shadow 프리팽의 ShadowInstance가 정확히 이 글의 구현체다. 실내 DayCycleHandler\_House에 Rim·Ambient가 비어 있는 이유도 여기서 설명된다(태양 방향광이 실내에는 불필요). 포럼에서 "Global Light도 노멀 맵 방향을 가지면 안 되나"는 질문에 Unity 직원이 긍정적으로 답했다.

## 4. How to create art and gameplay with 2D tilemaps

Unity Technical Article, 2025-05-15. 5편 중 유일하게 게임플레이 코드(Terrain Manager)를 다룬다 — 1단계 분석의 핵심 교재.

**타일맵 도구**

- 왜 타일맵인가: 큰 그림 대신 반복 조각 → 아트 시간·메모리·CPU 절감, 전용 렌더러, 화면 밖 타일 비활성화. 모든 타일맵은 Grid 하나 아래, 타일 크기 통일.
- Tile Palette(Palette\_Tiles): 첫 줄이 자주 쓰는 Rule Tile.
- **Rule Tile**: 이웃 타일에 따라 테두리·모서리 자동 선택. Unity 6.1에 AutoTile 추가. **Rule Override Tile**: 규칙은 그대로 두고 그래픽만 바꿈 — 젖은 흙 타일이 이것.
- GameObject Brush(2D Tilemap Extras): 울타리 같은 GameObject를 격자에 배치. 유기적 배치는 타일맵 밖에.
- 정렬: 캐릭터 앞뒤로 가는 수직 소품은 별도 정렬 레이어의 GameObject로. 레이어를 겹치면 오버드로 → 깊이 평면이 정말 다를 때만 타일맵 분리.
- 보조 텍스처: 타일셋마다 같은 배치의 노멀·마스크 맵. 땅 타일 마스크는 대부분 검정(림 라이트 반사 안 함), 지붕 가장자리만 밝게.

**Happy Harvest의 타일맵 레이어** (Grid 아래)

| 타일맵 | 역할 |
| --- | --- |
| UnderwaterTiles | 물속·절벽·연못 바닥 |
| Water | 물 타일 — Shader Graph 매테리얼(Sprite-Lit)로 애니메이션 |
| GroundBase | 길·풀·진흙·돌 바닥 (Chunk 모드) |
| ObjectsInTiles | GameObject Brush로 놓은 울타리 등 |
| TilledTilemap | **게임 로직이 읽는 레이어** — 씨앗을 심을 수 있는 자리 |
| WateredTileLayer | 젖은 표시, 마르면 제거 |
| Warehouse, House | 건물 — 모양 바꾸기 쉽고 텍스처 공유 |
| Crop | 모든 작물 타일 — Terrain Manager가 직접 쓴다 |

**Terrain Manager** (Grid에 붙음): GroundData(젖음 지속 시간)·CropData(성장 속도, 미수확 생존 시간) 두 클래스. 참조: Ground/Crop/Water 타일맵 + Watered(Rule Override)·Tillable·Tilled(Rule) 타일. 함수는 Vector3Int 셀 좌표로 타일을 읽고 바꾸며 PlayerController가 도구를 통해 호출. 흐름: 도구 사용 → Terrain Manager 함수(셀 좌표) → Tillable 확인 → Tilled/Watered/Crop 타일 변경 → 시간 경과로 마름·성장·고사.

**성능**: Tilemap Renderer **Chunk 모드**로 배칭(GroundBase), 겹치는 레이어 피하기(자갈길 아래 풀 레이어를 깔지 않음 — Rendering Debugger Overdraw 모드로 확인), 텍스처 블리딩은 Sprite Atlas + 회전 금지 + 알파 확장(픽셀아트는 Point 필터).

**소스 분석과의 연결**: 이 글의 표가 Doxygen 문서 "보드는 상태 기계 / 타일맵이 곷 게임 상태" 설명의 원천이다. 씬 분석에서 확인한 실제 참조는 GroundTilemap=TilledTilemap, CropTilemap=Crop, WaterTilemap=WateredTileLayer, TilleableTile=Tiles\_Dirt1, TilledTile=RuleTile\_SoilDig, WateredTile=RuleTIle\_Soil\_Watered — 글과 일치한다. 글에 없는 것: 소스의 TerrainManager.Update가 매 프레임 모든 GroundData를 순회한다는 점, 수확 VFX 풀(작물당 4개), 저장 구조(TerrainDataSave).

## 5. Create 2D special effects in Unity with the VFX Graph and Shader Graph

Unity Technical Article. 효과마다 세 시스템(CPU 내장 파티클 / GPU VFX Graph / Shader Graph) 중 무엇을 골랐는지와 이유.

**2D에서 VFX Graph 쓰는 조건**: 2022 LTS부터 2D Renderer와 호환. Output Particle Quad 등 메시 출력에 Shader Graph를 붙이면 Lit/Unlit 대신 그 셰이더의 조명 모델을 따른다. URP 스프라이트 셰이더 3종(Lit·Unlit·Custom). 셰이더의 노출 프로퍼티를 VFX Graph에서 시간에 따라 바꿀 수 있다.

**효과별 시스템 선택**

| 효과 | 시스템 | 만드는 법 |
| --- | --- | --- |
| 비 (메인·전경) | VFX Graph + Sprite Lit Shader Graph | 땅 위·오브젝트 뒤 정렬 레이어. Lit이라 낮/밤 조명에 따라 틴트가 변함 |
| 빗방울 틀김 | VFX Graph 플립북 파티클 1개 | Trigger On Die로 낙하 파티클이 죽을 때 생성 |
| 천둥 | VFX Graph | 화면 크기 파티클 1개 + Color Over Lifetime 투명도로 번쩍임, Output Event로 오디오 재생 |
| 비 최적화 | 카메라 타일링 | Set Position(AABox) + Surface 스폰 + Tile/Warp Positions 블록을 카메라 Transform에 연결 — 화면 안에만 파티클 생성, 박스는 화면보다 약간 크게 |
| 불꽃 | Shader Graph (ShaderGraph\_Fire) | 마스크 1장으로 타원형, 스크롤하는 Voronoi + 노이즈 + 틴트, Saturate로 외곽선 |
| 불티·연기 | VFX Graph (VFX\_Fire 공유) | Velocity/Scale/Initial Position 블록 + Lit Shader Graph + 스프라이트 1장 |
| 물 타일 | Shader Graph (ShaderGraph\_Water) | Water 타일맵의 렌더러 매테리얼 교체(그래서 물 타일만 모은 타일맵). 코스틱 = 늘린 반전 Voronoi, 굴절 = **Camera Sorting Layer Texture**(Objects 레이어까지 렌더 스냅샷)를 노멀로 UV 왜곡 후 Lerp |
| 냙잎·먼지·잔물결 | VFX Graph (VFX\_Leaves, VFX\_DustParticles, VFX/Water) | 덤불 통과 시 트리거 등 작은 장식 |
| 나방, 캐릭터 먼지 퍼프 | 내장 파티클 | 가로등 주변(P\_VFX\_Moths), 캐릭터에 부착 |
| 경작 퍼프·수확 효과 | VFX Graph 프리팽 | 아래 "게임플레이 연결" |

**게임플레이에서 VFX를 부르는 방식**

- 도구 효과: 물뿌리개 프리팽은 방향별 애니메이션 클립을 가지고, 애니메이션 이벤트가 **ToolAnimationEventHandler**를 불러 해당 방향의 VFX 인스턴스를 켜고 낌.
- 타일 효과: TerrainManager가 TillingPuff 프리팽을 `Grid.GetCellCenterWorld`로 셀 중심에 옮기고 재생. 수확 효과는 **Crop ScriptableObject의 HarvestEffect** 필드에서 가져와 TerrainManager가 시작 시 풀(m\_HarvestEffectPool)을 만든다.

**성능 메모**: 대량 파티클은 GPU(VFX Graph), 카메라 타일링·Frustum Culling으로 수 제한, 물은 전용 타일맵 + 매테리얼 1개, VFX 프리팽 풀링. 수치 벤치마크는 없음. 이 글이 다루지 않는 것: 발자국, 식물 흔들림(1번 글에만 언급).

**소스 분석과의 연결**: 3단계 시범 구현 "새 작물 추가"에서 Crop 에셋의 HarvestEffect를 기존 작물 것으로 복사해도 동작하는 이유가 이 풀링 구조다. 또 WebGL 배포 시 비·천둥이 안 보이면 VFX Graph 컴퓨트 셰이더 제약이 원인이고, 이 효과들이 WeatherSystemElement로 묶여 있어 끄기 쉬운 것도 씬 분석과 맞물린다. 부록으로 Dragon Crashers의 "알파 클리핑 애니메이션"도 소개된다.

## 6. 강의에 쓰는 법

| 주차 | 읽을 글 | 어떻게 |
| --- | --- | --- |
| 0 (사전 과제) | 1번 전체 | "이 샘플이 보여주려는 것 4가지"를 한 줄씩 적어 오기 — 제작 의도 파악 연습 |
| 1 (분석) | 4번 전체, 3번의 DayCycleHandler 부분 | 기능분해표 "담당 씬/프리팽" 열을 채울 때 4번의 타일맵 표를 그대로 인용. "글에 없는데 소스에 있는 것"(Update 순회, VFX 풀, 저장)을 찾게 하는 점검 질문 |
| 2 (설계) | 5번의 "게임플레이 연결", 2번의 Animator 파라미터 | 새 도구·새 작물 기능 명세 시 "아트·클립·VFX 중 무엇을 만들어야 하나"를 정확히 적게 함 |
| 3 (구현) | 2번 9·10번 항목(새 클립, IK), 3번 성능 지침 | 애니메이션 트랙 팀원의 작업 절차서 |
| 4 (배포) | 3번·4번·5번의 성능 부분 | WebGL 최적화 체크리스트의 근거: 큰 라이트·그림자 라이트 수, Chunk 모드, 오버드로, VFX 파티클 수 |

**분석 범위 선언에 활용**: 강의에서 "셰이더·VFX·리깅은 수정하지 않는다"고 선언할 때, "대신 이 네 편을 읽으면 무엇인지는 설명할 수 있다"가 학생에게 주는 출구다. 아트·연출 전공 학생은 이 네 편으로 선택 과제(새 캐릭터 스킨, 새 날씨 VFX)를 할 수 있다.

**주의**: 네 편의 포럼 페이지는 댓글이 붙는다. 예를 들어 2번의 "4방향 리깅 왜곡" 지적과 3번의 Global Light 노멀 맵 질문은 학생이 실제로 부딪힐 한계이므로 수업에서 미리 언급할 가치가 있다.

**출처** (모두 직접 열어 확인, 2026-11-17)

- [Happy Harvest demo: See the latest 2D techniques](https://unity.com/blog/games/happy-harvest-demo-latest-2d-techniques) — Unity 블로그, 2023-07-05
- [How to animate 2D characters in Unity 6](https://discussions.unity.com/t/how-to-animate-2d-characters-in-unity-6/1642950) — Unity Discussions 기술 글
- [2D light and shadow techniques with the Universal Render Pipeline](https://discussions.unity.com/t/2d-light-and-shadow-techniques-with-the-universal-render-pipeline/1641848)
- [How to create art and gameplay with 2D tilemaps](https://discussions.unity.com/t/how-to-create-art-and-gameplay-with-2d-tilemaps/1643416) — 2025-05-15
- [Create 2D special effects in Unity with the VFX Graph and Shader Graph](https://discussions.unity.com/t/create-2d-special-effects-in-unity-with-the-vfx-graph-and-shader-graph/1646746)
- 네 편이 공통으로 가리키는 e-book: 《2D game art, animation, and lighting for artists》(Unity, 무료)
