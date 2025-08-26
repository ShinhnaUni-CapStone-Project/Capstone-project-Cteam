using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.Save;

public class MapTraversalController : MonoBehaviour
{
    [Header("Marker")]
    public Transform playerMarker; // 현재 위치 마커(없으면 표시만 생략)

    Dictionary<(int floor, int index), NodeGoScene> _nodes;
    string _runId;
    CurrentRun _run;

    private ShopOverlayController _shopOverlay; //상점 오버레이 저장

    void Awake()
    {
        // 게임 시작 시 상점 오버레이를 한 번만 찾아둡니다.
        _shopOverlay = FindObjectOfType<ShopOverlayController>();
    }
    


    void Start()
    {
        DatabaseManager.Instance.Connect();

        _runId = PlayerPrefs.GetString("lastRunId", "");
        var data = string.IsNullOrEmpty(_runId) ? null : DatabaseManager.Instance.LoadCurrentRun(_runId);
        if (data == null) { Debug.LogError("[Traversal] 런 로드 실패"); return; }

        _run = data.Run;

        // 씬의 모든 노드 수집
        var list = FindObjectsByType<NodeGoScene>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        _nodes = list.ToDictionary(n => (n.floor, n.index), n => n);

        // 초기 표시
        PlaceMarker(_run.Floor, _run.NodeIndex);
        UpdateReachable(_run.Floor, _run.NodeIndex);
    }



    public void OnNodeClicked(NodeGoScene target)
    {
        // 1. 현재 노드 정보를 가져옵니다.
        if (!_nodes.TryGetValue((_run.Floor, _run.NodeIndex), out var curNode)) return;

        // 2. 현재 클릭이 어떤 종류인지 정의합니다.
        bool isMoveToChild = curNode.children != null && curNode.children.Contains(target);
        bool isReclickShop = (curNode == target && target.nodeType == NodeType.Shop);

        // 3. 유효하지 않은 클릭은 입구에서 차단합니다 (Guard Clause).
        if (!isMoveToChild && !isReclickShop)
        {
            return;
        }

        // --- 여기까지 통과했다면, 클릭은 '유효'한 것으로 확정 ---

        // 4. 상태 변경: **실제로 '새로운 노드로 이동'이 발생할 때만** 실행됩니다.
        if (isMoveToChild)
        {
            // 이동하기 전에, 이전에 열었던 상점 정보를 리셋합니다.
            _shopOverlay?.ResetShopSession();

            // 위치 이동에 따른 모든 상태 변경(DB 저장, 마커 이동 등)을 처리합니다.
            _run.Floor = target.floor;
            _run.NodeIndex = target.index;
            _run.UpdatedAtUtc = System.DateTime.UtcNow.ToString("o");

            var visited = new MapNodeState {
                RunId = _run.RunId, Act = _run.Act,
                Floor = target.floor, NodeIndex = target.index,
                Type = (Game.Save.NodeType)target.nodeType, Visited = true
            };
            
            DatabaseManager.Instance.SaveCurrentRun(
                _run,
                cards:null, relics:null, potions:null,
                nodes:new List<MapNodeState>{ visited },
                rngStates:null
            );

            PlaceMarker(target.floor, target.index);
            UpdateReachable(target.floor, target.index);
        }

        // 5. 최종 행동 결정: 모든 검사와 상태 변경이 끝난 후, 딱 한 번만 결정합니다.
        if (target.nodeType == NodeType.Shop)
        {
            // 목표가 상점이면 (새로 이동했든, 다시 클릭했든) 상점 오버레이를 엽니다.
            _shopOverlay?.OpenForNode(target.floor, target.index);
        }
        else if (isMoveToChild) // 상점이 아닌 다른 노드는, '이동'했을 때만 씬을 전환합니다.
        {
            target.GoToAssignedScene();
        }
    }

    void PlaceMarker(int floor, int index)
    {
        // 마커나 노드 데이터가 없으면 즉시 종료
        if (playerMarker == null || _nodes == null) return;
        if (!_nodes.TryGetValue((floor, index), out var node)) return;

        var markerRect = playerMarker as RectTransform;
        var nodeTransform = node.transform;

        // 1. 마커가 UI 오브젝트일 경우 (가장 흔한 케이스)
        if (markerRect != null)
        {
            // 마커가 속한 캔버스와 렌더링용 카메라를 찾습니다.
            var canvas = markerRect.GetComponentInParent<Canvas>();
            Camera cam = (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) ? canvas.worldCamera : null;

            // 노드의 월드 좌표를 화면 좌표로 변환합니다.
            Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(cam, nodeTransform.position);

            // 변환된 화면 좌표를 마커의 부모 UI 기준 로컬 좌표(anchoredPosition)로 다시 변환합니다.
            var parentRect = markerRect.parent as RectTransform;
            if (parentRect != null &&
                RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, cam, out var localPoint))
            {
                markerRect.anchoredPosition = localPoint;
            }
            else
            {
                // 변환이 실패하면 최후의 수단으로 월드 좌표라도 맞춰줍니다.
                markerRect.position = nodeTransform.position;
            }

            // (선택사항) 마커가 다른 UI에 가려지지 않도록 맨 위로 올립니다.
            markerRect.SetAsLastSibling();
        }
        // 2. 마커가 UI가 아닌 일반 3D/2D 오브젝트일 경우
        else
        {
            // 간단하게 월드 좌표를 그대로 복사합니다.
            playerMarker.position = nodeTransform.position;
        }
    }

    void UpdateReachable(int floor, int index)
    {
        if (_nodes == null) return;

        // 1. 일단 지도 위의 모든 노드를 비활성화합니다.
        foreach (var node in _nodes.Values)
        {
            node.SetReachable(false);
        }

        // 2. 현재 내가 위치한 노드를 찾습니다.
        if (_nodes.TryGetValue((floor, index), out var curNode))
        {
            // 3. 다음 층으로 갈 수 있는 모든 자식 노드들을 활성화합니다.
            if (curNode.children != null)
            {
                foreach (var child in curNode.children)
                {
                    child.SetReachable(true);
                }
            }

            // 4. [예외 규칙] 만약 현재 노드가 '상점'이라면, 자기 자신도 활성화합니다.
            // 이렇게 하면 닫았던 상점 문을 다시 열 수 있습니다.
            if (curNode.nodeType == NodeType.Shop /* && !curNode.IsCleared */)
            {
                curNode.SetReachable(true);
            }
        }
    }
}