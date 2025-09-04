using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[CreateAssetMenu(fileName = "RelicData", menuName = "Data/Relic", order = 1)]
public class RelicData : ScriptableObject
{
    [Header("Display")]
    public string relicId;
    public string displayName;

    [TextArea] public string description;
    public Sprite icon;

    [Header("Rule")]
    public bool stackable = false;
    [Min(1)] public int maxStacks = 1;
}
