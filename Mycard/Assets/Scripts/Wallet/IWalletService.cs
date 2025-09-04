using System;

// 런(게임 세션) 동안 플레이어의 골드를 관리하는 서비스입니다.
// - DB 우선 업데이트로 일관성을 보장합니다.
// - UI 동기화를 위한 변경 브로드캐스트(OnGoldChanged)를 제공합니다.
public interface IWalletService
{
    int Gold { get; }
    event Action<int> OnGoldChanged;

    bool TrySpend(int amount);
    void Add(int amount);

    // DB-우선 적용 후 성공 시 메모리/이벤트를 갱신합니다.
    bool Set(int amount);

    // 새로운 runId로 재바인딩하고 DB에서 최신 값을 동기화합니다.
    void RebindRun(string runId);
}

