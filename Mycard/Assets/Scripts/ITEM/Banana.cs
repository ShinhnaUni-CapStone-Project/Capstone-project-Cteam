using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Banana : Item
{
    
    

    private void Awake()
    {
        this.ItemNumber = 1;
        this.ItemName = "�ٳ���";
        this.ItemSprite = Resources.Load<Sprite>("Sprites/38_Banana");
        this.ItemImage = new GameObject("BananaImage").AddComponent<Image>();
        this.get_Count = 1;
        this.isget = true;

    }

    
    
}
