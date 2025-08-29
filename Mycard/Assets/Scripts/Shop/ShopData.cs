// '택배 상자' 설계도
[System.Serializable]
public class ShopSessionDTO
{
    public int rerollCount;
    public SlotDTO[] slots;
}

// '개별 물품' 설계도
[System.Serializable]
public class SlotDTO
{
    public string itemId; // 카드의 고유 ID
    public bool soldOut;

    public string detail;   // 아이템 타입 ("Card", "Relic" 등)
    public int price;       // 할인이 적용된 '최종 가격'
    public bool isDeal;     // '특가' 상품이었는지 여부
}