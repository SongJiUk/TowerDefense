# 개발 진행 현황

> 마지막 업데이트: 2026-04-22

---

## ✅ 완료

### 기반 시스템
- [x] GridNode, GridSystem (Raycast 기반 자동 초기화, Marker 큐브 자동 스폰)
- [x] PathFinder (A* 맨해튼 거리 4방향)
- [x] ResourceManager Addressable 로딩 구조
- [x] LoadingScene (LoadGroupAsync "Preload" → StartButton 활성화 → TitleScene)
- [x] LevelData ScriptableObject (20레벨, 경험치 테이블)
- [x] GameDataGenerator (에디터 메뉴 → 적 5종 + 스테이지 4개 자동 생성)

### 적/웨이브
- [x] EnemyController (UniTask 경로 이동, 데미지/사망, 경로 재계산, 풀 이중반환 버그 수정)
- [x] WaveManager (공식 기반 스케일링, 보스 웨이브, UniTask 스폰, 웨이브 클리어 골드 보상)
- [x] BuffSystem (IBuff, BuffEffect, BuffHandler, StatModifier, SlowEffect, PoisonEffect)

### 타워
- [x] TowerPlacer (Marker 클릭 → 원형 팝업 → 설치, buildCostMultiplier 연결)
- [x] TowerController (타겟팅, 공격 타이머, 투사체 발사, 업그레이드 스탯, 포신 회전, 치명타 시스템)
- [x] ProjectileController (타겟 추적, 데미지 후 풀 반환)
- [x] RangeIndicator (LineRenderer + Cylinder, 지형 따라가는 외곽선)
- [x] SlowTowerController, PoisonTowerController, CannonTowerController, LightningTowerController
- [x] 타워 프리팹 6종 제작 + UI Image 등록

### UI
- [x] UI_TowerSelectPopup (DOTween 방사형 애니메이션, 호버 사거리 표시)
- [x] 카메라 핀치 줌 / 드래그 패닝 (모바일 대응)
- [x] 모바일 터치 입력 대응 (IsPointerOverGameObject fingerId, GetMouseButtonUp)

### 카드 시스템
- [x] CardData / CardManager (11종, A/B/C/D 카테고리, 가중치 랜덤, 스택 제한)
- [x] UI_LevelUpPopup / UI_CardItem (레벨업 시 카드 3장 선택)
- [x] 글로벌 멀티플라이어 연결
  - EnemyController.Init(): HP × `nextWaveEnemyHpMultiplier`
  - EnemyController.Die(): 골드 × `killRewardMultiplier`
  - TowerPlacer: 건설비 × `buildCostMultiplier`
  - WaveManager: 클리어 골드 (웨이브번호×10×`waveBonusMultiplier`) 지급 후 1f 리셋
  - TowerController.Fire(): `criticalChanceBonus` 확률로 데미지 2배

### 스킬 시스템
- [x] SkillData 5종 (Block, ArrowRain, Freeze, LightningStorm, PoisonMist)
- [x] SkillManager (슬롯 3개, 쿨다운 UniTask, OnSlotChanged/OnCooldownChanged 이벤트)
- [x] UI_SkillSlot (스킬 획득 시 HUD에 동적 생성, 쿨다운 fillAmount)
- [x] UI_SkillSelectPopup (미보유 스킬 중 랜덤 3종 선택, Fisher-Yates 셔플)
- [x] UI_SkillItem (아이콘 GetAtlas, 이름, 쿨타임 표시)
- [x] CardManager.SkillSelect → UIManager.ShowPopup() 연결

---

## 🔴 즉시 할 것

- [x] **스킬 실제 로직 구현** — `SkillManager.ExecuteSkill()` 5종 효과
  - `Block` — 임시 장애물 생성 (경로 재계산 트리거)
  - `ArrowRain` — 범위 내 모든 적에게 대미지
  - `Freeze` — 범위 내 적에게 SlowEffect 적용
  - `LightningStorm` — 범위 내 **모든 적**에게 번개 (연쇄 제거)
  - `PoisonMist` — 지속 도트존 생성 (PoisonMistZone 유니태스크 루프)
- [x] **타워 업그레이드 UI** — 클릭 시 공격력/사거리/속도 3열 독립 업그레이드 팝업
  - TowerController: DamageLevel/RangeLevel/SpeedLevel 독립 관리
  - TowerController: TryUpgrade/GetSellPrice/Sell 구현
  - UI_TowerUpgradePopup: Binding 패턴 (Txts/Btns/GO enum + 2D 인덱스 테이블)
  - PoisonTowerController: 독 미적용 적 우선 타겟 (HasEffect<PoisonEffect> 활용)
- [x] **TowerData ScriptableObject 자동 생성** — TowerDataGenerator (에디터 메뉴)
- [x] **Addressables PrevLoad 전환** — TowerPlacer/TowerController Inspector 의존 제거

⚠️ **Unity Editor 작업 필요**
- UI_TowerUpgradePopup 프리팹 제작 (오브젝트 이름: Card_D0~S2, Done_D0~S2, Btn_D0~S2, Txt_*_Name/Desc/Cost)
- TowerDefense → 타워 데이터 자동 생성 실행 후 addressableKey/projectilePrefabKey/iconKey 연결
- Addressables에 UI_TowerSelectPopup + UI_TowerUpgradePopup + TowerData 6종 → PrevLoad 레이블 추가

## 🟡 콘텐츠 추가

- [ ] **Enemy 추가** — 빠름/탱커/부활/분열/중간보스/최종보스 (총 7종) EnemyData 생성
- [ ] **적 HP바 UI** — EnemyController에 월드스페이스 HP바 연결

## 🟢 마무리

- [ ] **게임오버 화면** — 코어 HP 0 → DOTween 연출 → 결과창
- [ ] **타이틀 씬** — 시작 버튼, 세팅
- [ ] **모바일 빌드 테스트** (Android APK)
