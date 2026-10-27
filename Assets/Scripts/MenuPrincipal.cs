using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPrincipal : MonoBehaviour
{
    [Header("Painéis Opcionais (Caso os créditos fiquem na mesma cena)")]
    public GameObject painelMenuInicial;
    public GameObject painelCreditos;

    private void Start()
    {
        // Garante que o menu começa aberto e os créditos fechados
        if (painelMenuInicial != null) painelMenuInicial.SetActive(true);
        if (painelCreditos != null) painelCreditos.SetActive(false);
    }

    // Botão de Iniciar o Jogo (Vai para a cena de Seleção de Cartas)
    public void IniciarJogo()
    {
        Time.timeScale = 1.0f;
        // Substitua "TelaSelecao" pelo nome exato da sua cena de seleção de cartas no Build Settings
        SceneManager.LoadScene("SelecaoCartas");
    }

    // Botão para Abrir os Créditos
    public void AbrirCreditos()
    {
        if (painelMenuInicial != null) painelMenuInicial.SetActive(false);
        if (painelCreditos != null) painelCreditos.SetActive(true);
    }

    // Botão para Fechar os Créditos e voltar ao Menu
    public void FecharCreditos()
    {
        if (painelCreditos != null) painelCreditos.SetActive(false);
        if (painelMenuInicial != null) painelMenuInicial.SetActive(true);
    }

    // Botão para Sair do Jogo
    public void SairDoJogo()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }
}