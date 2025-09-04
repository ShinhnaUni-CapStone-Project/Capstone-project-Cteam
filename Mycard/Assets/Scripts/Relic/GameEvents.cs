using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameEvents
{   
    
    // ���� �帧
    public static Action OnBattleStart;                // ���� ����
    public static Action OnBattleEnd;                  // ���� ����

    // �� �帧
    public static Action<bool> OnTurnStart;            // (isPlayerTurn)
    public static Action<bool> OnTurnEnd;              // (isPlayerTurn)

    // ī��/���� ��
    public static Action<Card> OnCardDrawn;            // ī�� ��ο�
    public static Action<Card> OnCardPlayed;           // ī�� ���
    public static Action<int, bool> OnDamageDealt;     // (damage, isFromPlayer)

    // ���� ����(���� ��, �ʿ��ϸ� ȣ��)
    public static Func<int, int> ModifyPlayerAttack;   // ���� ü��(��: ���� ���ݷ� = ü�� ��� ���)
    public static Func<int, int> ModifyPlayerMana;     // �÷��̾� ���� ����


}
