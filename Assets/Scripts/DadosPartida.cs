using System.Collections.Generic;

public static class DadosPartida
{
    // Armazena as 2 cartas que o Jogador 1 escolheu no Draft
    public static List<CartaData> cartasJogador1 = new List<CartaData>();

    // Armazena as 2 cartas que o Jogador 2 escolheu no Draft
    public static List<CartaData> cartasJogador2 = new List<CartaData>();

    // Armazena os atributos já distribuídos de cada carta para a batalha
    public static List<AtributosRuntime> atributosFinalJ1 = new List<AtributosRuntime>();
    public static List<AtributosRuntime> atributosFinalJ2 = new List<AtributosRuntime>();

    // Limpa os dados ao reiniciar o jogo
    public static void ResetarPartida()
    {
        cartasJogador1.Clear();
        cartasJogador2.Clear();
        atributosFinalJ1.Clear();
        atributosFinalJ2.Clear();
    }
}