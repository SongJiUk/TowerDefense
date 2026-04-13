# 🏰 Tower Defense

> 웨이브를 버텨 코어를 지켜라 — 3D 로그라이크 타워디펜스

**개발 기간:** 2026.04 ~  
**개발 인원:** 1인  
**플랫폼:** PC (Unity 2022.3 LTS)  
**시점:** 2.5D (카메라 45도 고정)

---

## 🎮 게임 개요

적이 정해진 경로를 따라 코어를 향해 이동합니다.  
타워를 배치하고 업그레이드해 적을 처치하세요.  
웨이브를 클리어할 때마다 랜덤 스킬/타워를 선택하는 로그라이크 요소가 추가됩니다.

### 핵심 루프

```
웨이브 시작 → 적이 경로를 따라 코어로 이동
      ↓
타워 배치 / 업그레이드로 적 처치
      ↓
웨이브 클리어 → 랜덤 스킬 / 타워 선택 (로그라이크)
      ↓
코어 HP 0 → 런 종료
```

---

## 🗺 스테이지 테마

| 순서 | 테마 |
|------|------|
| 1 | 🌲 숲 (Forest) |
| 2 | 🏜 사막 (Desert) |
| 3 | ❄️ 겨울 (Winter) |
| 4 | 🔥 던전 (Dungeon) |

---

## 🗼 타워

추가 예정

---

## ⚙️ 핵심 시스템

- [ ] A* 경로탐색
- [ ] 웨이브 시스템 (UniTask)
- [ ] 로그라이크 스킬 선택
- [ ] 타워 배치 / 업그레이드
- [ ] UI 바인딩
- [ ] ObjectPool (적 / 투사체)
- [ ] ScriptableObject 데이터 관리

---

## 🛠 기술 스택

![Unity](https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)
![UniTask](https://img.shields.io/badge/UniTask-a78bfa?style=flat-square)
![DOTween](https://img.shields.io/badge/DOTween-ff6b6b?style=flat-square)
