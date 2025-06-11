using UnityEngine;

public class TitleTextAnimation : MonoBehaviour
{
    private Transform myTransform;
    private bool isIncreasing;

    [SerializeField, Range(0, 10)]
    private float changeSizeSpeed = 5;
    [SerializeField, Range(0, 2)]
    private float minScale = 1f;
    [SerializeField, Range(0, 2)]
    private float maxScale = 1.2f;
    private float currentScale;

    void Start()
    {
        myTransform = transform;
        isIncreasing = true;
    }

    void Update()
    {
        float changeAmount = changeSizeSpeed * Time.deltaTime;
        if (isIncreasing)
        {
            currentScale += changeAmount;
        }
        else
        {
            currentScale -= changeAmount;
            
        }
        
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
        Vector3 newSize = new Vector3(currentScale, currentScale, currentScale);
        myTransform.localScale = newSize;
    }
}
