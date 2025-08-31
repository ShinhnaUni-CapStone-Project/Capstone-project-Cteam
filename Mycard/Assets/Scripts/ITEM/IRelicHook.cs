using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRelicHook
{
    void OnBattleStart() { }
    void OnPlayerTurnStart() { }
    void OnPlayerCardDrawn(Card card) { }
    void OnPlayerCardPlayed(Card card) { }

    // ���� ���� ����(������)
    // ī�尡 ȭ��/������ ǥ�õǰų� ���� �� ȣ���ؼ� ����ġ �ο�
    int ModifyCardAttack(int baseAttack) { return baseAttack; }
    int ModifyManaAtTurnStart(int baseGain) { return baseGain; }
    int ModifyExtraDrawAtTurnStart(int baseDraw) { return baseDraw; }


}
