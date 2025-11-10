using UnityEngine;

public class AudioMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public AudioSource musicAudioSource;
    public AudioSource buttonAudioSource;


    public AudioClip musicClip;
    public AudioClip buttonClip;

    void Start()
    {
        musicAudioSource.clip = musicClip;
        musicAudioSource.volume = 0.2f;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public void PlayButtonSound()
    {
        buttonAudioSource.clip = buttonClip;
        buttonAudioSource.PlayOneShot(buttonClip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
