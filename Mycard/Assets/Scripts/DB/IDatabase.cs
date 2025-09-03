using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Save;

// '의존성 주입'을 위해, DB가 제공해야 할 기능의 목록을 정의하는 인터페이스입니다.
// 지금은 이벤트와 관련된 기능만 구현되어 있지만 확장 될 수 있습니다.
public interface IDatabase
{
    // --- 런(Run) 기본 정보 ---
    RunLoadResult LoadCurrentRun(string runId);
    void UpdateRunGold(string runId, int newGold);
    void UpdateRunHp(string runId, int newHp);

    // --- 이벤트 세션 (단일 행) ---
    // '이어하기' 시 이벤트를 복원하기 위한 전용 저장/로드 기능입니다.
    void UpsertActiveEventSession(string runId, string json);
    string LoadActiveEventSessionJson(string runId);
    void DeleteActiveEventSession(string runId);

    // --- 노드 상태 (단일 행) ---
    // 이벤트 결과를 맵 노드에 기록하기 위한 기능입니다.
    void UpsertNodeState(MapNodeState node);
}