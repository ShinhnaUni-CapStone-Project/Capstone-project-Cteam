using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelicsUI : MonoBehaviour
{
    [SerializeField] private Transform gridParent;     // GridLayoutGroup
    [SerializeField] private GameObject iconPrefab;    // Image 들어있는 프리팹

    private readonly Dictionary<string, RelicIconUI> map = new();

    public void AddOrStack(Relic relic)     // 신규 추가 또는 스택 증가에 공용
    {
        if (map.TryGetValue(relic.Data.relicId, out var ui))
        {
            ui.SetStacks(relic.Stacks);
            return;
        }
        var go = Instantiate(iconPrefab, gridParent);
        var icon = go.GetComponent<RelicIconUI>();
        icon.Setup(relic.Data.icon, relic.Stacks);
        map[relic.Data.relicId] = icon;
    }

    public void UpdateStacks(Relic relic)   // 이미 있는 아이콘의 스택만 갱신
    {
        if (map.TryGetValue(relic.Data.relicId, out var ui))
            ui.SetStacks(relic.Stacks);
    }

    public void Remove(string relicId)      // 제거
    {
        if (map.TryGetValue(relicId, out var ui))
        {
            Destroy(ui.gameObject);
            map.Remove(relicId);
        }
    }

    public void Refresh(IReadOnlyList<Relic> relics)  // 풀 리프레시(필요 시)
    {
        foreach (var kv in map) Destroy(kv.Value.gameObject);
        map.Clear();
        foreach (var r in relics) AddOrStack(r);
    }
}
