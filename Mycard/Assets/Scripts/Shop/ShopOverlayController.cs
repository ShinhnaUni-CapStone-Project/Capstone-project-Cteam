using System.Collections.Generic;
using UnityEngine;
using Game.Save;

public class ShopOverlayController : MonoBehaviour
{
    [SerializeField] private ShopUI shopUI;

    // 각 노드 주소별 상점 상태를 기억할 '기억 노트'
    private readonly Dictionary<(int floor, int index), ShopSessionDTO> _sessionMemory = new();
    private (int floor, int index) _currentKey;
    private CurrentRun _currentRun; // 현재 플레이어의 런 데이터

    void Awake()
    {
        if (shopUI == null) shopUI = FindObjectOfType<ShopUI>(true);

        // --- 지갑 연결 ---
        // 1. 현재 런 데이터를 불러옵니다.
        var runId = PlayerPrefs.GetString("lastRunId", "");
        var data = string.IsNullOrEmpty(runId) ? null : DatabaseManager.Instance.LoadCurrentRun(runId);
        _currentRun = data?.Run;

        if (_currentRun != null)
        {
            // 2. ShopUI의 '지갑 기능'을 실제 런 데이터와 연결합니다.
            shopUI.GetGold = () => _currentRun.Gold;
            shopUI.SpendGold = (amount) => {
                if (_currentRun == null) return;
                _currentRun.Gold = Mathf.Max(0, _currentRun.Gold - amount);

                // 골드 업데이트 함수 호출!
                DatabaseManager.Instance.UpdateRunGold(_currentRun.RunId, _currentRun.Gold);
            };
        }
        // 3. ShopUI의 상태가 바뀌면(OnSessionChanged) 자동으로 저장하도록 연결합니다.
        shopUI.OnSessionChanged += SaveCurrentShopSession;
    }

    // "특정 주소의 상점 문을 열어라!"
    public void OpenForNode(int floor, int index)
    {
        _currentKey = (floor, index);
        Debug.Log($"<color=cyan>OPENING shop for ({floor},{index})...</color>", this);

        if (_currentRun == null)
        {
            var runId = PlayerPrefs.GetString("lastRunId", "");
            var data = string.IsNullOrEmpty(runId) ? null : DatabaseManager.Instance.LoadCurrentRun(runId);
            _currentRun = data?.Run;
        }

        ShopSessionDTO dto = null;

        // [1순위] '이어하기' 상황을 위해 DB(장기 기억)를 먼저 확인합니다.
        if (_currentRun != null)
        {
            // DatabaseManager에 새로 추가한 LoadShopSession 함수를 호출합니다.
            var json = DatabaseManager.Instance.LoadActiveShopSessionJson(_currentRun.RunId);
            if (!string.IsNullOrEmpty(json))
            {
                // JSON 파싱에 실패할 경우를 대비해 try-catch로 감싸줍니다.
                try { dto = JsonUtility.FromJson<ShopSessionDTO>(json); }
                catch (System.Exception e) { Debug.LogWarning($"[Shop] JSON parse fail: {e.Message}", this); }
            }
        }

        // [DB 데이터 확인 결과]
        if (dto != null)
        {
            // DB에서 데이터를 성공적으로 찾았으면, 그 정보로 상점을 복원합니다.
            Debug.Log($"<color=green>SUCCESS: Loaded from DATABASE.</color>", this);
            _sessionMemory[_currentKey] = dto; // 메모리 캐시도 최신 정보로 갱신해줍니다.
            shopUI.ImportSession(dto);
        }
        // [2순위] DB에 데이터가 없을 때만, 메모리 캐시(단기 기억)를 확인합니다.
        else if (_sessionMemory.TryGetValue(_currentKey, out var memDto))
        {
            // 같은 게임 세션 내에서 재방문한 경우, 메모리 정보로 상점을 복원합니다.
            Debug.Log($"<color=green>SUCCESS: Loaded from MEMORY CACHE.</color>", this);
            shopUI.ImportSession(memDto);
        }
        // [3순위] DB와 메모리 두 곳 모두에 데이터가 없을 경우입니다.
        else
        {
            // '완전 최초 방문'이므로, 새로운 상점으로 초기화합니다.
            Debug.Log($"<color=yellow>INFO: Nothing found in DB or Memory. Resetting session.</color>", this);
            shopUI.ResetSession();
        }

        // --- 3. UI 표시 및 초기 상태 저장 ---
        // 준비된 내용으로 상점 UI를 화면에 표시합니다.
        shopUI.Open();

        // '완전 최초 방문'이었는지 최종적으로 다시 확인합니다.
        // (dto == null) → DB에 데이터가 없었다는 뜻.
        // (!_sessionMemory.ContainsKey(...)) → 메모리에도 데이터가 없었다는 뜻.
        if (dto == null && !_sessionMemory.ContainsKey(_currentKey))
        {
            // 두 조건이 모두 참일 때만, 방금 생성된 초기 상태를 DB에 저장합니다.
            Debug.Log($"<color=orange>First visit detected. Saving initial state to DB...</color>", this);
            SaveCurrentShopSession();
        }
    }

    // 현재 상점의 상태를 '기억 노트'에 저장하는 함수
    public void SaveCurrentShopSession()
    {
        if (_currentRun == null)
        {
            // [CCTV] 저장 실패: _currentRun이 없음
            Debug.LogError($"<color=red>SAVE FAILED:</color> _currentRun is null. Cannot save shop state.", this);
            return;
        }

        var dto = shopUI.ExportSession();
        var json = JsonUtility.ToJson(dto);

        Debug.Log($"<color=orange>SAVING shop state for ({_currentKey.floor},{_currentKey.index}):</color>\n{json}", this);

        // 1) DB에 RunId 단일 세션으로 upsert
        DatabaseManager.Instance.UpsertActiveShopSession(_currentRun.RunId, json);

        // 2) 메모리 캐시도 최신화
        _sessionMemory[_currentKey] = dto;
    }

    // 외부에서 "이전 상점 기록은 잊어라"고 명령할 함수
    public void ResetShopSession()
    {
        shopUI?.ResetSession();
    }
}