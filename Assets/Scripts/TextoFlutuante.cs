using UnityEngine;
using TMPro;
using DG.Tweening;

public class TextoFlutuante : MonoBehaviour
{
    public TextMeshProUGUI textoMesh;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void ExibirTexto(string mensagem, Color corTexto, Vector3 posicaoInicial)
    {
        gameObject.SetActive(true);
        transform.position = posicaoInicial;

        if (rectTransform != null)
        {
            rectTransform.anchoredPosition += new Vector2(0f, 35f);
        }

        if (textoMesh != null)
        {
            textoMesh.text = mensagem;
            textoMesh.color = corTexto;
        }

        canvasGroup.alpha = 1f;

        // Animação estendida para 1.2 segundos subindo mais alto
        Sequence seq = DOTween.Sequence();
        seq.SetUpdate(true);
        seq.Append(rectTransform.DOAnchorPosY(rectTransform.anchoredPosition.y + 110f, 1.2f).SetEase(Ease.OutQuad));
        seq.Join(canvasGroup.DOFade(0f, 1.2f).SetEase(Ease.InCubic));
        seq.OnComplete(() => {
            Destroy(gameObject);
        });
    }
}