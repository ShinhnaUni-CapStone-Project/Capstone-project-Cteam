using System.Collections;
using UnityEngine;

public partial class ShopUI : MonoBehaviour
{
    

    public void Open()
    {
        // 이미 열려있으면 아무것도 안 함
        if (_isOpen) return;


        // UI 애니메이션을 위한 준비
        if (!gameObject.activeSelf) gameObject.SetActive(true);

        panel.blocksRaycasts = false;
        panel.interactable = false;
        panel.alpha = 0f;
        if (window) window.localScale = Vector3.one * ScaleFrom;


        // '방문중' (_sessionInitialized)을 확인합니다.
        // 만약 이번 노드 방문에서 처음 여는 것이라면, (방문중이 아니라면)
        if (!_sessionInitialized)
        {
            // "방문중" 팻말을 세웁니다. 이제 방문중에는 이 코드가 실행되지 않습니다.
            _sessionInitialized = true;

            // --- 상점의 상태를 초기화하고 상품을 처음 진열하는 코드는 모두 이 안으로 들어옵니다 ---
            _rerollCount = 0;

            BuildDummySlots();
            BuildCardSlotsInitial();
            ApplyDeals();
        }


        RebuildGrid();
        RefreshTopbar();

        // UI를 여는 애니메이션 실행
        if (_animCo != null) StopCoroutine(_animCo);
        _animCo = StartCoroutine(AnimateOpen());
        _isOpen = true;
    }

    public void Close()
    {
        if (!_isOpen) return;
        if (_animCo != null) StopCoroutine(_animCo);
        _animCo = StartCoroutine(AnimateClose());
        _isOpen = false;
    }

    private void HideImmediate()
    {
        if (panel == null) panel = GetComponent<CanvasGroup>();
        gameObject.SetActive(false);
        panel.alpha = 0f;
        panel.interactable = false;
        panel.blocksRaycasts = false;
        if (window) window.localScale = Vector3.one * ScaleFrom;
    }

    private IEnumerator AnimateOpen()
    {
        float t = 0f;
        while (t < OpenDur)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / OpenDur);

            panel.alpha = u;
            if (window)
            {
                float s = Mathf.SmoothStep(ScaleFrom, 1f, u);
                window.localScale = new Vector3(s, s, 1f);
            }
            yield return null;
        }

        panel.alpha = 1f;
        if (window) window.localScale = Vector3.one;
        panel.blocksRaycasts = true;
        panel.interactable = true;
        _animCo = null;
    }

    private IEnumerator AnimateClose()
    {
        panel.blocksRaycasts = false;
        panel.interactable = false;

        float startAlpha = panel.alpha;
        float t = 0f;
        while (t < CloseDur)
        {
            t += Time.unscaledDeltaTime;
            float u = Mathf.Clamp01(t / CloseDur);

            panel.alpha = Mathf.Lerp(startAlpha, 0f, u);
            if (window)
            {
                float s = Mathf.SmoothStep(1f, ScaleFrom, u);
                window.localScale = new Vector3(s, s, 1f);
            }
            yield return null;
        }

        panel.alpha = 0f;
        if (window) window.localScale = Vector3.one * ScaleFrom;
        gameObject.SetActive(false);
        _animCo = null;
    }
}