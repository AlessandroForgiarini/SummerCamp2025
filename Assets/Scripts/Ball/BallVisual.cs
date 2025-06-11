using System;
using System.Collections.Generic;
using UnityEngine;

public class BallVisual : MonoBehaviour
{
    [SerializeField] private Renderer ballRenderer;
    [SerializeField] private Material ballMaterial;
    [SerializeField] private ElementsListSO.ElementType activeElementType;
    [SerializeField] private ElementsListSO elementSos;
    [SerializeField] private ParticleSystem smokeParticles;
    [SerializeField] private List<ParticleSystem> explosionParticles;
    [SerializeField, Range(0f,5f)] private float explosionRadius = 5f;
    
    private void OnValidate()
    {
        UpdateVisual(activeElementType);
    }

    public void UpdateVisual(ElementsListSO.ElementType ballElement)
    {
        activeElementType = ballElement;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        Color ballColor = elementSos.GetColorFromElement(activeElementType);
        Material newMaterial = new Material(ballMaterial);
        float startAlpha = ballMaterial.color.a;
        newMaterial.color = new Color(ballColor.r, ballColor.g, ballColor.b, startAlpha);
        ballRenderer.sharedMaterial = newMaterial;
        
        ParticleSystem.MainModule mainSmoke = smokeParticles.main;
        mainSmoke.startColor = ballColor;
        
        UpdateExplosionParticle();
    }

    private void UpdateExplosionParticle()
    {
        Color ballColor = elementSos.GetColorFromElement(activeElementType);
        
        foreach (ParticleSystem particle in explosionParticles)
        {
            ParticleSystem.MainModule main = particle.main;
            main.startColor = ballColor;
            
            ParticleSystem.ShapeModule shape = particle.shape;
            shape.radius = explosionRadius;
        }
    }

    public void DisableVisuals()
    {
        ballRenderer.enabled = false;
        smokeParticles.Stop();
        
        foreach (ParticleSystem particle in explosionParticles)
        {
            particle.Stop();
        }
    }

    public void EnableVisuals()
    {
        ballRenderer.enabled = true;
        smokeParticles.Play();
    }

    public void ExplodeVisual(float radius, float explosionDuration)
    {
        smokeParticles.Stop();
        ballRenderer.enabled = false;
        explosionRadius = radius;
        
        UpdateExplosionParticle();
        
        foreach (ParticleSystem particle in explosionParticles)
        {
            particle.Play();
        }

        Invoke(nameof(StopExplosion), explosionDuration);
    }

    private void StopExplosion()
    {
        foreach (ParticleSystem particle in explosionParticles)
        {
            particle.Stop();
        }
    }
}