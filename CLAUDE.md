# TowerDefense

송지욱(Jiuk Song)의 3D 로그라이크 타워디펜스 포트폴리오 프로젝트.

## 프로젝트 개요

- **장르:** 3D 로그라이크 타워디펜스 / **시점:** 2.5D (카메라 45도 고정)
- **플랫폼:** 모바일 (Android / iOS) — 가로 고정 (Landscape Left) / 1920x1080
- **스택:** Unity 2022.3 LTS / C# / URP / UniTask / DOTween / ObjectPool / ScriptableObject
- **GitHub:** https://github.com/SongJiUk/TowerDefense

## 카메라

- Perspective / FOV 60 / Position (0, 10, -8) / Rotation (45, 0, 0)
- 줌: 카메라 position 앞뒤 이동 / 이동: 좌우 버튼 홀드
- 스테이지 기준점: (0, 0, 0)

## 핵심 루프

```
웨이브 시작 → 적이 경로(A*)를 따라 코어로 이동
→ 타워 배치/업그레이드로 적 처치
→ 웨이브 클리어 → 랜덤 스킬/타워 선택 (로그라이크)
→ 코어 HP 0 → 런 종료
```

## 스테이지 테마

| # | 테마 | 에셋 |
|---|------|------|
| 1 | 숲 | Forest Low Poly Toon Battle Arena |
| 2 | 사막 | Desert Low Poly Toon Battle Arena |
| 3 | 겨울 | Winter Forest Low Poly Toon Battle Arena |
| 4 | 던전 | Dungeon Low Poly Toon Battle Arena |

## 프로젝트 구조

```
TowerDefense/
├── CLAUDE.md / SCRIPTS.md / README.md
└── TowerDefense/
    └── Assets/Scripts/
        ├── Core/        GridNode, GridSystem, PathFinder, Core
        ├── Controller/  EnemyController
        ├── Tower/       TowerPlacer, TowerController, ProjectileController, RangeIndicator
        ├── Wave/        WaveManager, WaveStarter, SpawnPoint
        ├── Managers/    Managers, PoolManager, ResourceManager, UIManager
        ├── UI/          UI_Base, UI_EventHandler, UI_GameScene, UI_TowerSelectPopup
        ├── Data/        TowerData, EnemyData, StageData
        ├── Utils/       Define
        └── Editor/      GameDataGenerator
```

---

## 개발 진행 현황

### 완료
- [x] GridNode, GridSystem (Raycast 기반 자동 초기화, Marker 큐브 자동 스폰)
- [x] PathFinder (A* 맨해튼 거리 4방향)
- [x] EnemyController (UniTask 경로 이동, 데미지/사망, 경로 재계산)
- [x] WaveManager (공식 기반 스케일링, 보스 웨이브, UniTask 스폰)
- [x] TowerPlacer (Marker 클릭 → 원형 팝업 → 설치)
- [x] TowerController (타겟팅, 공격 타이머, 투사체 발사, 업그레이드 스탯)
- [x] ProjectileController (타겟 추적, 데미지 후 풀 반환)
- [x] UI_TowerSelectPopup (DOTween 방사형 애니메이션, 호버 사거리 표시)
- [x] GameDataGenerator (에디터 메뉴 → 적 5종 + 스테이지 4개 자동 생성)

### 다음 작업 (우선순위 순)
- [ ] **코어 HP** — Core.cs에 HP 추가, 적 도달 시 차감, HUD 표시, 게임오버
- [ ] **골드 보상** — EnemyController.Die()의 TODO 한 줄
- [ ] **웨이브 클리어 로그라이크 UI** — 3택 1 스킬/타워 선택 팝업
- [ ] **타워 업그레이드 UI** — 배치된 타워 클릭 → 업그레이드 팝업
- [ ] 적 HP바 UI
- [ ] 게임오버 화면 / DOTween 연출
- [ ] 모바일 빌드 테스트 (Android)

---

## 씬 설정 체크리스트

```
필수 오브젝트
├── @Managers          — Managers.cs (자동 생성)
├── GridSystem         — Road/Placeable 레이어 연결
├── SpawnPoint         — SpawnPoint.cs, Road 타일 위 배치
├── Core               — Core.cs, Road 타일 위 배치 (반드시!)
├── WaveStarter        — StageData 연결
├── TowerPlacer        — Camera / MarkerLayer / Popup / TowerData[6] 연결
└── Canvas
    ├── UI_GameScene   — Text_Gold, Text_Wave, Btn_StartWave
    └── UI_TowerSelectPopup — 기본 비활성, Tower_Basic~Tower_Lighting 6개

레이어
├── Road     — 길 타일 (A* 통과 가능)
├── Placeable — 설치 가능 타일
├── Marker   — Placeable 위 큐브 (클릭 대상, 자동 스폰)
└── Enemy    — 적 프리팹 (TowerController OverlapSphere 감지용)
```

> 스크립트별 상세 설명 → [SCRIPTS.md](SCRIPTS.md)

---

## 주의사항

- Core는 반드시 Road 타일 위에 배치 (A* endNode가 null이 되면 적이 출발 안 함)
- 텍스처: Android → ETC2 / iOS → ASTC
- `GetComponent` 반드시 `Awake`에서 캐싱, Update 호출 금지
- 매 프레임 할당 (`new List<>()`) 금지
- 터치: `Input.GetMouseButtonDown(0)` 모바일 동작

---

## Claude 행동 규칙

**학습 우선:** 코드 바로 제공 X → 접근법·힌트 먼저 → 사용자 시도 → 요청 시 전체 코드
단, 반복 작업·자동화·데이터 변환은 바로 제공 가능

**Git:** 한국어 커밋 / `[feat|fix|refactor|chore|docs] 내용`
