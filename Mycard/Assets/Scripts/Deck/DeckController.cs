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

public class DeckController : MonoBehaviour
{
    public static DeckController instance;

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
    public void ShuffleDeckToUse()
    {
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
    public void SaveDeck()
    {
        var data = new DeckSaveData(); //DeckSaveData()로 Id배열 저장
        foreach (var so in deckToUse)
        {
            if (so == null) continue;
            if (string.IsNullOrEmpty(so.CardId))
            {
                Debug.LogWarning($"[DeckController1] 저장 스킵: Id가 비어있는 카드: {so.name}");
                continue;
            }
            data.ids.Add(so.CardId);
        }
        // file save
        string Decklist = JsonUtility.ToJson(data); //Json직렬화 유저 데이터 저장

        //File.WriteAllText(Application.dataPath + "/DeckData.json", JsonUtility.ToJson(data)); //이 코드를 쓰면 DeckData.json이 저장된다 Application.dataPath는 작업폴더
        PlayerPrefs.SetString(PlayerPrefsKey, Decklist); //PlayerPrefs는 간단한 데이터를 로컬로 저장할때 쓰인다 PlayerPrefs를 문자형태로 변경 
        //49번 줄의 deck_1을  뜻함 PlayerPrefs은 <Key Value>로 데이터를 저장하는 클래스이다. Key값은 string이며, Key는 Value를 찾기 위한 식별자를 의미한다.
        PlayerPrefs.Save(); //수정된 모든 preferences를 파일에 저장한다.
        Debug.Log($"[DeckController1] 덱 저장 완료. 카드 수: {data.ids.Count}");
    }
    public bool LoadDeck(bool rebuildDrawPile = true)
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsKey)) return false; //PlayerPrefs가 Key가 존재하는지 확인한다.
        // file load 
        string Decklist = PlayerPrefs.GetString(PlayerPrefsKey);
        var data = JsonUtility.FromJson<DeckSaveData>(Decklist);
        if (data == null || data.ids == null) return false; //데이터와 데이터 id가 없다면 실패

        deckToUse.Clear();
        foreach (var id in data.ids)
        {
            if (dbById.TryGetValue(id, out var so))
            {
                deckToUse.Add(so);
            }
            else
            {
                Debug.LogWarning($"[DeckController1] 로드 실패: 알 수 없는 Id {id}");
            }
        }
        NotifyChanged();
        if (rebuildDrawPile) RebuildDrawPile(true);
        Debug.Log($"[DeckController1] 덱 로드 완료. 카드 수: {deckToUse.Count}");
        return true;
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

//DeckController.instance.AddCardToDeckById("1", 1);//id 문자열임 id로 호출해서 갯수만큼추가
//DeckController.Instance.RemoveCardById("1", 1); //id문자열로 id로 호출해서 갯수만큼 삭제
//DeckController.Instance.SaveDeck();//저장
//DeckController.Instance.LoadDeck();//불러오기
//DeckController.Instance.Shuffle();
//DeckController.Instance.AddCard(fireballSO, 2);// 비추천 so명칭으로 불러오기

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
