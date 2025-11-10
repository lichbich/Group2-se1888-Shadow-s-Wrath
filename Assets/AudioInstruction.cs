using UnityEngine;

public class AudioInstruction : MonoBehaviour
{

    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;


    public AudioClip musicClip;
    public AudioClip buttonClip;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        musicAudioSource.clip = musicClip;
        musicAudioSource.volume = 0.2f;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public void PlayButtonSound()
    {
        sfxAudioSource.PlayOneShot(buttonClip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
