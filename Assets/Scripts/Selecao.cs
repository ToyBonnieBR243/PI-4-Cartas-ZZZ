using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class GerenciadorSelecao : MonoBehaviour
{
    [Header("Banco de Cartas")]
    public CartaData[] todasAsCartas;
    private int indiceCartaAtual = -1;
    private bool[] cartasEscolhidas;

    [Header("Controle de Jogadores")]
    public int jogadorAtual = 1;
    private int totalCartasEscolhidas = 0;

    public TextMeshProUGUI textoJogador;
    public Image fundoJogador;

    public Color corJogador1 = new Color(0.08f, 0.28f, 0.55f, 1f);
    public Color corJogador2 = new Color(0.55f, 0.10f, 0.10f, 1f);
    public Color corFim = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Referências da Grade de Cartas")]
    public Button[] botoesSlots;
    public Image[] imagensPersonagens;
    public ScrollRect scrollRectCardas; // <-- Variável declarada corretamente aqui

    [Header("Configurações do DOTween (Destaque)")]
    public float escalaDestaque = 1.15f;
    public float duracaoAnimacao = 0.2f;

    [Header("Painel e Textos de Detalhes")]
    public GameObject painelDetalhes;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoLore;
    public TextMeshProUGUI textoHabilidades;

    [Header("Botão de Alternância")]
    public TextMeshProUGUI textoBotaoLoreHab;
    private bool mostrandoLore = true;

    private void Start()
    {
        cartasEscolhidas = new bool[todasAsCartas.Length];
        AtualizarIndicadorJogador();
        ResetarEfeitosCartas();
    }

    public void AbrirPainel(int indice)
    {
        if (cartasEscolhidas[indice]) return;

        indiceCartaAtual = indice;
        mostrandoLore = true;

        DestacarCartaComDOTween(indice);

        if (painelDetalhes != null) painelDetalhes.SetActive(true);
        AtualizarInterface();
    }

    private void AtualizarInterface()
    {
        if (indiceCartaAtual < 0 || indiceCartaAtual >= todasAsCartas.Length) return;

        CartaData carta = todasAsCartas[indiceCartaAtual];
        if (textoNome != null) textoNome.text = carta.nomePersonagem;
        if (textoLore != null) textoLore.text = carta.historiaLore;
        if (textoHabilidades != null) textoHabilidades.text = carta.habilidades;

        if (textoLore != null) textoLore.gameObject.SetActive(mostrandoLore);
        if (textoHabilidades != null) textoHabilidades.gameObject.SetActive(!mostrandoLore);

        if (textoBotaoLoreHab != null) textoBotaoLoreHab.text = mostrandoLore ? "Habilidades" : "Lore";
    }

    public void BotaoAlternarLoreHabilidade()
    {
        mostrandoLore = !mostrandoLore;
        AtualizarInterface();
    }

    public void BotaoProximaCarta()
    {
        int totalCartas = todasAsCartas.Length;
        int tentativas = 0;

        do
        {
            indiceCartaAtual = (indiceCartaAtual + 1) % totalCartas;
            tentativas++;

            if (tentativas >= totalCartas) return;

        } while (cartasEscolhidas[indiceCartaAtual]);

        DestacarCartaComDOTween(indiceCartaAtual);
        AtualizarInterface();
    }

    public void SelecionarCarta()
    {
        if (indiceCartaAtual < 0 || indiceCartaAtual >= todasAsCartas.Length) return;
        if (cartasEscolhidas[indiceCartaAtual]) return;

        cartasEscolhidas[indiceCartaAtual] = true;
        CartaData cartaEscolhida = todasAsCartas[indiceCartaAtual];

        // Salva a carta para o jogador correspondente
        if (jogadorAtual == 1)
        {
            DadosPartida.cartasJogador1.Add(cartaEscolhida);
        }
        else if (jogadorAtual == 2)
        {
            DadosPartida.cartasJogador2.Add(cartaEscolhida);
        }

        if (botoesSlots != null && indiceCartaAtual < botoesSlots.Length && botoesSlots[indiceCartaAtual] != null)
        {
            Transform slotTransform = botoesSlots[indiceCartaAtual].transform;
            slotTransform.localScale = Vector3.one;
            botoesSlots[indiceCartaAtual].interactable = false;
        }

        if (imagensPersonagens != null && indiceCartaAtual < imagensPersonagens.Length && imagensPersonagens[indiceCartaAtual] != null)
        {
            imagensPersonagens[indiceCartaAtual].gameObject.SetActive(false);
        }

        FecharPainel();
        totalCartasEscolhidas++;

        // Regra de Turnos Alternados (P1 -> P2 -> P1 -> P2)
        if (totalCartasEscolhidas == 1 || totalCartasEscolhidas == 3)
        {
            jogadorAtual = 2;
        }
        else if (totalCartasEscolhidas == 2)
        {
            jogadorAtual = 1;
        }
        else if (totalCartasEscolhidas >= 4)
        {
            jogadorAtual = 0;
            SceneManager.LoadScene("TelaDistribuicao");
            return;
        }

        AtualizarIndicadorJogador();
    }

    public void FecharPainel()
    {
        ResetarEfeitosCartas();
        if (painelDetalhes != null) painelDetalhes.SetActive(false);
    }

    private void DestacarCartaComDOTween(int indice)
    {
        ResetarEfeitosCartas();

        if (botoesSlots != null && indice < botoesSlots.Length && botoesSlots[indice] != null)
        {
            Transform slot = botoesSlots[indice].transform;

            // Aumenta a escala levemente para dar foco visual
            slot.DOScale(Vector3.one * escalaDestaque, duracaoAnimacao)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(() => {
                    // Garante que o balanço continua rodando após o zoom
                    CartaFlutuanteUI flutuante = slot.GetComponent<CartaFlutuanteUI>();
                    if (flutuante != null) flutuante.IniciarEfeitoFlutuante();
                });

            if (scrollRectCardas != null)
            {
                int linhaAtual = indice / 2;
                int totalLinhas = Mathf.CeilToInt(botoesSlots.Length / 2f);

                if (totalLinhas > 1)
                {
                    float posicaoScroll = 1f - ((float)linhaAtual / (totalLinhas - 1));
                    scrollRectCardas.DOVerticalNormalizedPos(posicaoScroll, duracaoAnimacao)
                        .SetEase(Ease.OutCubic);
                }
            }
        }
    }

    private void ResetarEfeitosCartas()
    {
        if (botoesSlots == null) return;

        for (int i = 0; i < botoesSlots.Length; i++)
        {
            if (botoesSlots[i] != null)
            {
                botoesSlots[i].transform.DOScale(Vector3.one, duracaoAnimacao);

                // Reinicia o balanço em todas as cartas para evitar que fiquem travadas
                CartaFlutuanteUI flutuante = botoesSlots[i].GetComponent<CartaFlutuanteUI>();
                if (flutuante != null) flutuante.IniciarEfeitoFlutuante();
            }
        }
    }

    private void AtualizarIndicadorJogador()
    {
        if (textoJogador == null || fundoJogador == null) return;

        if (totalCartasEscolhidas == 0)
        {
            textoJogador.text = "Jogador 1: Escolha sua 1ª carta";
            fundoJogador.DOColor(corJogador1, duracaoAnimacao);
        }
        else if (totalCartasEscolhidas == 1)
        {
            textoJogador.text = "Jogador 2: Escolha sua 1ª carta";
            fundoJogador.DOColor(corJogador2, duracaoAnimacao);
        }
        else if (totalCartasEscolhidas == 2)
        {
            textoJogador.text = "Jogador 1: Escolha sua 2ª carta";
            fundoJogador.DOColor(corJogador1, duracaoAnimacao);
        }
        else if (totalCartasEscolhidas == 3)
        {
            textoJogador.text = "Jogador 2: Escolha sua 2ª carta";
            fundoJogador.DOColor(corJogador2, duracaoAnimacao);
        }
        else
        {
            textoJogador.text = "Seleção Concluída!";
            fundoJogador.DOColor(corFim, duracaoAnimacao);
        }
    }
}