using UnityEngine;

public class PlayerAnimController : MonoBehaviour
{
    [SerializeField] Animator animPlayer;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
    public void HookAnim()
    {
        animPlayer.SetBool("Change", true);
    }

    public void IdleAnim()
    {
        animPlayer.SetBool("Change", false);
    }

    public void Wake()
    {
        animPlayer.SetBool("Wake", true);
    }

}
