using UnityEngine;

public class EstadoStatusCarta : MonoBehaviour
{
    [Header("Estados de Controle")]
    public bool estaCongelado = false;
    public bool estaAtordoado = false;
    public bool estaConfuso = false;
    public bool estaQueimando = false;

    [Header("Temporizadores")]
    public float tempoConfusaoRestante = 0f;
    public float tempoAtordoamentoRestante = 0f;
    public float tempoRouboNicoleRestante = 0f;

    [Header("Acúmulos / Contadores de Passivas")]
    public int contagemGolpesAcertados = 0;
    public int bonusIntelectoEllen = 0;
    public int danoQueimacaoPorSegundo = 2;

    private CartaBatalhaUI cartaUI;

    private void Awake()
    {
        cartaUI = GetComponent<CartaBatalhaUI>();
    }

    private void Update()
    {
        if (tempoConfusaoRestante > 0f)
        {
            tempoConfusaoRestante -= Time.deltaTime;
            if (tempoConfusaoRestante <= 0f) estaConfuso = false;
        }

        if (tempoAtordoamentoRestante > 0f)
        {
            tempoAtordoamentoRestante -= Time.deltaTime;
            if (tempoAtordoamentoRestante <= 0f) estaAtordoado = false;
        }

        if (tempoRouboNicoleRestante > 0f)
        {
            tempoRouboNicoleRestante -= Time.deltaTime;
        }

        if (estaQueimando && cartaUI != null && cartaUI.VidaAtual > 0)
        {
            int danoAtual = (estaCongelado || estaAtordoado) ? danoQueimacaoPorSegundo * 2 : danoQueimacaoPorSegundo;
            if (Random.value < Time.deltaTime)
            {
                cartaUI.ReceberDano(danoAtual);
            }
        }
    }

    public void AplicarConfusao(float duracao)
    {
        estaConfuso = true;
        tempoConfusaoRestante = duracao;
    }

    public void AplicarAtordoamento(float duracao)
    {
        estaAtordoado = true;
        tempoAtordoamentoRestante = duracao;
    }

    public void AplicarCongelamento()
    {
        estaCongelado = true;
    }

    public void RemoverCongelamento()
    {
        estaCongelado = false;
    }

    public void AplicarQueimacao(int danoPorSegundo = 2)
    {
        estaQueimando = true;
        danoQueimacaoPorSegundo = danoPorSegundo;
    }
}