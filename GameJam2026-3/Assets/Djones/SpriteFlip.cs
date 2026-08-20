using UnityEngine;

public class SpriteFlip : MonoBehaviour
{
    private Rigidbody2D rb;
    private SpriteRenderer sprite;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Indo para a direita
        if (rb.linearVelocity.x > 0.01f)
        {
            sprite.flipX = false;
        }
        // Indo para a esquerda
        else if (rb.linearVelocity.x < -0.01f)
        {
            sprite.flipX = true;
        }
    }

}
