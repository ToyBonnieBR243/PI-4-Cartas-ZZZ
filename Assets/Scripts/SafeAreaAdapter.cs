using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class SafeAreaAdapter : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rect ultimaSafeArea = new Rect(0, 0, 0, 0);

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        AplicarSafeArea();
    }

    private void Update()
    {
        // Garante a atualização caso a tela rotacione ou o simulador mude
        if (ultimaSafeArea != Screen.safeArea)
        {
            AplicarSafeArea();
        }
    }

    private void AplicarSafeArea()
    {
        Rect safeArea = Screen.safeArea;
        ultimaSafeArea = safeArea;

        // Converte a área segura (em pixels) para proporções relativas (0 a 1)
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        // Ajusta as âncoras do RectTransform para coincidir exatamente com a Safe Area
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
    }
}