using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Save;

// EventManager가 외부에 제공해야 할 공식적인 기능 목록입니다.
public interface IEventManager
{
    EventSessionDTO LoadActiveOrCreate(string eventIdFallback);
    void ApplyChoice(EventSessionDTO session, EventChoiceDTO choice);

    // DB에서 활성 세션을 '생성하지 않고' 불러오기만 시도하는 기능
    EventSessionDTO TryLoadActive();
}