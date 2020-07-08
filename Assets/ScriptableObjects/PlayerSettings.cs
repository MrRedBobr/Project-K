using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayerPridSet
{
    [CreateAssetMenu(menuName = "Player/Settings")]
    public class PlayerSettings : ScriptableObject
    {
        public float allowRotation;
        public float directionRotationSpeed;
        [Range(1, 10)] public float gravity;

        [Header("RightHandProperty")]
        public Vector3 rHandElucidatorRotation;

        [Header("spineProperty")]
        public Vector3 spineElucidatorRotation;
        public Vector3 spineElucidatorPosition;
    }
}