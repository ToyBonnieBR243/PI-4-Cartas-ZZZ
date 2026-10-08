using UnityEngine;
using TMPro;
using UnityEngine.UI;
using DG.Tweening;

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
    public ScrollRect scrollRectCartas;

    [Header("Configurações do DOTween")]
    public float escalaDestaque = 1.15f; // Zoom aplicado ao selecionar a carta
    public float duracaoAnimacao = 0.2f;

    [Header("Painel e Textos")]
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
        painelDetalhes.SetActive(true);
        AtualizarInterface();
    }

    private void AtualizarInterface()
    {
        CartaData carta = todasAsCartas[indiceCartaAtual];
        textoNome.text = carta.nomePersonagem;
        textoLore.text = carta.historiaLore;
        textoHabilidades.text = carta.habilidades;

        textoLore.gameObject.SetActive(mostrandoLore);
        textoHabilidades.gameObject.SetActive(!mostrandoLore);

        textoBotaoLoreHab.text = mostrandoLore ? "Habilidades" : "Lore";
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
        cartasEscolhidas[indiceCartaAtual] = true;

        // Reseta o tamanho e desativa a carta selecionada
        Transform slotTransform = botoesSlots[indiceCartaAtual].transform;
        slotTransform.DOKill();
        slotTransform.localScale = Vector3.one;

        botoesSlots[indiceCartaAtual].interactable = false;
        imagensPersonagens[indiceCartaAtual].gameObject.SetActive(false);

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
        }

        AtualizarIndicadorJogador();
    }

    public void FecharPainel()
    {
        ResetarEfeitosCartas();
        painelDetalhes.SetActive(false);
    }

    private void DestacarCartaComDOTween(int indice)
    {
        ResetarEfeitosCartas();

        if (botoesSlots != null && indice < botoesSlots.Length)
        {
            Transform slot = botoesSlots[indice].transform;

            // Aplica o zoom suave na carta focada
            slot.DOScale(Vector3.one * escalaDestaque, duracaoAnimacao)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);

            // FAZ O SCROLL VIEW ROLAR ATÉ A CARTA AUTOMATICAMENTE
            if (scrollRectCartas != null)
            {
                // Como temos 2 colunas, dividimos o índice por 2 para saber em qual linha a carta está
                int linhaAtual = indice / 2;
                int totalLinhas = Mathf.CeilToInt(botoesSlots.Length / 2f);

                if (totalLinhas > 1)
                {
                    // Calcula a posição (1 = topo, 0 = final)
                    float posicaoScroll = 1f - ((float)linhaAtual / (totalLinhas - 1));

                    // Move o Scroll de forma suave com o DOTween
                    scrollRectCartas.DOVerticalNormalizedPos(posicaoScroll, duracaoAnimacao)
                        .SetEase(Ease.OutCubic);
                }
            }
        }
    }

    private void ResetarEfeitosCartas()
    {
        for (int i = 0; i < botoesSlots.Length; i++)
        {
            if (botoesSlots[i] != null)
            {
                botoesSlots[i].transform.DOKill();
                botoesSlots[i].transform.DOScale(Vector3.one, duracaoAnimacao);
            }
        }
    }

    private void AtualizarIndicadorJogador()
    {
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