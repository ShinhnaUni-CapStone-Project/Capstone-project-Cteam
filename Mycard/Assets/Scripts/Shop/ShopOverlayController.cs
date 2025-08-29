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
                _currentRun.Gold -= amount;
                // (실제로는 여기서 DB 저장이 일어나야 하지만, 지금은 상태 변경만)
            };
        }
        // 3. ShopUI의 상태가 바뀌면(OnSessionChanged) 자동으로 저장하도록 연결합니다.
        shopUI.OnSessionChanged += SaveCurrentShopSession;
    }

    // "특정 주소의 상점 문을 열어라!"
    public void OpenForNode(int floor, int index)
    {
        _currentKey = (floor, index);

        // '기억 노트'에 이 상점의 정보가 있는지 확인
        if (_sessionMemory.TryGetValue(_currentKey, out var dto))
        {
            // 정보가 있다면, 그 정보로 상점 상태를 복원
            shopUI.ImportSession(dto);
        }
        else
        {
            // 정보가 없다면, 새로운 상점이므로 상태를 초기화
            shopUI.ResetSession();
        }

        shopUI.Open();

        // 만약 처음 연 상점이라면, 초기 상태를 바로 '기억 노트'에 기록
        if (!_sessionMemory.ContainsKey(_currentKey))
        {
            SaveCurrentShopSession();
        }
    }

    // 현재 상점의 상태를 '기억 노트'에 저장하는 함수
    public void SaveCurrentShopSession()
    {
        var dto = shopUI.ExportSession();
        _sessionMemory[_currentKey] = dto;
        Debug.Log($"[{_currentKey.floor},{_currentKey.index}] 상점 정보가 메모리에 저장됨.");
    }

    // 외부에서 "이전 상점 기록은 잊어라"고 명령할 함수
    public void ResetShopSession()
    {
        shopUI?.ResetSession();
    }
}