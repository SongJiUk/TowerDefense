# 개발 진행 현황

> 마지막 업데이트: 2026-04-29

---

## ✅ 완료

### 기반 시스템
- [x] GridSystem (Raycast 기반 자동 초기화, Marker 큐브 자동 스폰, ReadyTask 대기)
- [x] PathFinder (A* 맨해튼 거리 4방향)
- [x] ResourceManager Addressable 로딩 구조
- [x] LoadingScene (LoadGroupAsync "PrevLoad" → StartButton 활성화 → GameScene)
- [x] LevelData ScriptableObject (20레벨, 경험치 테이블)
- [x] GameDataGenerator (에디터 메뉴 → 적 5종 + 스테이지 4개 자동 생성)
- [x] URP 전환 (SetPass 833 → 30, SRP Batcher)

### 테스트 시스템
- [x] GameSceneBootstrap (테스트 모드 직접 진입 시 비동기 초기화, ReadyTask 신호)
- [x] EditorStartInit (시작 씬/현재 씬/테스트모드 Stage 1~4 메뉴)
- [x] DevTestInput (1~7 키 적 소환, L 레벨업, 골드 9999·무한, WaveStarter 비활성)
- [x] AddressableAutoSetup (에디터 메뉴 한 번으로 전체 PrevLoad 자동 등록)

### 적/웨이브
- [x] EnemyController (UniTask 경로 이동, 데미지/사망, 경로 재계산, 풀 이중반환 버그 수정)
- [x] SplitEnemyController / SplitEnemyData (사망 시 분열, afterSplitKey 기반 소환)
- [x] ReviveEnemyController / ReviveEnemyData (첫 사망 시 HP 회복 부활)
- [x] MiddleBossEnemyController / BossEnemyController
- [x] WaveManager (공식 기반 스케일링, 보스 웨이브, UniTask 스폰, 웨이브 클리어 골드 보상)
- [x] WaveStarter (ReadyTask 대기 후 자동 시작, DisableAutoStart)
- [x] BuffSystem (IBuff, BuffEffect, BuffHandler, StatModifier, SlowEffect, PoisonEffect)
- [x] EnemyHPBar (월드스페이스, Follow 컴포넌트)

### 타워
- [x] TowerPlacer (ReadyTask 대기 후 팝업 초기화)
- [x] TowerController (타겟팅, 공격 타이머, 투사체 발사, 업그레이드 스탯, 포신 회전, 치명타)
- [x] ProjectileController (타겟 추적, 데미지 후 풀 반환)
- [x] RangeIndicator (LineRenderer + Cylinder, 지형 외곽선)
- [x] SlowTower / PoisonTower / CannonTower / LightningTower / SniperTower
- [x] TowerData ScriptableObject 6종 (TowerDataGenerator 자동 생성)

### 카드 시스템
- [x] CardData / CardManager (14종, A/B/C/D 카테고리, 가중치 랜덤, 스택 제한)
- [x] CardManager.Init() 키: `{EffectType}Data` 형식
- [x] UI_LevelUpPopup / UI_CardItem (레벨업 시 카드 3장 선택)
- [x] 글로벌 멀티플라이어 전체 연결

### 스킬 시스템
- [x] SkillData 5종 (Block, ArrowRain, Freeze, LightningStorm, PoisonMist)
- [x] SkillManager (슬롯 3개, 쿨다운 UniTask, 이벤트)
- [x] UI_SkillSlot / UI_SkillSelectPopup / UI_SkillItem

### UI
- [x] UI_TowerSelectPopup (DOTween 방사형 애니메이션)
- [x] UI_TowerUpgradePopup (독립 3열 업그레이드)
- [x] UI_GameOverPopup (DOTween 연출, Retry / MainMenu 버튼)
- [x] 카메라 핀치 줌 / 드래그 패닝

---

## ⚠️ Unity Editor 작업 필요 (코드 완료, 프리팹/설정만 남음)

- [ ] **TowerDefense/MyEditor/Addressables PrevLoad 자동 설정** 실행
- [ ] UI_GameOverPopup 프리팹 제작 (Text_Title, Text_WaveCount, Button_Retry, Button_MainMenu, Image_BG)
- [ ] SplitEnemyData.afterSplitKey 값 확인 (일반 EnemyData 키를 가리켜야 함, SplitEnemyData 금지)
- [ ] DevTools 오브젝트에 GameSceneBootstrap + DevTestInput 컴포넌트 추가, _enemies 배열 7개 연결

---

## 🟢 마무리

- [ ] **타이틀/MainMenu 씬** — 시작 버튼, 난이도 선택, 세팅
- [ ] **모바일 빌드 테스트** (Android APK)
