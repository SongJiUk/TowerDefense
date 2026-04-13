# TowerDefense

송지욱(Jiuk Song)의 3D 로그라이크 타워디펜스 포트폴리오 프로젝트.

## 프로젝트 개요

- **장르:** 3D 로그라이크 타워디펜스
- **시점:** 2.5D (카메라 45도 고정)
- **플랫폼:** PC (Unity 2022.3 LTS)
- **GitHub:** https://github.com/SongJiUk/TowerDefense

## 프로젝트 구조

```
TowerDefense/          # Git 루트
├── CLAUDE.md
├── README.md
└── TowerDefense/      # Unity 프로젝트
    ├── Assets/
    │   └── Scripts/
    │       ├── Core/        # GameManager, Managers 싱글톤
    │       ├── Tower/       # 타워 배치, 업그레이드
    │       ├── Enemy/       # 적 AI, 경로탐색
    │       ├── Wave/        # 웨이브 시스템
    │       ├── UI/          # UI 바인딩
    │       └── Data/        # ScriptableObject 데이터
    ├── ProjectSettings/
    └── Packages/
```

## 게임 핵심 루프

```
웨이브 시작 → 적이 경로(A*)를 따라 코어로 이동
      ↓
타워 배치 / 업그레이드로 적 처치
      ↓
웨이브 클리어 → 랜덤 스킬 / 타워 선택 (로그라이크)
      ↓
코어 HP 0 → 런 종료
```

## 스테이지 테마

| 순서 | 테마 | 에셋 |
|------|------|------|
| 1 | 🌲 숲 (Forest) | Forest Low Poly Toon Battle Arena |
| 2 | 🏜 사막 (Desert) | Desert Low Poly Toon Battle Arena |
| 3 | ❄️ 겨울 (Winter) | Winter Forest Low Poly Toon Battle Arena |
| 4 | 🔥 던전 (Dungeon) | Dungeon Low Poly Toon Battle Arena |

## 구현할 핵심 시스템

- [ ] A* 경로탐색
- [ ] 웨이브 시스템 (UniTask 기반)
- [ ] 로그라이크 스킬/타워 선택
- [ ] 타워 배치 / 업그레이드
- [ ] UI 바인딩 (Enum 기반 자동 바인딩)
- [ ] ObjectPool (적 / 투사체)
- [ ] ScriptableObject 데이터 관리 (타워, 적, 웨이브)

## 타워 종류

추가 예정

---

## 개발 순서

### Phase 1 — 프로젝트 세팅 (1~2일)
- [ ] Unity 프로젝트 생성 (URP)
- [ ] UniTask, DOTween 패키지 설치
- [ ] 폴더 구조 생성 (Core / Tower / Enemy / Wave / UI / Data)
- [ ] 에셋 임포트 (숲 테마부터)
- [ ] 2.5D 카메라 세팅 (45도 고정)
- [ ] 기본 씬 구성 (맵, 경로, 코어 배치)

### Phase 2 — 적 & 경로탐색 (3~5일)
- [ ] 경로(Waypoint) 시스템 구현
- [ ] A* 또는 Waypoint 기반 적 이동
- [ ] 적 ScriptableObject 데이터 설계 (HP, 속도, 보상)
- [ ] 적 ObjectPool 구현
- [ ] 적 HP바 UI

### Phase 3 — 타워 시스템 (5~7일)
- [ ] 타워 ScriptableObject 데이터 설계 (공격력, 사거리, 속도)
- [ ] 타워 배치 시스템 (클릭으로 설치)
- [ ] 타겟팅 로직 (가장 앞, 가장 강한 적 등)
- [ ] 투사체 ObjectPool 구현
- [ ] 타워 업그레이드 시스템

### Phase 4 — 웨이브 시스템 (3~4일)
- [ ] WaveData ScriptableObject (웨이브별 적 구성)
- [ ] UniTask 기반 웨이브 타이밍 관리
- [ ] 웨이브 시작/종료 이벤트
- [ ] 웨이브 클리어 후 로그라이크 선택 UI

### Phase 5 — 로그라이크 (3~4일)
- [ ] 스킬/타워 랜덤 선택 UI (3개 중 1개)
- [ ] 버프 ScriptableObject 설계
- [ ] 선택 결과 즉시 적용

### Phase 6 — UI & 마무리 (3~4일)
- [ ] Enum 기반 UI 자동 바인딩
- [ ] 메인 HUD (코어 HP, 골드, 웨이브)
- [ ] 게임 오버 / 런 종료 화면
- [ ] DOTween 연출 추가
- [ ] 나머지 스테이지 테마 적용 (사막 → 겨울 → 던전)

### Phase 7 — 포트폴리오 마무리
- [ ] 스크린샷 촬영
- [ ] 플레이 영상 녹화 및 유튜브 업로드
- [ ] README 업데이트
- [ ] 노션 페이지 작성
- [ ] jiuk.dev 포트폴리오 등록

## 기술 스택

- Unity 2022.3 LTS / C# / URP
- UniTask (비동기 웨이브 타이밍)
- DOTween (연출)
- ObjectPool (적/투사체 재사용)
- ScriptableObject (데이터 분리)

## 개발 시 주의사항

- Unity 프로젝트는 `TowerDefense/TowerDefense/` 하위에 있음
- 씬 파일: `Assets/Scenes/`
- 스크립트: `Assets/Scripts/`
