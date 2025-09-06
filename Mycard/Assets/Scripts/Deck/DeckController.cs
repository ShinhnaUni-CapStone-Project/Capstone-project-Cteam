using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



//추가1
[Serializable]
public class DeckSaveData
{
    public List<string> ids = new List<string>(); // deckToUse를 Id 배열로 저장
}

[Obsolete("DeckController는 레거시입니다. IDeckService를 사용하세요.", true)]
public class DeckController : MonoBehaviour
{
    public static DeckController instance;

    // --- 서비스 참조 (Phase 2: 주입 예정) ---
    private IDeckService _deckService;
    private ICardCatalog _cardCatalog;
    private bool _isInitialized = false;

    private void Awake()
    {
        instance = this;
        //잘작동함
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject); // 이 오브젝트를 다음 씬으로 그대로 가져감

    }
    [Header("현재 사용 덱(인스펙터/코드로 편집)")]
    public List<CardScriptableObject> deckToUse = new List<CardScriptableObject>();
    
    [Header("드로우용 활성 카드(셔플 결과)")]
    private List<CardScriptableObject> activeCards = new List<CardScriptableObject>();
    
    
    [Header("카드 생성 규칙")]
    public Card cardToSpawn;
    public int drawCardCost = 2;
    public float waitBetweenDrawingCards = .25f;
    //public int maxDeckSize = 60;         // 필요 없으면 -1
    //public int maxCopiesPerCard = 4;     // 필요 없으면 -1

    //추가2
    [Header("카드 DB(전체 카드 목록 등록 권장)")]
    [Tooltip("Id→SO 매칭용 전체 카드 DB. 비워도 동작하지만 ID로 추가하려면 여기에 등록해야 안정적임.")]
    public List<CardScriptableObject> cardDatabase = new List<CardScriptableObject>();
    // 빠른 조회용: Id -> SO
    private Dictionary<string, CardScriptableObject> dbById = new Dictionary<string, CardScriptableObject>();
    
    //추가3
    // 덱 변경 이벤트(UI/핸드/카운터 등 갱신 훅)
    public event Action<IReadOnlyList<CardScriptableObject>> OnDeckChanged;
    //추가4
    private const string PlayerPrefsKey = "deck_1";

    void Start()
    {
        BuildIndex();
        SetupDeck();
        NotifyChanged();
        /* 덱 컨트롤러가 불릴때 세이브 된걸 호출 단 이 방식은 이전 씬에서 호출되고 그대로 들고가는 amloader같은 방식이면 필요없음
            bool loaded = LoadDeck(true); // 여기서 '로드 시도'가 실제로 실행됨

            if (!loaded) {
            SetupDeck();     // 저장본이 없거나 실패하면
            NotifyChanged(); // 기본 셔플 + UI 갱신
            }
        //위에와 같은 예시        
        
            BuildIndex();                 // Id -> SO 매핑 먼저 구축
            if (!LoadDeck(true))          // 저장본이 있으면 로드 + 드로우 더미 재구성
            {
            SetupDeck();              // 저장본이 없으면 기존 방식으로 셔플
            NotifyChanged();          // UI/카운터 갱신
            }
        
         
         */
    }
    /*
     void Awake() //이러면 다음씬에서도 유지된다 단 이걸 쓸 경우 다른씬에서 덱컨트롤러를 없애야한다 즉 이걸 호출하는 경우가 첫번째 배틀인경우임
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject); // 이 오브젝트를 다음 씬으로 그대로 가져감
    }
     
     
     */

    // Update is called once per frame
    void Update()
    {

    }

    public void SetupDeck()
    {
        activeCards.Clear();

        List<CardScriptableObject> tempDeck = new List<CardScriptableObject>(); // a = temp temp = b a=b 
        tempDeck.AddRange(deckToUse); //리스트 배열 추가

        int interations = 0;
        while (tempDeck.Count > 0 && interations < 500)
        {
            int selected = UnityEngine.Random.Range(0, tempDeck.Count); //랜덤 변수가 system과 unityengine사이의 모호성 있어서 변경
            activeCards.Add(tempDeck[selected]);
            tempDeck.RemoveAt(selected); //선택되지 않은 activecard 값을 줄여준다.
            interations++; //증가
        }
    }

    /// <summary>
    /// BattleSceneBootstrap에서 호출해 서비스를 주입합니다. 이 메서드가 호출되기 전에는 동작하지 않아야 합니다.
    /// </summary>
    public void Initialize(IDeckService deckService, ICardCatalog cardCatalog)
    {
        _deckService = deckService ?? throw new ArgumentNullException(nameof(deckService));
        _cardCatalog = cardCatalog ?? throw new ArgumentNullException(nameof(cardCatalog));
        _isInitialized = true;
        // 이벤트 구독은 후속 단계에서 OnEnable/OnDisable 등으로 이전
    }

    private void RequireInitialization()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("DeckController가 초기화되지 않았습니다. BattleSceneBootstrap에서 Initialize()를 호출해야 합니다.");
    }
    public void RebuildDrawPile(bool shuffle = true) //카드 추가시 재구성
    {
        if (shuffle)
        {
            SetupDeck();
        }
        else
        {
            activeCards.Clear();
            activeCards.AddRange(deckToUse);
        }
    }


    [Obsolete("카드 드로우는 IDeckService.DrawCards()를 사용하세요.", false)]
    public void DrawCardToHand()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[DEPRECATED] DeckController.DrawCardToHand() 레거시 경로 호출. 서비스 전환 전 임시로 동작합니다. Stack: {Environment.StackTrace}");
#endif
        if (activeCards.Count == 0)
        {
            SetupDeck();
        }
        Card newCard = Instantiate(cardToSpawn, transform.position, transform.rotation);
        newCard.cardSO = activeCards[0];
        newCard.SetupCard();

        activeCards.RemoveAt(0);

        HandController.instance.AddCardToHand(newCard);
        GameEvents.OnCardDrawn?.Invoke(newCard);      // +++ 추가 카드 뽑을 때
        AudioManager.instance.PlaySFX(3); //sfx3
    }

    [Obsolete("카드 드로우는 IDeckService.DrawCards()를 사용하세요.", false)]
    public void DrawCardForMana() //드로우 카드의 코스트 기제
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[DEPRECATED] DeckController.DrawCardForMana() 레거시 경로 호출. 서비스 전환 전 임시로 동작합니다. Stack: {Environment.StackTrace}");
#endif
        if (BattleController.instance.playerMana >= drawCardCost)
        {
            DrawCardToHand();
            BattleController.instance.SpendPlayerMana(drawCardCost);

        }
        else
        {
            UIController.instance.ShowManaWarning();
            UIController.instance.drawCardButton.SetActive(false);
        }
    }
    [Obsolete("카드 드로우는 IDeckService.DrawCards()를 사용하세요.", false)]
    public void DrawMulitpleCards(int amountToDraw)
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[DEPRECATED] DeckController.DrawMulitpleCards({amountToDraw}) 레거시 경로 호출. 서비스 전환 전 임시로 동작합니다. Stack: {Environment.StackTrace}");
#endif
       StartCoroutine(DrawMultipleCo(amountToDraw));
    }

    [Obsolete("레거시 코루틴은 사용하지 않습니다.", false)]
    IEnumerator DrawMultipleCo(int amountToDraw)
    {
        for (int i = 0; i < amountToDraw; i++)
        {
            DrawCardToHand();

            yield return new WaitForSeconds(waitBetweenDrawingCards);
        }
    }

    // =========================
    //   덱 조작용 신규 코드 추가
    // =========================

    // 덱 읽기 전용 뷰
    public IReadOnlyList<CardScriptableObject> CurrentDeck => deckToUse;
    public IReadOnlyList<CardScriptableObject> CurrentDrawPile => activeCards;

    public bool AddCardToDeck(CardScriptableObject so, int count = 1, bool rebuildDrawPile = true)
    {
        //갯수 제한이나 복사제한을 둘때 사용하시오
        /*
            if (maxDeckSize > 0 && deckToUse.Count + count > maxDeckSize) return false;

            if (maxCopiesPerCard > 0)
            {
                int now = CountOfInDeck(so);
                if (now + count > maxCopiesPerCard) return false;
            }
         
         */


        if (so == null || count <= 0) return false;
        for (int i = 0; i < count; i++)
        {
            deckToUse.Add(so);
        }
        NotifyChanged();

        if (rebuildDrawPile)
        {
            RebuildDrawPile(true);
        }
        return true;
    }

    public bool AddCardToDeckById(string id, int count = 1, bool rebuildDrawpile = true)
    {
        if (string.IsNullOrEmpty(id)) return false;

        if (!dbById.TryGetValue(id, out var so))
        {
            Debug.LogWarning($"[DeckController1] 알 수 없는 카드 Id: {id}");
            return false;
        }
        return AddCardToDeck(so, count, rebuildDrawpile);
    }
    //card so 이름으로 삭제 뒤에서 부터 제거
    public int RemoveCardFromDeck(CardScriptableObject so, int count = 1, bool rebuildDrawPile = true)
    {
        if (so == null || count <= 0) return 0;
        int removed = 0;
        for (int i = deckToUse.Count - 1; i >= 0 && removed < count; i--)
        {
            if (deckToUse[i] == so)
            {
                deckToUse.RemoveAt(i);
                removed++;
            }
        }
        if (removed > 0)
        {
            NotifyChanged();
            if (rebuildDrawPile) RebuildDrawPile(true);
        }
        return removed;
    }
    //ID로 삭제
    public int RemoveCardFromDeckById(string id, int count = 1, bool rebuildDrawPile = true) 
    {
        if (string.IsNullOrEmpty(id)) return 0;
        if (!dbById.TryGetValue(id, out var so))
        {
            Debug.LogWarning($"[DeckController1] 알 수 없는 카드 Id: {id}");
            return 0;
        }
        return RemoveCardFromDeck(so, count, rebuildDrawPile);
    }

    public void ClearDeck(bool rebuildDrawPile = true) //덱 비우기
    {
        deckToUse.Clear();

        NotifyChanged();
        if (rebuildDrawPile) RebuildDrawPile(true);
    }
    [Obsolete("셔플은 IDeckService가 자동으로 처리합니다.", false)]
    public void ShuffleDeckToUse()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[DEPRECATED] DeckController.ShuffleDeckToUse() 레거시 경로 호출. 서비스 전환 전 임시로 동작합니다. Stack: {Environment.StackTrace}");
#endif
        // 덱 자체를 섞고 싶을 때
        System.Random RandomDeck = new System.Random();
        for (int i = deckToUse.Count - 1; i > 0; i--)
        {
            int j = RandomDeck.Next(i + 1);
            (deckToUse[i], deckToUse[j]) = (deckToUse[j], deckToUse[i]);
        }
        NotifyChanged();
        RebuildDrawPile(true);
    }
    public int CountOfInDeck(CardScriptableObject so) //카드숫자를 세기
    {
        int c = 0;
        foreach (var x in deckToUse) if (x == so) c++;
        return c;
    }
    [Obsolete("덱 저장은 IDeckService/DB가 자동으로 처리합니다.", false)]
    public void SaveDeck()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[DEPRECATED] DeckController.SaveDeck() 호출은 무시됩니다. Stack: {Environment.StackTrace}");
#endif
    }
    [Obsolete("덱 로드는 IDeckService.LoadAndPrepareDeck()으로 대체되었습니다.", false)]
    public bool LoadDeck(bool rebuildDrawPile = true)
    {
        // 레거시 호출 차단
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        Debug.LogWarning($"[DEPRECATED] DeckController.LoadDeck() 호출은 무시됩니다. Stack: {Environment.StackTrace}");
#endif
        return false;
    }

    private void BuildIndex()
    {
        dbById.Clear();

        // 1) cardDatabase에 등록된 전체 카드 DB인덱싱
        foreach (var so in cardDatabase)
            TryIndex(so);

        // 2) deckToUse에 이미 들어있는 카드(에디터에서 드래그해둔 것)도 인덱싱
        foreach (var so in deckToUse)
            TryIndex(so);
    }


    private void TryIndex(CardScriptableObject so)
    {
        if (so == null) return;
        if (string.IsNullOrEmpty(so.CardId))
        {
            Debug.LogWarning($"[DeckController] 카드에 Id가 비어있습니다: {so.name}");
            return;
        }
        dbById[so.CardId] = so; // 마지막 등록 우선
    }

    private void NotifyChanged() //deck이 변했음을 알리는 코드
    {
        OnDeckChanged?.Invoke(deckToUse);
        // 필요시 여기서 UI/핸드/카운터 직접 갱신 트리거 가능
        // UIController.instance?.RefreshDeckList(deckToUse);
        // HandController.instance?.OnDeckChanged(deckToUse); // 이런 메서드가 있다면 여기서 호출된다
    }

}

// (레거시 예제 코드 제거됨)

//
/* 원본
 
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckController : MonoBehaviour
{
    public static DeckController instance;

    private void Awake()
    {
        instance = this;
    }

    public List<CardScriptableObject> deckToUse = new List<CardScriptableObject>();
    private List<CardScriptableObject> activeCards = new List<CardScriptableObject>();
    public Card cardToSpawn;
    public int drawCardCost = 2;
    public float waitBetweenDrawingCards = .25f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetupDeck();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetupDeck()
    {
        activeCards.Clear();

        List<CardScriptableObject> tempDeck = new List<CardScriptableObject>();
        tempDeck.AddRange(deckToUse);
        int interations = 0;
        while (tempDeck.Count > 0 && interations < 500)
        {
            int selected = Random.Range(0, tempDeck.Count);
            activeCards.Add(tempDeck[selected]);
            tempDeck.RemoveAt(selected); //선택되지 않은 activecard 값을 줄여준다.
            interations++;
        }
    }

    public void DrawCardToHand()
    {
        if (activeCards.Count == 0)
        {
            SetupDeck();
        }
        Card newCard = Instantiate(cardToSpawn, transform.position, transform.rotation);
        newCard.cardSO = activeCards[0];
        newCard.SetupCard();

        activeCards.RemoveAt(0);

        HandController.instance.AddCardToHand(newCard);

        AudioManager.instance.PlaySFX(3); //sfx3
    }

    public void DrawCardForMana() //드로우 카드의 코스트 기제
    {
        if (BattleController.instance.playerMana >= drawCardCost)
        {
            DrawCardToHand();
            BattleController.instance.SpendPlayerMana(drawCardCost);

        }
        else
        {
            UIController.instance.ShowManaWarning();
            UIController.instance.drawCardButton.SetActive(false);
        }
    }
    public void DrawMulitpleCards(int amountToDraw)
    {
       StartCoroutine(DrawMultipleCo(amountToDraw));
    }

    IEnumerator DrawMultipleCo(int amountToDraw)
    {
        for (int i = 0; i < amountToDraw; i++)
        {
            DrawCardToHand();

            yield return new WaitForSeconds(waitBetweenDrawingCards);
        }
    }
}
 
 */
