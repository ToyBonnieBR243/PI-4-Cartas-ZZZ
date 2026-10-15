using UnityEngine;

public enum TipoAlcanceAtaque
{
    AlvoUnico,
    AmbasAsCartas
}

[CreateAssetMenu(fileName = "NovaCartaData", menuName = "Jogo/Carta Data")]
public class CartaData : ScriptableObject
{
    [Header("Dados Básicos")]
    public string nomePersonagem;
    public Sprite arteCarta;

    [Header("Lore e Descrição Completa (Para Tela de Seleção)")]
    [TextArea(4, 10)]
    public string historiaLore;
    [TextArea(4, 10)]
    public string habilidades;

    [Header("Resumos Objetivos (Para Tela de Batalha / UI)")]
    [TextArea(2, 3)]
    public string descricaoResumidaSkill;
    [TextArea(2, 3)]
    public string descricaoResumidaUlt;

    [Header("Cargas Necessárias para Batalha")]
    public int cargasNecessariasSkill = 3;
    public int cargasNecessariasUlt = 6;

    [Header("Alcance e Alvo do Ataque")]
    public TipoAlcanceAtaque alcanceSkill = TipoAlcanceAtaque.AlvoUnico;
    public TipoAlcanceAtaque alcanceUlt = TipoAlcanceAtaque.AlvoUnico;

    [Header("Valores da Habilidade (Skill)")]
    public bool skillPodeCritar = false;
    public int danoBaseSkill = 0;
    public int danoCriticoSkill = 0;
    public int valorCuraSkill = 0;
    public int valorBuffEscudoSkill = 0;

    [Header("Valores da Ultimate (Ult)")]
    public bool ultPodeCritar = false;
    public int danoBaseUlt = 0;
    public int danoCriticoUlt = 0;
    public int valorCuraUlt = 0;
    public int valorBuffEscudoUlt = 0;

    [Header("Buffs de Status da Habilidade (Skill)")]
    public int buffForcaSkill = 0;
    public int buffVelocidadeSkill = 0;
    public int buffIntelectoSkill = 0;
    public int buffDestrezaSkill = 0;
    public int buffRaivaSkill = 0;

    [Header("Buffs de Status da Ultimate (Ult)")]
    public int buffForcaUlt = 0;
    public int buffVelocidadeUlt = 0;
    public int buffIntelectoUlt = 0;
    public int buffDestrezaUlt = 0;
    public int buffRaivaUlt = 0;
}