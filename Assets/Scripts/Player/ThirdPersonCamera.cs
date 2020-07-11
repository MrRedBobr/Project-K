using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour
{
	public CameraConfig camConf;
	public Transform playerCamHolder;

	float yaw;
	float pitch;

	Vector3 rotationSmoothVelocity;
	Vector3 currentRotation;

	public bool lockMode;

	private void Start()
	{
		if(lockMode)
		{
			Cursor.lockState = CursorLockMode.Locked;
			Cursor.visible = false;
		}
	}

	private void Update()
	{
		yaw += Input.GetAxis("Mouse X") * 10;
		pitch -= Input.GetAxis("Mouse Y") * 10;
		pitch = Mathf.Clamp(pitch, camConf.minAngle, camConf.maxAngle);

		currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotationSmoothVelocity, camConf.rotationSmoothTime);

		Vector3 targetRot = currentRotation;
		transform.eulerAngles = targetRot;

		transform.position = playerCamHolder.position - transform.forward * 4f;
	}
}
