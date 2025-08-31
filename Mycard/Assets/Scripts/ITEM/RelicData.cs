using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "New Relic Item", menuName = "Data/Relic", order = 1)]
public class RelicData : ScriptableObject
{

    public string relicId;
    public string relicName; //displayname
    public Sprite relicSprite;

}
