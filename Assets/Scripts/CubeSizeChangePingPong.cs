using UnityEngine;

public class CubeSizeChangePingPong : MonoBehaviour
{
    
    private Transform myTransform;
    [SerializeField, Range(0, 10)] private float changeSizeSpeed = 5;
    [SerializeField, Range(0, 10)] private float minScale = 0.5f;
    [SerializeField, Range(0, 10)] private float maxScale = 1.5f;

    private void Start()
    {
        myTransform = transform;
    }

    private void Update()
    {
        // Determino la velocità di cambiamento
        float changeSpeed = Time.time * changeSizeSpeed;
        
        // Determino l'ampiezza pura del cambiamento
        float length = maxScale-minScale;
        
        // Calcolo il punto in cui sono durante l'animazione
        float animationStep = Mathf.PingPong(changeSpeed, length) + minScale;
        
        // Converto il numero puro a Vector3
        Vector3 finalScale = Vector3.one * animationStep;
        
        // Applico la nuova scalatura
        myTransform.localScale = finalScale;
    }
}
