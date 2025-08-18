using UnityEngine;

[CreateAssetMenu(fileName = "New Character", menuName = "Character", order = 2)]
public class CharacterSO : ScriptableObject
{
    public string characterName;
    public Sprite portraitSprite;  // �ʻ�ȭ �̹���
    public string description;     // �ι� ����
}
