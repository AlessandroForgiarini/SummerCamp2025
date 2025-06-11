using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ElementsListSO : ScriptableObject
{
    [Serializable]
    public enum ElementType
    {
        INVALID,
        FIRE,
        WATER,
        GRASS
    }

    [Serializable]
    public struct ElementData
    {
        public ElementType type;
        public Color color;
    }
    
    public List<ElementData> elements;

    public Color GetColorFromElement(ElementType elementType)
    {
        Color color = Color.magenta;
        foreach (ElementData element in elements)
        {
            if (elementType == element.type)
            {
                color = element.color;
                break;
            }
        }
        return color;
    }

    public ElementData GetDataFromElement(ElementType elementType)
    {
        ElementData data = elements[0];
        foreach (ElementData element in elements)
        {
            if (elementType == element.type)
            {
                data = element;
                break;
            }
        }
        return data;
    }
}