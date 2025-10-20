using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public AudioSource musicAudioSource;
    public AudioSource vfxAudioSource;

    public AudioClip musicClip;
    public AudioClip buttonClip;
    void Start()
    {
        musicAudioSource.clip = musicClip;
        musicAudioSource.Play();
    }

    // Update is called once per frame
    public void PlaySFX(AudioClip sfxClip)
    {
        vfxAudioSource.clip = sfxClip;
        vfxAudioSource.PlayOneShot(sfxClip);
    }


}
