# TowerDefense 개발 체크리스트

---

## 1단계 — 적 프리팹 (게임 플레이 기반)

- [x] 일반 적 프리팹 생성 + EnemyData ScriptableObject
- [x] 빠름 적 프리팹 생성 + EnemyData
- [x] 탱커 적 프리팹 생성 + EnemyData
- [x] 부활 적 프리팹 생성 + EnemyData (죽으면 작은 적 2마리 소환)
- [x] 중간 보스 프리팹 생성 + EnemyData
- [x] 최종 보스 프리팹 생성 + EnemyData
- [x] 각 프리팹 Addressables 등록 + PrevLoad 레이블
- [x] StageData 3개에 enemyPool 배치 (fromWave, spawnWeight 설정)

---

## 2단계 — 사운드 시스템

- [x] SoundManager 구현 (BGM / SFX 채널 분리)
- [x] AudioSource 오브젝트 풀링
- [x] 볼륨 저장 (PlayerPrefs)
- [ ] 타워 공격 SFX 연결
- [ ] 적 사망 SFX 연결
- [ ] 웨이브 시작 / 클리어 SFX 연결
- [ ] 카드 선택 SFX 연결
- [ ] 스킬 발동 SFX 연결
- [ ] 타이틀 BGM / 인게임 BGM 설정

---

## 3단계 — 배속 / 일시정지

- [x] 배속 버튼 UI (x1 / x2 토글) — Button_Speed (UI_GameScene에 로직 추가, 프리팹 연결 필요)
- [x] Time.timeScale 1f / 2f 전환
- [x] 일시정지 팝업 (UI_PausePopup) — 계속하기 / 포기
- [x] 일시정지 중 UI 텍스트 이상 없는지 확인 (SetUpdate(true) 체크)

---

## 4단계 — 타이틀씬 / 스테이지 선택

- [ ] 타이틀씬 배경 + 게임 로고 배치
- [x] "게임 시작" 버튼 → UI_StageSelectPopup → UI_DifficultySelectPopup → 페이드 → GameScene
- [x] UI_StageSelectPopup 구현
  - [x] 스테이지 1~4 버튼
  - [x] 클리어 여부 자물쇠 표시
  - [x] 각 스테이지 테마 색 적용 (숲/사막/겨울/악마성)
- [ ] 씬 전환 로딩 화면 (UI_LoadingScene)
- [x] 선택한 스테이지 Managers.SelectedStage에 저장 후 GameScene 로드

### ⚠️ Unity 에디터 작업 — 4단계

- [ ] **TitleScene Canvas > UI_TitleScene** 하위에 `Panel_Fade` 추가
  - RectTransform: 앵커 stretch 전체 화면 (Left/Right/Top/Bottom = 0)
  - Image 컴포넌트: Color = 검정 (0,0,0,255), Raycast Target = **OFF**
  - **CanvasGroup** 컴포넌트 추가
  - 계층 순서: 모든 UI 중 맨 마지막 자식 (가장 위에 렌더링)

---

## 5단계 — 난이도 선택창

- [x] UI_DifficultyPopup 구현 (쉬움 / 보통 / 어려움)
- [x] 스테이지 선택 후 난이도 선택 → 게임 시작
- [x] 난이도별 설명 텍스트 표시 (적 HP 배율, 코어 HP 배율)

---

## 6단계 — 저장 / 불러오기

- [x] SaveData 클래스 정의 (레벨, 경험치, 스테이지 클리어 여부)
- [x] SaveManager 구현 (PlayerPrefs 또는 JSON 파일)
- [ ] 게임 시작 시 불러오기
- [x] 스테이지 클리어 / 게임오버 시 저장
- [ ] 레벨·경험치 스테이지 간 누적 유지

---

## 보스 HP바

- [x] EnemyController OnHpChanged / OnDeathEvent 이벤트 추가
- [x] WaveManager OnBossSpawned 이벤트 추가 (보스 컨트롤러 참조 전달)
- [x] UI_BossHPBar 스크립트 작성 (보스 등장 시 자동 표시 · 사망 시 숨김)

### ⚠️ Unity 에디터 작업 — 보스 HP바

- [ ] **GameScene Canvas** 하위에 `UI_BossHPBar` GameObject 추가
  - 위치: 화면 상단 중앙
  - 하위 오브젝트:
    - `Text_BossName` — TMP 텍스트 (보스 이름)
    - `Image_HPFill` — Image 컴포넌트, **Image Type = Filled**, Fill Method = Horizontal
  - `UI_BossHPBar` 컴포넌트 연결
  - 초기 상태: **비활성화** (SetActive false) — 코드에서 보스 등장 시 자동 활성화

---

## 7단계 — 데미지 숫자 표시

- [ ] FloatingText 프리팹 생성 (TMP + 위로 올라가며 사라지는 애니메이션) ← Unity 에디터 작업
- [x] FloatingTextPool 구현 (ObjectPool)
- [x] EnemyController.TakeDamage에서 FloatingText 호출
- [x] 치명타 / 독 / 슬로우 색상 구분
- [ ] FloatingText Addressables 등록

### ⚠️ Unity 에디터 작업 — FloatingText

- [ ] Canvas (World Space) 또는 UI Canvas에 `FloatingText` 프리팹 생성
  - TMP 텍스트 오브젝트
  - Animator 또는 DOTween: 위로 1~1.5초 이동 + FadeOut
  - Addressables PrevLoad 그룹에 키 `FloatingText`로 등록

---

## 8단계 — 게임 이펙트

- [x] EffectManager 구현 (Pool 기반 Play / PlayLine)
- [x] TowerData / EnemyData / SkillData 이펙트 키 필드 추가
- [x] 타워별 발사 이펙트 (hitEffectKey / shootEffectKey)
- [x] 적 피격 이펙트 (투사체 착탄 위치에 재생)
- [x] 적 사망 이펙트 (deathEffectKey / deathEffectDuration)
- [x] 보스 / 중간보스 / 분열 / 부활 이펙트 분리
- [x] 버프 상태 이펙트 (FX_Buff_Slow / FX_Buff_Poison — 적 자식으로 루프)
- [x] 스킬 발동 이펙트 (skill.effectKey)
- [x] 타워 설치 이펙트 (FX_Tower_Place)
- [x] 타워 업그레이드 단계별 이펙트 (FX_Stage_Green/Blue/Red — child SetActive)
- [x] 번개 체인 이펙트 (chainEffectKey)
- [ ] 보스 등장 연출 (카메라 줌인 등)
- [ ] 웨이브 클리어 이펙트
- [ ] 코어 피격 이펙트

---

## 9단계 — 설정창

- [x] UI_SettingsPopup 구현
- [x] BGM 볼륨 슬라이더
- [x] SFX 볼륨 슬라이더
- [x] 그래픽 품질 설정 (Low / Medium / High)
- [x] 설정값 PlayerPrefs 저장

---

## 10단계 — 업적 시스템

- [ ] 업적 목록 기획 (예: 타워 100개 설치, 웨이브 10연속 클리어 등)
- [x] AchievementData ScriptableObject
- [x] AchievementManager 구현
- [x] 업적 달성 시 팝업 알림
- [x] UI_AchievementPopup 구현

---

## 11단계 — 파이어베이스 / 랭킹 (선택)

- [ ] Firebase SDK 설치 (Authentication + Firestore)
- [ ] 익명 로그인 또는 구글 로그인
- [ ] 랭킹 데이터 구조 설계 (스테이지 클리어 시간, 점수)
- [ ] UI_RankingPopup 구현
- [ ] 점수 등록 / 조회 연동

---

## 마무리 — 빌드 / 배포

- [ ] Android 빌드 설정 (ETC2 텍스처, 해상도 고정)
- [ ] 프레임 캡 설정 (60fps)
- [ ] 메모리 프로파일링 (Profiler로 누수 확인)
- [ ] APK / AAB 빌드 후 실기기 테스트
- [ ] 스크린샷 / 영상 캡처 (포트폴리오용)

---

---

## 세션 (2026-05-07 ~ 2026-05-12) — 완료 항목

- [x] 게임씬 + 타이틀씬 연결 (FadeAndLoad, PrepareStage, GameSceneBootstrap 정리)
- [x] Bind 안 쓰는 Text 제거 (Text_LogoGuard, Text_Description, Text_WaveAnnounce, Object_WaveAnnounce)
- [x] 몬스터 미등장 수정 — Object_WaveAnnounce 없어서 BeginSpawning 미호출 → OnWaveStart에서 직접 호출
- [x] stageKey 버그 수정 — GameSceneBootstrap·DifficultySelectPopup "Stage{n}" → "Stage{n}Data"
- [x] StageData.waveStartDelay WaveStarter에 연결 (스테이지별 웨이브 대기시간 적용)
- [x] UI_NextWavePanel Image_Wave 추가 (Atlas wave_boss·wave_middleboss·wave_normal)
- [x] ForceClose double-release 버그 수정 (_closed 플래그)
- [x] UI_SkillSlot 바인드 후 CoolDown 초기 비활성화

## 세션 (2026-06-10 ~ 2026-06-11) — 완료 항목

- [x] EffectManager 신규 구현 (Pool 기반 Play / PlayLine)
- [x] ScriptableObject 이펙트 키 필드 추가 (TowerData / EnemyData / SkillData / LightningTowerData / ReviveEnemyData)
- [x] 투사체 히트 이펙트 (ProjectileController — aimPos 기준 재생)
- [x] 적 사망 이펙트 전 계층 분리 (Normal / Boss / MiddleBoss / Split / Revive)
- [x] 버프 상태 VFX (SlowEffect / PoisonEffect — OnApply SetParent, OnRemove 풀 반환)
- [x] 스킬 이펙트 (SkillManager — skill.effectKey)
- [x] 타워 설치 이펙트 (TowerPlacer)
- [x] 타워 업그레이드 단계 이펙트 활성화 (UpdateVisualEffect 주석 해제)
- [x] 번개 체인 이펙트 (chainEffectKey / chainEffectHeightOffset)

---

## 🗓️ 이번 주 목표 (2026-06-11 ~ 2026-06-15)

### Day 1-2 (목~금) — 사운드
- [ ] 타워 공격 SFX 연결 (Basic / Cannon / Slow / Sniper / Poison / Lightning)
- [ ] 적 사망 SFX 연결
- [ ] 웨이브 시작 / 클리어 SFX 연결
- [ ] 카드 선택 SFX 연결
- [ ] 스킬 발동 SFX 연결
- [ ] 인게임 BGM 설정
- [ ] 타이틀 BGM 설정

### Day 3 (토) — 타이틀씬 + 에디터 마무리
- [ ] 타이틀씬 배경 + 게임 로고 배치
- [ ] TitleScene Panel_Fade 추가 (CanvasGroup, 검정 Image, Raycast OFF)
- [ ] UI_BossHPBar 프리팹 배치 (Text_BossName, Image_HPFill)

### Day 4-5 (일) — 빌드 + 포트폴리오
- [ ] Android APK 빌드 (ETC2 텍스처, 60fps 프레임 캡)
- [ ] 실기기 테스트
- [ ] 포트폴리오용 스크린샷 캡처
- [ ] 플레이 영상 촬영 (1~2분)

---

> 마지막 업데이트: 2026-06-11
