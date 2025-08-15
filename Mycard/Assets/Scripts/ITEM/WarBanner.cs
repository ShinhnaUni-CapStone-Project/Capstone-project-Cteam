using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WarBanner : Item
{
    public int attackBoostAmount = 2; // �÷��� ���ݷ� ��ġ

    private void Awake()
    {
        this.ItemNumber = 3;
        this.ItemName = "���� ���";
        this.ItemSprite = Resources.Load<Sprite>("Sprites/3_WarBanner");
        this.ItemImage = new GameObject("WarBannerImage").AddComponent<Image>();
        this.get_Count = 1;
        this.isget = true;
    }

    public override void OnAddItem()
    {
        // ���� �ʵ忡 �ִ� ��� �÷��̾� ī�� ���ݷ� ���
        foreach (CardPlacePoint point in CardPointsController.instance.playerCardPoints)
        {
            if (point.activeCard != null && point.activeCard.isPlayer)
            {
                point.activeCard.attackPower += attackBoostAmount;
                point.activeCard.UpdateCardDisplay();

                // �ܰ���(Outline) �߰�
                Outline outline = point.activeCard.attackText.gameObject.GetComponent<Outline>();
                if (outline == null)
                    outline = point.activeCard.attackText.gameObject.AddComponent<Outline>();

                outline.effectColor = Color.green;
                outline.effectDistance = new Vector2(2, 2);
            }
        }

        //  ������ ���� �̴� ī�嵵 ����ǵ��� HandController�� heldCards�� ó��
        foreach (Card card in HandController.instance.heldCards)
        {
            if (card.isPlayer)
            {
                card.attackPower += attackBoostAmount;
                card.UpdateCardDisplay();

                Outline outline = card.attackText.gameObject.GetComponent<Outline>();
                if (outline == null)
                    outline = card.attackText.gameObject.AddComponent<Outline>();

                outline.effectColor = Color.green;
                outline.effectDistance = new Vector2(2, 2);
            }
        }
    }
}