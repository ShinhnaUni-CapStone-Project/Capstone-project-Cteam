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
}