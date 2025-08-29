using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;



//�߰�1
[Serializable]
public class DeckSaveData
{
    public List<string> ids = new List<string>(); // deckToUse�� Id �迭�� ����
}

public class DeckController : MonoBehaviour
{
    public static DeckController instance;

    private void Awake()
    {
        instance = this;
    }
    [Header("���� ��� ��(�ν�����/�ڵ�� ����)")]
    public List<CardScriptableObject> deckToUse = new List<CardScriptableObject>();
    
    [Header("��ο�� Ȱ�� ī��(���� ���)")]
    private List<CardScriptableObject> activeCards = new List<CardScriptableObject>();
    
    
    [Header("ī�� ���� ��Ģ")]
    public Card cardToSpawn;
    public int drawCardCost = 2;
    public float waitBetweenDrawingCards = .25f;
    //public int maxDeckSize = 60;         // �ʿ� ������ -1
    //public int maxCopiesPerCard = 4;     // �ʿ� ������ -1

    //�߰�2
    [Header("ī�� DB(��ü ī�� ��� ��� ����)")]
    [Tooltip("Id��SO ��Ī�� ��ü ī�� DB. ����� ���������� ID�� �߰��Ϸ��� ���⿡ ����ؾ� ��������.")]
    public List<CardScriptableObject> cardDatabase = new List<CardScriptableObject>();
    // ���� ��ȸ��: Id -> SO
    private Dictionary<string, CardScriptableObject> dbById = new Dictionary<string, CardScriptableObject>();
    
    //�߰�3
    // �� ���� �̺�Ʈ(UI/�ڵ�/ī���� �� ���� ��)
    public event Action<IReadOnlyList<CardScriptableObject>> OnDeckChanged;
    //�߰�4
    private const string PlayerPrefsKey = "deck_1";

    void Start()
    {
        BuildIndex();
        SetupDeck();
        NotifyChanged();
        /* �� ��Ʈ�ѷ��� �Ҹ��� ���̺� �Ȱ� ȣ�� �� �� ����� ���� ������ ȣ��ǰ� �״�� ����� amloader���� ����̸� �ʿ����
            bool loaded = LoadDeck(true); // ���⼭ '�ε� �õ�'�� ������ �����

            if (!loaded) {
            SetupDeck();     // ���庻�� ���ų� �����ϸ�
            NotifyChanged(); // �⺻ ���� + UI ����
            }
        //������ ���� ����        
        
            BuildIndex();                 // Id -> SO ���� ���� ����
            if (!LoadDeck(true))          // ���庻�� ������ �ε� + ��ο� ���� �籸��
            {
            SetupDeck();              // ���庻�� ������ ���� ������� ����
            NotifyChanged();          // UI/ī���� ����
            }
        
         
         */
    }
    /*
     void Awake() �̷��� ������������ �����ȴ� �� �̰� �� ��� �ٸ������� ����Ʈ�ѷ��� ���־��Ѵ� �� �̰� ȣ���ϴ� ��찡 ù��° ��Ʋ�ΰ����
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject); // �� ������Ʈ�� ���� ������ �״�� ������
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
        tempDeck.AddRange(deckToUse); //����Ʈ �迭 �߰�

        int interations = 0;
        while (tempDeck.Count > 0 && interations < 500)
        {
            int selected = UnityEngine.Random.Range(0, tempDeck.Count); //���� ������ system�� unityengine������ ��ȣ�� �־ ����
            activeCards.Add(tempDeck[selected]);
            tempDeck.RemoveAt(selected); //���õ��� ���� activecard ���� �ٿ��ش�.
            interations++; //����
        }
    }
    public void RebuildDrawPile(bool shuffle = true) //ī�� �߰��� �籸��
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

    public void DrawCardForMana() //��ο� ī���� �ڽ�Ʈ ����
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
    //   �� ���ۿ� �ű� �ڵ� �߰�
    // =========================

    // �� �б� ���� ��
    public IReadOnlyList<CardScriptableObject> CurrentDeck => deckToUse;
    public IReadOnlyList<CardScriptableObject> CurrentDrawPile => activeCards;

    public bool AddCardToDeck(CardScriptableObject so, int count = 1, bool rebuildDrawPile = true)
    {
        //���� �����̳� ���������� �Ѷ� ����Ͻÿ�
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
            Debug.LogWarning($"[DeckController1] �� �� ���� ī�� Id: {id}");
            return false;
        }
        return AddCardToDeck(so, count, rebuildDrawpile);
    }
    //card so �̸����� ���� �ڿ��� ���� ����
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
    //ID�� ����
    public int RemoveCardFromDeckById(string id, int count = 1, bool rebuildDrawPile = true) 
    {
        if (string.IsNullOrEmpty(id)) return 0;
        if (!dbById.TryGetValue(id, out var so))
        {
            Debug.LogWarning($"[DeckController1] �� �� ���� ī�� Id: {id}");
            return 0;
        }
        return RemoveCardFromDeck(so, count, rebuildDrawPile);
    }

    public void ClearDeck(bool rebuildDrawPile = true) //�� ����
    {
        deckToUse.Clear();

        NotifyChanged();
        if (rebuildDrawPile) RebuildDrawPile(true);
    }
    public void ShuffleDeckToUse()
    {
        // �� ��ü�� ���� ���� ��
        System.Random RandomDeck = new System.Random();
        for (int i = deckToUse.Count - 1; i > 0; i--)
        {
            int j = RandomDeck.Next(i + 1);
            (deckToUse[i], deckToUse[j]) = (deckToUse[j], deckToUse[i]);
        }
        NotifyChanged();
        RebuildDrawPile(true);
    }
    public int CountOfInDeck(CardScriptableObject so) //ī����ڸ� ����
    {
        int c = 0;
        foreach (var x in deckToUse) if (x == so) c++;
        return c;
    }
    public void SaveDeck()
    {
        var data = new DeckSaveData(); //DeckSaveData()�� Id�迭 ����
        foreach (var so in deckToUse)
        {
            if (so == null) continue;
            if (string.IsNullOrEmpty(so.Id))
            {
                Debug.LogWarning($"[DeckController1] ���� ��ŵ: Id�� ����ִ� ī��: {so.name}");
                continue;
            }
            data.ids.Add(so.Id);
        }
        // file save
        string Decklist = JsonUtility.ToJson(data); //Json����ȭ ���� ������ ����

        //File.WriteAllText(Application.dataPath + "/DeckData.json", JsonUtility.ToJson(data)); //�� �ڵ带 ���� DeckData.json�� ����ȴ� Application.dataPath�� �۾�����
        PlayerPrefs.SetString(PlayerPrefsKey, Decklist); //PlayerPrefs�� ������ �����͸� ���÷� �����Ҷ� ���δ� PlayerPrefs�� �������·� ���� 
        //49�� ���� deck_1��  ���� PlayerPrefs�� <Key Value>�� �����͸� �����ϴ� Ŭ�����̴�. Key���� string�̸�, Key�� Value�� ã�� ���� �ĺ��ڸ� �ǹ��Ѵ�.
        PlayerPrefs.Save(); //������ ��� preferences�� ���Ͽ� �����Ѵ�.
        Debug.Log($"[DeckController1] �� ���� �Ϸ�. ī�� ��: {data.ids.Count}");
    }
    public bool LoadDeck(bool rebuildDrawPile = true)
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsKey)) return false; //PlayerPrefs�� Key�� �����ϴ��� Ȯ���Ѵ�.
        // file load 
        string Decklist = PlayerPrefs.GetString(PlayerPrefsKey);
        var data = JsonUtility.FromJson<DeckSaveData>(Decklist);
        if (data == null || data.ids == null) return false; //�����Ϳ� ������ id�� ���ٸ� ����

        deckToUse.Clear();
        foreach (var id in data.ids)
        {
            if (dbById.TryGetValue(id, out var so))
            {
                deckToUse.Add(so);
            }
            else
            {
                Debug.LogWarning($"[DeckController1] �ε� ����: �� �� ���� Id {id}");
            }
        }
        NotifyChanged();
        if (rebuildDrawPile) RebuildDrawPile(true);
        Debug.Log($"[DeckController1] �� �ε� �Ϸ�. ī�� ��: {deckToUse.Count}");
        return true;
    }

    private void BuildIndex()
    {
        dbById.Clear();

        // 1) cardDatabase�� ��ϵ� ��ü ī�� DB�ε���
        foreach (var so in cardDatabase)
            TryIndex(so);

        // 2) deckToUse�� �̹� ����ִ� ī��(�����Ϳ��� �巡���ص� ��)�� �ε���
        foreach (var so in deckToUse)
            TryIndex(so);
    }


    private void TryIndex(CardScriptableObject so)
    {
        if (so == null) return;
        if (string.IsNullOrEmpty(so.Id))
        {
            Debug.LogWarning($"[DeckController] ī�忡 Id�� ����ֽ��ϴ�: {so.name}");
            return;
        }
        dbById[so.Id] = so; // ������ ��� �켱
    }

    private void NotifyChanged() //deck�� �������� �˸��� �ڵ�
    {
        OnDeckChanged?.Invoke(deckToUse);
        // �ʿ�� ���⼭ UI/�ڵ�/ī���� ���� ���� Ʈ���� ����
        // UIController.instance?.RefreshDeckList(deckToUse);
        // HandController.instance?.OnDeckChanged(deckToUse); // �̷� �޼��尡 �ִٸ� ���⼭ ȣ��ȴ�
    }

}

//DeckController.instance.AddCardToDeckById("1", 1);//id ���ڿ��� id�� ȣ���ؼ� ������ŭ�߰�
//DeckController.Instance.RemoveCardById("1", 1); //id���ڿ��� id�� ȣ���ؼ� ������ŭ ����
//DeckController.Instance.SaveDeck();//����
//DeckController.Instance.LoadDeck();//�ҷ�����
//DeckController.Instance.Shuffle();
//DeckController.Instance.AddCard(fireballSO, 2);// ����õ so��Ī���� �ҷ�����

//
/* ����
 
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
            tempDeck.RemoveAt(selected); //���õ��� ���� activecard ���� �ٿ��ش�.
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

    public void DrawCardForMana() //��ο� ī���� �ڽ�Ʈ ����
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
