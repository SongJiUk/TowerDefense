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

- [ ] 타워별 발사 이펙트 (파티클)
- [ ] 적 피격 이펙트
- [ ] 적 사망 이펙트
- [ ] 보스 등장 연출 (카메라 줌인 등)
- [ ] 웨이브 클리어 이펙트
- [ ] 스킬 발동 이펙트 (ArrowRain, Freeze 등)
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

## 다음 세션 시작 순서 (2026-05-06 기준)

### ✅ 이번 세션 완료 (코드)
- UI_NextWavePanel — 다음 웨이브 예고 패널 (타입별 색상, 적 구성, 카운트다운, 즉시 시작)
- WaveManager — PrepareNextWave / RequestEarlyStart / OnNextWaveReady / OnBossSpawned
- WaveStarter — 딜레이 CTS 분리, OnEarlyStartRequested 처리
- EnemyController — OnHpChanged / OnDeathEvent 이벤트 추가
- UI_BossHPBar — 보스 등장 시 자동 표시, HP 실시간 반영
- UI_TitleScene — 페이드 아웃 후 GameScene 로드 (FadeAndLoad)
- UI_StageSelectPopup — 스테이지 테마 색 적용 (잠금 시 회색)
- UI_DifficultySelectPopup — 씬 로드 권한을 TitleScene으로 위임

### ⚠️ 다음 세션 시작 전 Unity 에디터 작업 (먼저 해야 코드가 동작함)

**① TitleScene — Panel_Fade 추가**
- Canvas > UI_TitleScene 하위 맨 마지막에 `Panel_Fade` 추가
- Image: 검정(0,0,0,255), Raycast Target OFF
- CanvasGroup 컴포넌트 추가

**② GameScene — UI_BossHPBar 배치**
- Canvas 하위에 `UI_BossHPBar` GameObject 추가 (상단 중앙)
- 하위: `Text_BossName`(TMP), `Image_HPFill`(Image, Filled/Horizontal)
- UI_BossHPBar 컴포넌트 연결, 초기 비활성화

**③ GameScene — UI_NextWavePanel 프리팹 (이미 만들었으면 확인만)**
- `UI_NextWavePanel` 컴포넌트의 enum 순서와 하위 오브젝트 이름 일치 여부 확인
- 에디터에서 N키 눌러 패널 표시 테스트

### 🔜 다음 코드 작업 순서

**1. FloatingText 프리팹** ← Unity 에디터 작업
- TMP 오브젝트 생성, 위로 올라가며 FadeOut (DOTween)
- Addressables PrevLoad 그룹, 키 `FloatingText`로 등록

**2. 저장/불러오기 완성**
- 게임 시작 시 레벨·경험치 불러오기 (`SaveM.ApplyToGame` 이미 있음 → 적용 확인)
- 스테이지 클리어 후 다음 스테이지 이어서 레벨 누적

**3. SFX 연결**
- 타워 공격 / 적 사망 / 웨이브 시작·클리어 / 카드·스킬

**4. 씬 전환 로딩 화면 (선택)**
- 현재는 TitleScene 백그라운드 로딩 후 직접 GameScene 진입
- 필요 시 LoadingScene 추가

---

> 마지막 업데이트: 2026-05-06
