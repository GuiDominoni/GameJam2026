using UnityEngine;
using DG.Tweening;

/// <summary>
/// Coloque este script na própria Camera. Ele move a câmera da posição de menu
/// (a que você já deixou enquadrada na cena) até ficar centralizada no player,
/// passa a segui-lo, e depois faz o zoom out final após o diálogo.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraIntroController : MonoBehaviour
{
    [Header("Alvo")]
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 followOffset = new Vector3(0f, 2f, -10f);

    [Header("Transição inicial (menu -> player)")]
    [SerializeField] private float transitionDuration = 1.2f;
    [SerializeField] private Ease transitionEase = Ease.InOutSine;

    [Header("Zoom out pós-diálogo (defina aqui os valores)")]
    [SerializeField] private float zoomOutAmount = 2f; // somado ao Orthographic Size, ou ao FOV
    [SerializeField] private float zoomOutDuration = 1f;

    private Camera cam;
    private bool isFollowing;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    public void StartFollowingPlayer()
    {
        if (player == null)
        {
            Debug.LogWarning("[CameraIntroController] 'player' não foi atribuído no Inspector.");
            return;
        }

        transform.DOKill();
        isFollowing = false;

        Vector3 targetPos = player.position + followOffset;

        transform.DOMove(targetPos, transitionDuration)
            .SetEase(transitionEase)
            .OnComplete(() =>
            {
                // Garante que a câmera fique exatamente na posição desejada
                transform.position = player.position + followOffset;

                // Só depois começa o follow
                isFollowing = true;
            });
    }
    private void LateUpdate()
    {
        if (!isFollowing || player == null) return;

        // Grudado direto no player. Se quiser um follow mais suave, troque por:
        // transform.position = Vector3.Lerp(transform.position, player.position + followOffset, Time.deltaTime * followSmooth);
        transform.position = player.position + followOffset;
    }

    public void ZoomOut()
    {
        cam.DOKill();

        if (cam.orthographic)
        {
            cam.DOOrthoSize(cam.orthographicSize + zoomOutAmount, zoomOutDuration)
                .SetEase(Ease.OutSine);
        }
        else
        {
            cam.DOFieldOfView(cam.fieldOfView + zoomOutAmount, zoomOutDuration)
                .SetEase(Ease.OutSine);
        }
    }
}