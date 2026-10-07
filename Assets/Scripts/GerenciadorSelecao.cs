using UnityEngine;
using TMPro; // Necessário para acessar os textos do TextMeshPro
using UnityEngine.UI;

public class GerenciadorSelecao : MonoBehaviour
{
    [Header("Banco de Cartas")]
    public CartaData[] todasAsCartas; // Arraste os 10 arquivos ScriptableObject para cá depois
    private int indiceCartaAtual = 0;

    [Header("Painel e Textos")]
    public GameObject painelDetalhes;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoLore;
    public TextMeshProUGUI textoHabilidades;

    [Header("Botão de Alternância")]
    public TextMeshProUGUI textoBotaoLoreHab; // Arraste o filho (Text TMP) do botão aqui
    private bool mostrandoLore = true; // Controla o estado atual

    // Chamado quando o jogador clica em uma carta no Grid
    public void AbrirPainel(int indice)
    {
        indiceCartaAtual = indice;
        mostrandoLore = true; // Sempre abre mostrando a Lore primeiro
        painelDetalhes.SetActive(true);
        AtualizarInterface();
    }

    // Atualiza todos os textos com os dados da carta atual
    private void AtualizarInterface()
    {
        CartaData carta = todasAsCartas[indiceCartaAtual];
        textoNome.text = carta.nomePersonagem;
        textoLore.text = carta.historiaLore;
        textoHabilidades.text = carta.habilidades;

        // Ativa/Desativa os textos na tela
        textoLore.gameObject.SetActive(mostrandoLore);
        textoHabilidades.gameObject.SetActive(!mostrandoLore);

        // Regra do Botão: Se mostra Lore, botão diz "Habilidades". Se mostra Hab, botão diz "Lore".
        textoBotaoLoreHab.text = mostrandoLore ? "Habilidades" : "Lore";
    }

    // Função para o Botão "Lore/Hab"
    public void BotaoAlternarLoreHabilidade()
    {
        mostrandoLore = !mostrandoLore; // Inverte o estado (de true pra false ou vice-versa)
        AtualizarInterface();
    }

    // Função para o Botão "Próxima Carta"
    public void BotaoProximaCarta()
    {
        indiceCartaAtual++;

        // Se passar da última carta, volta para a primeira
        if (indiceCartaAtual >= todasAsCartas.Length)
        {
            indiceCartaAtual = 0;
        }

        AtualizarInterface();
    }

    // Função para fechar (Botão "Sair")
    public void FecharPainel()
    {
        painelDetalhes.SetActive(false);
    }

    // Função temporária para o botão "Selecionar"
    public void SelecionarCarta()
    {
        Debug.Log("Carta escolhida: " + todasAsCartas[indiceCartaAtual].nomePersonagem);
        FecharPainel();
        // Depois adicionaremos a lógica de passar o turno para o Jogador 2 aqui
    }
}