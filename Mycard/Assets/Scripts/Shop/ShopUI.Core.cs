using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public partial class ShopUI : MonoBehaviour
{
    [Header("Root & Window")]
    [SerializeField] private CanvasGroup panel;      // ShopUI 루트(CanvasGroup)
    [SerializeField] private RectTransform window;   // 팝업 창(Scale 애니용)
    [SerializeField] private Button dimmerButton;    // 바깥 클릭 닫기
    [SerializeField] private Button closeButton;     // 닫기 버튼

    [Header("M2 Grid")]
    [SerializeField] private Transform gridParent;      // Window/Body/Grid
    [SerializeField] private ShopSlotView slotPrefab;   // ShopSlotView 프리팹

    [Header("Topbar")]
    [SerializeField] private TMP_Text goldText;   // 상단 골드 표시
    [SerializeField] private int testGold = 300;  // 테스트용 시작 골드
    [SerializeField] private TMP_Text rerollPriceText; // 리롤 가격
    [SerializeField] private Button rerollButton;      // 리롤 버튼

    [Header("Reroll Economy")]
    [SerializeField] private int baseReroll = 30;      // 기본 리롤 비용
    [SerializeField] private float rerollGrowth = 1.2f;// 리롤시 매번 가격 20% 증가

    [Header("Deals (오늘의 특가)")]
    [SerializeField, Range(0f,1f)] private float dealChance = 0.25f; // 아이템별 특가 확률
    [SerializeField] private int maxDeals = 2;                         // 이번 상점 최대 특가 수
    [SerializeField] private float dealDiscount = 0.20f;               // 20% 할인

    private const string CardsPath = "Cards"; // Assets/Resources/Cards/*.asset

    [Header("Card Sources")]
    [SerializeField] private List<CardScriptableObject> cardPool = new List<CardScriptableObject>();
    private CardScriptableObject[] _cardSources = new CardScriptableObject[3];

    [Header("Reroll (Cooldown)")]
    [SerializeField, Tooltip("Seconds to lock reroll button after a reroll")]
    private float rerollCooldownSec = 0.20f;   // 리롤 쿨타임
    private bool _isRerollCooling = false;     // 버튼 락 상태


    [SerializeField] private bool verboseLogs = false; //디버그 활성화

    // 상점 진입중 확인용 (노드에 있는동안 다시 열수 있게)
    private bool _sessionInitialized = false;
    

    // 내부 상태
    private readonly List<ShopSlotView> _views = new();
    private List<ShopSlotVM> _dummy;
    private Coroutine _animCo;
    private bool _isOpen = false;
    public bool IsOpen => _isOpen;
    private int _rerollCount;

    private const float OpenDur = 0.18f;   // 페이드/스케일 시간
    private const float CloseDur = 0.16f;
    private const float ScaleFrom = 0.92f; // 팝업 열릴 때 시작 스케일

    // (유물/소모품 풀 – 계속 쓰면 유지)
    private static readonly string[] RelicsPool = {
        "Happy Flower","Anchor","Bronze Idol","Bag of Prep","Kunai","Incense Burner"
    };
    private static readonly string[] ConsumablesPool = {
        "Block Potion","Strength Potion","Dex Potion","Energy Tonic","Small Potion"
    };

        #region --- 데이터 포장/개봉 (DTO) ---



    // '포장 기술' (ShopUI의 현재 상태를 -> 택배 상자에 담기)
    public ShopSessionDTO ExportSession()
    {
        var dto = new ShopSessionDTO
        {
            rerollCount = _rerollCount,
            slots = new SlotDTO[_dummy.Count]
        };
        for (int i = 0; i < _dummy.Count; i++)
        {
            var vm = _dummy[i];
            dto.slots[i] = new SlotDTO {
                itemId = vm.cardData?.cardId ?? vm.title, // 안정적인 cardId 사용
                soldOut = vm.soldOut
            };
        }
        return dto;
    }

    // '개봉 기술' (택배 상자를 열어서 -> ShopUI의 상태를 복원)
    public void ImportSession(ShopSessionDTO dto)
    {
        _rerollCount = dto.rerollCount;
        // ... (이 부분은 Phase 6에서 DB 데이터를 실제로 불러올 때 최종 완성됩니다) ...
        _sessionInitialized = true;
    }

    #endregion

    #region --- 지갑 연동 ---

    // 외부에서 진짜 지갑의 기능을 연결해줄 통로 (Delegate)
    public System.Func<int> GetGold;
    public System.Action<int> SpendGold;
    public System.Action OnSessionChanged; // 구매/리롤 시 "상태 바뀌었음!"이라고 알리는 신호

    // 기존 Gold 프로퍼티를 아래와 같이 수정
    private int Gold
    {
        get => GetGold != null ? GetGold() : testGold;
    }

    #endregion

    private void Awake()
    {
        VLog("[ShopUI] Awake running");
        LoadAllCardData();
        if (dimmerButton) dimmerButton.onClick.AddListener(Close);
        if (closeButton)  closeButton.onClick.AddListener(Close);
        if (rerollButton) rerollButton.onClick.AddListener(OnReroll);
        HideImmediate();
    }

    /// <summary>
    /// verboseLogs가 true일 때만 Debug.Log를 출력하는 헬퍼 함수입니다.
    /// </summary>
    /// <param name="message">출력할 메시지</param>
    private void VLog(string message)
    {
        // 스위치가 꺼져있으면(false) 여기서 즉시 함수가 종료됩니다.
        if (!verboseLogs) return;

        // 스위치가 켜져있을 때만 아래 코드가 실행됩니다.
        Debug.Log(message, this);
    }

    // 노드에서 떠나면 진입중에서 벗어남
    public void ResetSession()
    {
        _sessionInitialized = false;
    }


}