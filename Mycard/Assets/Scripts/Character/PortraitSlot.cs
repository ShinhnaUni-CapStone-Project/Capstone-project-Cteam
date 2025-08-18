using UnityEngine;
using UnityEngine.UI;

public class PortraitSlot : MonoBehaviour
{
    [SerializeField] private Image portraitImage;
    private CharacterSO character;

    // �� �������� Ȯ��(��������Ʈ ������� �� �������� ����)
    public bool IsEmpty => portraitImage == null || portraitImage.sprite == null;

    public void SetSlot(CharacterSO newCharacter)
    {
        character = newCharacter;
        portraitImage.sprite = character.portraitSprite;
        portraitImage.enabled = true;

        // Ȥ�� ���İ� 0�� ��� ���
        var c = portraitImage.color;
        c.a = 1f;
        portraitImage.color = c;
    }

    public void ClearSlot()
    {
        character = null;
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
        }
    }
}