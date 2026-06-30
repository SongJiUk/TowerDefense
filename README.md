# 🏰 3D 로그라이크 타워디펜스

> 웨이브를 버텨 코어를 지켜라 — A* 경로탐색 · 난이도 4단계 · 로그라이크 카드·스킬 시스템

**개발 기간:** 2026.04 ~ 2026.06
**개발 인원:** 1인
**플랫폼:** Android (모바일)
**엔진:** Unity 2022.3 LTS / URP
**시점:** 2.5D (카메라 45도 고정)

---

## 📸 스크린샷

| 인게임 | 카드 선택 | 보스 전투 |
|--------|-----------|-----------|
| ![gameplay](images/TowerDefense/GamePlay.PNG) | ![card](images/TowerDefense/CardSelect.PNG) | ![boss](images/TowerDefense/Boss.PNG) |

---

## 🎮 게임 개요

3D 환경에서 2.5D 시점으로 진행하는 로그라이크 타워디펜스 모바일 게임입니다.
A* 경로탐색을 직접 구현했으며, 타워 6종·적 7종·버프 시스템·시너지·카드·스킬 시스템을 설계·구현했습니다.

### 핵심 루프

```
웨이브 시작 → 적이 A* 경로로 코어 이동
      ↓
타워 배치 / 업그레이드로 적 처치
      ↓
웨이브 클리어 → 랜덤 카드 / 스킬 선택
      ↓
코어 HP 0 → 런 종료
```

---

## 🎯 난이도 시스템

Easy만 기본 해금, 클리어 시 다음 난이도 순차 해금. 해금 상태는 Firebase에 동기화됩니다.

| 난이도 | 웨이브 | 적 HP | 적 속도 | 골드 | 코어 HP | 중간보스 |
|--------|--------|-------|---------|------|---------|---------|
| 🟢 EASY | 10 | ×0.80 | ×0.90 | ×1.20 | ×1.50 | 웨이브 8~ |
| 🔵 NORMAL | 15 | ×1.00 | ×1.00 | ×1.00 | ×1.00 | 웨이브 5~ |
| 🔴 HARD | 20 | ×1.30 | ×1.15 | ×0.90 | ×0.80 | 웨이브 3~ |
| 💀 HELL | 25 | ×1.70 | ×1.30 | ×0.75 | ×0.50 | 웨이브 1~ |

---

## 🗺️ 스테이지 테마

| # | 테마 | 적 HP 배율 |
|---|------|-----------|
| 1 | 🌲 숲 (Forest) | ×1.0 |
| 2 | 🏜️ 사막 (Desert) | ×1.6 |
| 3 | ❄️ 겨울 (Winter) | ×2.5 |
| 4 | 🏰 악마성 (Dungeon) | ×3.8 |

---

## 🗼 타워 · 적

**타워 6종** — 업그레이드 3단계 (Damage / Range / Speed)

| 타워 | 특징 |
|------|------|
| Basic | 기본 단일 공격 |
| Cannon | 범위 폭발 데미지 |
| Slow | 적 이동속도 감소 |
| Sniper | 긴 사거리, 높은 단일 데미지 |
| Poison | 지속 독 데미지 |
| Lightning | 체인 번개, 다수 타겟 |

**적 7종**

| 적 | 특징 |
|----|------|
| 일반 (Basic) | 표준 |
| 빠름 (Runner) | 높은 이동속도 |
| 탱커 (Tank) | 높은 HP |
| 분열 (Split) | 사망 시 소형 적 소환 |
| 부활 (Revive) | 사망 후 재기동 |
| 중간보스 (MiddleBoss) | 강화된 능력치 |
| 최종보스 (Boss) | 보스 웨이브 등장, 잡몹 동반 |

---

## ⚙️ 핵심 시스템

### A* 경로탐색 + 동적 재탐색
- A* 알고리즘 직접 구현 (GridSystem + GridNode + PathFinder)
- 장벽(Block) 스킬로 상자 설치 시 `RecalculatePath()` 이벤트로 모든 적 즉시 재탐색
- 장벽 지속시간 < 쿨타임으로 설계 — 영구 경로 봉쇄 불가능
- 맨해튼 거리 휴리스틱, 탐색 후 G/H/Parent 초기화로 오염 방지

### 버프/디버프 시스템 (BuffHandler)
- `IBuff` 인터페이스 + `BuffHandler` 컴포넌트로 다중 스택 관리
- `AllowStack` 플래그로 중복 시 Refresh / 스택 분기
- `StatModifier` 패턴으로 최종 스탯 연산 (슬로우·독·동결·공격강화)
- `ITickable` 등록 — 별도 코루틴 없이 만료 체크

### 로그라이크 카드 · 스킬 시스템
- 웨이브 클리어마다 카드 3장 중 1장 선택
- 카드 카테고리: **A** 전투강화 / **B** 경제 / **C** 특수 / **D** 스킬
- 스킬 5종, 마스터 레벨 3, 최대 3개 보유·교체 (교체 시 레벨 초기화)

### 시너지 시스템
- 타워 조합 감지 → `OnSynergyChanged` 이벤트 발행
- 모든 타워가 구독해 스탯 자동 재계산

### Managers 싱글톤 아키텍처
- `@Managers` DontDestroyOnLoad, 15개 서브매니저 모듈화
- Pool · Resource · Effect · UI · Game · Card · Skill · Synergy · Sound · Save · Firebase · Difficulty 등
- `ITickable` / `UpdateManager`로 중앙 집중식 업데이트

### Firebase 저장 연동
- Firebase Authentication 익명 로그인 + Firestore 저장
- PlayerPrefs + Firebase 이중 저장 구조
- `OnApplicationPause`에서 백그라운드 진입 시 자동 동기화

### ObjectPool · EffectManager
- 투사체·이펙트·사운드·FloatingText 전부 ObjectPool 적용
- `EffectManager.Play(key, pos)` / `PlayLine(key, from, to)` 로 일원화
- `UniTaskVoid`로 duration 후 자동 풀 반환

---

## 🛠️ 기술 스택

![Unity](https://img.shields.io/badge/Unity_2022.3_LTS-000000?style=flat-square&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![Firebase](https://img.shields.io/badge/Firebase-FFCA28?style=flat-square&logo=firebase&logoColor=black)
![UniTask](https://img.shields.io/badge/UniTask-a78bfa?style=flat-square)
![DOTween](https://img.shields.io/badge/DOTween-ff6b6b?style=flat-square)
![Addressables](https://img.shields.io/badge/Addressables-00d4ff?style=flat-square)

---

## 📁 프로젝트 구조

```
Assets/Scripts/
├── Core/           # GridSystem, GridNode, PathFinder
├── Controller/     # EnemyController, BossEnemyController 등
├── Tower/          # TowerController, 각 타워별 Controller
├── Buff/           # IBuff, BuffHandler, Effects/
├── Managers/       # Managers, GameManager, WaveManager 등
├── Data/           # ScriptableObject 데이터 클래스
├── UI/             # UI_GameScene, UI_TitleScene 등
└── Utils/          # Define, UpdateManager, ObjectPool
```

---

## 🤖 AI 보조 개발

이 프로젝트는 **Claude Code** (AI 코딩 어시스턴트)를 활용해 개발했습니다.
아키텍처 설계 검토, 코드 리뷰, 버그 트래킹, 반복 작업 자동화에 사용했습니다.
