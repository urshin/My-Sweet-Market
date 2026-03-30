# 🧁 My Sweet Market

> 모바일 게임 **My Sweet Bakery**를 모작한 Unity 3D 싱글플레이어 게임

[![Unity](https://img.shields.io/badge/Unity-2022.3-black?logo=unity)](https://unity.com/)
[![Language](https://img.shields.io/badge/Language-C%23-239120?logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![Platform](https://img.shields.io/badge/Platform-Mobile-lightgrey?logo=android)](https://play.google.com/)

---

## 🎬 플레이 영상

[![플레이 영상](https://img.shields.io/badge/YouTube-플레이%20영상%20보기-FF0000?logo=youtube)](https://youtu.be/eOYtMX66Yvs)

---

## 📖 프로젝트 개요

| 항목 | 내용 |
|------|------|
| 장르 | 시뮬레이션 (마켓 운영) |
| 플랫폼 | 모바일 |
| 개발 도구 | Unity |
| 개발 언어 | C# |
| 목표 | 디자인 패턴을 적용한 모작 프로젝트 |

---

## 🛠 기술 스택

![Unity](https://img.shields.io/badge/Unity-000000?style=for-the-badge&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=csharp&logoColor=white)
![ShaderLab](https://img.shields.io/badge/ShaderLab-HLSL-blue?style=for-the-badge)

---

## ✨ 주요 구현 기능

### 🔷 디자인 패턴

#### Singleton Pattern
- `GameManager`, `DataManager`, `SoundManager` 등 공용 매니저 클래스에 적용
- 씬 전환 시에도 데이터가 유지되도록 `DontDestroyOnLoad` 처리

```csharp
public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static GameManager Instance
    {
        get { return instance; }
    }
}
```

---

#### Object Pool Pattern
- 돈, 사과 등 반복 생성·파괴되는 오브젝트의 성능 최적화
- `Dictionary<GameObject, Queue<GameObject>>` 구조로 프리팹별 풀 관리
- 중복 반환 방지 로직 포함

```csharp
// 오브젝트 사용
public GameObject GetObject(GameObject prefab, Vector3 position, Quaternion rotation)

// 오브젝트 반환
public void ReturnObject(GameObject prefab, GameObject obj)
```

---

### 🔷 고객 이동 시스템 (Customer Movement)

- **목적 큐(Queue)** 와 **조건 딕셔너리(Dictionary)** 를 활용한 이동 관리
- NavMesh 대신 직접 구현 → 도착 후 동작을 자유롭게 제어 가능

```csharp
public Queue<Transform> wayPointQueue = new Queue<Transform>();
public Dictionary<Transform, System.Func<bool>> waitConditions = new Dictionary<Transform, System.Func<bool>>();
```

- 고객이 웨이포인트에 순서대로 이동하고, 도착 후 `waitConditions` 조건을 충족해야 다음 목적지로 이동

---

### 🔷 유틸리티 (Utils)

공통으로 사용하는 기능들을 정적 클래스로 모듈화

| 기능 | 설명 |
|------|------|
| `MoveBezier` | 베지어 곡선을 따라 오브젝트 이동 (돈·아이템 연출) |
| `GetTheBag` | 가방 오브젝트 부모-자식 설정 및 위치 정렬 |
| `AddCroassant` | 크로아상 스택 적재 및 높이 자동 계산 |
| `CalculateMoney` | 크로아상 수량 기반 돈 오브젝트 격자 배치 생성 |
| `PlayerTakeMoneyRoutine` | 돈 수집 코루틴 (베지어 곡선 이동 + 사운드) |
| `PopUp` | AnimationCurve 기반 팝업 스케일 연출 |

---

### 🔷 Sprite Atlas

- 고객이 요청하는 제품 변경에 유연하게 대응
- 텍스처 드로우콜 최소화를 위해 Sprite Atlas 활용

---

## 🔧 개선 사항 및 회고

| 구분 | 내용 |
|------|------|
| 고객 이동 | NavMesh 대신 큐·델리게이트 방식으로 도착 후 동작 제어 자유도 확보 |
| 고객 다양화 | 현재 상품 1종 → 향후 **Factory Pattern** 적용으로 고객 및 상품 다양화 예정 |

---

## 📁 프로젝트 구조

```
My-Sweet-Market/
└── mysweetbakery/
    ├── Assets/
    │   ├── Scripts/
    │   │   ├── Manager/       # GameManager, DataManager, SoundManager 등
    │   │   ├── Customer/      # 고객 이동, 행동 로직
    │   │   ├── Utils/         # 공용 유틸리티
    │   │   └── Pool/          # ObjectPool
    │   ├── Shaders/           # ShaderLab / HLSL
    │   └── ...
    └── ...
```

---

## 📬 Contact

- GitHub: [@urshin](https://github.com/urshin)
