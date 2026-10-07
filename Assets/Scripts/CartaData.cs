using UnityEngine;

[CreateAssetMenu(fileName = "NovaCarta", menuName = "Cartas/Dados da Carta")]
public class CartaData : ScriptableObject
{
    public string nomePersonagem;
    public Sprite arteCarta;

    [TextArea(4, 10)]
    public string historiaLore;

    [TextArea(4, 10)]
    public string habilidades;

    // Valores base fixos exigidos nas regras do projeto
    public int vidaBase = 100;
    public int danoMinimo = 10;
    public int danoMaximo = 20;
}