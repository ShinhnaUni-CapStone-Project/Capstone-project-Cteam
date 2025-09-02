using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Save;

// 기존의 DatabaseManager 싱글턴을 'IDatabase'라는 표준 규격에 맞게 감싸주는 어댑터 클래스입니다.
public sealed class DatabaseFacade : IDatabase
{
    public RunLoadResult LoadCurrentRun(string runId)
        => DatabaseManager.Instance.LoadCurrentRun(runId);

    public void UpdateRunGold(string runId, int newGold)
        => DatabaseManager.Instance.UpdateRunGold(runId, newGold);

    public void UpdateRunHp(string runId, int newHp)
        => DatabaseManager.Instance.UpdateRunHp(runId, newHp);

    public void UpsertActiveEventSession(string runId, string json)
        => DatabaseManager.Instance.UpsertActiveEventSession(runId, json);

    public string LoadActiveEventSessionJson(string runId)
        => DatabaseManager.Instance.LoadActiveEventSessionJson(runId);

    public void DeleteActiveEventSession(string runId)
        => DatabaseManager.Instance.DeleteActiveEventSession(runId);

    public void UpsertNodeState(MapNodeState node)
        => DatabaseManager.Instance.UpsertNodeState(node);
}