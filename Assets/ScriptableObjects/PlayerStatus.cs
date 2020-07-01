using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Player/Status")]
public class PlayerStatus : ScriptableObject
{
	public bool isSprint;
	public bool isArmed;
	public bool isGround;
}
