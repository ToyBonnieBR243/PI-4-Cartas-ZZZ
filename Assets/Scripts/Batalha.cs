using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public enum AcaoCartaBatalha
{
    Nenhuma,
    Atacar,
    Defender
}

public class GerenciadorBatalha : MonoBehaviour
{
    [Header("Slots na Arena (Transforms do Canvas)")]
    public Transform slotP1_Carta0;
    public Transform slotP1_Carta1;
    public Transform slotP2_Carta0;
    public Transform slotP2_Carta1;

    [Header("Prefab da Carta de Batalha")]
    public GameObject prefabCartaBatalha;

    [Header("Containers / Painéis de Controle de UI")]
    public GameObject painelControleP1; // Pai dos botões do P1
    public GameObject painelControleP2; // Pai dos botões do P2

    [Header("UI Geral")]
    public TextMeshProUGUI textoTurno;

    [Header("Fallback para Teste Direto na Cena")]
    public CartaData[] cartasFallbackParaTeste;

    // Instâncias das Cartas
    private CartaBatalhaUI[] cartasP1 = new CartaBatalhaUI[2];
    private CartaBatalhaUI[] cartasP2 = new CartaBatalhaUI[2];

    // Escolhas de Ação da rodada
    private AcaoCartaBatalha[] acoesP1 = new AcaoCartaBatalha[2];
    private AcaoCartaBatalha[] acoesP2 = new AcaoCartaBatalha[2];

    // Estados de Escolha do Turno
    private int jogadorDecidindoAtual = 1; // 1 para P1, 2 para P2
    private int numeroRodada = 1;
    private bool emResolucaoDeTurno = false;

    private void Start()
    {
        CarregarEInicializarCartas();
        IniciarFaseDeComandos();
    }

    private void CarregarEInicializarCartas()
    {
        List<CartaData> listaP1 = DadosPartida.cartasJogador1;
        List<CartaData> listaP2 = DadosPartida.cartasJogador2;
        List<AtributosRuntime> attrP1 = DadosPartida.atributosFinalJ1;
        List<AtributosRuntime> attrP2 = DadosPartida.atributosFinalJ2;

        bool usarFallback = (listaP1.Count < 2 || listaP2.Count < 2);

        if (usarFallback && cartasFallbackParaTeste.Length >= 4)
        {
            listaP1 = new List<CartaData> { cartasFallbackParaTeste[0], cartasFallbackParaTeste[1] };
            listaP2 = new List<CartaData> { cartasFallbackParaTeste[2], cartasFallbackParaTeste[3] };
            attrP1 = new List<AtributosRuntime> { GerarBuildSugerida(cartasFallbackParaTeste[0]), GerarBuildSugerida(cartasFallbackParaTeste[1]) };
            attrP2 = new List<AtributosRuntime> { GerarBuildSugerida(cartasFallbackParaTeste[2]), GerarBuildSugerida(cartasFallbackParaTeste[3]) };
        }

        cartasP1[0] = InstanciarCarta(listaP1[0], attrP1[0], slotP1_Carta0);
        cartasP1[1] = InstanciarCarta(listaP1[1], attrP1[1], slotP1_Carta1);

        cartasP2[0] = InstanciarCarta(listaP2[0], attrP2[0], slotP2_Carta0);
        cartasP2[1] = InstanciarCarta(listaP2[1], attrP2[1], slotP2_Carta1);
    }

    private CartaBatalhaUI InstanciarCarta(CartaData data, AtributosRuntime attr, Transform slotTarget)
    {
        if (slotTarget == null || prefabCartaBatalha == null) return null;

        GameObject obj = Instantiate(prefabCartaBatalha);

        RectTransform rect = obj.GetComponent<RectTransform>();
        rect.SetParent(slotTarget, false);
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one;

        CartaBatalhaUI cardUI = obj.GetComponent<CartaBatalhaUI>();
        if (cardUI != null)
        {
            cardUI.InicializarCarta(data, attr);

            if (data.nomePersonagem.Trim() == "Caesar King")
            {
                cardUI.SetarEscudoInicial(30);
            }
        }
        return cardUI;
    }

    private AtributosRuntime GerarBuildSugerida(CartaData carta)
    {
        AtributosRuntime attr = new AtributosRuntime(carta);
        string nome = carta.nomePersonagem.Trim();

        if (nome == "Hoshimi Miyabi") { attr.velocidade = 10; attr.destreza = 10; }
        else if (nome == "Astra Yao") { attr.intelecto = 10; attr.velocidade = 10; }
        else if (nome == "Phaethon") { attr.intelecto = 10; attr.raiva = 10; }
        else if (nome == "Ellen Joe") { attr.forca = 10; attr.destreza = 10; }
        else if (nome == "Caesar King") { attr.forca = 10; attr.intelecto = 10; }
        else if (nome == "Jane Doe") { attr.velocidade = 10; attr.intelecto = 5; attr.destreza = 5; }
        else if (nome == "Anby") { attr.forca = 7; attr.velocidade = 7; attr.destreza = 6; }
        else if (nome == "Billy Kid") { attr.destreza = 10; attr.raiva = 10; }
        else if (nome == "Nicole Demara") { attr.forca = 10; attr.raiva = 10; }
        else { attr.forca = 10; attr.velocidade = 10; }

        attr.pontosRestantes = 0;
        return attr;
    }

    // ==========================================
    // FASE DE COMANDOS ALTERNADOS
    // ==========================================
    public void IniciarFaseDeComandos()
    {
        emResolucaoDeTurno = false;
        jogadorDecidindoAtual = 1;

        // Limpa escudos temporários (10 DEF) da rodada anterior
        ResetarEscudosTemporarios(cartasP1);
        ResetarEscudosTemporarios(cartasP2);

        for (int i = 0; i < 2; i++)
        {
            acoesP1[i] = AcaoCartaBatalha.Nenhuma;
            acoesP2[i] = AcaoCartaBatalha.Nenhuma;
        }

        AtualizarVisibilidadePaineis();
    }

    private void ResetarEscudosTemporarios(CartaBatalhaUI[] time)
    {
        foreach (var carta in time)
        {
            if (carta != null && carta.VidaAtual > 0)
            {
                if (carta.CartaDados == null || carta.CartaDados.nomePersonagem.Trim() != "Caesar King")
                {
                    carta.SetarEscudoInicial(0);
                }
            }
        }
    }

    private void AtualizarVisibilidadePaineis()
    {
        if (emResolucaoDeTurno)
        {
            if (painelControleP1 != null) painelControleP1.SetActive(false);
            if (painelControleP2 != null) painelControleP2.SetActive(false);
            if (textoTurno != null) textoTurno.text = "RESOLVENDO TURNO...";
            return;
        }

        bool turnoP1 = (jogadorDecidindoAtual == 1);

        if (painelControleP1 != null) painelControleP1.SetActive(turnoP1);
        if (painelControleP2 != null) painelControleP2.SetActive(!turnoP1);

        if (textoTurno != null)
        {
            textoTurno.text = turnoP1 ? "Jogador 1: Escolha sua Ação" : "Jogador 2: Escolha sua Ação";
        }
    }

    // --- MÉTODOS PARA O JOGADOR 1 (CARTA 0) ---
    public void P1_Carta0_Atacar() { if (jogadorDecidindoAtual == 1 && !emResolucaoDeTurno) { acoesP1[0] = AcaoCartaBatalha.Atacar; VerificarConclusaoEscolhaP1(); } }
    public void P1_Carta0_Defender() { if (jogadorDecidindoAtual == 1 && !emResolucaoDeTurno) { acoesP1[0] = AcaoCartaBatalha.Defender; VerificarConclusaoEscolhaP1(); } }

    // --- MÉTODOS PARA O JOGADOR 1 (CARTA 1) ---
    public void P1_Carta1_Atacar() { if (jogadorDecidindoAtual == 1 && !emResolucaoDeTurno) { acoesP1[1] = AcaoCartaBatalha.Atacar; VerificarConclusaoEscolhaP1(); } }
    public void P1_Carta1_Defender() { if (jogadorDecidindoAtual == 1 && !emResolucaoDeTurno) { acoesP1[1] = AcaoCartaBatalha.Defender; VerificarConclusaoEscolhaP1(); } }

    // --- MÉTODOS PARA O JOGADOR 2 (CARTA 0) ---
    public void P2_Carta0_Atacar() { if (jogadorDecidindoAtual == 2 && !emResolucaoDeTurno) { acoesP2[0] = AcaoCartaBatalha.Atacar; VerificarConclusaoEscolhaP2(); } }
    public void P2_Carta0_Defender() { if (jogadorDecidindoAtual == 2 && !emResolucaoDeTurno) { acoesP2[0] = AcaoCartaBatalha.Defender; VerificarConclusaoEscolhaP2(); } }

    // --- MÉTODOS PARA O JOGADOR 2 (CARTA 1) ---
    public void P2_Carta1_Atacar() { if (jogadorDecidindoAtual == 2 && !emResolucaoDeTurno) { acoesP2[1] = AcaoCartaBatalha.Atacar; VerificarConclusaoEscolhaP2(); } }
    public void P2_Carta1_Defender() { if (jogadorDecidindoAtual == 2 && !emResolucaoDeTurno) { acoesP2[1] = AcaoCartaBatalha.Defender; VerificarConclusaoEscolhaP2(); } }

    private void VerificarConclusaoEscolhaP1()
    {
        bool c0Pronta = (cartasP1[0] == null || cartasP1[0].VidaAtual <= 0 || acoesP1[0] != AcaoCartaBatalha.Nenhuma);
        bool c1Pronta = (cartasP1[1] == null || cartasP1[1].VidaAtual <= 0 || acoesP1[1] != AcaoCartaBatalha.Nenhuma);

        if (c0Pronta && c1Pronta)
        {
            jogadorDecidindoAtual = 2;
            AtualizarVisibilidadePaineis();
        }
    }

    private void VerificarConclusaoEscolhaP2()
    {
        bool c0Pronta = (cartasP2[0] == null || cartasP2[0].VidaAtual <= 0 || acoesP2[0] != AcaoCartaBatalha.Nenhuma);
        bool c1Pronta = (cartasP2[1] == null || cartasP2[1].VidaAtual <= 0 || acoesP2[1] != AcaoCartaBatalha.Nenhuma);

        if (c0Pronta && c1Pronta)
        {
            emResolucaoDeTurno = true;
            AtualizarVisibilidadePaineis();
            StartCoroutine(ExecutarResolucaoTurnoRoutine());
        }
    }

    // ==========================================
    // RESOLUÇÃO SEQUENCIAL DO TURNO
    // ==========================================
    private IEnumerator ExecutarResolucaoTurnoRoutine()
    {
        // 1. Aplica o escudo temporário de 10 DEF
        AplicarDefesasSeNecessario(cartasP1, acoesP1);
        AplicarDefesasSeNecessario(cartasP2, acoesP2);

        yield return new WaitForSeconds(0.6f);

        // 2. Ordena por Velocidade
        List<ParticipanteTurno> filaAcoes = CriarFilaOrdenadaPorVelocidade();

        foreach (var participante in filaAcoes)
        {
            if (participante.carta == null || participante.carta.VidaAtual <= 0) continue;
            if (participante.acao != AcaoCartaBatalha.Atacar) continue;

            if (participante.carta.Status != null && (participante.carta.Status.estaCongelado || participante.carta.Status.estaAtordoado))
            {
                yield return new WaitForSeconds(0.4f);
                continue;
            }

            CartaBatalhaUI alvo = ObterAlvoPorSlot(participante);

            if (alvo != null)
            {
                bool acaoConcluida = false;
                ExecutarAtaqueBasico(participante.carta, alvo, () => {
                    acaoConcluida = true;
                });

                yield return new WaitUntil(() => acaoConcluida);
                yield return new WaitForSeconds(0.3f);
            }
        }

        yield return new WaitForSeconds(0.8f);

        numeroRodada++;
        IniciarFaseDeComandos();
    }

    private void AplicarDefesasSeNecessario(CartaBatalhaUI[] time, AcaoCartaBatalha[] acoes)
    {
        for (int i = 0; i < 2; i++)
        {
            if (time[i] != null && time[i].VidaAtual > 0 && acoes[i] == AcaoCartaBatalha.Defender)
            {
                time[i].AdicionarEscudo(10);
                time[i].MostrarTextoPopUp("DEFESA!", new Color(0f, 0.85f, 1f));
            }
        }
    }

    private CartaBatalhaUI ObterAlvoPorSlot(ParticipanteTurno participante)
    {
        CartaBatalhaUI[] timeInimigo = participante.eDoP1 ? cartasP2 : cartasP1;
        int slotOrigem = participante.indiceSlot;

        if (timeInimigo[slotOrigem] != null && timeInimigo[slotOrigem].VidaAtual > 0)
        {
            return timeInimigo[slotOrigem];
        }

        int outroSlot = (slotOrigem == 0) ? 1 : 0;
        if (timeInimigo[outroSlot] != null && timeInimigo[outroSlot].VidaAtual > 0)
        {
            return timeInimigo[outroSlot];
        }

        return null;
    }

    private List<ParticipanteTurno> CriarFilaOrdenadaPorVelocidade()
    {
        List<ParticipanteTurno> lista = new List<ParticipanteTurno>();

        for (int i = 0; i < 2; i++)
        {
            if (cartasP1[i] != null && cartasP1[i].VidaAtual > 0)
            {
                int vel = cartasP1[i].Atributos != null ? cartasP1[i].Atributos.velocidade : 0;
                lista.Add(new ParticipanteTurno(cartasP1[i], acoesP1[i], vel, true, i));
            }
            if (cartasP2[i] != null && cartasP2[i].VidaAtual > 0)
            {
                int vel = cartasP2[i].Atributos != null ? cartasP2[i].Atributos.velocidade : 0;
                lista.Add(new ParticipanteTurno(cartasP2[i], acoesP2[i], vel, false, i));
            }
        }

        lista.Sort((a, b) => b.velocidade.CompareTo(a.velocidade));
        return lista;
    }

    private void ExecutarAtaqueBasico(CartaBatalhaUI atacante, CartaBatalhaUI alvo, System.Action aoTerminarAtaque)
    {
        if (atacante == null || alvo == null)
        {
            aoTerminarAtaque?.Invoke();
            return;
        }

        if (atacante.CartaDados != null && atacante.CartaDados.nomePersonagem.Trim() == "Astra Yao")
        {
            aoTerminarAtaque?.Invoke();
            return;
        }

        string nomeAtacante = atacante.CartaDados != null ? atacante.CartaDados.nomePersonagem.Trim() : "";
        string nomeAlvo = alvo.CartaDados != null ? alvo.CartaDados.nomePersonagem.Trim() : "";

        Vector3 posicaoDestinoAtaque = alvo.transform.position;

        if (nomeAtacante == "Billy Kid" || (atacante.CartaDados != null && atacante.CartaDados.alcanceSkill == TipoAlcanceAtaque.AmbasAsCartas))
        {
            CartaBatalhaUI[] timeInimigo = (atacante == cartasP1[0] || atacante == cartasP1[1]) ? cartasP2 : cartasP1;
            if (timeInimigo[0] != null && timeInimigo[1] != null)
            {
                posicaoDestinoAtaque = (timeInimigo[0].transform.position + timeInimigo[1].transform.position) / 2f;
            }
        }

        atacante.AnimarInvestidaEVoltar(posicaoDestinoAtaque, () =>
        {
            if (alvo == null || atacante == null)
            {
                aoTerminarAtaque?.Invoke();
                return;
            }

            int intAlvo = alvo.Atributos != null ? alvo.Atributos.intelecto : 0;
            int bonusEllen = alvo.Status != null ? alvo.Status.bonusIntelectoEllen : 0;
            float chanceEsquiva = (intAlvo + bonusEllen) * 0.03f;

            if (atacante.Status != null && atacante.Status.estaConfuso) chanceEsquiva += 0.20f;

            bool esquivou = Random.value < chanceEsquiva;

            if (esquivou)
            {
                alvo.AnimarEsquiva(atacante.transform.position - alvo.transform.position);
                if (nomeAlvo == "Ellen Joe" && alvo.Status != null) alvo.Status.bonusIntelectoEllen += 2;
                aoTerminarAtaque?.Invoke();
                return;
            }

            if (nomeAlvo == "Ellen Joe" && alvo.Status != null) alvo.Status.bonusIntelectoEllen = 0;

            int forcaAtacante = atacante.Atributos != null ? atacante.Atributos.forca : 0;
            int destrezaAtacante = atacante.Atributos != null ? atacante.Atributos.destreza : 0;
            int raivaAtacante = atacante.Atributos != null ? atacante.Atributos.raiva : 0;

            int danoBase = Random.Range(5 + forcaAtacante, 11 + forcaAtacante);
            if (nomeAtacante == "Billy Kid") danoBase = Mathf.Max(1, danoBase / 2);

            float chanceCritico = destrezaAtacante * 0.07f;
            bool foiCritico = Random.value < chanceCritico;

            if (foiCritico) danoBase = Mathf.RoundToInt(danoBase * 1.5f);

            if (atacante.VidaMaxima > 0 && ((float)atacante.VidaAtual / atacante.VidaMaxima) < 0.30f)
            {
                float bonusRaiva = 1f + (raivaAtacante * 0.06f);
                danoBase = Mathf.RoundToInt(danoBase * bonusRaiva);
            }

            if (foiCritico)
            {
                alvo.PiscarCarta(Color.red);
                alvo.MostrarTextoPopUp("CRÍTICO!", Color.red);
                StartCoroutine(RotinaCameraLentaCritico(0.15f));

                if (Camera.main != null)
                {
                    Camera.main.transform.DOShakePosition(0.35f, strength: 45f, vibrato: 25).SetUpdate(true);
                }
            }
            else
            {
                alvo.PiscarCarta(Color.white);

                if (Camera.main != null)
                {
                    Camera.main.transform.DOShakePosition(0.2f, strength: 25f, vibrato: 18);
                }
            }

            alvo.ReceberDano(danoBase);

            atacante.AdicionarCargaSkill(1);
            atacante.AdicionarCargaUlt(1);

            aoTerminarAtaque?.Invoke();
        });
    }

    // --- CORROTINA DA CÂMERA LENTA NO CRÍTICO ---
    private IEnumerator RotinaCameraLentaCritico(float duracaoReal)
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(duracaoReal);
        Time.timeScale = 1.0f;
    }
}

// ==========================================
// CLASSE AUXILIAR DE SUPORTE AO TURNO
// ==========================================
public class ParticipanteTurno
{
    public CartaBatalhaUI carta;
    public AcaoCartaBatalha acao;
    public int velocidade;
    public bool eDoP1;
    public int indiceSlot;

    public ParticipanteTurno(CartaBatalhaUI c, AcaoCartaBatalha a, int v, bool p1, int slot)
    {
        carta = c;
        acao = a;
        velocidade = v;
        eDoP1 = p1;
        indiceSlot = slot;
    }
}