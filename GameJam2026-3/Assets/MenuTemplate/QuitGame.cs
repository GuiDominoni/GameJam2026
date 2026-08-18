using UnityEngine;

public class QuitGame : MonoBehaviour
{
    public void SairDoJogo()
    {
        // Log para confirmar que o botão foi clicado no console
        Debug.Log("Saindo do jogo...");

        // Fecha o aplicativo (funciona no jogo buildado)
        Application.Quit();

        // Para o modo de play se estiver testando dentro do Editor do Unity
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}