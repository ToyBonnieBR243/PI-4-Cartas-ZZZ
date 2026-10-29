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
        bool houveMudancaTempo = false;

        // 1. Controle de Confusão
        if (tempoConfusaoRestante > 0f)
        {
            tempoConfusaoRestante -= Time.deltaTime;
            houveMudancaTempo = true;
            if (tempoConfusaoRestante <= 0f)
            {
                tempoConfusaoRestante = 0f;
                estaConfuso = false;
            }
        }

        // 2. Controle de Atordoamento
        if (tempoAtordoamentoRestante > 0f)
        {
            tempoAtordoamentoRestante -= Time.deltaTime;
            houveMudancaTempo = true;
            if (tempoAtordoamentoRestante <= 0f)
            {
                tempoAtordoamentoRestante = 0f;
                estaAtordoado = false;
            }
        }

        if (tempoRouboNicoleRestante > 0f)
        {
            tempoRouboNicoleRestante -= Time.deltaTime;
            if (tempoRouboNicoleRestante <= 0f) tempoRouboNicoleRestante = 0f;
        }

        // 3. Controle de Queimadura (Dura exatamente 2 segundos)
        if (estaQueimando)
        {
            tempoQueimacaoRestante -= Time.deltaTime;
            cronometroPulsoFogo += Time.deltaTime;
            houveMudancaTempo = true;

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
                tempoQueimacaoRestante = 0f;
                estaQueimando = false;
            }
        }

        // Atualiza a UI da carta se houver efeitos ativos
        if (houveMudancaTempo)
        {
            AtualizarTemporizadorUI();
        }
    }

    // Conta quantos estados estão ativos no momento
    private int ObterQuantidadeEstadosAtivos()
    {
        int total = 0;
        if (estaConfuso) total++;
        if (estaAtordoado) total++;
        if (estaCongelado) total++;
        if (estaQueimando) total++;
        return total;
    }

    private void AtualizarTemporizadorUI()
    {
        if (cartaUI == null) return;

        string textoTempo = "";

        if (estaConfuso && tempoConfusaoRestante > 0)
            textoTempo += $"Confuso: {Mathf.Ceil(tempoConfusaoRestante)}s\n";

        if (estaAtordoado && tempoAtordoamentoRestante > 0)
            textoTempo += $"Atordoado: {Mathf.Ceil(tempoAtordoamentoRestante)}s\n";

        if (estaQueimando && tempoQueimacaoRestante > 0)
            textoTempo += $"Queimando: {Mathf.Ceil(tempoQueimacaoRestante)}s\n";

        if (estaCongelado)
            textoTempo += "Congelado\n";

        cartaUI.AtualizarTextoTemporizador(textoTempo.Trim());
    }

    // --- MÉTODOS COM PROTEÇÃO (MÁXIMO DE 2 ESTADOS SIMULTÂNEOS) ---

    public void AplicarConfusao(float duracao)
    {
        if (estaConfuso)
        {
            tempoConfusaoRestante = duracao; // Renova se já tiver
            return;
        }

        // Se já atingiu o limite de 2 estados diferentes, bloqueia o novo
        if (ObterQuantidadeEstadosAtivos() >= 2) return;

        estaConfuso = true;
        tempoConfusaoRestante = duracao;
        AtualizarTemporizadorUI();
    }

    public void AplicarAtordoamento(float duracao)
    {
        if (estaAtordoado)
        {
            tempoAtordoamentoRestante = duracao;
            return;
        }

        if (ObterQuantidadeEstadosAtivos() >= 2) return;

        estaAtordoado = true;
        tempoAtordoamentoRestante = duracao;
        AtualizarTemporizadorUI();
    }

    public void AplicarCongelamento()
    {
        if (estaCongelado) return;

        if (ObterQuantidadeEstadosAtivos() >= 2) return;

        estaCongelado = true;
        AtualizarTemporizadorUI();
    }

    public void RemoverCongelamento()
    {
        estaCongelado = false;
        AtualizarTemporizadorUI();
    }

    public void AplicarQueimacao(int danoPorSegundo = 3)
    {
        if (estaQueimando)
        {
            tempoQueimacaoRestante = 2.0f;
            return;
        }

        if (ObterQuantidadeEstadosAtivos() >= 2) return;

        estaQueimando = true;
        danoQueimacaoPorSegundo = danoPorSegundo;
        tempoQueimacaoRestante = 2.0f;
        cronometroPulsoFogo = 0f;
        AtualizarTemporizadorUI();
    }
}