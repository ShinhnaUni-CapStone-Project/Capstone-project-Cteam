using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ManaBoostGemRelic : Relic
{
    // ���ݱ��� ������ ������ �� ������(���� ��ȭ/���� ����)
    private int grantedTotal = 0;

    public ManaBoostGemRelic(RelicData data) : base(data) { }

    public override void OnAdd()
    {
        ApplyOrAdjust();
        Debug.Log($"[Relic] {Data.displayName} ȹ�� �� playerMax/playerMana +{grantedTotal}");
    }

    protected override void OnStacksChanged()
    {
        ApplyOrAdjust();
        Debug.Log($"[Relic] {Data.displayName} ����:{Stacks} �� ���� +{grantedTotal}");
    }

    public override void OnRemove()
    {
        var bc = BattleController.instance;
        if (bc != null && grantedTotal != 0)
        {
            bc.playermaxMana -= grantedTotal;
            bc.playerMana = Mathf.Max(0, bc.playerMana - grantedTotal);
            UpdateManaUI();
            grantedTotal = 0;
        }
        Debug.Log($"[Relic] {Data.displayName} ���� �� ���� ����");
    }

    // ���� ���� ��(= ��ǥ ������)�� ���� ���׸�ŭ �ݿ���
    private void ApplyOrAdjust()
    {
        var bc = BattleController.instance;
        if (bc == null) return;

        int target = Stacks;                 // ���� 1�� +1
        int delta = target - grantedTotal;  // ���ݱ��� �� �Ͱ� ��ǥ�� ����
        if (delta == 0) return;

        bc.playermaxMana += delta;           // �ִ� ���� ���� ��� +delta
        bc.playerMana += delta;           // ���� ������ ��� +delta

        grantedTotal = target;
        UpdateManaUI();

        // (����) ���� ������ "ä�� �� �ִ� �ִ�ġ"���� ��� �ø��� �ʹٸ�:
        // bc.SetCurrentPlayerMaxMana(bc.GetCurrentPlayerMaxMana() + delta);
        // ���� ���� �޼��带 BattleController�� ����� ȣ���ϼ���.
    }

    private void UpdateManaUI()
    {
        var bc = BattleController.instance;
        if (bc != null) UIController.instance?.SetPlayerManaText(bc.playerMana);
    }
}