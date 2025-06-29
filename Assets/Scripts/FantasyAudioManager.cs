using UnityEngine;
using UnityEngine.Audio;

public class FantasyAudioManager : MonoBehaviour
{
    public static FantasyAudioManager Instance { get; private set; }
    
    [Header("Audio Clips")]
    [SerializeField] private AudioClip uiClickEffect;
    [SerializeField] private AudioClip gamePlayMusic;
    
    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicAudioSource;
    [SerializeField] private AudioSource uiAudioSource;
    
    [Header("Audio Mixers")]
    [SerializeField] private AudioMixer gameAudioMixer;
    [SerializeField] private AudioMixerGroup effectsAudioMixerGroup;
    [SerializeField] private AudioMixerGroup musicAudioMixerGroup;
    
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        PlayBackgroundMusic();
    }

    private void PlayBackgroundMusic()
    {
        musicAudioSource.clip = gamePlayMusic;
        musicAudioSource.loop = true;
        musicAudioSource.Play();
    }

    public void PlayUIClickEffect()
    {
        uiAudioSource.Stop();
        uiAudioSource.clip = uiClickEffect;
        uiAudioSource.loop = false;
        uiAudioSource.Play();
    }
    
    public void PlayEffect(AudioSource audioSource, AudioClip clip)
    {
        audioSource.outputAudioMixerGroup = effectsAudioMixerGroup;
        audioSource.Stop();
        audioSource.PlayOneShot(clip, 1);
    }

    public void PlayEffect(AudioSource audioSource, AudioClip clip, float volume)
    {
        audioSource.outputAudioMixerGroup = effectsAudioMixerGroup;
        volume = Mathf.Clamp01(volume);
        audioSource.PlayOneShot(clip, volume);
    }
}