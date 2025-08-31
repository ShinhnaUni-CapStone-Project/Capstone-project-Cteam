using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relic
{
    public Sprite Image => data.relicSprite;

    private readonly RelicData data;
    public Relic(RelicData relicData)
    {
        data = relicData;
    }

    public void OnAdd()
    {
        RelicRuntime.Apply(data.relicId, +1);
    }

    public void OnRemove()
    {
        RelicRuntime.Apply(data.relicId, -1);
    }
}
