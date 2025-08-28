using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public partial class ShopUI : MonoBehaviour
{
    private void LoadAllCardData()
    {
        if (cardPool == null) cardPool = new List<CardScriptableObject>();

        //카드 로드해서 정보 가져옴
        var loadedCards = Resources.LoadAll<CardScriptableObject>(CardsPath);
        if (loadedCards != null && loadedCards.Length > 0)
        {
            var set = new HashSet<CardScriptableObject>(cardPool);
            foreach (var c in loadedCards)
                if (c != null && !set.Contains(c))
                    cardPool.Add(c);
        }
        VLog($"[ShopUI] cardPool merged count = {cardPool.Count}");
        if (cardPool.Count == 0)
            Debug.LogWarning($"[ShopUI] No cards found in inspector or Resources/{CardsPath}");

        // 카드 ID 매핑 구축 (여기가 빠지면 ImportSession에서 카드 3칸이 제대로 복원 안 됨)
        _cardIdMap.Clear();
        _cardNameMap.Clear();
        foreach (var card in cardPool)
        {
            if (card == null) continue;

            // Id 맵
            if (!string.IsNullOrEmpty(card.cardId))
            {
                if (_cardIdMap.ContainsKey(card.cardId))
                    Debug.LogWarning($"[ShopUI] Duplicate CardId: {card.cardId}");
                else
                    _cardIdMap[card.cardId] = card;
            }

            // Name 맵 (표시용 이름)
            if (!string.IsNullOrEmpty(card.cardName))
            {
                if (_cardNameMap.ContainsKey(card.cardName))
                    Debug.LogWarning($"[ShopUI] Duplicate CardName: {card.cardName}");
                else
                    _cardNameMap[card.cardName] = card;
            }
        }
        // [CCTV] 전화번호부가 완성되었는지 확인
        Debug.Log($"<color=purple>[CCTV] '카드 전화번호부'(_cardIdMap) 생성 완료. 총 {cardPool.Count}개의 카드 중 { _cardIdMap.Count}개가 등록되었습니다.</color>", this);
    }

    private void BuildDummySlots()
    {
        // _dummy는 상점에 진열될 상품 정보를 담는 '진열대' 역할을 합니다.
        // _dummy 리스트를 6칸짜리 새 리스트로 초기화합니다.
        _dummy = new List<ShopSlotVM>(6)
        {
            new ShopSlotVM{ title="Strike",       detail="Card" },
            new ShopSlotVM{ title="Defend",       detail="Card" },
            new ShopSlotVM{ title="Fireball",     detail="Card" },
            new ShopSlotVM{ title="Happy Flower", detail="Relic" },
            new ShopSlotVM{ title="Anchor",       detail="Relic" },
            new ShopSlotVM{ title="Block Potion", detail="Consumable" },
        };

        // 방금 진열한 모든 아이템('마네킹' 포함)을 하나씩 돌면서 가격을 계산하고 설정합니다.
        for (int i = 0; i < _dummy.Count; i++)
        {
            var vm = _dummy[i];
            vm.price = BasePriceOf(vm.detail, vm.title);
            _dummy[i] = vm;
        }
    }

    private ShopSlotVM ToVM(CardScriptableObject so)
    {
        var icon = so.characterSprite != null ? so.characterSprite : so.bgSprite;
        return new ShopSlotVM
        {
            cardData = so,
            title   = so.cardName,
            detail  = "Card",
            icon    = icon,
            price   = BasePriceOf("Card", so.cardName),
            soldOut = false,
            isDeal  = false,
        };
    }

    private void BuildCardSlotsInitial()
    {
        // (안전장치) 카드 데이터가 담긴 '창고'(cardPool)가 비어있으면, 함수를 즉시 종료합니다.
        if (cardPool == null || cardPool.Count == 0) return;

        // 뽑았던 카드를 다시 뽑지 않기 위한 '제외 목록'
        var exclude = new HashSet<string>();

        // 상점의 첫 3칸(카드 전용 슬롯)에 대해서만 반복 작업을 합니다.
        for (int i = 0; i < 3; i++)
        {
            var pick = DrawUniqueCard(exclude);
            if (pick == null) { _cardSources[i] = null; continue; }

            exclude.Add(pick.cardName);     // 이름 기반
            _cardSources[i] = pick;
            // 진열대(_dummy)의 i번째 칸에 있던 '마네킹'을 방금 뽑은 진짜 카드(pick) 정보로 교체합니다.
            _dummy[i] = ToVM(pick);
        }
    }

    private void RerollCardSlots()
    {
        var exclude = new HashSet<string>();
        for (int i = 0; i < 3; i++)
            if (_cardSources[i] != null)
                exclude.Add(_cardSources[i].cardName);

        for (int slot = 0; slot < 3; slot++)
        {
            if (_dummy[slot].soldOut) continue;

            var pick = DrawUniqueCard(exclude);
            if (pick == null) continue;

            exclude.Add(pick.cardName);
            _cardSources[slot] = pick;
            _dummy[slot] = ToVM(pick);
        }
    }

    private CardScriptableObject DrawUniqueCard(HashSet<string> exclude)
    {
        if (cardPool == null || cardPool.Count == 0) return null;

        var candidates = new List<CardScriptableObject>(cardPool.Count);
        foreach (var c in cardPool)
        {
            if (c == null) continue;
            if (exclude != null && exclude.Contains(c.cardName)) continue;
            candidates.Add(c);
        }
        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    // 유물/소모품 문자열 풀용
    private string DrawUnique(string[] pool, HashSet<string> exclude)
    {
        var candidates = new List<string>(pool.Length);
        for (int i = 0; i < pool.Length; i++)
        {
            string p = pool[i];
            if (!exclude.Contains(p))
                candidates.Add(p);
        }
        if (candidates.Count == 0) return null;
        int idx = Random.Range(0, candidates.Count);
        return candidates[idx];
    }

    private void OnReroll()
    {
        if (_isRerollCooling) return;

        int cost = CurrentRerollCost();
        if (Gold < cost) return;

        _isRerollCooling = true;
        if (rerollButton) rerollButton.interactable = false;

        SpendGold?.Invoke(cost);
        _rerollCount++;

        RerollCardSlots(); // 카드 3칸 교체

        // (유물/소모품 교체 로직)
        var exclude = new HashSet<string>();
        for (int i = 0; i < _dummy.Count; i++)
            if (!string.IsNullOrEmpty(_dummy[i].title))
                exclude.Add(_dummy[i].title);

        for (int i = 3; i < _dummy.Count; i++)
        {
            if (_dummy[i].soldOut) continue;
            string[] pool = _dummy[i].detail == "Relic" ? RelicsPool : ConsumablesPool;
            string newId = DrawUnique(pool, exclude);
            if (string.IsNullOrEmpty(newId)) continue;

            exclude.Add(newId);
            var vm = _dummy[i];
            vm.title = newId;
            vm.icon = null;
            vm.price = BasePriceOf(vm.detail, vm.title);
            _dummy[i] = vm;
        }

        OnSessionChanged?.Invoke(); // 상태 변화 감지로 db 저장 할수 있게

        ApplyDeals();
        RefreshViews();
        RefreshTopbar();
        _isRerollCooling = true;
        StartCoroutine(RerollCooldown());
        
    }

    // 쿨다운 코루틴
    private IEnumerator RerollCooldown()
    {
        VLog($"[ShopUI] Reroll cooldown start ({rerollCooldownSec}s)");
        // UI는 보통 TimeScale 0에서도 동작해야 하니 unscaled 사용
        float t = 0f;
        while (t < rerollCooldownSec)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        _isRerollCooling = false; // 락 해제
        RefreshTopbar();          // 조건(돈/후보) 맞으면 자동 재활성
        VLog("[ShopUI] Reroll cooldown end");
    }
    
}