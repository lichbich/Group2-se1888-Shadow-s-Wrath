using UnityEngine;

public class AudioLevel1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public AudioSource musicAudioSource;
    public AudioSource sfxAudioSource;


    public AudioClip musicClip;
    public AudioClip buttonClip;
    public AudioClip winClip;
    public AudioClip loseClip;
    public AudioClip chestClip;
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
        musicAudioSource.PlayOneShot(loseClip, 1.2f);
    }

    public void playChestSound()
    {
        sfxAudioSource.PlayOneShot(chestClip, 1.2f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
