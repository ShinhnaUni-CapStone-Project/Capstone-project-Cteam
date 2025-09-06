using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Save DTOs
[Serializable]
public class RelicSaveEntry
{
    public string id;
    public int stacks;
}
[Serializable]
public class RelicSaveData
{
    public List<RelicSaveEntry> entries = new();
}
#endregion

public class RelicSystem : MonoBehaviour
{
    public static RelicSystem Instance { get; private set; }

    [SerializeField] private RelicsUI relicsUI;   // �ɼ�(UI ǥ��)

    [Header("Relic DB (Id �� SO ���ο�)")]
    [Tooltip("���⿡ ��� ������ ��� RelicData ������ ����ϼ���.")]
    public List<RelicData> relicDatabase = new List<RelicData>();

    // ���� ��ȸ��: id -> SO
    private readonly Dictionary<string, RelicData> dbById = new();

    // ��Ÿ�� ���� ����(���� id�� �������� ��������)
    private readonly List<Relic> relics = new();

    // (����) ���� ��ȭ �˸�: ī�� ǥ�� ���� � ���
    public event Action RelicsChanged; //�߰�+++
    private void FireRelicsChanged() => RelicsChanged?.Invoke(); //�߰�+++

    private const string PlayerPrefsKey = "relics_1";

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildIndex(); // DB �ε���
    }

    private void Start()
    {
        //LoadRelics();//�� ���� �� �ڵ����� �ҷ������
        //RelicSystem.Instance.LoadRelics(); //�������� �ҷ��� �Ÿ� �ƹ� �������� ȣ��.
    }

    public void AttachUI(RelicsUI ui)
    {
        relicsUI = ui;
        relicsUI.Refresh(relics); // �̹� ���� ������� UI�� ��� ������
    }

    private void OnEnable()
    {
        // �̺�Ʈ ����
        GameEvents.OnBattleStart += HandleBattleStart;
        GameEvents.OnBattleEnd += HandleBattleEnd;
        GameEvents.OnTurnStart += HandleTurnStart;
        GameEvents.OnTurnEnd += HandleTurnEnd;
        GameEvents.OnCardDrawn += HandleCardDrawn;
        GameEvents.OnCardPlayed += HandleCardPlayed;
        GameEvents.OnDamageDealt += HandleDamageDealt;

        // ü���� ������ ����(���� ���� ����ǵ� null ����)
        GameEvents.ModifyPlayerAttack += ChainModifyPlayerAttack;
        GameEvents.ModifyPlayerMana += ChainModifyPlayerMana;
    }

    private void OnDisable()
    {
        GameEvents.OnBattleStart -= HandleBattleStart;
        GameEvents.OnBattleEnd -= HandleBattleEnd;
        GameEvents.OnTurnStart -= HandleTurnStart;
        GameEvents.OnTurnEnd -= HandleTurnEnd;
        GameEvents.OnCardDrawn -= HandleCardDrawn;
        GameEvents.OnCardPlayed -= HandleCardPlayed;
        GameEvents.OnDamageDealt -= HandleDamageDealt;

        GameEvents.ModifyPlayerAttack -= ChainModifyPlayerAttack;
        GameEvents.ModifyPlayerMana -= ChainModifyPlayerMana;
    }

    #region DB & Factory
    private void BuildIndex()
    {
        dbById.Clear();
        foreach (var so in relicDatabase)
        {
            if (so == null || string.IsNullOrEmpty(so.relicId)) continue;
            dbById[so.relicId] = so; // ������ ��� �켱
        }
    }

    // id�� ���� �ùٸ� �Ļ� ���� Ŭ������ new �ؼ� ������
    private Relic CreateRelicFromId(string relicId, RelicData data)
    {
        // �ʿ��� �� �Ʒ� switch�� Ȯ���ϼ���.
        switch (relicId)
        {
            case "WarBanner": return new WarBannerRelic(data);//�����
            case "ManaBoostGem": return new ManaGem(data);//ManaGem�� �־����
            case "HappyFlower": return new HappyFlowerRelic(data);//
            // TODO: �� ������ �߰��� �� case �߰�
            default:
                Debug.LogWarning($"[RelicSystem] �� �� ���� relicId: {relicId}");
                return null;
        }
    }

    #endregion

    #region Public: ���� ID ��� �߰�/����

    public bool AddRelicById(string relicId, int stacks = 1, bool save = true)
    {
        if (string.IsNullOrEmpty(relicId)) return false;
        if (!dbById.TryGetValue(relicId, out var data))
        {
            Debug.LogWarning($"[RelicSystem] DB�� ���� relicId: {relicId}");
            return false;
        }

        // ���� id ���� ������ ���ø� �ø�
        var existing = relics.Find(r => r.Data.relicId == relicId);
        if (existing != null)
        {
            for (int k = 0; k < Mathf.Max(0, stacks); k++)
                existing.AddStack();

            relicsUI?.UpdateStacks(existing);
            FireRelicsChanged();
            if (save) SaveRelics();
            return true;
        }

        // ���� ����
        var relic = CreateRelicFromId(relicId, data);
        if (relic == null) return false;

        relics.Add(relic);
        relic.OnAdd();
        // stacks�� 1���� ũ�� �߰� ���� �ݿ�
        for (int k = 1; k < Mathf.Max(1, stacks); k++)
            relic.AddStack();

        relicsUI?.AddOrStack(relic);
        FireRelicsChanged();
        if (save) SaveRelics();
        return true;
    }
    public void RemoveRelic(string relicId, bool save = true)
    {
        int idx = relics.FindIndex(r => r.Data.relicId == relicId);
        if (idx < 0) return;

        relics[idx].OnRemove();
        relics.RemoveAt(idx);
        relicsUI?.Remove(relicId);
        FireRelicsChanged();
        if (save) SaveRelics();
    }

    public void ClearRelics(bool save = true)
    {
        foreach (var r in relics) r.OnRemove();
        relics.Clear();
        relicsUI?.Refresh(relics);
        FireRelicsChanged();
        if (save) SaveRelics();
    }

    public int CountStacks(string relicId)
    {
        var r = relics.Find(x => x.Data.relicId == relicId);
        return r != null ? r.Stacks : 0;
    }


    #endregion

    public void NotifyStackChanged(Relic r)
    {
        relicsUI?.UpdateStacks(r);
        FireRelicsChanged();
    }


    #region Public API
    public void AddRelic(Relic newRelic)
    {
        // ���� ID ���� ó��
        var existing = relics.Find(r => r.Data.relicId == newRelic.Data.relicId);
        if (existing != null && existing.Data.stackable)
        {
            existing.AddStack();
            relicsUI?.UpdateStacks(existing); // ���� �ؽ�Ʈ�� ����
            FireRelicsChanged();              // <- �߰�
            return;
        }

        relics.Add(newRelic);
        newRelic.OnAdd();
        relicsUI?.AddOrStack(newRelic);
        FireRelicsChanged();              // <- �߰�
    }
    /*
    public void RemoveRelic(string relicId)
    {
        var idx = relics.FindIndex(r => r.Data.relicId == relicId);
        if (idx >= 0)
        {
            relics[idx].OnRemove();
            relics.RemoveAt(idx);
            //relicsUI?.Refresh(relics);
            relicsUI?.Remove(relicId);
            FireRelicsChanged();              // <- �߰�
        }
    }*/
    #endregion

    #region Save / Load (PlayerPrefs + JSON)
    public void SaveRelics()
    {
        var data = new RelicSaveData();
        foreach (var r in relics)
        {
            if (r?.Data == null || string.IsNullOrEmpty(r.Data.relicId)) continue;
            data.entries.Add(new RelicSaveEntry { id = r.Data.relicId, stacks = Mathf.Max(1, r.Stacks) });
        }

        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString(PlayerPrefsKey, json);
        PlayerPrefs.Save();
        // DeckController�� ���� ���: PlayerPrefs + JsonUtility��turn2file9��DeckController.cs��L32-L51��
    }

    public bool LoadRelics(bool clearBeforeLoad = true)
    {
        if (!PlayerPrefs.HasKey(PlayerPrefsKey)) return false;

        string json = PlayerPrefs.GetString(PlayerPrefsKey);
        var data = JsonUtility.FromJson<RelicSaveData>(json);
        if (data == null || data.entries == null) return false;

        if (clearBeforeLoad) ClearRelics(false);

        foreach (var e in data.entries)
        {
            // DB�� ������ ��ŵ(���)
            if (!dbById.ContainsKey(e.id))
            {
                Debug.LogWarning($"[RelicSystem] �ε� ����: �� �� ���� relicId {e.id}");
                continue;
            }
            AddRelicById(e.id, Mathf.Max(1, e.stacks), save: false);
        }

        relicsUI?.Refresh(relics);
        FireRelicsChanged();
        return true;
    }
    #endregion

    #region Event Handlers
    private void HandleBattleStart()
    {
        foreach (var r in relics) r.OnBattleStart();
    }
    private void HandleBattleEnd()
    {
        foreach (var r in relics) r.OnBattleEnd();
    }
    private void HandleTurnStart(bool isPlayer)
    {
        foreach (var r in relics) r.OnTurnStart(isPlayer);
    }
    private void HandleTurnEnd(bool isPlayer)
    {
        foreach (var r in relics) r.OnTurnEnd(isPlayer);
    }
    private void HandleCardDrawn(Card c)
    {
        foreach (var r in relics) r.OnCardDrawn(c);
    }
    private void HandleCardPlayed(Card c)
    {
        foreach (var r in relics) r.OnCardPlayed(c);
    }
    private void HandleDamageDealt(int dmg, bool fromPlayer)
    {
        foreach (var r in relics) r.OnDamageDealt(dmg, fromPlayer);
    }

    private int ChainModifyPlayerAttack(int baseAttack)
    {
        int v = baseAttack;
        foreach (var r in relics) v = r.ModifyPlayerAttack(v);
        return v;
    }
    private int ChainModifyPlayerMana(int curMana)
    {
        int v = curMana;
        foreach (var r in relics) v = r.ModifyPlayerMana(v);
        return v;
    }
    #endregion
    /*
     

    // ����
    RelicSystem.Instance.AddRelicById("war_banner", stacks: 1);   // ���� ���� ���� 2,3...
    // ����
    RelicSystem.Instance.RemoveRelic("war_banner");
    // ����/�ε�
    RelicSystem.Instance.SaveRelics();
    RelicSystem.Instance.LoadRelics();
     
     
     */
}


/*
 �����ڵ�
using System.Collections;
using System.Collections.Generic;
using UnityEngine; 



 public class RelicSystem : MonoBehaviour
{
    public static RelicSystem Instance { get; private set; }

    [SerializeField] private RelicsUI relicsUI;   // �ɼ�(UI ǥ��)

    private readonly List<Relic> relics = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        // �̺�Ʈ ����
        GameEvents.OnBattleStart += HandleBattleStart;
        GameEvents.OnBattleEnd += HandleBattleEnd;
        GameEvents.OnTurnStart += HandleTurnStart;
        GameEvents.OnTurnEnd += HandleTurnEnd;
        GameEvents.OnCardDrawn += HandleCardDrawn;
        GameEvents.OnCardPlayed += HandleCardPlayed;
        GameEvents.OnDamageDealt += HandleDamageDealt;

        // ü���� ������ ����(���� ���� ����ǵ� null ����)
        GameEvents.ModifyPlayerAttack += ChainModifyPlayerAttack;
        GameEvents.ModifyPlayerMana += ChainModifyPlayerMana;
    }

    private void OnDisable()
    {
        GameEvents.OnBattleStart -= HandleBattleStart;
        GameEvents.OnBattleEnd -= HandleBattleEnd;
        GameEvents.OnTurnStart -= HandleTurnStart;
        GameEvents.OnTurnEnd -= HandleTurnEnd;
        GameEvents.OnCardDrawn -= HandleCardDrawn;
        GameEvents.OnCardPlayed -= HandleCardPlayed;
        GameEvents.OnDamageDealt -= HandleDamageDealt;

        GameEvents.ModifyPlayerAttack -= ChainModifyPlayerAttack;
        GameEvents.ModifyPlayerMana -= ChainModifyPlayerMana;
    }

    public void NotifyStackChanged(Relic r)
    {
        relicsUI?.UpdateStacks(r);
        FireRelicsChanged();
    }

    public event System.Action RelicsChanged; //�߰�+++
    private void FireRelicsChanged() => RelicsChanged?.Invoke(); //�߰�+++

    #region Public API
    public void AddRelic(Relic newRelic)
    {
        // ���� ID ���� ó��
        var existing = relics.Find(r => r.Data.relicId == newRelic.Data.relicId);
        if (existing != null && existing.Data.stackable)
        {
            existing.AddStack();
            relicsUI?.UpdateStacks(existing); // ���� �ؽ�Ʈ�� ����
            FireRelicsChanged();              // <- �߰�
            return;
        }

        relics.Add(newRelic);
        newRelic.OnAdd();
        relicsUI?.AddOrStack(newRelic);
        FireRelicsChanged();              // <- �߰�
    }

    public void RemoveRelic(string relicId)
    {
        var idx = relics.FindIndex(r => r.Data.relicId == relicId);
        if (idx >= 0)
        {
            relics[idx].OnRemove();
            relics.RemoveAt(idx);
            //relicsUI?.Refresh(relics);
            relicsUI?.Remove(relicId);
            FireRelicsChanged();              // <- �߰�
        }
    }
    #endregion

    #region Event Handlers
    private void HandleBattleStart()
    {
        foreach (var r in relics) r.OnBattleStart();
    }
    private void HandleBattleEnd()
    {
        foreach (var r in relics) r.OnBattleEnd();
    }
    private void HandleTurnStart(bool isPlayer)
    {
        foreach (var r in relics) r.OnTurnStart(isPlayer);
    }
    private void HandleTurnEnd(bool isPlayer)
    {
        foreach (var r in relics) r.OnTurnEnd(isPlayer);
    }
    private void HandleCardDrawn(Card c)
    {
        foreach (var r in relics) r.OnCardDrawn(c);
    }
    private void HandleCardPlayed(Card c)
    {
        foreach (var r in relics) r.OnCardPlayed(c);
    }
    private void HandleDamageDealt(int dmg, bool fromPlayer)
    {
        foreach (var r in relics) r.OnDamageDealt(dmg, fromPlayer);
    }

    private int ChainModifyPlayerAttack(int baseAttack)
    {
        int v = baseAttack;
        foreach (var r in relics) v = r.ModifyPlayerAttack(v);
        return v;
    }
    private int ChainModifyPlayerMana(int curMana)
    {
        int v = curMana;
        foreach (var r in relics) v = r.ModifyPlayerMana(v);
        return v;
    }
    #endregion
}

 
 
 */