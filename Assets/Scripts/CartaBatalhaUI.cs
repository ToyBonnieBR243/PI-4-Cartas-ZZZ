using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

public class CartaBatalhaUI : MonoBehaviour
{
    [Header("Arte Única da Carta")]
    public Image imagemArteCompleta;

    [Header("Textos de Habilidades")]
    public TextMeshProUGUI textoHabilidade;
    public TextMeshProUGUI textoUltimate;

    [Header("Textos de Atributos na Carta")]
    public TextMeshProUGUI textoForca;
    public TextMeshProUGUI textoVelocidade;
    public TextMeshProUGUI textoIntelecto;
    public TextMeshProUGUI textoDestreza;
    public TextMeshProUGUI textoRaiva;

    [Header("Sistema de Vida (Coração)")]
    public Image coracaoPreenchimento; // Image Type = Filled, Fill Method = Vertical
    public TextMeshProUGUI textoHP;

    [Header("Containers de Cargas (Horizontal Layout Group)")]
    public Transform containerHab;
    public Transform containerUlt;

    [Header("Prefab Base dos Ícones")]
    public GameObject prefabIconoCarga;

    [Header("Sprites Compartilhados da Habilidade (Skill)")]
    public Sprite spriteSkillApagada;
    public Sprite spriteSkillAcesa;

    [Header("Sprites Compartilhados da Ultimate (Ult)")]
    public Sprite spriteUltApagada;
    public Sprite spriteUltAcesa;

    [Header("Sistema de Escudo")]
    public CanvasGroup groupEscudo;
    public TextMeshProUGUI textoEscudo; // Cor Ciano/Azul

    // Listas internas de instâncias
    private List<Image> listaIconesSkill = new List<Image>();
    private List<Image> listaIconesUlt = new List<Image>();

    // Propriedades Runtime
    public CartaData CartaDados { get; private set; }
    public AtributosRuntime Atributos { get; private set; }

    public int VidaAtual { get; private set; }
    public int VidaMaxima { get; private set; }
    public int EscudoAtual { get; private set; }
    public int CargasSkillAtuais { get; private set; }
    public int CargasUltAtuais { get; private set; }

    public void InicializarCarta(CartaData cartaData, AtributosRuntime atributos)
    {
        CartaDados = cartaData;
        Atributos = atributos;

        if (imagemArteCompleta != null) imagemArteCompleta.sprite = cartaData.arteCarta;

        // Configura textos descritivos das habilidades
        if (textoHabilidade != null) textoHabilidade.text = cartaData.descricaoResumidaSkill;
        if (textoUltimate != null) textoUltimate.text = cartaData.descricaoResumidaUlt;

        // Exibe os valores dos atributos distribuídos diretamente na carta
        if (textoForca != null) textoForca.text = atributos.forca + " FR";
        if (textoVelocidade != null) textoVelocidade.text = atributos.velocidade + " VL";
        if (textoIntelecto != null) textoIntelecto.text = atributos.intelecto + " IT";
        if (textoDestreza != null) textoDestreza.text = atributos.destreza + " DT";
        if (textoRaiva != null) textoRaiva.text = atributos.raiva + " RV";

        // Cor ciano/azul no texto do escudo
        if (textoEscudo != null)
        {
            textoEscudo.color = new Color(0f, 0.85f, 1f, 1f);
        }

        // Cálculo de HP baseado na força
        VidaMaxima = 100 + (atributos.forca * 10);
        VidaAtual = VidaMaxima;

        EscudoAtual = 0;
        CargasSkillAtuais = 0;
        CargasUltAtuais = 0;

        GerarIconesCargas();

        AtualizarInterfaceVida(false);
        AtualizarInterfaceEscudo();
        AtualizarCargas();
    }

    private void GerarIconesCargas()
    {
        foreach (Transform child in containerHab) Destroy(child.gameObject);
        listaIconesSkill.Clear();

        foreach (Transform child in containerUlt) Destroy(child.gameObject);
        listaIconesUlt.Clear();

        for (int i = 0; i < CartaDados.cargasNecessariasSkill; i++)
        {
            GameObject novoIcone = Instantiate(prefabIconoCarga, containerHab);
            Image img = novoIcone.GetComponent<Image>();
            if (img != null) listaIconesSkill.Add(img);
        }

        for (int i = 0; i < CartaDados.cargasNecessariasUlt; i++)
        {
            GameObject novoIcone = Instantiate(prefabIconoCarga, containerUlt);
            Image img = novoIcone.GetComponent<Image>();
            if (img != null) listaIconesUlt.Add(img);
        }
    }

    // --- MANIPULAÇÃO DE HP E ESCUDO ---
    public void ReceberDano(int quantidadeDano)
    {
        int danoRestante = quantidadeDano;

        if (EscudoAtual > 0)
        {
            if (danoRestante <= EscudoAtual)
            {
                EscudoAtual -= danoRestante;
                danoRestante = 0;
            }
            else
            {
                danoRestante -= EscudoAtual;
                EscudoAtual = 0;
            }
            AtualizarInterfaceEscudo();
        }

        if (danoRestante > 0)
        {
            VidaAtual = Mathf.Max(0, VidaAtual - danoRestante);
            AtualizarInterfaceVida(true);
        }

        transform.DOShakePosition(0.3f, strength: 12f, vibrato: 15);
    }

    public void AdicionarEscudo(int quantidadeEscudo)
    {
        EscudoAtual += quantidadeEscudo;
        AtualizarInterfaceEscudo();
    }

    private void AtualizarInterfaceVida(bool animar)
    {
        float pctVida = (float)VidaAtual / VidaMaxima;

        if (textoHP != null) textoHP.text = $"{VidaAtual} HP";

        if (coracaoPreenchimento != null)
        {
            if (animar)
                coracaoPreenchimento.DOFillAmount(pctVida, 0.35f).SetEase(Ease.OutCubic);
            else
                coracaoPreenchimento.fillAmount = pctVida;
        }
    }

    private void AtualizarInterfaceEscudo()
    {
        if (groupEscudo == null) return;

        if (EscudoAtual > 0)
        {
            groupEscudo.gameObject.SetActive(true);
            if (textoEscudo != null) textoEscudo.text = EscudoAtual.ToString();

            float alphaCalculado = Mathf.Clamp01((float)EscudoAtual / 50f);
            groupEscudo.DOFade(Mathf.Max(0.4f, alphaCalculado), 0.2f);
        }
        else
        {
            groupEscudo.DOFade(0f, 0.2f).OnComplete(() => {
                groupEscudo.gameObject.SetActive(false);
            });
        }
    }

    // --- MANIPULAÇÃO DE CARGAS / HITS ---
    public void AdicionarCargaSkill(int quantidade = 1)
    {
        CargasSkillAtuais = Mathf.Min(CartaDados.cargasNecessariasSkill, CargasSkillAtuais + quantidade);
        AtualizarCargas();
    }

    public void AdicionarCargaUlt(int quantidade = 1)
    {
        CargasUltAtuais = Mathf.Min(CartaDados.cargasNecessariasUlt, CargasUltAtuais + quantidade);
        AtualizarCargas();
    }

    public void ResetarCargasSkill() { CargasSkillAtuais = 0; AtualizarCargas(); }
    public void ResetarCargasUlt() { CargasUltAtuais = 0; AtualizarCargas(); }

    private void AtualizarCargas()
    {
        for (int i = 0; i < listaIconesSkill.Count; i++)
        {
            if (listaIconesSkill[i] != null)
                listaIconesSkill[i].sprite = (i < CargasSkillAtuais) ? spriteSkillAcesa : spriteSkillApagada;
        }

        for (int i = 0; i < listaIconesUlt.Count; i++)
        {
            if (listaIconesUlt[i] != null)
                listaIconesUlt[i].sprite = (i < CargasUltAtuais) ? spriteUltAcesa : spriteUltApagada;
        }
    }
}