using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelicGrantButton : MonoBehaviour
{
    [SerializeField] private RelicData relicData;
    [SerializeField] private string relicType; // "WarBanner", "HappyFlower", "Anchor" µî
    // Start is called before the first frame update
    public void Grant()
    {
        Relic relic = relicType switch
        {
            "WarBanner" => new WarBannerRelic(relicData),
            "HappyFlower" => new HappyFlowerRelic(relicData),
            "ManaBoostGem" => new ManaBoostGemRelic(relicData),
            _ => null
        };
        if (relic != null)
            RelicSystem.Instance.AddRelic(relic);
    }
}
