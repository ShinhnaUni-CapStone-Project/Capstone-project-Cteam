using UnityEngine;

/// <summary>
/// 게임이 시작될 때, DatabaseManager와 같은 핵심 시스템을 깨우고 초기화하는 역할을 합니다.
/// </summary>
[DefaultExecutionOrder(-10000)] // 이 스크립트가 가장 먼저 실행되도록 보장합니다.
public class GameInitializer : MonoBehaviour
{
    private static bool _bootstrapped;
    
    void Awake()
    {
        
        if (_bootstrapped) { Destroy(gameObject); return; } // ← 중복 방지 가드
        _bootstrapped = true;
        DontDestroyOnLoad(gameObject); // (선택) 씬이 바뀌어도 조립 담당자가 사라지지 않게 함

        // 새 게임을 시작하거나 씬을 다시 로드할 때를 대비해, 보관소를 항상 깨끗하게 비웁니다.
        ServiceRegistry.ClearAll();

        // 1. [기반 시스템 준비] 데이터베이스에 먼저 연결합니다.
        DatabaseManager.Instance.Connect();

        // 2. [부품 생성] '가벽' 역할을 할 DatabaseFacade를 생성합니다.
        var dbFacade = new DatabaseFacade();
        //    '보관소'에 IDatabase라는 이름으로 등록하여, 다른 전문가들이 찾아 쓸 수 있게 합니다.
        ServiceRegistry.Register<IDatabase>(dbFacade);

        // 3. 러닝 컨텍스트(runId) 확보
        // 런 ID가 비어있어도, EventManager 내부에서 안전하게 처리할 것입니다.
        var runId = PlayerPrefs.GetString("lastRunId", "");

        // 4. 월렛(지갑) 서비스 등록: DB-우선 골드 관리 + 브로드캐스트
        var wallet = new WalletService(dbFacade, runId);
        ServiceRegistry.Register<IWalletService>(wallet);

        // 5. '이어하기'든 '새 게임'이든 상관없이 항상 EventManager를 등록합니다.
        var eventManager = new EventManager(dbFacade, runId);
        ServiceRegistry.Register<IEventManager>(eventManager);

        Debug.Log("GameInitializer: 모든 시스템 조립 및 등록이 완료되었습니다.");
    }
}
