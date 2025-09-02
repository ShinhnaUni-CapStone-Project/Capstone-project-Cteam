using UnityEngine;
using Game.Save;
using System;
using System.Linq;
using System.Collections.Generic;

public sealed class EventManager : IEventManager 
{
    private readonly IDatabase _db;
    private readonly string _runId;
    private CurrentRun _run;

    public EventManager(IDatabase db, string runId)
    {
        _db = db;
        _runId = runId;
        _run = _db.LoadCurrentRun(_runId)?.Run;
        if (_run == null) Debug.LogError("[EventManager] 현재 런 정보를 찾을 수 없습니다.");
    }

    public EventSessionDTO LoadActiveOrCreate(string eventIdFallback)
    {
        if (_run == null) return null;

        var json = _db.LoadActiveEventSessionJson(_run.RunId);
        if (!string.IsNullOrEmpty(json))
        {
            try { return JsonUtility.FromJson<EventSessionDTO>(json); }
            catch (Exception e) { Debug.LogWarning($"[EventManager] 활성 이벤트 JSON 파싱 실패: {e.Message}"); }
        }

        var eventSO = Resources.Load<EventScriptableObject>($"Events/{eventIdFallback}");
        if (eventSO == null)
        {
            Debug.LogError($"[EventManager] 이벤트 원본 파일 없음: {eventIdFallback}");
            return null;
        }

        var newSession = new EventSessionDTO {
            eventId = eventSO.eventId,
            resolved = false,
            description = eventSO.description, // ★ 추가: DTO에 텍스트 포함
            choices = eventSO.choices.Select(c => new EventChoiceDTO {
                id = c.id, label = c.label,
                effects = c.effects.Select(e => new EventEffectDTO { type = e.type, amount = e.amount, refId = e.refId }).ToArray()
            }).ToArray()
        };

        _db.UpsertActiveEventSession(_run.RunId, JsonUtility.ToJson(newSession));
        return newSession;
    }

    public EventSessionDTO TryLoadActive()
    {
        if (_run == null) return null;

        var json = _db.LoadActiveEventSessionJson(_run.RunId);
        if (string.IsNullOrEmpty(json))
        {
            return null; // DB에 없으면 null 반환 (새로 생성 안 함)
        }

        try
        {
            return JsonUtility.FromJson<EventSessionDTO>(json);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"[EventManager] 활성 이벤트 JSON 파싱 실패: {e.Message}");
            return null;
        }
    }

    public void ApplyChoice(EventSessionDTO session, EventChoiceDTO choice)
    {
        if (choice == null || _run == null) return;

        // --- 1. 효과 적용 ---
        foreach (var effect in choice.effects)
        {
            if (effect.type == "HpDelta") _db.UpdateRunHp(_run.RunId, _run.CurrentHp + effect.amount);
            else if (effect.type == "GoldDelta") _db.UpdateRunGold(_run.RunId, _run.Gold + effect.amount);
        }

        // --- 2. 로컬 런 상태 재동기화 (가장 안전한 방식) ---
        var updatedRunResult = _db.LoadCurrentRun(_run.RunId);
        if (updatedRunResult != null && updatedRunResult.Run != null) _run = updatedRunResult.Run;
        
        // --- 3. 상세한 결과 기록 생성 ---
        var resolution = new EventResolutionSnapshot {
            eventId = session.eventId, // DTO에서 이벤트 ID를 가져옴
            selectedChoiceId = choice.id,
            appliedEffects = choice.effects,
            resolvedAtUtc = DateTime.UtcNow.ToString("o")
        };

        // --- 4. MapNodeState에 해결 기록 저장 ---
        var node = new MapNodeState {
            RunId = _run.RunId, Act = _run.Act, Floor = _run.Floor, NodeIndex = _run.NodeIndex,
            Type = Game.Save.NodeType.Event, Visited = true, Cleared = true,
            EventResolutionJson = JsonUtility.ToJson(resolution)
        };
        _db.UpsertNodeState(node);

        // --- 5. 활성 이벤트 세션 삭제 ---
        _db.DeleteActiveEventSession(_run.RunId);
    }

    // 결과 기록용 내부 클래스
    [Serializable]
    private class EventResolutionSnapshot
    {
        public string eventId;
        public string selectedChoiceId;
        public EventEffectDTO[] appliedEffects;
        public string resolvedAtUtc;
    }
}