using UnityEngine;
using TMPro;

public class CountersUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _drawPileCountText;
    [SerializeField] private TextMeshProUGUI _discardPileCountText;

    private IDeckService _deckService;

    private void OnEnable()
    {
        try
        {
            _deckService = ServiceRegistry.GetRequired<IDeckService>();
            _deckService.OnPileCountsChanged += UpdateCounters;
            UpdateCounters(_deckService.GetPileCounts());
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[CountersUI] IDeckService를 찾을 수 없어 UI를 비활성화합니다: {e.Message}");
            this.enabled = false;
        }
    }

    private void OnDisable()
    {
        if (_deckService != null)
        {
            _deckService.OnPileCountsChanged -= UpdateCounters;
        }
    }

    private void UpdateCounters(PileCounts counts)
    {
        if (_drawPileCountText != null)
            _drawPileCountText.text = counts.Draw.ToString();

        if (_discardPileCountText != null)
            _discardPileCountText.text = counts.Discard.ToString();
    }
}

