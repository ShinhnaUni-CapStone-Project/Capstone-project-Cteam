using System.Collections.Generic;
using UnityEngine;


public class PlayerBuffs : MonoBehaviour
{
    public static PlayerBuffs instance;

    [Header("Global Buffs")]
    public int attackBonus = 0;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // �� �̵��ص� ����
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddAttackBonus(int amount)
    {
        attackBonus += amount;
        RecomputeAllPlayerCards();
    }

    public void RecomputeAllPlayerCards()
    {
        // 1) �ʵ� �� �÷��̾� ī�� ����
        if (CardPointsController.instance != null)
        {
            foreach (var p in CardPointsController.instance.playerCardPoints)
            {
                if (p.activeCard != null && p.activeCard.isPlayer)
                {
                    ApplyAttackToCardFromBase(p.activeCard);
                }
            }
        }

        // 2) ���� ī�� ����
        if (HandController.instance != null)
        {
            foreach (var c in HandController.instance.heldCards)
            {
                if (c != null && c.isPlayer) ApplyAttackToCardFromBase(c);
            }
        }
    }

    private void ApplyAttackToCardFromBase(Card card)
    {
        // �׻� "�⺻�� + ��������"�� �缳��(�ߺ� ���� ����)
        if (card.cardSO != null)
        {
            card.attackPower = card.cardSO.attackPower + attackBonus;
            card.UpdateCardDisplay();
            card.ApplyAttackBuffOutline(attackBonus > 0);
        }
    }
}
//����������� �Ҿ����� warbanner�߰����� ���ݷ� �ö󰡴� ���� �߰�