using UnityEngine;

[CreateAssetMenu]
public class GoblinDataSO : ScriptableObject
{
    [Range(1,3)]  public int MaxHealth;
    [Range(1,10)] public float GatherTime;
    [Range(1,10)] public float WalkingSpeed;
    [Range(1,10)] public float RunningSpeed;
}
