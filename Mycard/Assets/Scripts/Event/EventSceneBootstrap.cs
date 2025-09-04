using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.Save; // EventChoiceDTO를 사용하기 위함
using UnityEngine.SceneManagement;

public class EventSceneBootstrap : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private TMP_Text descriptionText; // 1. 이벤트 설명 텍스트
    [SerializeField] private Button choiceButton1;     // 2. 선택지 버튼 1
    [SerializeField] private TMP_Text choice1Label;   // 버튼 안의 텍스트를 직접 연결
    [SerializeField] private Button choiceButton2;     // 3. 선택지 버튼 2
    [SerializeField] private TMP_Text choice2Label;   // 버튼 안의 텍스트를 직접 연결 2

    [Header("씬 이름/기본값")]
    [SerializeField] private string mapSceneName = "Scenes/Map Scene"; // 하드코딩 제거
    [SerializeField] private string fallbackEventId = "GenericEvent01";


    private IEventManager _eventManager;
    private EventSessionDTO _currentSession;
    private bool _isResolving; // 중복 입력을 막기 위한 '잠금 장치'

    void Start()
    {
        // EventManager가 없으면 안전하게 맵으로 돌아갑니다.
        try { _eventManager = ServiceRegistry.GetRequired<IEventManager>(); }
        catch (System.Exception e)
        {
            Debug.LogError($"[EventScene] EventManager가 없습니다: {e.Message}");
            SafeGoMap();
            return;
        }

        // EventManager에게 현재 진행 중인 이벤트 정보를 요청합니다.
        // "fallbackEventId"은 혹시 모를 비상 상황을 대비한 기본값입니다.
        _currentSession = _eventManager.LoadActiveOrCreate(fallbackEventId);
        if (_currentSession == null)
        {
            Debug.LogError("[EventScene] 세션을 불러올 수 없습니다.");
            SafeGoMap();
            return;
        }

        // ★ 예외 처리: 이미 해결된 이벤트라면 즉시 맵으로 복귀합니다.
        if (_currentSession.resolved)
        {
            Debug.LogWarning("[EventScene] 이미 해결된 이벤트 세션입니다. 맵으로 복귀합니다.");
            SafeGoMap();
            return;
        }

        // 받아온 정보로 UI를 채웁니다.
        BindUI();
    }

    private void BindUI()
    {

        // 1. 설명 텍스트를 채웁니다.
        descriptionText.text = _currentSession.description ?? "";

        // 버튼에 리스너를 추가하기 전, 기존에 등록된 리스너를 모두 제거합니다.
        choiceButton1.onClick.RemoveAllListeners();
        choiceButton2.onClick.RemoveAllListeners();

        // 2. 각 버튼에 맞는 선택지 내용과 클릭 이벤트를 연결합니다.
        // 첫 번째 선택지 버튼을 설정합니다.
        if (_currentSession.choices != null && _currentSession.choices.Length > 0)
        {
            var c0 = _currentSession.choices[0];
            choice1Label.text = c0.label ?? ""; // 직접 연결된 텍스트 사용
            choiceButton1.onClick.AddListener(() => OnChoicePicked(c0));
            choiceButton1.gameObject.SetActive(true);
        }
        else
        {
            choiceButton1.gameObject.SetActive(false);
        }

        // 두 번째 선택지 버튼을 설정합니다.
        if (_currentSession.choices != null && _currentSession.choices.Length > 1)
        {
            var c1 = _currentSession.choices[1];
            choice2Label.text = c1.label ?? ""; // 직접 연결된 텍스트 사용
            choiceButton2.onClick.AddListener(() => OnChoicePicked(c1));
            choiceButton2.gameObject.SetActive(true);
        }
        else
        {
            choiceButton2.gameObject.SetActive(false);
        }
    }

    private void OnChoicePicked(EventChoiceDTO choice)
    {
        if (_isResolving) return; // 중복 클릭 방지
        _isResolving = true;

        choiceButton1.interactable = false;
        choiceButton2.interactable = false;

        // 플레이어가 내린 선택을 EventManager에게 알려 결과를 적용시킵니다.
        _eventManager.ApplyChoice(_currentSession, choice);

        // 결과 적용 후, 맵 씬으로 돌아갑니다.
        SafeGoMap();
    }
    
    // 씬 전환을 위한 안전한 함수
    private void SafeGoMap()
    {
        if (!string.IsNullOrEmpty(mapSceneName))
            SceneManager.LoadScene(mapSceneName);
        else
            Debug.LogError("[EventScene] mapSceneName이 비어있어 씬 전환이 불가합니다.");
    }

}