using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Save; // NodeType을 사용하기 위함

// 1. 이벤트 원본 데이터를 담는 '설계도'
[CreateAssetMenu(fileName = "New Event", menuName = "Events/New Event")]
public class EventScriptableObject : ScriptableObject
{
    public string eventId;
    public string description;
    public List<EventChoice> choices;
}

// 2. 이벤트 선택지 정보를 담는 클래스
[System.Serializable]
public class EventChoice
{
    public string id;
    public string label; // 버튼에 표시될 텍스트
    public List<EventEffect> effects;
}

// 3. 선택지의 효과를 담는 클래스
[System.Serializable]
public class EventEffect
{
    public string type; // "HpDelta", "GoldDelta", "AddCard" 등
    public int amount;
    public string refId; // 카드를 추가할 경우 CardId 등
}

// 4. '이어하기' 저장을 위한 데이터 묶음 (DTO)
[System.Serializable]
public class EventSessionDTO
{
    public string eventId;
    public bool resolved; // 이미 해결된 이벤트인지 여부
    public string pickedChoiceId;
    public string description;  // UI가 SO 없이도 복원 가능하도록 텍스트 포함
    public EventChoiceDTO[] choices;
}

[System.Serializable]
public class EventChoiceDTO
{
    public string id;
    public string label;
    public EventEffectDTO[] effects;
}

[System.Serializable]
public class EventEffectDTO
{
    public string type;
    public int amount;
    public string refId;
}