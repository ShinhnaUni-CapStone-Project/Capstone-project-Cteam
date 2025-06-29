using UnityEngine;
using DG.Tweening;

/// <summary>
/// ī�� ���� �� �ܺ� ȣ��� ī�޶� �ε巴�� �̵�/ȸ����Ű��,
/// �ʿ� �� ����ġ�� ���ͽ�Ű�� �̱��� ��� ��Ʈ�ѷ��Դϴ�.
/// </summary>
public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    [Tooltip("�� ī�޶�(transform) (����θ� Camera.main �ڵ� �Ҵ�)")]
    public Camera mainCamera;
    [Tooltip("ī�޶� ���� ��ġ/ȸ���� Transform")]
    public Transform homeTransform;
    [Tooltip("ī�޶� ��ġ/ȸ���� Transform")]
    public Transform battleTransform;
    public Transform handTransform;
    [Tooltip("�̵� �� ȸ�� �ð� (��)")]
    public float moveDuration = 1f;
    [Tooltip("��¡ � ����(DOTween Ease)")]
    public Ease easeType = Ease.InOutSine;

    private void Awake()
    {
        // �̱��� �ν��Ͻ� �Ҵ�
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    /// <summary>
    /// ������ Transform ��ġ�� ȸ������ ī�޶� �ε巴�� �̵�/ȸ����ŵ�ϴ�.
    /// </summary>
    /// <param name="target">�̵�/ȸ�� ��ǥ Transform</param>
    public void MoveTo(Transform target)
    {
        if (mainCamera == null) return;
        Transform cam = mainCamera.transform;

        cam.DOKill();
        // ��ġ
        cam.DOMove(target.position, moveDuration).SetEase(easeType);
        // ȸ��
        cam.DORotateQuaternion(target.rotation, moveDuration).SetEase(easeType);
    }

  
 
}
