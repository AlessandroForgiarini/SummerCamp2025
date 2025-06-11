using UnityEngine;

public class CubeSizeChange : MonoBehaviour
{

    private Transform myTransform;
    private bool isIncreasing;
    [SerializeField, Range(0, 10)] private float changeSizeSpeed = 5;
    [SerializeField, Range(0, 10)] private float minScale = 1f;
    [SerializeField, Range(0, 10)] private float maxScale = 1.5f;
    private float currentScale = 5;

    private void Start()
    {
        myTransform = transform;
        isIncreasing = true;
    }

    private void Update()
    {
        // Determino il cambiamento
        float changeAmount = changeSizeSpeed * Time.deltaTime;
        
        // In base alla modalità aumento o diminuisco la scala
        if (isIncreasing)
        {
            currentScale += changeAmount;
        }
        else
        {
            currentScale -= changeAmount;
        }
        
        // Se sforo i limiti, riporto i valori nell'intervallo corretto e inverto il cambiamento
        if (currentScale > maxScale)
        {
            currentScale = maxScale;
            isIncreasing = false;
        }
        else if (currentScale < minScale)
        {
            currentScale = minScale;
            isIncreasing = true;
        }
        
        // Applico la nuova scala
        Vector3 newSize = new Vector3(currentScale, currentScale, currentScale);
        myTransform.localScale = newSize;
    }
}
