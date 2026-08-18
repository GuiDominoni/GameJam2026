using UnityEngine;

public class AimMouse : MonoBehaviour
{
    void Update()
    {
        // Centro da tela
        Vector2 screenCenter = new Vector2(
            Screen.width / 2f,
            Screen.height / 2f
        );

        // Posição do mouse na tela
        Vector2 mousePosition = Input.mousePosition;

        // Direção do centro da tela até o mouse
        Vector2 direction = mousePosition - screenCenter;

        // Calcula o ângulo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotaciona
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}