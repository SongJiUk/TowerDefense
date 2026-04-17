# BuffSystem — 기술 설계서

> 작성일: 2026-04-17
> 대상 파일: `Assets/Scripts/Buff/`

---

## 1. 개요

런타임 중 타워·적의 스탯을 동적으로 변경하는 시스템.
포이즌 타워의 독 데미지, 슬로우 타워의 이동속도 감소, 스킬의 공격력 증가 등 모든 버프/디버프를 통일된 구조로 관리한다.

### 설계 원칙

| 원칙 | 내용 |
| ---- | ---- |
| 단일 책임 | 효과(Effect)는 동작만, 수치 계산은 StatModifier만 담당 |
| 중복 차단 | 같은 타입의 버프는 동시에 하나만 (AllowStack 플래그로 제어) |
| 프레임 할당 없음 | `_expiredBuffer` 재사용 — 만료 버프 처리 시 new List 금지 |
| 느슨한 결합 | BuffHandler는 IBuff 인터페이스만 알고, 구체 효과를 몰라도 됨 |

---

## 2. 클래스 구조

```
<<interface>>
IBuff
 ├─ OnApply(BuffHandler)
 ├─ OnRemove(BuffHandler)
 ├─ Tick(float deltaTime)
 └─ IsExpired : bool

     ↑ implements

<<abstract>>
BuffEffect
 ├─ _elapsed : float          ← 경과 시간
 ├─ Duration : float          ← 지속 시간
 ├─ AllowStack : bool         ← 중복 허용 여부
 ├─ IsExpired : bool          ← _elapsed >= Duration
 ├─ EffectType : Type  ← abstract  (중복 차단 키)
 ├─ OnApply() ← abstract
 ├─ OnRemove() ← abstract
 └─ Tick()  →  _elapsed += dt  (virtual)

     ↑ extends

 ┌───────────────┬───────────────┬───────────────┐
 │  PoisonEffect │  SlowEffect   │   AtkBuff     │
 ├───────────────┼───────────────┼───────────────┤
 │ _target       │ _modifier     │ _modifier     │
 │ _dps          │ _multiplier   │ _multiplier   │
 ├───────────────┼───────────────┼───────────────┤
 │ OnApply: 없음 │ AddModifier   │ AddModifier   │
 │ OnRemove: 없음│ RemoveModifier│ RemoveModifier│
 │ Tick: TakeDmg │               │               │
 └───────────────┴───────────────┴───────────────┘
```

---

## 3. 파일 목록

| 파일 | 위치 | 역할 |
| ---- | ---- | ---- |
| `IBuff.cs` | `Buff/` | 버프 계약 인터페이스 |
| `BuffEffect.cs` | `Buff/` | 공통 로직 추상 기반 클래스 |
| `BuffHandler.cs` | `Buff/` | MonoBehaviour — 효과 관리·Tick |
| `StatModifier.cs` | `Buff/` | 스탯 수식자 (Flat / Percent) |
| `PoisonEffect.cs` | `Buff/Effects/` | 매 프레임 독 데미지 |
| `SlowEffect.cs` | `Buff/Effects/` | 이동속도 감소 |
| `AtkBuff.cs` | `Buff/Effects/` | 공격력 증가 |

수정 파일:

| 파일 | 변경 내용 |
| ---- | --------- |
| `Utils/Define.cs` | `StatType`, `ModifierType` enum 추가 |
| `Tower/TowerController.cs` | BuffHandler 연동, `_baseDamage/Speed/Range` 분리 |
| `Controller/EnemyController.cs` | BuffHandler 연동, `_baseSpeed` 분리 |

---

## 4. StatModifier

스탯 수정 방식을 표현하는 순수 데이터 클래스.

```
StatType    어떤 스탯을 변경하는지
Value       수정 값
ModType     Flat(덧셈) or Percent(곱셈)
```

### 계산 방식

| ModifierType | 공식 | 예시 |
| ------------ | ---- | ---- |
| Flat | `base + Value` | 공격력 +10 → 20 + 10 = 30 |
| Percent | `base × Value` | 속도 ×0.5 → 3.0 × 0.5 = 1.5 |

> Percent는 배율값 기준. 감속 50%면 `Value = 0.5f`, 공격력 30% 증가면 `Value = 1.3f`

---

## 5. BuffHandler

MonoBehaviour로 타워/적 오브젝트에 컴포넌트로 붙인다.

### 내부 데이터

| 필드 | 타입 | 역할 |
| ---- | ---- | ---- |
| `_effects` | `List<IBuff>` | 현재 활성 효과 목록 |
| `_modifiers` | `List<StatModifier>` | 현재 적용 중인 수식자 목록 |
| `_expiredBuffer` | `List<IBuff>` | 만료된 효과 임시 보관 (재사용) |

### 이벤트

```
event Action OnModifiersChanged
```
`AddModifier` / `RemoveModifier` 호출 시 발생.
TowerController·EnemyController가 구독해서 스탯 재계산.

### 주요 메서드

| 메서드 | 설명 |
| ------ | ---- |
| `AddEffect(BuffEffect)` | 중복 체크 후 OnApply() 호출 |
| `GetStat(StatType, float base)` | modifiers 순회 → 최종 스탯 반환 |
| `AddModifier(StatModifier)` | 리스트 추가 → 이벤트 발생 |
| `RemoveModifier(StatModifier)` | 리스트 제거 → 이벤트 발생 |

### Update() 흐름

```
매 프레임
  └─ _effects 각각 Tick(dt)
  └─ IsExpired → _expiredBuffer에 추가
  └─ (순회 완료 후) _expiredBuffer 순회
       └─ OnRemove(this)
       └─ _effects에서 제거
  └─ _expiredBuffer.Clear()
```

> 순회 중 _effects를 직접 Remove하지 않는 이유:
> `foreach` 중 컬렉션을 수정하면 `InvalidOperationException` 발생.
> _expiredBuffer에 담았다가 순회가 끝난 뒤 처리.

---

## 6. 효과별 동작 상세

### PoisonEffect (독)

```
생성자 인자:  IDamageable target, float dps, float duration
AllowStack:  false (같은 독 중복 금지)

OnApply  → 없음 (StatModifier 없음)
OnRemove → 없음
Tick     → base.Tick(dt) + target.TakeDamage(dps × dt)
```

독은 스탯을 바꾸는 게 아니라 매 프레임 직접 데미지를 입힌다.
StatModifier 사용 안 함.

### SlowEffect (감속)

```
생성자 인자:  float speedMultiplier, float duration
AllowStack:  false

OnApply  → StatModifier(StatType.Speed, multiplier, Percent) 생성
           → BuffHandler.AddModifier() 호출
OnRemove → BuffHandler.RemoveModifier() 호출
Tick     → base.Tick(dt)만 (스탯 변경은 이미 적용됨)
```

감속 50%면 `speedMultiplier = 0.5f`.

### AtkBuff (공격력 증가)

```
생성자 인자:  float damageMultiplier, float duration
AllowStack:  false

OnApply  → StatModifier(StatType.Damage, multiplier, Percent) 생성
           → BuffHandler.AddModifier() 호출
OnRemove → BuffHandler.RemoveModifier() 호출
Tick     → base.Tick(dt)만
```

공격력 30% 증가면 `damageMultiplier = 1.3f`.

---

## 7. TowerController 연동

업그레이드 계산 후 버프를 추가로 적용하는 2단계 구조.

```
ApplyStats() 흐름:

1단계 — 업그레이드 기반 베이스 계산
  _baseDamage      = Data.baseDamage × (업그레이드 배율)
  _baseAttackSpeed = Data.baseAttackSpeed × (업그레이드 배율)
  _baseRange       = Data.baseRange + (업그레이드 보너스)

2단계 — 버프 적용
  _currentDamage      = BuffHandler.GetStat(StatType.Damage,      _baseDamage)
  _currentAttackSpeed = BuffHandler.GetStat(StatType.AttackSpeed, _baseAttackSpeed)
  _currentRange       = BuffHandler.GetStat(StatType.Range,       _baseRange)
```

BuffHandler.OnModifiersChanged 이벤트 → ApplyStats() 재호출.
이벤트 구독/해제는 Awake/OnDisable에서 처리.

---

## 8. EnemyController 연동

웨이브 배율 적용 후 버프를 추가로 반영하는 구조.

```
Init() 흐름:
  _baseSpeed = data.baseMoveSpeed × speedMultiplier  ← 웨이브 배율 포함
  _speed     = _baseSpeed                            ← 초기에는 버프 없음

RecalculateSpeed():
  _speed = BuffHandler.GetStat(StatType.Speed, _baseSpeed)
```

BuffHandler.OnModifiersChanged 이벤트 → RecalculateSpeed() 호출.
이벤트 해제는 기존 OnDisable에 추가.

---

## 9. 사용 예시

### 포이즌 타워 → 적에게 독 적용

```csharp
var handler = enemy.GetComponent<BuffHandler>();
var damageable = enemy.GetComponent<IDamageable>();
handler?.AddEffect(new PoisonEffect(damageable, dps: 5f, duration: 4f));
```

### 슬로우 타워 → 적에게 감속 적용

```csharp
var handler = enemy.GetComponent<BuffHandler>();
handler?.AddEffect(new SlowEffect(speedMultiplier: 0.5f, duration: 2f));
```

### 스킬 → 타워에 공격력 버프 적용

```csharp
var handler = tower.GetComponent<BuffHandler>();
handler?.AddEffect(new AtkBuff(damageMultiplier: 1.3f, duration: 10f));
```

---

## 10. 씬 설정

BuffHandler는 MonoBehaviour이므로 타워·적 프리팹에 컴포넌트로 추가해야 한다.

```
Enemy 프리팹
  └─ EnemyController
  └─ BuffHandler          ← 추가 필요

Tower 프리팹
  └─ TowerController
  └─ BuffHandler          ← 추가 필요
```

`GetComponent<BuffHandler>()`는 Awake에서 캐싱.
프리팹에 컴포넌트가 없으면 null 반환 → `?.` 연산자로 안전하게 처리.

---

## 11. StatType 목록

`Define.cs` 에 정의.

| StatType | 대상 | 사용 |
| -------- | ---- | ---- |
| `Damage` | 타워 | AtkBuff |
| `AttackSpeed` | 타워 | (추후 추가 가능) |
| `Range` | 타워 | (추후 추가 가능) |
| `Speed` | 적 | SlowEffect |

---

_이 문서는 BuffSystem 구현 진행에 따라 업데이트됩니다._
