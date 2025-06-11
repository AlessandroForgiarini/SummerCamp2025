using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class LevelSO : ScriptableObject
{
    [Serializable]
    public struct GoblinWaveData
    {
        [Range(0,15)]
        public int totalGoblins;
        [Range(0,5)]
        public float timeToSpawnGoblin;
        public ElementsListSO.ElementType[] availableElements;

        public ElementsListSO.ElementType GetRandomAvailableElement()
        {
            return availableElements[UnityEngine.Random.Range(0,availableElements.Length)];
        }
    }

    public ElementsListSO.ElementType[] GetElementsInWaves()
    {
        List<ElementsListSO.ElementType> elements = new List<ElementsListSO.ElementType>();
        foreach (GoblinWaveData waveData in goblinWaves)
        {
            foreach (var element in waveData.availableElements)
            {
                if(!elements.Contains(element))
                {
                    elements.Add(element);
                }
            }
        }

        return elements.ToArray();
    }
    
    public List<GoblinWaveData> goblinWaves;
}