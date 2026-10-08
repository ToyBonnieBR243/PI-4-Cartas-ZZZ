using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GerenciadorSelecao : MonoBehaviour
{
    [Header("Banco de Cartas")]
    public CartaData[] todasAsCartas;
    private int indiceCartaAtual = 0;
    private bool[] cartasEscolhidas; // Controla quais cartas já foram tomadas

    [Header("Controle de Jogadores")]
    public int jogadorAtual = 1; // 1 = Jogador 1 (Azul), 2 = Jogador 2 (Vermelho)
    public TextMeshProUGUI textoJogador;
    public Image fundoJogador;
    public Color corJogador1 = new Color(0f, 0.5f, 1f, 1f); // Azul
    public Color corJogador2 = new Color(1f, 0.2f, 0.2f, 1f); // Vermelho

    [Header("Referências da Grade de Cartas")]
    public Button[] botoesSlots;          // Componente Button de cada CardSlot
    public Image[] imagensPersonagens;   // Componente Image da arte de cada CardSlot
    public GameObject[] overlaysDestaque; // Componente Image/GameObject do Overlay de cada CardSlot

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
        EsconderTodosOverlays();
    }

    // Chamado ao clicar em uma carta do Grid
    public void AbrirPainel(int indice)
    {
        // Se a carta já foi escolhida por um jogador, ignora o clique
        if (cartasEscolhidas[indice]) return;

        indiceCartaAtual = indice;
        mostrandoLore = true;

        DestacarCartaAtual(indice);
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

    // Navega para a próxima carta disponível na grade
    public void BotaoProximaCarta()
    {
        int totalCartas = todasAsCartas.Length;
        int tentativas = 0;

        // Procura a próxima carta que AINDA NÃO foi escolhida
        do
        {
            indiceCartaAtual = (indiceCartaAtual + 1) % totalCartas;
            tentativas++;

            // Se todas as cartas foram escolhidas, encerra
            if (tentativas >= totalCartas) return;

        } while (cartasEscolhidas[indiceCartaAtual]);

        DestacarCartaAtual(indiceCartaAtual);
        AtualizarInterface();
    }

    // Confirma a seleção da carta para o jogador ativo
    public void SelecionarCarta()
    {
        // Marca a carta como escolhida
        cartasEscolhidas[indiceCartaAtual] = true;

        // Desativa o botão e esconde a imagem da arte do personagem
        botoesSlots[indiceCartaAtual].interactable = false;
        imagensPersonagens[indiceCartaAtual].gameObject.SetActive(false);

        FecharPainel();

        // Alterna de jogador ou encerra a seleção
        if (jogadorAtual == 1)
        {
            jogadorAtual = 2;
            AtualizarIndicadorJogador();
        }
        else
        {
            textoJogador.text = "Seleção Concluída!";
            Debug.Log("Ambos os jogadores escolheram suas cartas!");
        }
    }

    public void FecharPainel()
    {
        EsconderTodosOverlays();
        painelDetalhes.SetActive(false);
    }

    private void DestacarCartaAtual(int indice)
    {
        EsconderTodosOverlays();
        if (overlaysDestaque.Length > indice && overlaysDestaque[indice] != null)
        {
            overlaysDestaque[indice].SetActive(true);
        }
    }

    private void EsconderTodosOverlays()
    {
        for (int i = 0; i < overlaysDestaque.Length; i++)
        {
            if (overlaysDestaque[i] != null)
                overlaysDestaque[i].SetActive(false);
        }
    }

    private void AtualizarIndicadorJogador()
    {
        if (jogadorAtual == 1)
        {
            textoJogador.text = "Jogador 1: Escolha sua carta";
            fundoJogador.color = corJogador1;
        }
        else
        {
            textoJogador.text = "Jogador 2: Escolha sua carta";
            fundoJogador.color = corJogador2;
        }
    }
}