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

        // Posi��o do mouse na tela
        Vector2 mousePosition = Input.mousePosition;

        // Dire��o do centro da tela at� o mouse
        Vector2 direction = mousePosition - screenCenter;

        // Calcula o �ngulo
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Rotaciona
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }
}