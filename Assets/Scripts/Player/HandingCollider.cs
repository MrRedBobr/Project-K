using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandingCollider : MonoBehaviour
{
    [SerializeField] [Range(0, 1)] private int index;
	[SerializeField] private Movement _movement;

	private void OnTriggerEnter(Collider other)
	{
		if(!_movement.Handing[index]) _movement.Handing[index] = other;
	}
	private void OnTriggerExit(Collider other)
	{
		_movement.Handing[index] = null;
	}
}
