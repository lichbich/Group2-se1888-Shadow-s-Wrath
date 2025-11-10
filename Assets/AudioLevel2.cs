using UnityEngine;

public class AudioLevel2 : MonoBehaviour
{

    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;


    public AudioClip musicClip;
    public AudioClip buttonClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip jumpClip;
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

    public void playWinSound()
    {
        musicAudioSource.clip = winClip;
        musicAudioSource.PlayOneShot(winClip);
    }

    public void playLoseSound()
    {
        musicAudioSource.clip = loseClip;
        musicAudioSource.PlayOneShot(loseClip);
    }

    public void playJumpSound()
    {
        sfxAudioSource.PlayOneShot(jumpClip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
