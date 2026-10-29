using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System.Collections;
using System.Collections.Generic;

public enum AcaoCartaBatalha
{
    Nenhuma,
    Atacar,
    Defender,
    Skill,
    Ult
}

public class GerenciadorBatalha : MonoBehaviour
{
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

    [Header("Slots na Arena (Transforms do Canvas)")]
    public Transform slotP1_Carta0;
    public Transform slotP1_Carta1;
    public Transform slotP2_Carta0;
    public Transform slotP2_Carta1;

    [Header("Prefab da Carta de Batalha")]
    public GameObject prefabCartaBatalha;

    [Header("Containers / Painéis de Controle de UI")]
    public GameObject painelControleP1;
    public GameObject painelControleP2;

    [Header("Botões de Ação do Jogador 1 (Azul)")]
    public Button btnAtacarP1_C0;
    public Button btnDefenderP1_C0;
    public Button btnAtacarP1_C1;
    public Button btnDefenderP1_C1;

    [Header("Botões de Ação do Jogador 2 (Vermelho)")]
    public Button btnAtacarP2_C0;
    public Button btnDefenderP2_C0;
    public Button btnAtacarP2_C1;
    public Button btnDefenderP2_C1;

    [Header("UI Geral")]
    public TextMeshProUGUI textoTurno;

    [Header("Painel de Resultado / Fim de Jogo")]
    public GameObject painelResultado;
    public TextMeshProUGUI textoVencedor;
    public TextMeshProUGUI textoResumo;

    [Header("Fallback para Teste Direto na Cena")]
    public CartaData[] cartasFallbackParaTeste;

    private CartaBatalhaUI[] cartasP1 = new CartaBatalhaUI[2];
    private CartaBatalhaUI[] cartasP2 = new CartaBatalhaUI[2];

    private AcaoCartaBatalha[] acoesP1 = new AcaoCartaBatalha[2];
    private AcaoCartaBatalha[] acoesP2 = new AcaoCartaBatalha[2];

    private Dictionary<CartaBatalhaUI, int> danoCausadoMap = new Dictionary<CartaBatalhaUI, int>();
    private Dictionary<CartaBatalhaUI, int> danoRecebidoMap = new Dictionary<CartaBatalhaUI, int>();
    private Dictionary<CartaBatalhaUI, int> esquivasMap = new Dictionary<CartaBatalhaUI, int>();
    private Dictionary<CartaBatalhaUI, int> defesasMap = new Dictionary<CartaBatalhaUI, int>();

    private int jogadorDecidindoAtual = 1;
    private int numeroRodada = 1;
    private bool emResolucaoDeTurno = false;
    private bool jogoEncerrado = false;

    private void Start()
    {
        if (painelResultado != null) painelResultado.SetActive(false);

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

        cartasP1[0] = InstanciarCarta(listaP1[0], attrP1[0], slotP1_Carta0, true, 0);
        cartasP1[1] = InstanciarCarta(listaP1[1], attrP1[1], slotP1_Carta1, true, 1);

        cartasP2[0] = InstanciarCarta(listaP2[0], attrP2[0], slotP2_Carta0, false, 0);
        cartasP2[1] = InstanciarCarta(listaP2[1], attrP2[1], slotP2_Carta1, false, 1);

        RegistrarEstatisticasIniciais();
    }

    private void RegistrarEstatisticasIniciais()
    {
        danoCausadoMap.Clear();
        danoRecebidoMap.Clear();
        esquivasMap.Clear();
        defesasMap.Clear();

        RegistrarCartaStats(cartasP1[0]);
        RegistrarCartaStats(cartasP1[1]);
        RegistrarCartaStats(cartasP2[0]);
        RegistrarCartaStats(cartasP2[1]);
    }

    private void RegistrarCartaStats(CartaBatalhaUI carta)
    {
        if (carta != null)
        {
            danoCausadoMap[carta] = 0;
            danoRecebidoMap[carta] = 0;
            esquivasMap[carta] = 0;
            defesasMap[carta] = 0;
        }
    }

    private CartaBatalhaUI InstanciarCarta(CartaData data, AtributosRuntime attr, Transform slotTarget, bool eDoP1, int indiceSlot)
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
            cardUI.InicializarCarta(data, attr, eDoP1, indiceSlot, this);
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

    public void IniciarFaseDeComandos()
    {
        if (jogoEncerrado) return;

        emResolucaoDeTurno = false;
        jogadorDecidindoAtual = 1;

        ResetarTodosEscudos(cartasP1);
        ResetarTodosEscudos(cartasP2);

        AplicarEscudoPassivoCaesar(cartasP1);
        AplicarEscudoPassivoCaesar(cartasP2);

        ProcessarPassivaAstraYao(cartasP1);
        ProcessarPassivaAstraYao(cartasP2);

        for (int i = 0; i < 2; i++)
        {
            acoesP1[i] = AcaoCartaBatalha.Nenhuma;
            acoesP2[i] = AcaoCartaBatalha.Nenhuma;

            if (cartasP1[i] != null) cartasP1[i].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP1[i]);
            if (cartasP2[i] != null) cartasP2[i].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP2[i]);
        }

        AtualizarVisibilidadePaineis();
        ResetarEstadoBotoesAcao();
    }

    private void ResetarTodosEscudos(CartaBatalhaUI[] time)
    {
        foreach (var carta in time)
        {
            if (carta != null && carta.VidaAtual > 0) carta.SetarEscudoInicial(0);
        }
    }

    private void AplicarEscudoPassivoCaesar(CartaBatalhaUI[] time)
    {
        foreach (var carta in time)
        {
            if (carta != null && carta.VidaAtual > 0)
            {
                string nome = carta.CartaDados != null ? carta.CartaDados.nomePersonagem.Trim() : "";
                if (nome == "Caesar King") carta.AdicionarEscudo(30);
            }
        }
    }

    private void ProcessarPassivaAstraYao(CartaBatalhaUI[] time)
    {
        foreach (var carta in time)
        {
            if (carta != null && carta.VidaAtual > 0)
            {
                string nome = carta.CartaDados != null ? carta.CartaDados.nomePersonagem.Trim() : "";
                if (nome == "Astra Yao") carta.AdicionarCargaSkill(1);
            }
        }
    }

    private void ResetarEstadoBotoesAcao()
    {
        if (cartasP1[0] != null && cartasP1[0].VidaAtual > 0) { SetBotaoInterativo(btnAtacarP1_C0, true); SetBotaoInterativo(btnDefenderP1_C0, true); }
        if (cartasP1[1] != null && cartasP1[1].VidaAtual > 0) { SetBotaoInterativo(btnAtacarP1_C1, true); SetBotaoInterativo(btnDefenderP1_C1, true); }

        if (cartasP2[0] != null && cartasP2[0].VidaAtual > 0) { SetBotaoInterativo(btnAtacarP2_C0, true); SetBotaoInterativo(btnDefenderP2_C0, true); }
        if (cartasP2[1] != null && cartasP2[1].VidaAtual > 0) { SetBotaoInterativo(btnAtacarP2_C1, true); SetBotaoInterativo(btnDefenderP2_C1, true); }
    }

    private void SetBotaoInterativo(Button btn, bool ativo)
    {
        if (btn != null) btn.interactable = ativo;
    }

    private void AtualizarVisibilidadePaineis()
    {
        if (jogoEncerrado) return;

        if (emResolucaoDeTurno)
        {
            if (painelControleP1 != null) painelControleP1.SetActive(false);
            if (painelControleP2 != null) painelControleP2.SetActive(false);
            if (textoTurno != null) textoTurno.text = "RESOLVENDO TURNO...";

            DefinirOcultacaoCartas(cartasP1, false);
            DefinirOcultacaoCartas(cartasP2, false);
            return;
        }

        bool turnoP1 = (jogadorDecidindoAtual == 1);

        if (painelControleP1 != null) painelControleP1.SetActive(turnoP1);
        if (painelControleP2 != null) painelControleP2.SetActive(!turnoP1);

        if (textoTurno != null)
        {
            textoTurno.text = turnoP1 ? "Jogador 1: Escolha sua Ação" : "Jogador 2: Escolha sua Ação";
        }

        DefinirOcultacaoCartas(cartasP1, !turnoP1);
        DefinirOcultacaoCartas(cartasP2, turnoP1);
    }

    private void DefinirOcultacaoCartas(CartaBatalhaUI[] time, bool ocultar)
    {
        foreach (var carta in time)
        {
            if (carta != null && carta.VidaAtual > 0)
            {
                CanvasGroup cg = carta.GetComponent<CanvasGroup>();
                if (cg == null) cg = carta.gameObject.AddComponent<CanvasGroup>();

                cg.alpha = ocultar ? 0.0f : 1.0f;
                cg.blocksRaycasts = !ocultar;
            }
        }
    }

    // Ações padrão para P1 (Atualiza o estado visual de todos os botões da carta)
    public void P1_Carta0_Atacar()
    {
        if (jogadorDecidindoAtual == 1 && !emResolucaoDeTurno)
        {
            acoesP1[0] = AcaoCartaBatalha.Atacar;
            SetBotaoInterativo(btnAtacarP1_C0, false);
            SetBotaoInterativo(btnDefenderP1_C0, true);
            if (cartasP1[0] != null) cartasP1[0].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP1[0]);
            VerificarConclusaoEscolhaP1();
        }
    }
    public void P1_Carta0_Defender()
    {
        if (jogadorDecidindoAtual == 1 && !emResolucaoDeTurno)
        {
            acoesP1[0] = AcaoCartaBatalha.Defender;
            SetBotaoInterativo(btnAtacarP1_C0, true);
            SetBotaoInterativo(btnDefenderP1_C0, false);
            if (cartasP1[0] != null) cartasP1[0].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP1[0]);
            VerificarConclusaoEscolhaP1();
        }
    }
    public void P1_Carta1_Atacar()
    {
        if (jogadorDecidindoAtual == 1 && !emResolucaoDeTurno)
        {
            acoesP1[1] = AcaoCartaBatalha.Atacar;
            SetBotaoInterativo(btnAtacarP1_C1, false);
            SetBotaoInterativo(btnDefenderP1_C1, true);
            if (cartasP1[1] != null) cartasP1[1].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP1[1]);
            VerificarConclusaoEscolhaP1();
        }
    }
    public void P1_Carta1_Defender()
    {
        if (jogadorDecidindoAtual == 1 && !emResolucaoDeTurno)
        {
            acoesP1[1] = AcaoCartaBatalha.Defender;
            SetBotaoInterativo(btnAtacarP1_C1, true);
            SetBotaoInterativo(btnDefenderP1_C1, false);
            if (cartasP1[1] != null) cartasP1[1].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP1[1]);
            VerificarConclusaoEscolhaP1();
        }
    }

    // Ações padrão para P2 (Atualiza o estado visual de todos os botões da carta)
    public void P2_Carta0_Atacar()
    {
        if (jogadorDecidindoAtual == 2 && !emResolucaoDeTurno)
        {
            acoesP2[0] = AcaoCartaBatalha.Atacar;
            SetBotaoInterativo(btnAtacarP2_C0, false);
            SetBotaoInterativo(btnDefenderP2_C0, true);
            if (cartasP2[0] != null) cartasP2[0].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP2[0]);
            VerificarConclusaoEscolhaP2();
        }
    }
    public void P2_Carta0_Defender()
    {
        if (jogadorDecidindoAtual == 2 && !emResolucaoDeTurno)
        {
            acoesP2[0] = AcaoCartaBatalha.Defender;
            SetBotaoInterativo(btnAtacarP2_C0, true);
            SetBotaoInterativo(btnDefenderP2_C0, false);
            if (cartasP2[0] != null) cartasP2[0].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP2[0]);
            VerificarConclusaoEscolhaP2();
        }
    }
    public void P2_Carta1_Atacar()
    {
        if (jogadorDecidindoAtual == 2 && !emResolucaoDeTurno)
        {
            acoesP2[1] = AcaoCartaBatalha.Atacar;
            SetBotaoInterativo(btnAtacarP2_C1, false);
            SetBotaoInterativo(btnDefenderP2_C1, true);
            if (cartasP2[1] != null) cartasP2[1].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP2[1]);
            VerificarConclusaoEscolhaP2();
        }
    }
    public void P2_Carta1_Defender()
    {
        if (jogadorDecidindoAtual == 2 && !emResolucaoDeTurno)
        {
            acoesP2[1] = AcaoCartaBatalha.Defender;
            SetBotaoInterativo(btnAtacarP2_C1, true);
            SetBotaoInterativo(btnDefenderP2_C1, false);
            if (cartasP2[1] != null) cartasP2[1].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP2[1]);
            VerificarConclusaoEscolhaP2();
        }
    }

    // Seleção de Skill ou Ult (Permite trocar livremente e atualiza os visuais)
    public void SelecionarAcaoEspecial(bool eDoP1, int indiceSlot, AcaoCartaBatalha novaAcao)
    {
        if (emResolucaoDeTurno || jogoEncerrado) return;

        if (eDoP1 && jogadorDecidindoAtual == 1)
        {
            acoesP1[indiceSlot] = novaAcao;
            if (indiceSlot == 0)
            {
                SetBotaoInterativo(btnAtacarP1_C0, true);
                SetBotaoInterativo(btnDefenderP1_C0, true);
                if (cartasP1[0] != null) cartasP1[0].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP1[0]);
            }
            else
            {
                SetBotaoInterativo(btnAtacarP1_C1, true);
                SetBotaoInterativo(btnDefenderP1_C1, true);
                if (cartasP1[1] != null) cartasP1[1].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP1[1]);
            }
            VerificarConclusaoEscolhaP1();
        }
        else if (!eDoP1 && jogadorDecidindoAtual == 2)
        {
            acoesP2[indiceSlot] = novaAcao;
            if (indiceSlot == 0)
            {
                SetBotaoInterativo(btnAtacarP2_C0, true);
                SetBotaoInterativo(btnDefenderP2_C0, true);
                if (cartasP2[0] != null) cartasP2[0].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP2[0]);
            }
            else
            {
                SetBotaoInterativo(btnAtacarP2_C1, true);
                SetBotaoInterativo(btnDefenderP2_C1, true);
                if (cartasP2[1] != null) cartasP2[1].AtualizarEstadoBotoesAcoesEspeciais(true, acoesP2[1]);
            }
            VerificarConclusaoEscolhaP2();
        }
    }

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

    private IEnumerator ExecutarResolucaoTurnoRoutine()
    {
        AplicarDefesasSeNecessario(cartasP1, acoesP1);
        AplicarDefesasSeNecessario(cartasP2, acoesP2);

        yield return new WaitForSeconds(0.6f);

        List<ParticipanteTurno> filaAcoes = CriarFilaOrdenadaPorVelocidade();

        foreach (var participante in filaAcoes)
        {
            if (jogoEncerrado) yield break;
            if (participante.carta == null || participante.carta.VidaAtual <= 0) continue;

            if (participante.carta.Status != null && (participante.carta.Status.estaCongelado || participante.carta.Status.estaAtordoado))
            {
                yield return new WaitForSeconds(0.4f);
                continue;
            }

            CartaBatalhaUI alvo = ObterAlvoPorSlot(participante);

            if (alvo != null && alvo.VidaAtual > 0)
            {
                // 1. AÇÃO DE HABILIDADE (SKILL)
                if (participante.acao == AcaoCartaBatalha.Skill)
                {
                    string nomePersonagem = participante.carta.CartaDados != null ? participante.carta.CartaDados.nomePersonagem.Trim() : "";

                    participante.carta.ResetarCargasSkill();
                    yield return StartCoroutine(RotinaAnimacaoSkill(participante.carta, alvo));

                    if (nomePersonagem == "Astra Yao")
                    {
                        ExecutarSkillAstraYao(participante.carta, alvo);
                    }
                    else
                    {
                        Debug.Log($"{nomePersonagem} usou a Skill!");
                    }

                    yield return new WaitForSeconds(0.5f);
                    if (VerificarFimDeJogo()) yield break;
                }
                // 2. AÇÃO DE ULTIMATE (ULT)
                else if (participante.acao == AcaoCartaBatalha.Ult)
                {
                    string nomePersonagem = participante.carta.CartaDados != null ? participante.carta.CartaDados.nomePersonagem.Trim() : "";

                    Debug.Log($"{nomePersonagem} usou a Ultimate!");
                    participante.carta.ResetarCargasUlt();

                    yield return StartCoroutine(RotinaAnimacaoUltima(participante.carta, alvo));

                    yield return new WaitForSeconds(0.5f);
                    if (VerificarFimDeJogo()) yield break;
                }
                // 3. AÇÃO DE ATAQUE BÁSICO
                else if (participante.acao == AcaoCartaBatalha.Atacar)
                {
                    bool acaoConcluida = false;
                    ExecutarAtaqueBasico(participante.carta, alvo, 1.0f, () => { acaoConcluida = true; });
                    yield return new WaitUntil(() => acaoConcluida);

                    if (VerificarFimDeJogo()) yield break;

                    yield return new WaitForSeconds(0.2f);

                    int vel = participante.carta.Atributos != null ? participante.carta.Atributos.velocidade : 0;

                    if (vel >= 5 && alvo.VidaAtual > 0)
                    {
                        float chanceDuplo = (vel >= 10) ? 0.50f : 0.25f;

                        if (Random.value < chanceDuplo)
                        {
                            float multiplicadorDanoExtra = (vel >= 10) ? 1.0f : 0.5f;
                            string textoMsgExtra = (vel >= 10) ? "ATAQUE DUPLO!" : "VELOCIDADE!";

                            participante.carta.MostrarTextoPopUp(textoMsgExtra, new Color(1f, 0.8f, 0f));
                            yield return new WaitForSeconds(0.2f);

                            bool ataqueExtraConcluido = false;
                            ExecutarAtaqueBasico(participante.carta, alvo, multiplicadorDanoExtra, () => { ataqueExtraConcluido = true; });
                            yield return new WaitUntil(() => ataqueExtraConcluido);

                            if (VerificarFimDeJogo()) yield break;

                            yield return new WaitForSeconds(0.3f);
                        }
                    }
                }
            }
        }

        yield return new WaitForSeconds(0.8f);

        if (!jogoEncerrado)
        {
            numeroRodada++;
            IniciarFaseDeComandos();
        }
    }

    public void ExecutarSkillAstraYao(CartaBatalhaUI astra, CartaBatalhaUI alvo)
    {
        if (astra == null || astra.CartaDados == null) return;
        astra.AdicionarCargaUlt(2);
        astra.MostrarTextoPopUp("SUPORTE!", new Color(0.8f, 0.4f, 1f));
    }

    private IEnumerator RotinaAnimacaoSkill(CartaBatalhaUI carta, CartaBatalhaUI alvo)
    {
        carta.PiscarCarta(new Color(0.2f, 0.8f, 1f));
        carta.MostrarTextoPopUp("HABILIDADE!", new Color(0.2f, 0.8f, 1f));

        Vector3 posOriginal = carta.transform.position;
        carta.transform.DOMove(posOriginal + (alvo.transform.position - posOriginal) * 0.2f, 0.15f).SetLoops(2, LoopType.Yoyo);

        yield return new WaitForSeconds(0.3f);
    }

    private IEnumerator RotinaAnimacaoUltima(CartaBatalhaUI carta, CartaBatalhaUI alvo)
    {
        if (Camera.main != null)
        {
            Camera.main.transform.DOShakePosition(0.40f, strength: 50f, vibrato: 25);
        }

        carta.PiscarCarta(new Color(1f, 0.5f, 0f));
        carta.MostrarTextoPopUp("ULTIMATE!", new Color(1f, 0.5f, 0f));

        Vector3 posOriginal = carta.transform.position;
        carta.transform.DOMove(posOriginal + (alvo.transform.position - posOriginal) * 0.35f, 0.2f).SetLoops(2, LoopType.Yoyo);

        yield return new WaitForSeconds(0.3f);

        int danoUlt = 30;
        alvo.ReceberDano(danoUlt);
        alvo.PiscarCarta(Color.red);
        alvo.MostrarTextoPopUp($"-{danoUlt} ULT", Color.red);

        if (danoCausadoMap.ContainsKey(carta)) danoCausadoMap[carta] += danoUlt;
        if (danoRecebidoMap.ContainsKey(alvo)) danoRecebidoMap[alvo] += danoUlt;

        VerificarMorteCarta(alvo);
    }

    private void AplicarDefesasSeNecessario(CartaBatalhaUI[] time, AcaoCartaBatalha[] acoes)
    {
        for (int i = 0; i < 2; i++)
        {
            if (time[i] != null && time[i].VidaAtual > 0 && acoes[i] == AcaoCartaBatalha.Defender)
            {
                time[i].AdicionarEscudo(10);
                time[i].MostrarTextoPopUp("DEFESA!", new Color(0f, 0.85f, 1f));

                if (defesasMap.ContainsKey(time[i]))
                    defesasMap[time[i]]++;
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

    private void ExecutarAtaqueBasico(CartaBatalhaUI atacante, CartaBatalhaUI alvo, float multiplicadorDano, System.Action aoTerminarAtaque)
    {
        if (atacante == null || alvo == null || atacante.VidaAtual <= 0 || alvo.VidaAtual <= 0)
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

                if (esquivasMap.ContainsKey(alvo)) esquivasMap[alvo]++;

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

            danoBase = Mathf.RoundToInt(danoBase * multiplicadorDano);

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

            if (danoCausadoMap.ContainsKey(atacante)) danoCausadoMap[atacante] += danoBase;
            if (danoRecebidoMap.ContainsKey(alvo)) danoRecebidoMap[alvo] += danoBase;

            VerificarMorteCarta(alvo);

            atacante.AdicionarCargaSkill(1);
            atacante.AdicionarCargaUlt(1);

            aoTerminarAtaque?.Invoke();
        });
    }

    private void VerificarMorteCarta(CartaBatalhaUI carta)
    {
        if (carta != null && carta.VidaAtual <= 0)
        {
            if (carta == cartasP1[0]) { SetBotaoInterativo(btnAtacarP1_C0, false); SetBotaoInterativo(btnDefenderP1_C0, false); }
            else if (carta == cartasP1[1]) { SetBotaoInterativo(btnAtacarP1_C1, false); SetBotaoInterativo(btnDefenderP1_C1, false); }
            else if (carta == cartasP2[0]) { SetBotaoInterativo(btnAtacarP2_C0, false); SetBotaoInterativo(btnDefenderP2_C0, false); }
            else if (carta == cartasP2[1]) { SetBotaoInterativo(btnAtacarP2_C1, false); SetBotaoInterativo(btnDefenderP2_C1, false); }

            carta.gameObject.SetActive(false);
        }
    }

    private bool VerificarFimDeJogo()
    {
        bool p1Derrotado = (cartasP1[0].VidaAtual <= 0 && cartasP1[1].VidaAtual <= 0);
        bool p2Derrotado = (cartasP2[0].VidaAtual <= 0 && cartasP2[1].VidaAtual <= 0);

        if (p1Derrotado || p2Derrotado)
        {
            jogoEncerrado = true;
            string vencedorTexto = p1Derrotado ? "Jogador 2 Venceu!" : "Jogador 1 Venceu!";
            string resumoTexto = ObterResumoEstatisticas();

            MostrarTelaResultado(vencedorTexto, resumoTexto);
            return true;
        }

        return false;
    }

    private string ObterResumoEstatisticas()
    {
        string maisDanoCausado = "Nenhuma";
        string maisDanoRecebido = "Nenhuma";
        string maisEsquivas = "Nenhuma";
        string maisDefesas = "Nenhuma";

        int maxDanoC = -1;
        int maxDanoR = -1;
        int maxEsq = -1;
        int maxDef = -1;

        foreach (var kvp in danoCausadoMap)
        {
            if (kvp.Key != null && kvp.Value > maxDanoC)
            {
                maxDanoC = kvp.Value;
                string nome = kvp.Key.CartaDados != null ? kvp.Key.CartaDados.nomePersonagem : "Carta";
                maisDanoCausado = $"{nome} ({maxDanoC} de dano)";
            }
        }

        foreach (var kvp in danoRecebidoMap)
        {
            if (kvp.Key != null && kvp.Value > maxDanoR)
            {
                maxDanoR = kvp.Value;
                string nome = kvp.Key.CartaDados != null ? kvp.Key.CartaDados.nomePersonagem : "Carta";
                maisDanoRecebido = $"{nome} ({maxDanoR} de dano)";
            }
        }

        foreach (var kvp in esquivasMap)
        {
            if (kvp.Key != null && kvp.Value > maxEsq)
            {
                maxEsq = kvp.Value;
                string nome = kvp.Key.CartaDados != null ? kvp.Key.CartaDados.nomePersonagem : "Carta";
                maisEsquivas = $"{nome} ({maxEsq} esquivas)";
            }
        }

        foreach (var kvp in defesasMap)
        {
            if (kvp.Key != null && kvp.Value > maxDef)
            {
                maxDef = kvp.Value;
                string nome = kvp.Key.CartaDados != null ? kvp.Key.CartaDados.nomePersonagem : "Carta";
                maisDefesas = $"{nome} ({maxDef} defesas)";
            }
        }

        return $"Rodadas jogadas: {numeroRodada}\n\n" +
               $"Maior Dano Causado: {maisDanoCausado}\n" +
               $"Mais Dano Recebido: {maisDanoRecebido}\n" +
               $"Mais Esquivas: {maisEsquivas}\n" +
               $"Mais Defesas: {maisDefesas}";
    }

    private void MostrarTelaResultado(string vencedor, string resumo)
    {
        if (painelControleP1 != null) painelControleP1.SetActive(false);
        if (painelControleP2 != null) painelControleP2.SetActive(false);
        if (textoTurno != null) textoTurno.gameObject.SetActive(false);

        if (textoVencedor != null) textoVencedor.text = vencedor;
        if (textoResumo != null) textoResumo.text = resumo;

        if (painelResultado != null) painelResultado.SetActive(true);
    }

    public void JogarNovamente()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VoltarAoMenu()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene("MenuPrincipal");
    }

    private IEnumerator RotinaCameraLentaCritico(float duracaoReal)
    {
        Time.timeScale = 0.2f;
        yield return new WaitForSecondsRealtime(duracaoReal);
        Time.timeScale = 1.0f;
    }
}