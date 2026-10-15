using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class GerenciadorDistribuicao : MonoBehaviour
{
    [Header("UI de Informações")]
    public TextMeshProUGUI textoTituloJogador;
    public TextMeshProUGUI textoPontosRestantes;
    public TextMeshProUGUI textoTempo;
    public TextMeshProUGUI textoPreviaGeral; // O "New Text" único para todas as prévias

    [Header("Slots Visuais (Cartas)")]
    public Image imagemCartaFoco;     // Imagem da carta grande no centro (Slot_Carta_0)
    public Image imagemCartaLateral;  // Imagem da carta menor na lateral (Slot_Carta_1)

    [Header("Textos de Valores dos Atributos")]
    public TextMeshProUGUI textoForcaValor;     // Ex: 10 FR
    public TextMeshProUGUI textoVelocidadeValor; // Ex: 10 VL
    public TextMeshProUGUI textoIntelectoValor; // Ex: 10 IT
    public TextMeshProUGUI textoDestrezaValor;  // Ex: 10 DT
    public TextMeshProUGUI textoRaivaValor;     // Ex: 10 RV

    [Header("Botão de Confirmação e Auto-Fill")]
    public Button botaoConfirmar;
    public TextMeshProUGUI textoBotaoConfirmar;

    [Header("Controle Visual de Jogadores")]
    public Image fundoJogador;
    public Color corJogador1 = new Color(0.08f, 0.28f, 0.55f, 1f); // Azul
    public Color corJogador2 = new Color(0.55f, 0.10f, 0.10f, 1f); // Vermelho
    public float duracaoAnimacao = 0.2f;

    [Header("Fallback para Testes Diretos na Cena")]
    public CartaData[] cartasFallbackParaTeste;

    // Controle de Turno e Cartas
    private int jogadorAtual = 1;
    private int indiceCartaDoJogador = 0; // 0 ou 1

    private AtributosRuntime[] atributosJ1 = new AtributosRuntime[2];
    private AtributosRuntime[] atributosJ2 = new AtributosRuntime[2];
    private AtributosRuntime atributosAtuais;
    private CartaData cartaAtivaAtual;

    // Cronômetro (120 segundos)
    private float tempoRestante = 120f;
    private bool cronometroAtivo = false;
    private bool tempoEsgotadoAplicado = false; // Controla se o auto-fill por tempo já ocorreu nesta carta

    private void Start()
    {
        // --- FALLBACK PARA TESTES ---
        if (DadosPartida.cartasJogador1.Count < 2 && cartasFallbackParaTeste.Length >= 4)
        {
            DadosPartida.cartasJogador1.Clear();
            DadosPartida.cartasJogador1.Add(cartasFallbackParaTeste[0]);
            DadosPartida.cartasJogador1.Add(cartasFallbackParaTeste[1]);
        }
        if (DadosPartida.cartasJogador2.Count < 2 && cartasFallbackParaTeste.Length >= 4)
        {
            DadosPartida.cartasJogador2.Clear();
            DadosPartida.cartasJogador2.Add(cartasFallbackParaTeste[2]);
            DadosPartida.cartasJogador2.Add(cartasFallbackParaTeste[3]);
        }
        // -----------------------------

        if (DadosPartida.cartasJogador1.Count >= 2)
        {
            atributosJ1[0] = new AtributosRuntime(DadosPartida.cartasJogador1[0]);
            atributosJ1[1] = new AtributosRuntime(DadosPartida.cartasJogador1[1]);
        }
        if (DadosPartida.cartasJogador2.Count >= 2)
        {
            atributosJ2[0] = new AtributosRuntime(DadosPartida.cartasJogador2[0]);
            atributosJ2[1] = new AtributosRuntime(DadosPartida.cartasJogador2[1]);
        }

        jogadorAtual = 1;
        indiceCartaDoJogador = 0;

        CarregarCartaAtual();

        tempoRestante = 120f;
        cronometroAtivo = true;
        tempoEsgotadoAplicado = false;
    }

    private void Update()
    {
        if (cronometroAtivo)
        {
            tempoRestante -= Time.deltaTime;

            if (textoTempo != null)
            {
                textoTempo.text = Mathf.Ceil(tempoRestante).ToString("0") + "s";
            }

            if (tempoRestante <= 0)
            {
                TratarTempoEsgotado();
            }
        }
    }

    private void CarregarCartaAtual()
    {
        CartaData cartaLateral;

        if (jogadorAtual == 1)
        {
            atributosAtuais = atributosJ1[indiceCartaDoJogador];
            cartaAtivaAtual = DadosPartida.cartasJogador1[indiceCartaDoJogador];
            cartaLateral = DadosPartida.cartasJogador1[indiceCartaDoJogador == 0 ? 1 : 0];
            textoTituloJogador.text = $"Jogador 1: Distribua os Pontos (Carta {indiceCartaDoJogador + 1}/2)";
        }
        else
        {
            atributosAtuais = atributosJ2[indiceCartaDoJogador];
            cartaAtivaAtual = DadosPartida.cartasJogador2[indiceCartaDoJogador];
            cartaLateral = DadosPartida.cartasJogador2[indiceCartaDoJogador == 0 ? 1 : 0];
            textoTituloJogador.text = $"Jogador 2: Distribua os Pontos (Carta {indiceCartaDoJogador + 1}/2)";
        }

        if (imagemCartaFoco != null) imagemCartaFoco.sprite = cartaAtivaAtual.arteCarta;
        if (imagemCartaLateral != null) imagemCartaLateral.sprite = cartaLateral.arteCarta;

        if (textoBotaoConfirmar != null)
        {
            textoBotaoConfirmar.text = (indiceCartaDoJogador == 0) ? "Próxima Carta" : "Confirmar";
        }

        if (fundoJogador != null)
        {
            Color corAlvo = (jogadorAtual == 1) ? corJogador1 : corJogador2;
            fundoJogador.DOColor(corAlvo, duracaoAnimacao);
        }

        tempoRestante = 120f;
        cronometroAtivo = true;
        tempoEsgotadoAplicado = false;

        AtualizarInterface();
    }

    public void BotaoPreenchimentoAutomatico()
    {
        if (cartaAtivaAtual == null) return;

        atributosAtuais.forca = 0;
        atributosAtuais.velocidade = 0;
        atributosAtuais.intelecto = 0;
        atributosAtuais.destreza = 0;
        atributosAtuais.raiva = 0;
        atributosAtuais.pontosRestantes = 20;

        string nome = cartaAtivaAtual.nomePersonagem.Trim();

        if (nome == "Hoshimi Miyabi")
        {
            atributosAtuais.velocidade = 10;
            atributosAtuais.destreza = 10;
        }
        else if (nome == "Astra Yao")
        {
            atributosAtuais.intelecto = 10;
            atributosAtuais.velocidade = 10;
        }
        else if (nome == "Phaethon")
        {
            atributosAtuais.intelecto = 10;
            atributosAtuais.raiva = 10;
        }
        else if (nome == "Ellen Joe")
        {
            atributosAtuais.forca = 10;
            atributosAtuais.destreza = 10;
        }
        else if (nome == "Caesar King")
        {
            atributosAtuais.forca = 10;
            atributosAtuais.intelecto = 10;
        }
        else if (nome == "Jane Doe")
        {
            atributosAtuais.velocidade = 10;
            atributosAtuais.intelecto = 5;
            atributosAtuais.destreza = 5;
        }
        else if (nome == "Anby")
        {
            atributosAtuais.forca = 7;
            atributosAtuais.velocidade = 7;
            atributosAtuais.destreza = 6;
        }
        else if (nome == "Billy Kid")
        {
            atributosAtuais.destreza = 10;
            atributosAtuais.raiva = 10;
        }
        else if (nome == "Nicole Demara")
        {
            atributosAtuais.forca = 10;
            atributosAtuais.raiva = 10;
        }
        else if (nome == "N.º 11")
        {
            atributosAtuais.forca = 10;
            atributosAtuais.velocidade = 10;
        }
        else
        {
            atributosAtuais.forca = 10;
            atributosAtuais.velocidade = 10;
        }

        atributosAtuais.pontosRestantes = 0;
        AtualizarInterface();
    }

    // --- MÉTODOS DE ADICIONAR (+) ---
    public void AdicionarForca() { if (atributosAtuais.pontosRestantes > 0 && atributosAtuais.forca < 10) { atributosAtuais.forca++; atributosAtuais.pontosRestantes--; AtualizarInterface(); } }
    public void AdicionarVelocidade() { if (atributosAtuais.pontosRestantes > 0 && atributosAtuais.velocidade < 10) { atributosAtuais.velocidade++; atributosAtuais.pontosRestantes--; AtualizarInterface(); } }
    public void AdicionarIntelecto() { if (atributosAtuais.pontosRestantes > 0 && atributosAtuais.intelecto < 10) { atributosAtuais.intelecto++; atributosAtuais.pontosRestantes--; AtualizarInterface(); } }
    public void AdicionarDestreza() { if (atributosAtuais.pontosRestantes > 0 && atributosAtuais.destreza < 10) { atributosAtuais.destreza++; atributosAtuais.pontosRestantes--; AtualizarInterface(); } }
    public void AdicionarRaiva() { if (atributosAtuais.pontosRestantes > 0 && atributosAtuais.raiva < 10) { atributosAtuais.raiva++; atributosAtuais.pontosRestantes--; AtualizarInterface(); } }

    // --- MÉTODOS DE REMOVER (-) ---
    public void RemoverForca() { if (atributosAtuais.forca > 0) { atributosAtuais.forca--; atributosAtuais.pontosRestantes++; AtualizarInterface(); } }
    public void RemoverVelocidade() { if (atributosAtuais.velocidade > 0) { atributosAtuais.velocidade--; atributosAtuais.pontosRestantes++; AtualizarInterface(); } }
    public void RemoverIntelecto() { if (atributosAtuais.intelecto > 0) { atributosAtuais.intelecto--; atributosAtuais.pontosRestantes++; AtualizarInterface(); } }
    public void RemoverDestreza() { if (atributosAtuais.destreza > 0) { atributosAtuais.destreza--; atributosAtuais.pontosRestantes++; AtualizarInterface(); } }
    public void RemoverRaiva() { if (atributosAtuais.raiva > 0) { atributosAtuais.raiva--; atributosAtuais.pontosRestantes++; AtualizarInterface(); } }

    private void AtualizarInterface()
    {
        if (textoPontosRestantes != null)
            textoPontosRestantes.text = "Pontos: " + atributosAtuais.pontosRestantes;

        if (textoForcaValor != null) textoForcaValor.text = atributosAtuais.forca + " FR";
        if (textoVelocidadeValor != null) textoVelocidadeValor.text = atributosAtuais.velocidade + " VL";
        if (textoIntelectoValor != null) textoIntelectoValor.text = atributosAtuais.intelecto + " IT";
        if (textoDestrezaValor != null) textoDestrezaValor.text = atributosAtuais.destreza + " DT";
        if (textoRaivaValor != null) textoRaivaValor.text = atributosAtuais.raiva + " RV";

        int danoMin = 10 + (atributosAtuais.forca * 2);
        int danoMax = 20 + (atributosAtuais.forca * 2);
        int esquiva = atributosAtuais.intelecto * 3;
        int critico = atributosAtuais.destreza * 7;
        int bonusRaiva = atributosAtuais.raiva * 6;

        if (textoPreviaGeral != null)
        {
            textoPreviaGeral.text = $"Dano: {danoMin} a {danoMax}\n" +
                                    $"Velocidade: {atributosAtuais.velocidade}\n" +
                                    $"Esquiva: {esquiva}%\n" +
                                    $"Crítico: {critico}%\n" +
                                    $"Fúria (<30% Vida): +{bonusRaiva}%";
        }

        if (botaoConfirmar != null)
        {
            botaoConfirmar.interactable = (atributosAtuais.pontosRestantes == 0);
        }
    }

    public void ConfirmarAtributos()
    {
        if (atributosAtuais.pontosRestantes > 0) return;

        indiceCartaDoJogador++;

        if (indiceCartaDoJogador < 2)
        {
            FadeTextosUI(0f, 0.08f, () =>
            {
                CarregarCartaAtual();
                FadeTextosUI(1f, 0.08f, null);
            });
        }
        else
        {
            if (jogadorAtual == 1)
            {
                DadosPartida.atributosFinalJ1.Add(atributosJ1[0]);
                DadosPartida.atributosFinalJ1.Add(atributosJ1[1]);

                jogadorAtual = 2;
                indiceCartaDoJogador = 0;

                FadeTextosUI(0f, 0.08f, () =>
                {
                    CarregarCartaAtual();
                    FadeTextosUI(1f, 0.08f, null);
                });
            }
            else
            {
                DadosPartida.atributosFinalJ2.Add(atributosJ2[0]);
                DadosPartida.atributosFinalJ2.Add(atributosJ2[1]);

                cronometroAtivo = false;
                SceneManager.LoadScene("TelaBatalha");
            }
        }
    }

    private void FadeTextosUI(float alphaDestino, float duracao, System.Action noConcluido)
    {
        Sequence seq = DOTween.Sequence();
        if (textoForcaValor) seq.Join(textoForcaValor.DOFade(alphaDestino, duracao));
        if (textoVelocidadeValor) seq.Join(textoVelocidadeValor.DOFade(alphaDestino, duracao));
        if (textoIntelectoValor) seq.Join(textoIntelectoValor.DOFade(alphaDestino, duracao));
        if (textoDestrezaValor) seq.Join(textoDestrezaValor.DOFade(alphaDestino, duracao));
        if (textoRaivaValor) seq.Join(textoRaivaValor.DOFade(alphaDestino, duracao));
        if (textoPontosRestantes) seq.Join(textoPontosRestantes.DOFade(alphaDestino, duracao));
        if (textoPreviaGeral) seq.Join(textoPreviaGeral.DOFade(alphaDestino, duracao));

        seq.OnComplete(() =>
        {
            noConcluido?.Invoke();
        });
    }

    private void TratarTempoEsgotado()
    {
        // Se o preenchimento automático ainda não foi aplicado nesta rodada
        if (!tempoEsgotadoAplicado)
        {
            if (atributosAtuais.pontosRestantes > 0)
            {
                BotaoPreenchimentoAutomatico(); // Aplica a build sugerida
            }

            // Dá 20 segundos extras para o jogador conferir
            tempoRestante = 20f;
            tempoEsgotadoAplicado = true;
        }
        else
        {
            // Os 20 segundos extras acabaram! Confirma automaticamente e avança
            cronometroAtivo = false;
            ConfirmarAtributos();
        }
    }
}