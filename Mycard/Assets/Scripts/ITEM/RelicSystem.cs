using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RelicSystem : MonoBehaviour
{
    [SerializeField] private RelicsUI relicsUI;
    public static RelicSystem instance;
    private readonly List<Relic> relics = new();


    private void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }
    // Start is called before the first frame update
    public void AddRelic(Relic relic)
    {
        relics.Add(relic);
        relicsUI.AddRelicUI(relic);
        relic.OnAdd();
    }
    public void RemoveRelic(Relic relic)
    {
        
        relics.Remove(relic);
        relicsUI.RemoveRelicUI(relic);
        relic.OnRemove();
    }
    //RelicSystem.Instance.AddRelic(new Relic(RelicData)
}
