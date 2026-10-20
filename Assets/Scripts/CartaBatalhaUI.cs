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
    public Image coracaoPreenchimento; // Image Type = Filled, Fill Method = Vertical, Fill Origin = Bottom
    public TextMeshProUGUI textoHP;

    [Header("Sistema de Escudo")]
    public CanvasGroup groupEscudo;
    public Image imagemPreenchimentoEscudo; // Image Type = Filled, Fill Method = Vertical, Fill Origin = Bottom
    public TextMeshProUGUI textoEscudo;

    [Header("Containers de Cargas")]
    public Transform containerHab;
    public Transform containerUlt;

    [Header("Prefab Base dos Ícones e Texto Flutuante")]
    public GameObject prefabIconoCarga;
    public GameObject prefabTextoFlutuante;

    [Header("Sprites Compartilhados da Habilidade (Skill)")]
    public Sprite spriteSkillApagada;
    public Sprite spriteSkillAcesa;

    [Header("Sprites Compartilhados da Ultimate (Ult)")]
    public Sprite spriteUltApagada;
    public Sprite spriteUltAcesa;

    private List<Image> listaIconesSkill = new List<Image>();
    private List<Image> listaIconesUlt = new List<Image>();

    public CartaData CartaDados { get; private set; }
    public AtributosRuntime Atributos { get; private set; }
    public EstadoStatusCarta Status { get; private set; }

    public int VidaAtual { get; private set; }
    public int VidaMaxima { get; private set; }
    public int EscudoAtual { get; private set; }
    public int EscudoMaximo { get; private set; }
    public int CargasSkillAtuais { get; private set; }
    public int CargasUltAtuais { get; private set; }

    public void InicializarCarta(CartaData cartaData, AtributosRuntime atributos)
    {
        CartaDados = cartaData;
        Atributos = atributos;

        Status = GetComponent<EstadoStatusCarta>();
        if (Status == null) Status = gameObject.AddComponent<EstadoStatusCarta>();

        if (imagemArteCompleta != null) imagemArteCompleta.sprite = cartaData.arteCarta;

        if (textoHabilidade != null) textoHabilidade.text = cartaData.descricaoResumidaSkill;
        if (textoUltimate != null) textoUltimate.text = cartaData.descricaoResumidaUlt;

        if (textoForca != null) textoForca.text = atributos.forca + " FR";
        if (textoVelocidade != null) textoVelocidade.text = atributos.velocidade + " VL";
        if (textoIntelecto != null) textoIntelecto.text = atributos.intelecto + " IT";
        if (textoDestreza != null) textoDestreza.text = atributos.destreza + " DT";
        if (textoRaiva != null) textoRaiva.text = atributos.raiva + " RV";

        // Trava Vida base em 100
        VidaMaxima = 100;
        VidaAtual = VidaMaxima;
        EscudoAtual = 0;
        EscudoMaximo = 0;

        GerarIconesCargas();

        AtualizarInterfaceVida(false);
        AtualizarInterfaceEscudo(false);
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

    // --- MANIPULAÇÃO DE DANO, CURA E ESCUDO ---
    public void ReceberDano(int quantidadeDano)
    {
        int danoRestante = quantidadeDano;

        // 1. Dano no Escudo
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
            AtualizarInterfaceEscudo(true);
        }

        // 2. Dano na Vida
        if (danoRestante > 0)
        {
            VidaAtual = Mathf.Max(0, VidaAtual - danoRestante);
            AtualizarInterfaceVida(true);
        }
    }

    public void SetarEscudoInicial(int valorEscudo)
    {
        EscudoAtual = valorEscudo;
        EscudoMaximo = valorEscudo; // Define o valor cheio como o limite máximo do escudo
        AtualizarInterfaceEscudo(false);
    }

    public void AdicionarEscudo(int quantidadeEscudo)
    {
        EscudoAtual += quantidadeEscudo;
        if (EscudoAtual > EscudoMaximo) EscudoMaximo = EscudoAtual;
        AtualizarInterfaceEscudo(true);
    }

    public void Curar(int quantidadeCura)
    {
        VidaAtual = Mathf.Min(VidaMaxima, VidaAtual + quantidadeCura);
        AtualizarInterfaceVida(true);
    }

    // --- INTERFACE VISUAL DE VIDA E ESCUDO ---
    private void AtualizarInterfaceVida(bool animar)
    {
        float pctVida = Mathf.Clamp01((float)VidaAtual / VidaMaxima);

        if (textoHP != null) textoHP.text = $"{VidaAtual} HP";

        if (coracaoPreenchimento != null)
        {
            coracaoPreenchimento.gameObject.SetActive(true);
            if (animar)
                coracaoPreenchimento.DOFillAmount(pctVida, 0.3f).SetEase(Ease.OutCubic).SetUpdate(true);
            else
                coracaoPreenchimento.fillAmount = pctVida;
        }
    }

    private void AtualizarInterfaceEscudo(bool animar)
    {
        if (groupEscudo == null) return;

        if (EscudoAtual > 0)
        {
            groupEscudo.gameObject.SetActive(true);
            groupEscudo.alpha = 1f;

            if (textoEscudo != null)
            {
                textoEscudo.text = $"{EscudoAtual} DEF";
                textoEscudo.color = new Color(0f, 0.85f, 1f, 1f); // Ciano Vivo
            }

            // Garante que o escudo comece em 1.0 (100% cheio) quando gerado
            float pctEscudo = (EscudoMaximo > 0) ? Mathf.Clamp01((float)EscudoAtual / EscudoMaximo) : 1f;

            if (imagemPreenchimentoEscudo != null)
            {
                imagemPreenchimentoEscudo.gameObject.SetActive(true);
                if (animar)
                    imagemPreenchimentoEscudo.DOFillAmount(pctEscudo, 0.3f).SetEase(Ease.OutCubic).SetUpdate(true);
                else
                    imagemPreenchimentoEscudo.fillAmount = pctEscudo;
            }
        }
        else
        {
            if (imagemPreenchimentoEscudo != null)
            {
                if (animar) imagemPreenchimentoEscudo.DOFillAmount(0f, 0.2f).SetUpdate(true);
                else imagemPreenchimentoEscudo.fillAmount = 0f;
            }

            groupEscudo.DOFade(0f, 0.25f).SetUpdate(true).OnComplete(() => {
                groupEscudo.gameObject.SetActive(false);
            });
        }
    }

    // --- ANIMAÇÕES DE MOVIMENTO BASEADAS NO SLOT COMO ÂNCORA ---
    public void AnimarInvestidaEVoltar(Vector3 posicaoAlvoWorld, TweenCallback aoAtingirImpacto)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 posLocalOriginal = Vector3.zero;

        Vector3 posAlvoLocal = transform.parent.InverseTransformPoint(posicaoAlvoWorld);

        Vector3 direcao = (posAlvoLocal - posLocalOriginal).normalized;
        float distanciaDeslocamento = Vector3.Distance(posLocalOriginal, posAlvoLocal);

        float distanciaAjustada = Mathf.Max(0f, distanciaDeslocamento - 110f);
        Vector3 pontoImpactoBorda = posLocalOriginal + (direcao * distanciaAjustada);

        Sequence seqInvestida = DOTween.Sequence();
        seqInvestida.SetUpdate(true);

        seqInvestida.Append(rectTransform.DOLocalMove(pontoImpactoBorda, 0.10f).SetEase(Ease.InQuad));
        seqInvestida.AppendCallback(() => {
            aoAtingirImpacto?.Invoke();
        });

        seqInvestida.Append(rectTransform.DOLocalMove(posLocalOriginal, 0.14f).SetEase(Ease.OutQuad));
    }

    public void AnimarEsquiva(Vector3 direcaoInimigo)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        Vector3 posLocalOriginal = Vector3.zero;

        Vector3 posRecuoExagerado = posLocalOriginal - (direcaoInimigo.normalized * 120f);

        Sequence seqEsquiva = DOTween.Sequence();
        seqEsquiva.SetUpdate(true);
        seqEsquiva.Append(rectTransform.DOLocalMove(posRecuoExagerado, 0.12f).SetEase(Ease.OutQuad));
        seqEsquiva.Append(rectTransform.DOLocalMove(posLocalOriginal, 0.22f).SetEase(Ease.OutBack));

        MostrarTextoPopUp("ESQUIVOU!", new Color(0.4f, 0.85f, 1f));
    }

    public void DefinirVisualEstado()
    {
        if (imagemArteCompleta == null) return;

        if (Status != null && Status.estaCongelado)
        {
            imagemArteCompleta.DOColor(new Color(0.4f, 0.7f, 1.0f), 0.25f);
        }
        else if (Status != null && Status.estaAtordoado)
        {
            imagemArteCompleta.DOColor(new Color(0.45f, 0.45f, 0.45f), 0.25f);
        }
        else
        {
            imagemArteCompleta.DOColor(Color.white, 0.2f);
        }
    }

    public void PiscarCarta(Color corPisca)
    {
        if (imagemArteCompleta == null) return;

        imagemArteCompleta.DOColor(corPisca, 0.08f).SetUpdate(true).OnComplete(() => {
            DefinirVisualEstado();
        });
    }

    public void MostrarTextoPopUp(string texto, Color cor)
    {
        if (prefabTextoFlutuante == null) return;

        GameObject objTexto = Instantiate(prefabTextoFlutuante, transform.parent);
        TextoFlutuante scriptTexto = objTexto.GetComponent<TextoFlutuante>();
        if (scriptTexto != null)
        {
            scriptTexto.ExibirTexto(texto, cor, transform.position);
        }
    }

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