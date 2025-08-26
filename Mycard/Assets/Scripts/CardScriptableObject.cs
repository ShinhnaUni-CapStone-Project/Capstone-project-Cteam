using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "card", order = 1)]
public class CardScriptableObject : ScriptableObject
{
    public string cardId; // 카드의 절대 변하지 않는 고유 ID (예: "card_strike", "card_fireball")

    public string cardName; // 카드에 표시될 유저들이 볼 이름

    [TextArea]
    public string actionDescription, cardLore;
    
    public int currentHealth, attackPower, manaCost;

    public Sprite characterSprite, bgSprite;
}
