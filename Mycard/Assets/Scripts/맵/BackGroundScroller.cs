using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BackGroundScroller : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Tooltip("��ũ�� �ӵ��� �����մϴ�.")]
    public float scrollSpeed = 0.5f;

    // UI RawImage ������Ʈ�� ���� ����
    private RawImage backgroundImage;
    // �巡�� ���� ���� ���콺 ��ġ
    private Vector2 dragStartPosition;

    void Awake()
    {
        // �� ��ũ��Ʈ�� �پ��ִ� ������Ʈ�� RawImage ������Ʈ�� �����ɴϴ�.
        backgroundImage = GetComponent<RawImage>();
    }

    // �巡�װ� ���۵� �� ȣ��Ǵ� �Լ�
    public void OnBeginDrag(PointerEventData eventData)
    {
        // ���� ���콺 ��ġ�� ����մϴ�.
        dragStartPosition = eventData.position;
    }

    // �巡���ϴ� ���� ����ؼ� ȣ��Ǵ� �Լ�
    public void OnDrag(PointerEventData eventData)
    {
        // ���� ��ġ�� ���� ��ġ�� ���̸� ����մϴ�.
        Vector2 delta = eventData.position - dragStartPosition;

        // y��(����)���� �󸶳� ���������� ����մϴ�.
        // ȭ�� ���̷� �����־�, ȭ�� ũ��� ������� ������ �ӵ��� �����̰� �մϴ�.
        float moveY = delta.y / Screen.height * scrollSpeed;

        // RawImage�� uvRect�� �����Ͽ� �ؽ�ó�� �����Դϴ�.
        // �̰��� �ٷ� ����� ��ũ�ѵǴ� ��ó�� ���̰� �ϴ� �ٽ� �����Դϴ�.
        Rect currentRect = backgroundImage.uvRect;
        currentRect.y += moveY;
        backgroundImage.uvRect = currentRect;

        // ���� ����� ���� ���� ��ġ�� �ٽ� ���� ��ġ�� ������Ʈ�մϴ�.
        dragStartPosition = eventData.position;
    }

    // �巡�װ� ������ �� ȣ��Ǵ� �Լ� (������ ����Ӵϴ�)
    public void OnEndDrag(PointerEventData eventData)
    {
        // �ʿ��ϴٸ� �巡�װ� ������ ���� ������ ���⿡ �߰��� �� �ֽ��ϴ�.
    }
}
