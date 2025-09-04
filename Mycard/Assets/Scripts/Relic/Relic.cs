using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Relic
{
    public RelicData Data { get; private set; }
    public int Stacks { get; private set; } = 1;

    protected Relic(RelicData data)
    {
        Data = data;
    }

    public void AddStack(int n = 1)
    {
        if (!Data.stackable) return;
        Stacks = Mathf.Clamp(Stacks + n, 1, Data.maxStacks);
        RelicSystem.Instance?.NotifyStackChanged(this);
        OnStacksChanged();
    }

    
    #region »ı¸íÁÖ±â(È¹µæ/Á¦°Å)
    public virtual void OnAdd() { }
    public virtual void OnRemove() { }
    protected virtual void OnStacksChanged() { }
    #endregion

    #region ÀüÅõ/ÅÏ/Çàµ¿ ÈÅ
    public virtual void OnBattleStart() { }
    public virtual void OnBattleEnd() { }
    public virtual void OnTurnStart(bool isPlayerTurn) { }
    public virtual void OnTurnEnd(bool isPlayerTurn) { }
    public virtual void OnCardDrawn(Card card) { }
    public virtual void OnCardPlayed(Card card) { }
    public virtual void OnDamageDealt(int damage, bool isFromPlayer) { }

    // ÇÊ¿ä½Ã ½ºÅÈ ¼öÁ¤ ÈÅ(Ã¼ÀÎ ¿¬°á¿ë)
    public virtual int ModifyPlayerAttack(int baseAttack) => baseAttack;
    public virtual int ModifyPlayerMana(int currentMana) => currentMana;
    #endregion
    
}
