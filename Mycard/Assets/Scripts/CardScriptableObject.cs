using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Card", menuName = "card", order = 1)]
public class CardScriptableObject : ScriptableObject
{

    public string cardName;
    public string CardId; //Id ī�� ���� Id
    //public CardType cardType; //�߰�1
    //public DamageType damageType;//�߰�2

    [TextArea]
    public string actionDescription, cardLore;
    
    public int currentHealth, attackPower, manaCost;

    public Sprite characterSprite, bgSprite;

    /*
    public enum CardType //�߰�1
    {
        Fire,
        Ice,
        Wind,
        electric,
        Light,
        Dark

    }
    public enum DamageType //�߰�2
    {
        Fire,
        Ice,
        Wind,
        electric,
        Light,
        Dark
    }
    */
}
