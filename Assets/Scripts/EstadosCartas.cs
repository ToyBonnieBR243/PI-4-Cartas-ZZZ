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
    public float tempoQueimacaoRestante = 0f;
    public float tempoRouboNicoleRestante = 0f;

    [Header("Acúmulos / Contadores de Passivas")]
    public int contagemGolpesAcertados = 0;
    public int bonusIntelectoEllen = 0;
    public int danoQueimacaoPorSegundo = 3;

    private CartaBatalhaUI cartaUI;
    private float cronometroPulsoFogo = 0f;

    private void Awake()
    {
        cartaUI = GetComponent<CartaBatalhaUI>();
    }

    private void Update()
    {
        // 1. Controle de Confusão
        if (tempoConfusaoRestante > 0f)
        {
            tempoConfusaoRestante -= Time.deltaTime;
            if (tempoConfusaoRestante <= 0f) estaConfuso = false;
        }

        // 2. Controle de Atordoamento
        if (tempoAtordoamentoRestante > 0f)
        {
            tempoAtordoamentoRestante -= Time.deltaTime;
            if (tempoAtordoamentoRestante <= 0f) estaAtordoado = false;
        }

        if (tempoRouboNicoleRestante > 0f)
        {
            tempoRouboNicoleRestante -= Time.deltaTime;
        }

        // 3. Controle de Queimadura (Dura exatamente 2 segundos)
        if (estaQueimando)
        {
            tempoQueimacaoRestante -= Time.deltaTime;
            cronometroPulsoFogo += Time.deltaTime;

            // Pulso de dano a cada 0.6s durante os 2s
            if (cronometroPulsoFogo >= 0.6f && cartaUI != null && cartaUI.VidaAtual > 0)
            {
                cronometroPulsoFogo = 0f;
                int danoFogo = danoQueimacaoPorSegundo;

                cartaUI.ReceberDano(danoFogo);
                cartaUI.PiscarCarta(new Color(1f, 0.45f, 0f)); // Pisca em Laranja Fogo
            }

            if (tempoQueimacaoRestante <= 0f)
            {
                estaQueimando = false;
            }
        }
    }

    // --- MÉTODOS COM PROTEÇÃO CONTRA RE-ACÚMULO DE STATUS ---

    public void AplicarConfusao(float duracao)
    {
        if (estaConfuso) return; // Impede acumular se já estiver confuso
        estaConfuso = true;
        tempoConfusaoRestante = duracao;
    }

    public void AplicarAtordoamento(float duracao)
    {
        if (estaAtordoado) return; // Impede acumular se já estiver atordoado
        estaAtordoado = true;
        tempoAtordoamentoRestante = duracao;
    }

    public void AplicarCongelamento()
    {
        if (estaCongelado) return; // Impede acumular se já estiver congelado
        estaCongelado = true;
    }

    public void RemoverCongelamento()
    {
        estaCongelado = false;
    }

    public void AplicarQueimacao(int danoPorSegundo = 3)
    {
        if (estaQueimando) return; // Impede acumular se já estiver queimando

        estaQueimando = true;
        danoQueimacaoPorSegundo = danoPorSegundo;
        tempoQueimacaoRestante = 2.0f; // Duração exata de 2 segundos
        cronometroPulsoFogo = 0f;
    }
}