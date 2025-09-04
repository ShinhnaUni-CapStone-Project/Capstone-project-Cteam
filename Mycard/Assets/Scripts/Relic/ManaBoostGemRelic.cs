using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ManaBoostGemRelic : Relic
{
    // 지금까지 실제로 적용한 총 증가량(스택 변화/제거 대응)
    private int grantedTotal = 0;

    public ManaBoostGemRelic(RelicData data) : base(data) { }

    public override void OnAdd()
    {
        ApplyOrAdjust();
        Debug.Log($"[Relic] {Data.displayName} 획득 → playerMax/playerMana +{grantedTotal}");
    }

    protected override void OnStacksChanged()
    {
        ApplyOrAdjust();
        Debug.Log($"[Relic] {Data.displayName} 스택:{Stacks} → 누적 +{grantedTotal}");
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
        Debug.Log($"[Relic] {Data.displayName} 제거 → 보정 해제");
    }

    // 현재 스택 수(= 목표 증가량)에 맞춰 차액만큼 반영함
    private void ApplyOrAdjust()
    {
        var bc = BattleController.instance;
        if (bc == null) return;

        int target = Stacks;                 // 스택 1당 +1
        int delta = target - grantedTotal;  // 지금까지 준 것과 목표의 차이
        if (delta == 0) return;

        bc.playermaxMana += delta;           // 최대 마나 상한 즉시 +delta
        bc.playerMana += delta;           // 현재 마나도 즉시 +delta

        grantedTotal = target;
        UpdateManaUI();

        // (선택) 지금 전투의 "채울 수 있는 최대치"까지 즉시 늘리고 싶다면:
        // bc.SetCurrentPlayerMaxMana(bc.GetCurrentPlayerMaxMana() + delta);
        // 같은 공개 메서드를 BattleController에 만들어 호출하세요.
    }

    private void UpdateManaUI()
    {
        var bc = BattleController.instance;
        if (bc != null) UIController.instance?.SetPlayerManaText(bc.playerMana);
    }
}