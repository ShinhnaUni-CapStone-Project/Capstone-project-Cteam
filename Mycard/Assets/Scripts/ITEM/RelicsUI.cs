using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RelicsUI : MonoBehaviour
{
    [SerializeField] private RelicUI relicUIPrefab;
    private readonly List<RelicUI> relicUIs = new();

    public void AddRelicUI(Relic relic)
    {
        RelicUI relicUI = Instantiate(relicUIPrefab,transform);
        relicUI.Setup(relic);
        relicUIs.Add(relicUI);
    }

    public void RemoveRelicUI(Relic relic)
    {
        RelicUI relicUI = relicUIs.Where(pui=>pui.Relic == relic).FirstOrDefault();
        if (relicUI != null)
        {
            relicUIs.Remove(relicUI);
            Destroy(relicUI.gameObject);
        }
    }
}
