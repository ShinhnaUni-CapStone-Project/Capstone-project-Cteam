using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))] // 이 스크립트는 Button이 필수
[RequireComponent(typeof(NodeGoScene))] // 자신의 주소를 알기 위해 필수
public class ShopNodeOpenShop : MonoBehaviour
{
    // 총괄 매니저를 연결할 변수
    [SerializeField] private ShopOverlayController overlay;
    private NodeGoScene _nodeInfo; // 자신의 주소 정보를 담을 변수

    void Awake()
    {
        // 만약 인스펙터에서 overlay 연결을 잊었다면, 씬에서 직접 찾습니다.
        if (overlay == null)
        {
            overlay = FindObjectOfType<ShopOverlayController>(true);
        }
        _nodeInfo = GetComponent<NodeGoScene>();

        // 이 오브젝트의 버튼을 가져옵니다.
        var btn = GetComponent<Button>();
        // 기존에 연결된 모든 기능을 지우고,
        btn.onClick.RemoveAllListeners();
        // "클릭되면 overlay의 OpenOverlay 함수를 호출하라"는 새 기능을 연결합니다.
        btn.onClick.AddListener(() => {
            if (overlay != null)
            {
                overlay.OpenForNode(_nodeInfo.floor, _nodeInfo.index);
            }
        });
    }
}