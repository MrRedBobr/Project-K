using PlayerPridSet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorAction : MonoBehaviour
{
	[SerializeField] private PlayerSettings _playerSettings;
	[Header("Item Places")]
	[SerializeField] private Transform _spineSword;
	[SerializeField] private Transform _waistRight;
	[SerializeField] private Transform _waistLeft;

	[SerializeField] private Transform _rightHand;
	[SerializeField] private Transform _leftHand;


	[Header("Items")]
	[SerializeField] private Transform _Elucidator;
	[SerializeField] private Transform _Slash;

	public void drawElucidator()
	{
		_Elucidator.SetParent(_rightHand);
		_Elucidator.localPosition = Vector3.zero;
		_Elucidator.localRotation = Quaternion.Euler(_playerSettings.rHandElucidatorRotation);
	}
	public void unDrawElucidator()
	{
		_Elucidator.SetParent(_spineSword);
		_Elucidator.localPosition = _playerSettings.spineElucidatorPosition;
		_Elucidator.localRotation = Quaternion.Euler(_playerSettings.spineElucidatorRotation);
	}
	public void Attack(float Zrot)
	{
		_Slash.gameObject.SetActive(false);
		_Slash.gameObject.SetActive(true);
		_Slash.localRotation = Quaternion.Euler(new Vector3(0f, 0f, Zrot));
	}
}
