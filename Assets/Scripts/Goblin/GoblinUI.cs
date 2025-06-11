using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GoblinUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image[] healthHearts;
    [SerializeField] private Image progressBar;

    [SerializeField, Range(0,3)] private int currentHealth = 1;
    [SerializeField, Range(0,1)] private float currentProgress = 0.5f;
    
    public void UpdateHeartsUI(int health)
    {
        for (int i = 0; i < healthHearts.Length; i++)
        {
            healthHearts[i].enabled = i < health;
        }
    }

    public void UpdateGatherProgressBar(float value)
    {
        value = Mathf.Clamp01(value);
        progressBar.fillAmount = value;
    }

    private void OnValidate()
    {
        UpdateHeartsUI(currentHealth);
        UpdateGatherProgressBar(currentProgress);
    }
}
