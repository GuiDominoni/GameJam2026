using UnityEngine;
using UnityEngine.SceneManagement;

public class GameScene : MonoBehaviour
{
    // Avança para a próxima cena
    public void ProximaCena()
    {
        int proximaCena = SceneManager.GetActiveScene().buildIndex + 1;

        // Se chegou na última, volta para a primeira
        if (proximaCena >= SceneManager.sceneCountInBuildSettings)
        {
            proximaCena = 0;
        }

        SceneManager.LoadScene(proximaCena);
    }

    // Reinicia a cena atual
    public void ReiniciarCena()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SairDoJogo()
    {
        Application.Quit();
    }
}