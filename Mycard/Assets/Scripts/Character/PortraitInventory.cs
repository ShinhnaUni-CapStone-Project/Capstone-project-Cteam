using UnityEngine;

public class PortraitInventory : MonoBehaviour
{
    public PortraitSlot[] portraitSlots;
    public static PortraitInventory instance;

    private void Awake()
    {
        instance = this;
    }

    public void AddCharacter(CharacterSO character)
    {
        foreach (var slot in portraitSlots)
        {
            // �ڽ� ������ �ƴ϶� '��������Ʈ�� �������'�� üũ
            if (slot != null && slot.IsEmpty)
            {
                slot.SetSlot(character);
                return;
            }
        }

        Debug.LogWarning("�� �ʻ�ȭ ������ �����ϴ�.");
    }
}
