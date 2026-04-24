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

## 게임 설계 확정사항

- **스테이지 구조:** 챕터 3개 × 웨이브 10개 = 30웨이브 / 스테이지
- **레벨:** 최대 20레벨, 스테이지 초기화 없이 누적
- **스킬:** 5종, 마스터 레벨 3, 최대 3개 보유 (교체 가능, 교체 시 레벨 초기화)
- **선택 카테고리:** A(전투강화) / B(경제) / C(특수) / D(스킬)
- **적 경험치:** 일반 8 / 빠름 6 / 탱커 15 / 부활 12 / 중간보스 50 / 최종보스 100

## 스테이지 테마

| # | 테마 | 에셋 | 바 배경 | 라인 색 | 강조 색 |
|---|------|------|---------|---------|---------|
| 1 | 🌲 숲 | Forest Low Poly Toon Battle Arena | `#232004` | `#5A8A1A` | `#8BC34A` |
| 2 | 🏜️ 사막 | Desert Low Poly Toon Battle Arena | `#341F02` | `#C8860A` | `#FFB300` |
| 3 | ❄️ 겨울 | Winter Forest Low Poly Toon Battle Arena | `#21211F` | `#4A90C8` | `#80D8FF` |
| 4 | 🏰 악마성 | Dungeon Low Poly Toon Battle Arena | `#2B0A00` | `#8B0000` | `#CC0033` |

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
├── Road      — 길 타일 (A* 통과 가능)
├── Placeable — 설치 가능 타일
├── Marker    — Placeable 위 큐브 (클릭 대상, 자동 스폰)
└── Enemy     — 적 프리팹 (TowerController OverlapSphere 감지용)
```

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

> 스크립트별 상세 설명 → [SCRIPTS.md](SCRIPTS.md)
> 개발 진행 현황 → [PROGRESS.md](PROGRESS.md)
