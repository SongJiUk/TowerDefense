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
| 1 | 🌲 숲 | Forest Low Poly Toon Battle Arena |
| 2 | 🏜 사막 | Desert Low Poly Toon Battle Arena |
| 3 | ❄️ 겨울 | Winter Forest Low Poly Toon Battle Arena |
| 4 | 🔥 던전 | Dungeon Low Poly Toon Battle Arena |

## 프로젝트 구조

```
TowerDefense/
├── CLAUDE.md / README.md
└── TowerDefense/          # Unity 프로젝트
    └── Assets/Scripts/
        ├── Core/           # Managers 싱글톤
        ├── Camera/         # CameraController
        ├── Tower/          # 배치, 업그레이드
        ├── Enemy/          # AI, 경로탐색
        ├── Wave/           # 웨이브 시스템
        ├── UI/             # UI 바인딩
        └── Data/           # ScriptableObject
```

---

## 개발 순서

### Phase 1 — 프로젝트 세팅
- [x] UniTask, DOTween 설치
- [x] 폴더 구조 생성
- [x] 에셋 임포트
- [x] CameraController (좌우 이동, 줌, 경계 제한)
- [x] 4개 스테이지 맵 제작 (숲/사막/겨울/던전) — 분기→합류 구조
- [x] 레이어 설정 (Road / Placeable / UnPlaceable)
- [x] GridNode, GridSystem 구현 (Raycast 기반 자동 초기화)
- [ ] Player Settings Landscape Left 고정
- [ ] 기본 씬 구성 (스폰, 코어 배치)
- [ ] Managers에 GridSystem 등록 (OnEnable/OnDisable)

### Phase 2 — 적 & 경로탐색
- [ ] A* PathFinder 구현
- [ ] Managers.GridSystem → PathFinder → Enemy 경로 전달 구조
- [ ] 적 이동 (A* 경로 따라 이동)
- [ ] 상자 설치 시 IsOccupied 업데이트 → 경로 재계산
- [ ] 우회로 없을 때 상자 공격 처리
- [ ] 적 ScriptableObject (HP, 속도, 보상)
- [ ] 적 ObjectPool / HP바 UI

## A* 경로 참조 구조

```
GridSystem (각 Stage 하위)
  OnEnable  → Managers.Grid = this 로 등록
  OnDisable → Managers.Grid = null 해제

A*PathFinder
  Managers.Grid 로 GridSystem 접근
  경로 계산 후 List<Vector3> 반환

Enemy (ObjectPool 재사용)
  A*PathFinder 에 경로 요청
  받은 경로만 따라 이동 (GridSystem 직접 참조 X)
```

### Phase 3 — 타워 시스템
- [ ] 타워 ScriptableObject (공격력, 사거리, 속도)
- [ ] 타워 배치 (터치)
- [ ] 타겟팅 로직
- [ ] 투사체 ObjectPool
- [ ] 타워 업그레이드

### Phase 4 — 웨이브 시스템
- [ ] WaveData ScriptableObject
- [ ] UniTask 기반 웨이브 타이밍
- [ ] 웨이브 클리어 후 로그라이크 선택 UI

### Phase 5 — 로그라이크
- [ ] 스킬/타워 랜덤 선택 UI (3택 1)
- [ ] 버프 ScriptableObject / 즉시 적용

### Phase 6 — UI & 마무리
- [ ] Enum 기반 UI 자동 바인딩
- [ ] 메인 HUD (코어 HP, 골드, 웨이브)
- [ ] 카메라 버튼 UI (◀ ▶ 줌인 줌아웃)
- [ ] 게임오버 화면 / DOTween 연출
- [ ] 나머지 스테이지 테마 적용

### Phase 7 — 포트폴리오 마무리
- [ ] 모바일 빌드 테스트 (Android)
- [ ] 플레이 영상 녹화 / 유튜브 업로드
- [ ] README / 노션 / jiuk.dev 등록

---

## 주의사항

- 텍스처: Android → ETC2 / iOS → ASTC
- `GetComponent` 반드시 `Awake`에서 캐싱, Update 호출 금지
- 매 프레임 할당 (`new List<>()`) 금지
- 터치: `Input.GetMouseButtonDown(0)` 모바일 동작, 멀티터치는 `Input.GetTouch()`
- Firebase 호출 → try-catch 필수 / API 키 하드코딩 금지

---

## Claude 행동 규칙

**학습 우선:** 코드 바로 제공 X → 접근법·힌트 먼저 → 사용자 시도 → 요청 시 전체 코드
단, 반복 작업·자동화·데이터 변환은 바로 제공 가능

**코드 리뷰:** 잘된 부분 먼저 → 문제 이유 설명 → 개선 힌트 → 직접 수정은 요청 시에만
리뷰 시 성능·모바일 메모리·배터리 영향 반드시 언급

**Git:** 한국어 커밋 / `[feat|fix|refactor|chore|docs] 내용`

**테스트:** 현 프로젝트는 손 테스트 + Crashlytics로 대체 (Managers 싱글톤 의존성 깊음)
