using Unity.VisualScripting;
using UnityEngine;

public class BotaoAnimacao : MonoBehaviour
{
    public Animator animator;

    public void WakeRaoni()
    {
        animator.SetTrigger("Wake");
    }

    private void Update()
    {
        if((Input.GetMouseButtonDown(0)))
        {
            WakeRaoni();
        }
    }
}