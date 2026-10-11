[System.Serializable]
public class AtributosRuntime
{
    public CartaData cartaBase;

    // Atributos distribuídos (máximo 10 cada, total 20 pontos)
    public int forca = 0;
    public int velocidade = 0;
    public int intelecto = 0;
    public int destreza = 0;
    public int raiva = 0;

    public int pontosRestantes = 20;

    public AtributosRuntime(CartaData carta)
    {
        cartaBase = carta;
        forca = 0;
        velocidade = 0;
        intelecto = 0;
        destreza = 0;
        raiva = 0;
        pontosRestantes = 20;
    }

    public int TotalDistribuido()
    {
        return forca + velocidade + intelecto + destreza + raiva;
    }
}