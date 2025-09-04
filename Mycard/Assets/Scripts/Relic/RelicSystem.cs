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

    public void NotifyStackChanged(Relic r) => relicsUI?.UpdateStacks(r);

    #region Public API
    public void AddRelic(Relic newRelic)
    {
        // ���� ID ���� ó��
        var existing = relics.Find(r => r.Data.relicId == newRelic.Data.relicId);
        if (existing != null && existing.Data.stackable)
        {
            existing.AddStack();
            relicsUI?.UpdateStacks(existing); // ���� �ؽ�Ʈ�� ����
            return;
        }

        relics.Add(newRelic);
        newRelic.OnAdd();
        relicsUI?.AddOrStack(newRelic);
    }

    public void RemoveRelic(string relicId)
    {
        var idx = relics.FindIndex(r => r.Data.relicId == relicId);
        if (idx >= 0)
        {
            relics[idx].OnRemove();
            relics.RemoveAt(idx);
            relicsUI?.Refresh(relics);
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
