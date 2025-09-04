using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelicsUI : MonoBehaviour
{
    [SerializeField] private Transform gridParent;     // GridLayoutGroup
    [SerializeField] private GameObject iconPrefab;    // Image ����ִ� ������

    private readonly Dictionary<string, RelicIconUI> map = new();

    public void AddOrStack(Relic relic)     // �ű� �߰� �Ǵ� ���� ������ ����
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

    public void UpdateStacks(Relic relic)   // �̹� �ִ� �������� ���ø� ����
    {
        if (map.TryGetValue(relic.Data.relicId, out var ui))
            ui.SetStacks(relic.Stacks);
    }

    public void Remove(string relicId)      // ����
    {
        if (map.TryGetValue(relicId, out var ui))
        {
            Destroy(ui.gameObject);
            map.Remove(relicId);
        }
    }

    public void Refresh(IReadOnlyList<Relic> relics)  // Ǯ ��������(�ʿ� ��)
    {
        foreach (var kv in map) Destroy(kv.Value.gameObject);
        map.Clear();
        foreach (var r in relics) AddOrStack(r);
    }
}
