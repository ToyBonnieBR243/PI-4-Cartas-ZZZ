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
    public Image coracaoPreenchimento;
    public TextMeshProUGUI textoHP;

    [Header("Sistema de Escudo")]
    public CanvasGroup groupEscudo;
    public Image imagemPreenchimentoEscudo;
    public TextMeshProUGUI textoEscudo;

    [Header("Containers de Cargas")]
    public Transform containerHab;
    public Transform containerUlt;

    [Header("Sistema de Status (Temporizador)")]
    public TextMeshProUGUI textoTemporizadorStatus;

    [Header("Prefab Base dos Ícones e Texto Flutuante")]
    public GameObject prefabIconoCarga;
    public GameObject prefabTextoFlutuante;

    [Header("Botões de Habilidade e Ult (Atrás do Texto)")]
    public Button btnUsarSkill;
    public Button btnUsarUlt;

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

    // Flags de seleção de ação especial para o turno atual
    public bool querUsarSkill { get; private set; } = false;
    public bool querUsarUlt { get; private set; } = false;

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

        VidaMaxima = 200;
        VidaAtual = VidaMaxima;
        EscudoAtual = 0;
        EscudoMaximo = 0;

        GerarIconesCargas();

        AtualizarInterfaceVida(false);
        AtualizarInterfaceEscudo(false);
        AtualizarCargas();

        Debug.Log($"[DEBUG-UI] Carta inicializada: {cartaData.nomePersonagem} | HP Inicial: {VidaAtual}");
    }

    // Atualiza a interatividade dos botões de Habilidade/Ult com base nas cargas
    public void AtualizarEstadoBotoesAcoesEspeciais(bool interativo)
    {
        if (btnUsarSkill != null)
        {
            bool podeUsarSkill = (CargasSkillAtuais >= CartaDados.cargasNecessariasSkill);
            btnUsarSkill.gameObject.SetActive(podeUsarSkill);
            btnUsarSkill.interactable = interativo && podeUsarSkill;
        }

        if (btnUsarUlt != null)
        {
            bool podeUsarUlt = (CargasUltAtuais >= CartaDados.cargasNecessariasUlt);
            btnUsarUlt.gameObject.SetActive(podeUsarUlt);
            btnUsarUlt.interactable = interativo && podeUsarUlt;
        }
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

    public void ReceberDano(int quantidadeDano)
    {
        Debug.Log($"[DEBUG-UI] ReceberDano chamado em {gameObject.name}. Dano recebido: {quantidadeDano}. HP antes: {VidaAtual} | Escudo antes: {EscudoAtual}");
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
                EscudoMaximo = 0;
            }
            AtualizarInterfaceEscudo(true);
        }

        if (danoRestante > 0)
        {
            VidaAtual = Mathf.Max(0, VidaAtual - danoRestante);
            Debug.Log($"[DEBUG-UI] HP após dano em {gameObject.name}: {VidaAtual}");
            AtualizarInterfaceVida(true);
        }
    }

    public void SetarEscudoInicial(int valorEscudo)
    {
        EscudoAtual = valorEscudo;
        EscudoMaximo = valorEscudo;

        // Garante ativação imediata para qualquer carta com escudo inicial > 0
        if (groupEscudo != null)
        {
            groupEscudo.gameObject.SetActive(valorEscudo > 0);
            groupEscudo.alpha = valorEscudo > 0 ? 1f : 0f;
        }

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

    private void AtualizarInterfaceVida(bool animar)
    {
        float pctVida = Mathf.Clamp01((float)VidaAtual / VidaMaxima);
        Debug.Log($"[DEBUG-UI] Atualizando UI Vida de {gameObject.name} -> Texto: {VidaAtual} HP | Pct Fill: {pctVida}");

        if (textoHP != null)
        {
            textoHP.text = $"{VidaAtual} HP";
        }
        else
        {
            Debug.LogWarning($"[AVISO-UI] 'textoHP' está nulo no Inspector de {gameObject.name}!");
        }

        if (coracaoPreenchimento != null)
        {
            coracaoPreenchimento.gameObject.SetActive(true);
            if (animar)
                coracaoPreenchimento.DOFillAmount(pctVida, 0.3f).SetEase(Ease.OutCubic).SetUpdate(true);
            else
                coracaoPreenchimento.fillAmount = pctVida;
        }
        else
        {
            Debug.LogWarning($"[AVISO-UI] 'coracaoPreenchimento' está nulo no Inspector de {gameObject.name}!");
        }
    }

    private void AtualizarInterfaceEscudo(bool animar)
    {
        if (groupEscudo == null) return;

        // O texto de HP Some se houver escudo ativo
        if (textoHP != null)
        {
            textoHP.gameObject.SetActive(EscudoAtual <= 0);
        }

        if (EscudoAtual > 0)
        {
            groupEscudo.gameObject.SetActive(true);
            groupEscudo.alpha = 1f;

            if (textoEscudo != null)
            {
                textoEscudo.text = $"{EscudoAtual} DEF";
                textoEscudo.color = new Color(0f, 0.85f, 1f, 1f);
            }

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

    // Método para atualizar o texto temporizador dos estados na UI da carta
    public void AtualizarTextoTemporizador(string textoTempo)
    {
        if (textoTemporizadorStatus != null)
        {
            textoTemporizadorStatus.text = textoTempo;
            textoTemporizadorStatus.gameObject.SetActive(!string.IsNullOrEmpty(textoTempo));
        }
    }

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
        Debug.Log($"[DEBUG-Cargas] {CartaDados.nomePersonagem} ganhou carga de Skill. Total: {CargasSkillAtuais}");
        AtualizarCargas();
    }

    public void AdicionarCargaUlt(int quantidade = 1)
    {
        CargasUltAtuais = Mathf.Min(CartaDados.cargasNecessariasUlt, CargasUltAtuais + quantidade);
        Debug.Log($"[DEBUG-Cargas] {CartaDados.nomePersonagem} ganhou carga de Ult. Total: {CargasUltAtuais}");
        AtualizarCargas();
    }

    public void BotaoClicadoSkill()
    {
        querUsarSkill = true;
        querUsarUlt = false;
        Debug.Log($"{CartaDados.nomePersonagem} selecionou Habilidade!");
    }

    public void BotaoClicadoUlt()
    {
        querUsarUlt = true;
        querUsarSkill = false;
        Debug.Log($"{CartaDados.nomePersonagem} selecionou Ultimate!");
    }

    public void LimparAcoesEspeciais()
    {
        querUsarSkill = false;
        querUsarUlt = false;
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