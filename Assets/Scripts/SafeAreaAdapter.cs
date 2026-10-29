using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaAdapter : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect ultimaSafeArea = new Rect(0, 0, 0, 0);
    private Canvas canvasPai;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasPai = GetComponentInParent<Canvas>();
        AplicarSafeArea();
    }

    private void Update()
    {
        if (ultimaSafeArea != Screen.safeArea)
        {
            AplicarSafeArea();
        }
    }

    private void AplicarSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        ultimaSafeArea = safeArea;

        // Se o canvas estiver em modo Overlay, usamos Screen.width e Screen.height diretamente
        float larguraTela = Screen.width;
        float alturaTela = Screen.height;

        if (canvasPai != null && canvasPai.renderMode == RenderMode.ScreenSpaceCamera && canvasPai.worldCamera != null)
        {
            // Caso utilize Camera, ajusta para os limites da câmera
            larguraTela = canvasPai.pixelRect.width;
            alturaTela = canvasPai.pixelRect.height;
        }

        // Converte a área segura em proporções normalizadas (0 a 1)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= larguraTela;
        anchorMin.y /= alturaTela;
        anchorMax.x /= larguraTela;
        anchorMax.y /= alturaTela;

        // Evita valores inválidos de clamping
        anchorMin.x = Mathf.Clamp01(anchorMin.x);
        anchorMin.y = Mathf.Clamp01(anchorMin.y);
        anchorMax.x = Mathf.Clamp01(anchorMax.x);
        anchorMax.y = Mathf.Clamp01(anchorMax.y);

        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;

        // Zera as margens para encaixar perfeitamente nas âncoras calculadas
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }
}