# TowerDefense 개발 체크리스트

---

## 1단계 — 적 프리팹 (게임 플레이 기반)

- [ ] 일반 적 프리팹 생성 + EnemyData ScriptableObject
- [ ] 빠름 적 프리팹 생성 + EnemyData
- [ ] 탱커 적 프리팹 생성 + EnemyData
- [ ] 부활 적 프리팹 생성 + EnemyData (죽으면 작은 적 2마리 소환)
- [ ] 중간 보스 프리팹 생성 + EnemyData
- [ ] 최종 보스 프리팹 생성 + EnemyData
- [ ] 각 프리팹 Addressables 등록 + PrevLoad 레이블
- [ ] StageData 3개에 enemyPool 배치 (fromWave, spawnWeight 설정)

---

## 2단계 — 사운드 시스템

- [ ] SoundManager 구현 (BGM / SFX 채널 분리)
- [ ] AudioSource 오브젝트 풀링
- [ ] 볼륨 저장 (PlayerPrefs)
- [ ] 타워 공격 SFX 연결
- [ ] 적 사망 SFX 연결
- [ ] 웨이브 시작 / 클리어 SFX 연결
- [ ] 카드 선택 SFX 연결
- [ ] 스킬 발동 SFX 연결
- [ ] 타이틀 BGM / 인게임 BGM 설정

---

## 3단계 — 배속 / 일시정지

- [ ] 배속 버튼 UI (x1 / x2 토글) — Button_Speed
- [ ] Time.timeScale 1f / 2f 전환
- [ ] 일시정지 팝업 (UI_PausePopup) — 계속하기 / 포기
- [ ] 일시정지 중 UI 텍스트 이상 없는지 확인 (SetUpdate(true) 체크)

---

## 4단계 — 타이틀씬 / 스테이지 선택

- [ ] 타이틀씬 배경 + 게임 로고 배치
- [ ] "게임 시작" 버튼 → 스테이지 선택 화면으로 전환
- [ ] UI_StageSelectPopup 구현
  - [ ] 스테이지 1~4 버튼
  - [ ] 클리어 여부 자물쇠 표시
  - [ ] 각 스테이지 테마 색 적용
- [ ] 씬 전환 로딩 화면 (UI_LoadingScene)
- [ ] 선택한 스테이지 Managers.SelectedStage에 저장 후 GameScene 로드

---

## 5단계 — 난이도 선택창

- [ ] UI_DifficultyPopup 구현 (쉬움 / 보통 / 어려움)
- [ ] 스테이지 선택 후 난이도 선택 → 게임 시작
- [ ] 난이도별 설명 텍스트 표시 (적 HP 배율, 코어 HP 배율)

---

## 6단계 — 저장 / 불러오기

- [ ] SaveData 클래스 정의 (레벨, 경험치, 스테이지 클리어 여부)
- [ ] SaveManager 구현 (PlayerPrefs 또는 JSON 파일)
- [ ] 게임 시작 시 불러오기
- [ ] 스테이지 클리어 / 게임오버 시 저장
- [ ] 레벨·경험치 스테이지 간 누적 유지

---

## 7단계 — 데미지 숫자 표시

- [ ] FloatingText 프리팹 생성 (TMP + 위로 올라가며 사라지는 애니메이션)
- [ ] FloatingTextPool 구현 (ObjectPool)
- [ ] EnemyController.TakeDamage에서 FloatingText 호출
- [ ] 치명타 / 독 / 슬로우 색상 구분
- [ ] Addressables 등록

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

- [ ] UI_SettingsPopup 구현
- [ ] BGM 볼륨 슬라이더
- [ ] SFX 볼륨 슬라이더
- [ ] 그래픽 품질 설정 (Low / Medium / High)
- [ ] 설정값 PlayerPrefs 저장

---

## 10단계 — 업적 시스템

- [ ] 업적 목록 기획 (예: 타워 100개 설치, 웨이브 10연속 클리어 등)
- [ ] AchievementData ScriptableObject
- [ ] AchievementManager 구현
- [ ] 업적 달성 시 팝업 알림
- [ ] UI_AchievementPopup 구현

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

> 마지막 업데이트: 2026-04-27
