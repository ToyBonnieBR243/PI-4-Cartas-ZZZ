using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public class GerenciadorBatalha : MonoBehaviour
{
    [Header("Slots na Arena (Transforms do Canvas)")]
    public Transform slotP1_Carta0;
    public Transform slotP1_Carta1;
    public Transform slotP2_Carta0;
    public Transform slotP2_Carta1;

    [Header("Prefab da Carta de Batalha")]
    public GameObject prefabCartaBatalha;

    [Header("UI de Controle do Jogador 1 (Azul)")]
    public Button botaoFinalizarP1;
    public Button botaoDefenderP1;
    public TextMeshProUGUI textoBotaoDefenderP1;

    [Header("UI de Controle do Jogador 2 (Vermelho)")]
    public Button botaoFinalizarP2;
    public Button botaoDefenderP2;
    public TextMeshProUGUI textoBotaoDefenderP2;

    [Header("UI Geral")]
    public TextMeshProUGUI textoTurno;

    [Header("Fallback para Teste Direto na Cena")]
    public CartaData[] cartasFallbackParaTeste;

    // Referências das instâncias ativas
    private CartaBatalhaUI[] cartasP1 = new CartaBatalhaUI[2];
    private CartaBatalhaUI[] cartasP2 = new CartaBatalhaUI[2];

    // Registra se as cartas estão em postura defensiva na rodada
    private bool[] emPosturaDefensivaP1 = new bool[2];
    private bool[] emPosturaDefensivaP2 = new bool[2];
    private bool p1EmDefesaGlobal = false;
    private bool p2EmDefesaGlobal = false;

    // Confirmações de encerramento da fase de planejamento
    private bool p1Pronto = false;
    private bool p2Pronto = false;

    // Temporizadores independentes para cooldown de ataque
    private float[] cdAtaqueP1 = new float[2];
    private float[] cdAtaqueP2 = new float[2];

    // Estado da partida
    private int numeroRodada = 1;
    private bool emTurnoAtivo = false;
    private bool emPausaHabilidade = false;
    private float duracaoTurno = 12f;
    private float tempoTurnoRestante;

    public EstadoStatusCarta Status { get; private set; }

    private void Start()
    {
        CarregarEInicializarCartas();
        IniciarFasePlanejamento();
    }

    private void Update()
    {
        if (!emTurnoAtivo || emPausaHabilidade) return;

        tempoTurnoRestante -= Time.deltaTime;

        if (textoTurno != null)
        {
            textoTurno.text = $"TURNO ATIVO: {Mathf.Ceil(tempoTurnoRestante)}s";
        }

        ProcessarAtaquesTime(cartasP1, cartasP2, cdAtaqueP1);
        ProcessarAtaquesTime(cartasP2, cartasP1, cdAtaqueP2);

        if (tempoTurnoRestante <= 0f)
        {
            EncerrarTurnoAtivo();
        }
    }

    // ==========================================
    // 1. CARREGAMENTO E SCRIPT DE TESTES (FALLBACK)
    // ==========================================
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
    // 2. CICLO DE RODADAS E POSTURA DEFENSIVA
    // ==========================================
    public void IniciarFasePlanejamento()
    {
        emTurnoAtivo = false;
        p1Pronto = false;
        p2Pronto = false;
        p1EmDefesaGlobal = false;
        p2EmDefesaGlobal = false;

        for (int i = 0; i < 2; i++)
        {
            emPosturaDefensivaP1[i] = false;
            emPosturaDefensivaP2[i] = false;
        }

        if (textoTurno != null)
        {
            textoTurno.text = $"{numeroRodada}ª RODADA: PLANEJAMENTO";
        }

        AtualizarBotoesPlanejamentoUI(true);
    }

    public void BotaoAlternarDefesaP1()
    {
        if (emTurnoAtivo || p1Pronto) return;

        p1EmDefesaGlobal = !p1EmDefesaGlobal;
        emPosturaDefensivaP1[0] = p1EmDefesaGlobal;
        emPosturaDefensivaP1[1] = p1EmDefesaGlobal;

        if (p1EmDefesaGlobal)
        {
            if (cartasP1[0] != null) cartasP1[0].AdicionarEscudo(20);
            if (cartasP1[1] != null) cartasP1[1].AdicionarEscudo(20);
        }

        if (textoBotaoDefenderP1 != null)
        {
            textoBotaoDefenderP1.text = p1EmDefesaGlobal ? "DEFESA: ATIVA" : "DEFENDER";
        }
    }

    public void BotaoAlternarDefesaP2()
    {
        if (emTurnoAtivo || p2Pronto) return;

        p2EmDefesaGlobal = !p2EmDefesaGlobal;
        emPosturaDefensivaP2[0] = p2EmDefesaGlobal;
        emPosturaDefensivaP2[1] = p2EmDefesaGlobal;

        if (p2EmDefesaGlobal)
        {
            if (cartasP2[0] != null) cartasP2[0].AdicionarEscudo(20);
            if (cartasP2[1] != null) cartasP2[1].AdicionarEscudo(20);
        }

        if (textoBotaoDefenderP2 != null)
        {
            textoBotaoDefenderP2.text = p2EmDefesaGlobal ? "DEFESA: ATIVA" : "DEFENDER";
        }
    }

    public void BotaoFinalizarJogadaP1()
    {
        p1Pronto = true;
        if (botaoFinalizarP1 != null) botaoFinalizarP1.interactable = false;
        if (botaoDefenderP1 != null) botaoDefenderP1.interactable = false;

        VerificarInicioDoTurno();
    }

    public void BotaoFinalizarJogadaP2()
    {
        p2Pronto = true;
        if (botaoFinalizarP2 != null) botaoFinalizarP2.interactable = false;
        if (botaoDefenderP2 != null) botaoDefenderP2.interactable = false;

        VerificarInicioDoTurno();
    }

    private void VerificarInicioDoTurno()
    {
        if (p1Pronto && p2Pronto)
        {
            AtualizarBotoesPlanejamentoUI(false);

            for (int i = 0; i < 2; i++)
            {
                cdAtaqueP1[i] = ObterIntervaloAtaque(cartasP1[i], emPosturaDefensivaP1[i]);
                cdAtaqueP2[i] = ObterIntervaloAtaque(cartasP2[i], emPosturaDefensivaP2[i]);
            }

            tempoTurnoRestante = duracaoTurno;
            emTurnoAtivo = true;
        }
    }

    private void EncerrarTurnoAtivo()
    {
        emTurnoAtivo = false;
        numeroRodada++;
        IniciarFasePlanejamento();
    }

    private void AtualizarBotoesPlanejamentoUI(bool ativo)
    {
        if (botaoFinalizarP1 != null) { botaoFinalizarP1.gameObject.SetActive(ativo); botaoFinalizarP1.interactable = ativo; }
        if (botaoDefenderP1 != null) { botaoDefenderP1.gameObject.SetActive(ativo); botaoDefenderP1.interactable = ativo; }
        if (botaoFinalizarP2 != null) { botaoFinalizarP2.gameObject.SetActive(ativo); botaoFinalizarP2.interactable = ativo; }
        if (botaoDefenderP2 != null) { botaoDefenderP2.gameObject.SetActive(ativo); botaoDefenderP2.interactable = ativo; }

        if (ativo)
        {
            if (textoBotaoDefenderP1 != null) textoBotaoDefenderP1.text = "DEFENDER";
            if (textoBotaoDefenderP2 != null) textoBotaoDefenderP2.text = "DEFENDER";
        }
    }

    // ==========================================
    // 3. PROCESSAMENTO DE ATAQUES E STATUS
    // ==========================================
    private float ObterIntervaloAtaque(CartaBatalhaUI carta, bool emDefesa)
    {
        if (carta == null || carta.VidaAtual <= 0) return 999f;

        float vel = carta.Atributos.velocidade;
        float penalidadeDefesa = emDefesa ? 0.8f : 0f;
        return Mathf.Max(1.5f, 3.5f - ((vel - penalidadeDefesa) * 0.17f));
    }

    private void ProcessarAtaquesTime(CartaBatalhaUI[] atacantes, CartaBatalhaUI[] defensores, float[] cooldowns)
    {
        for (int i = 0; i < 2; i++)
        {
            CartaBatalhaUI atacante = atacantes[i];
            if (atacante == null || atacante.VidaAtual <= 0) continue;

            cooldowns[i] -= Time.deltaTime;

            if (cooldowns[i] <= 0f)
            {
                CartaBatalhaUI alvo = ObterAlvoValido(defensores);
                if (alvo != null)
                {
                    ExecutarAtaqueBasico(atacante, alvo);
                }

                cooldowns[i] = ObterIntervaloAtaque(atacante, false);
            }
        }
    }

    private CartaBatalhaUI ObterAlvoValido(CartaBatalhaUI[] defensores)
    {
        List<CartaBatalhaUI> vivos = new List<CartaBatalhaUI>();
        foreach (var c in defensores)
        {
            if (c != null && c.VidaAtual > 0) vivos.Add(c);
        }

        if (vivos.Count == 0) return null;
        return vivos[Random.Range(0, vivos.Count)];
    }

    private void ExecutarAtaqueBasico(CartaBatalhaUI atacante, CartaBatalhaUI alvo)
    {
        if (atacante == null || alvo == null) return;

        // Astra Yao não ataca
        if (atacante.CartaDados != null && atacante.CartaDados.nomePersonagem.Trim() == "Astra Yao") return;

        // Se estiver incapacitado por status
        if (atacante.Status != null && (atacante.Status.estaCongelado || atacante.Status.estaAtordoado)) return;

        string nomeAtacante = (atacante.CartaDados != null) ? atacante.CartaDados.nomePersonagem.Trim() : "";
        string nomeAlvo = (alvo.CartaDados != null) ? alvo.CartaDados.nomePersonagem.Trim() : "";

        // Ponto de destino
        Vector3 posicaoDestinoAtaque = alvo.transform.position;
        if (nomeAtacante == "Billy Kid" || (atacante.CartaDados != null && atacante.CartaDados.alcanceSkill == TipoAlcanceAtaque.AmbasAsCartas))
        {
            CartaBatalhaUI[] timeInimigo = (atacante == cartasP1[0] || atacante == cartasP1[1]) ? cartasP2 : cartasP1;
            if (timeInimigo[0] != null && timeInimigo[1] != null)
            {
                posicaoDestinoAtaque = (timeInimigo[0].transform.position + timeInimigo[1].transform.position) / 2f;
            }
        }

        // Investida
        atacante.AnimarInvestidaEVoltar(posicaoDestinoAtaque, () =>
        {
            // --- PROTEÇÃO CONTRA NULL REFERENCE NO IMPACTO ---
            if (alvo == null || atacante == null) return;

            // 1. Esquiva
            int intAlvo = (alvo.Atributos != null) ? alvo.Atributos.intelecto : 0;
            int bonusEllen = (alvo.Status != null) ? alvo.Status.bonusIntelectoEllen : 0;
            float chanceEsquiva = (intAlvo + bonusEllen) * 0.03f;

            if (atacante.Status != null && atacante.Status.estaConfuso) chanceEsquiva += 0.20f;

            bool esquivou = Random.value < chanceEsquiva;

            if (esquivou && nomeAtacante == "Nicole Demara" && atacante.Status != null && atacante.Status.tempoRouboNicoleRestante <= 0f)
            {
                esquivou = false;
                atacante.Status.tempoRouboNicoleRestante = 15f;
            }

            if (esquivou)
            {
                alvo.AnimarEsquiva(atacante.transform.position - alvo.transform.position);
                if (nomeAlvo == "Ellen Joe" && alvo.Status != null) alvo.Status.bonusIntelectoEllen += 2;
                return;
            }

            if (nomeAlvo == "Ellen Joe" && alvo.Status != null) alvo.Status.bonusIntelectoEllen = 0;

            // 2. Cálculo de Dano e Crítico
            int forcaAtacante = (atacante.Atributos != null) ? atacante.Atributos.forca : 0;
            int destrezaAtacante = (atacante.Atributos != null) ? atacante.Atributos.destreza : 0;
            int raivaAtacante = (atacante.Atributos != null) ? atacante.Atributos.raiva : 0;

            int danoBase = Random.Range(10 + (forcaAtacante * 2), 21 + (forcaAtacante * 2));
            if (nomeAtacante == "Billy Kid") danoBase = Mathf.Max(1, danoBase / 2);

            float chanceCritico = destrezaAtacante * 0.07f;
            bool foiCritico = Random.value < chanceCritico;

            if (foiCritico) danoBase = Mathf.RoundToInt(danoBase * 1.5f);

            if (atacante.VidaMaxima > 0 && ((float)atacante.VidaAtual / atacante.VidaMaxima) < 0.30f)
            {
                float bonusRaiva = 1f + (raivaAtacante * 0.06f);
                danoBase = Mathf.RoundToInt(danoBase * bonusRaiva);
            }

            // 3. Efeitos Visuais e Shake Seguro (Sem quebrar se Camera.main for null)
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

            // 4. APLICAÇÃO EFETIVA DO DANO E CARGAS
            alvo.ReceberDano(danoBase);

            atacante.AdicionarCargaSkill(1);
            atacante.AdicionarCargaUlt(1);

            // 5. Passivas após acerto
            if (atacante.Status != null)
            {
                atacante.Status.contagemGolpesAcertados++;

                if (nomeAtacante == "Hoshimi Miyabi" && atacante.Status.contagemGolpesAcertados >= 5)
                {
                    if (alvo.Status != null)
                    {
                        alvo.Status.AplicarCongelamento();
                        alvo.DefinirVisualEstado();
                        alvo.MostrarTextoPopUp("CONGELADO!", new Color(0f, 0.95f, 1f));
                    }
                    atacante.Status.contagemGolpesAcertados = 0;
                }

                if (nomeAtacante == "Anby" && atacante.Status.contagemGolpesAcertados >= 4)
                {
                    if (alvo.Atributos != null) alvo.Atributos.velocidade = Mathf.Max(0, alvo.Atributos.velocidade - 1);
                    alvo.MostrarTextoPopUp("CHOQUE!", new Color(1f, 0.85f, 0f));
                    atacante.Status.contagemGolpesAcertados = 0;
                }
            }

            if (nomeAtacante == "N.º 11" && alvo.Status != null)
            {
                alvo.Status.AplicarQueimacao(2);
                alvo.MostrarTextoPopUp("QUEIMANDO!", new Color(1f, 0.4f, 0f));
            }

            if (alvo.Status != null)
            {
                if (nomeAlvo == "Phaethon" && Random.value < 0.15f)
                {
                    atacante.Status.AplicarConfusao(4f);
                    atacante.MostrarTextoPopUp("CONFUSO!", new Color(0.8f, 0.3f, 1f));
                }
                else if (nomeAlvo == "Jane Doe" && Random.value < 0.30f)
                {
                    atacante.Status.AplicarConfusao(4f);
                    atacante.MostrarTextoPopUp("CONFUSO!", new Color(0.8f, 0.3f, 1f));
                }
            }
        });
    }

    // Corrotina para gerar a câmera lenta instantânea no acerto crítico
    private IEnumerator RotinaCameraLentaCritico(float duracaoReal)
    {
        Time.timeScale = 0.2f; // Reduz a velocidade drasticamente
        yield return new WaitForSecondsRealtime(duracaoReal);
        Time.timeScale = 1.0f; // Restaura a velocidade normal
    }

    // ==========================================
    // 4. ÁREA RESERVADA PARA HABILIDADES / ULTS / PASSIVAS
    // ==========================================
    /*
    public void ExecutarHabilidadeCarta(CartaBatalhaUI atacante, int tipoHabilidade)
    {
        StartCoroutine(RoutineExecutarHabilidade(atacante, tipoHabilidade));
    }

    private IEnumerator RoutineExecutarHabilidade(CartaBatalhaUI atacante, int tipoHabilidade)
    {
        // Pausa imediatamente o tempo do turno e o fluxo de ataques
        emPausaHabilidade = true;

        // --- FUTURA INTEGRAÇÃO COM ANIMAÇÃO TIPO POKÉMON POCKET ---
        // Ex: Usar atacante.CartaDados.descricaoResumidaSkill / valorEfeitoSkill

        yield return new WaitForSeconds(1.5f); // Tempo de execução da habilidade ativa

        // Retoma o turno normalmente
        emPausaHabilidade = false;
    }
    */
}