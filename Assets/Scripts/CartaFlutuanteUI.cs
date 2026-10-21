using UnityEngine;
using DG.Tweening;

public class CartaFlutuanteUI : MonoBehaviour
{
    [Header("Balanço Estilo Balatro (Apenas Rotação)")]
    public float anguloMaximo = 1.8f;   // Inclinação para cada lado (-4° a +4°)
    public float duracaoCiclo = 4.2f;   // Tempo do ciclo de balanço

    private Tween tweenBalanco;

    private void OnEnable()
    {
        IniciarEfeitoFlutuante();
    }

    private void OnDisable()
    {
        PararEfeitoFlutuante();
    }

    // Mantido com o nome exato que o Selecao.cs chama!
    public void IniciarEfeitoFlutuante()
    {
        PararEfeitoFlutuante();

        // Variância aleatória de início para que as cartas não balancem 100% idênticas
        float offsetAleatorio = Random.Range(0f, 0.8f);

        // Posiciona na rotação inicial
        transform.localRotation = Quaternion.Euler(0f, 0f, -anguloMaximo);

        // Animação de pêndulo vai-e-vem usando Yoyo
        tweenBalanco = transform.DORotate(new Vector3(0f, 0f, anguloMaximo), duracaoCiclo / 2f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true)
            .SetDelay(offsetAleatorio);
    }

    // Mantido com o nome exato que o Selecao.cs chama!
    public void PararEfeitoFlutuante()
    {
        if (tweenBalanco != null && tweenBalanco.IsActive())
        {
            tweenBalanco.Kill();
        }
        transform.localRotation = Quaternion.identity;
    }
}