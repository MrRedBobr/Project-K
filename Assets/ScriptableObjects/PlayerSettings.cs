using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Settings")]
public class PlayerSettings : ScriptableObject
{
    public float allowRotation;
    public float directionRotationSpeed;
    [Range(1, 10)] public float gravity;
}
