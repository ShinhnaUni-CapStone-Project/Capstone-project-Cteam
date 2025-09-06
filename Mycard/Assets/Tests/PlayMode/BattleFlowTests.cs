using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using Game.Save;

public class BattleFlowTests
{
    private IDeckService _deckService;
    private BattleController _battleController;

    [SetUp]
    public void Setup()
    {
        // 1) 실제 카드 데이터(SSoT)에서 유효한 카드 ID 수집
        var allCards = Resources.LoadAll<CardScriptableObject>("Cards");
        Assert.IsNotEmpty(allCards, "Resources/Cards 폴더에 CardScriptableObject가 하나도 없습니다!");
        var validCardIds = allCards.Select(c => c.CardId).Where(id => !string.IsNullOrEmpty(id)).ToList();
        Assert.IsNotEmpty(validCardIds, "CardScriptableObject.CardId가 비어있습니다. 카드 에셋 구성을 확인하세요.");

        // 2) 테스트 덱(12장) 구성: 실제 카드 ID를 순환 사용
        var testDeckIds = new List<string>();
        for (int i = 0; i < 12; i++)
            testDeckIds.Add(validCardIds[i % validCardIds.Count]);

        // 3) DB/런 데이터 준비(헬퍼)
        PrepareTestRunAndDatabase(testDeckIds);
    }

    // --- 테스트 헬퍼: 주어진 카드 ID들로 런/DB 상태를 구성 ---
    private void PrepareTestRunAndDatabase(List<string> cardIds)
    {
        if (cardIds == null || cardIds.Count == 0)
            Assert.Fail("PrepareTestRunAndDatabase: cardIds가 비어있습니다.");

        var runId = Guid.NewGuid().ToString("N");
        DatabaseManager.Instance.Connect();

        var cardsInDeck = new List<CardInDeck>();
        foreach (var cardId in cardIds)
        {
            cardsInDeck.Add(new CardInDeck
            {
                InstanceId = Guid.NewGuid().ToString("N"),
                RunId = runId,
                CardId = cardId,
                IsUpgraded = false
            });
        }

        var run = new CurrentRun
        {
            RunId = runId,
            ProfileId = "TestProfile",
            Act = 1,
            Floor = 0,
            NodeIndex = 0,
            Gold = 100,
            CurrentHp = 50,
            MaxHpBase = 50,
            EnergyMax = 3,
            CreatedAtUtc = DateTime.UtcNow.ToString("o"),
            UpdatedAtUtc = DateTime.UtcNow.ToString("o")
        };

        var db = new DatabaseFacade();
        db.UpsertCurrentRun(run);
        db.ReplaceCardsInDeck(runId, cardsInDeck);

        PlayerPrefs.SetString("lastRunId", runId);
        PlayerPrefs.Save();
    }

    [UnitySetUp]
    public IEnumerator UnitySetUp()
    {
        // 1) 서비스 등록자 생성(씬 로드 전 1프레임 선행) → DontDestroyOnLoad
        new GameObject("TestGameInitializer").AddComponent<GameInitializer>();
        yield return null; // Awake 실행

        // 2) 배틀 씬 로드(서비스가 등록된 상태 보장)
        SceneManager.LoadScene("Battle_android");
        yield return null; // 씬 초기화 대기

        _deckService = ServiceRegistry.GetRequired<IDeckService>();
        _battleController = UnityEngine.Object.FindObjectOfType<BattleController>();

        Assert.IsNotNull(_deckService, "IDeckService를 찾지 못했습니다.");
        Assert.IsNotNull(_battleController, "BattleController를 찾지 못했습니다.");

        // 테스트 안정화를 위해 플레이어 턴 보장
        _battleController.currentPhase = BattleController.TurnOrder.playerActive;
    }

    [UnityTest]
    public IEnumerator 전투_시작_시_초기_핸드가_정상적으로_드로우된다()
    {
        // 초기 드로우가 반영되도록 한 프레임 추가 유예
        yield return null;

        var hand = _deckService.GetHandSnapshot();
        Assert.IsNotNull(hand);
        Assert.AreEqual(5, hand.Count, "초기 핸드 카드 수가 5가 아닙니다.");
    }

    [UnityTest]
    public IEnumerator 플레이어_드로우_요청_시_이벤트_수신_후_상태가_정상적으로_변경된다()
    {
        // 준비: 현재 상태 스냅샷
        var beforeCounts = _deckService.GetPileCounts();
        int beforeHand = _deckService.GetHandSnapshot().Count;

        bool received = false;
        void OnDraw(DrawResult r)
        {
            if (r != null && r.Reason == DrawReason.CardEffect) received = true;
        }
        _deckService.OnCardsDrawn += OnDraw;
        try
        {
            // 행위: 드로우 시도(마나 충분, 플레이어 턴)
            _battleController.currentPhase = BattleController.TurnOrder.playerActive;
            _battleController.AttemptPlayerDraw();

            // 대기: 이벤트 수신 또는 타임아웃
            yield return new WaitUntilWithTimeout(() => received, 5f);

            Assert.IsTrue(received, "OnCardsDrawn(CardEffect) 이벤트 미수신");

            var afterCounts = _deckService.GetPileCounts();
            int afterHand = _deckService.GetHandSnapshot().Count;

            Assert.AreEqual(beforeHand + 1, afterHand, "핸드 카드 수 증가 불일치");
            Assert.AreEqual(beforeCounts.Draw - 1, afterCounts.Draw, "드로우 더미 수 감소 불일치");
        }
        finally
        {
            _deckService.OnCardsDrawn -= OnDraw;
        }
    }

    [UnityTest]
    public IEnumerator 턴_종료_시_다음_턴_시작_드로우가_정상적으로_발생한다()
    {
        bool received = false;
        void OnDraw(DrawResult r)
        {
            if (r != null && r.Reason == DrawReason.TurnStart) received = true;
        }
        _deckService.OnCardsDrawn += OnDraw;
        try
        {
            _battleController.AdvanceTurn();
            yield return new WaitUntilWithTimeout(() => received, 5f);
            Assert.IsTrue(received, "OnCardsDrawn(TurnStart) 미수신");
        }
        finally
        {
            _deckService.OnCardsDrawn -= OnDraw;
        }
    }
}

// 타임아웃이 있는 대기 유틸리티
public class WaitUntilWithTimeout : CustomYieldInstruction
{
    private readonly Func<bool> _predicate;
    private readonly float _timeout;
    private readonly float _start;

    public override bool keepWaiting
    {
        get
        {
            if (Time.time - _start > _timeout)
            {
                Assert.Fail("테스트 시간 초과");
                return false;
            }
            return !_predicate();
        }
    }

    public WaitUntilWithTimeout(Func<bool> predicate, float timeoutSeconds)
    {
        _predicate = predicate;
        _timeout = timeoutSeconds;
        _start = Time.time;
    }
}
