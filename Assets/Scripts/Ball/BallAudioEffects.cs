using UnityEngine;

public class BallAudioEffects : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip throwEffect;
    [SerializeField] private AudioClip explodeEffect;
    
    public void Throw(float throwSpeedMagnitude)
    {
        float velocityMaxAmplitudeEffect = 10;
        float scaledVelocity = throwSpeedMagnitude / velocityMaxAmplitudeEffect;
        // if velocity is over max scale down to 1
        float volume = Mathf.Min(1, scaledVelocity);
        FantasyAudioManager.Instance.PlayEffect(audioSource, throwEffect, volume);
    }
    
    public void PlayExplodeEffect()
    {
        FantasyAudioManager.Instance.PlayEffect(audioSource, explodeEffect);
    }
}