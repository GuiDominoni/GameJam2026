using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private AudioSource _source;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _source = GetComponent<AudioSource>();
    }
    public void PlaySound(AudioClip clip)
    {
        _source.PlayOneShot(clip);
    }   
}
