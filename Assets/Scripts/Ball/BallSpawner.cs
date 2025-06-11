using System;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class BallSpawner : MonoBehaviour
{
    [SerializeField] private BallHandler ballPrefab;
    [SerializeField] private ElementsListSO.ElementType spawnerElement;
    [SerializeField] private Renderer spawnerRenderer;
    [SerializeField] private Material spawnerMaterial;
    [SerializeField] private ElementsListSO elementSos;
    [SerializeField] private ParticleSystem[] smokeParticles;

    private BallHandler _ballReference;
    
    private void Start()
    {
        UpdateVisual();
    }

    private void OnValidate()
    {
        UpdateVisual();
    }
    
    private void UpdateVisual()
    {
        ElementsListSO.ElementData elementData = elementSos.GetDataFromElement(spawnerElement);
        Color elementColor = elementData.color;
        Material newMaterial = new Material(spawnerMaterial);
        float startAlpha = elementColor.a;
        newMaterial.color = new Color(elementColor.r, elementColor.g, elementColor.b, startAlpha);
        spawnerRenderer.sharedMaterial = newMaterial;
        
        foreach (ParticleSystem smokeParticle in smokeParticles)
        {
            ParticleSystem.MainModule main = smokeParticle.main;
            main.startColor = newMaterial.color;
        }
    }
    
    public void SetSpawnerElement(ElementsListSO.ElementType element)
    {
        spawnerElement = element;
        UpdateVisual();
    }
    
    public BallHandler GetNewBall()
    {
        BallHandler ballHandler = Instantiate(ballPrefab);
        ballHandler.Init(transform.position, spawnerElement);
        return ballHandler;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        TrackedPoseDriver controller = other.gameObject.GetComponentInParent<TrackedPoseDriver>();
        
        if (!controller) return;
        
        BallHandler ballReference = GetNewBall();
        ballReference.DisableVisuals();
        _ballReference = ballReference;
    }

    private void OnTriggerStay(Collider other)
    {
        TrackedPoseDriver controller = other.gameObject.GetComponentInParent<TrackedPoseDriver>();
        
        if (!controller) return;
        if (!_ballReference) return;
        if (_ballReference.PickedUp) return;
        
        _ballReference.transform.position = other.transform.position;
    }

    private void OnTriggerExit(Collider other)
    {
        TrackedPoseDriver controller = other.gameObject.GetComponentInParent<TrackedPoseDriver>();
        
        if (!controller) return;
        if (!_ballReference) return;
        if (_ballReference.PickedUp) return;
        
        _ballReference.DestroyBall();
        _ballReference = null;
    }
}
