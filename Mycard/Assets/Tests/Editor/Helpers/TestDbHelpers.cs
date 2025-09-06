using System.Collections.Generic;
using Game.Save;

/// <summary>
/// 테스트 코드에서만 사용하는 DB 저장 헬퍼. SaveCurrentRun 대체 경로.
/// </summary>
public static class TestDbHelpers
{
    public static void SaveFullRunSnapshot(
        IDatabase db,
        CurrentRun run,
        IEnumerable<CardInDeck> cards = null,
        IEnumerable<RelicInPossession> relics = null,
        IEnumerable<PotionInPossession> potions = null,
        IEnumerable<MapNodeState> nodes = null,
        IEnumerable<RngState> rngStates = null)
    {
        // 런 메타 저장
        db.UpsertCurrentRun(run);

        // 카드/유물/포션 교체 저장 (있을 때만)
        if (cards != null)
            db.ReplaceCardsInDeck(run.RunId, cards);
        if (relics != null)
            db.ReplaceRelics(run.RunId, relics);
        if (potions != null)
            db.ReplacePotions(run.RunId, potions);

        // 노드/랜덤 상태가 필요하면 여기서 세분화된 API로 저장
        if (nodes != null)
        {
            foreach (var n in nodes)
                db.UpsertNodeState(n);
        }
        if (rngStates != null)
        {
            db.UpsertRngStates(run.RunId, rngStates);
        }
    }
}

