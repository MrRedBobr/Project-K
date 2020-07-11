using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IK : MonoBehaviour
{
	[SerializeField] private Animator anim;

	private Vector3 rightFootPosition, leftFootPosition, leftFootIkPosition, rightFootIkPosition;
	private Quaternion leftFootIkRotation, rightFootIkRotation;
	private float lastPelvisPositionY, lastRightFootPositionY, lastLeftFootPositionY;

	[Header("Feet Grounder")]
	[SerializeField] private bool enableFeetIK = true;
	[Range(0, 2)] [SerializeField] private float heightFromGroundRaycast = 1.14f;
	[Range(0, 2)] [SerializeField] private float raycastDistance = 1.5f;
	[SerializeField] private LayerMask envieronmantLayer;
	[SerializeField] private float pelvisOffset = 0f;
	[Range(0, 1)] [SerializeField] private float pelvisUpAndDownSpeed = 0.28f;
	[Range(0, 1)] [SerializeField] private float feetToIkPositionSpeed = 0.5f;

	public string leftFootAnimVariableName;
	public string rightFootAnimVariableName;

	public bool useProIkFeature = false;
	public bool showSolverDebug = true;
	#region feetGrounding

	//We are updating the AdjustFeetTarget method and  also find the position of each foot inside our Solver Position.
	private void FixedUpdate()
	{
		if (enableFeetIK == false) return;
		if (anim == null) return;

		AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);
		AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);

		//find and raycast to the ground to find positions

		FeetPositionSolver(rightFootPosition, ref rightFootIkPosition, ref rightFootIkRotation); //handle the solver for right foot
		FeetPositionSolver(leftFootPosition, ref leftFootIkPosition, ref leftFootIkRotation); //handle the solver for left foot
	}

	private void OnAnimatorIK(int layerIndex)
	{
		if (enableFeetIK == false) return;
		if (anim == null) return;

		MovePelvisHeight();
		// right foot ik position and rotation -- utilise the pro features in here
		anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1);

		if (useProIkFeature)
		{
			anim.SetIKRotationWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootAnimVariableName));
		}
		MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIkPosition, rightFootIkRotation, ref lastRightFootPositionY);

		// left foot ik position and rotation -- utilise the pro features in here
		anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1);

		if (useProIkFeature)
		{
			anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootAnimVariableName));
		}
		MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIkPosition, leftFootIkRotation, ref lastLeftFootPositionY);
	}

	#endregion

	#region feetGroundingMethods
	private void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPositionY)
	{
		Vector3 targetIkPosition = anim.GetIKPosition(foot);
		if(positionIkHolder != Vector3.zero)
		{
			targetIkPosition = transform.InverseTransformPoint(targetIkPosition);
			positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

			float yVariable = Mathf.Lerp(lastFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
			targetIkPosition.y += yVariable;

			lastFootPositionY = yVariable;

			targetIkPosition = transform.TransformPoint(targetIkPosition);
			anim.SetIKRotation(foot, rotationIkHolder);
		}
		anim.SetIKPosition(foot, targetIkPosition);
	}
	private void MovePelvisHeight()
	{
		if(rightFootIkPosition == Vector3.zero || leftFootIkPosition == Vector3.zero || lastPelvisPositionY == 0)
		{
			lastPelvisPositionY = anim.bodyPosition.y;
			return;
		}
		float lOffsetPosition = leftFootIkPosition.y - transform.position.y;
		float rOffsetPosition = rightFootIkPosition.y - transform.position.y;

		float totalOffset = (lOffsetPosition < rOffsetPosition) ? lOffsetPosition : rOffsetPosition;
		Vector3 newPelvisPosition = anim.bodyPosition + Vector3.up * totalOffset;
		newPelvisPosition.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPosition.y, pelvisUpAndDownSpeed);
		anim.bodyPosition = newPelvisPosition;
		lastPelvisPositionY = anim.bodyPosition.y;
	}
	private void FeetPositionSolver(Vector3 fromSkyPosition, ref Vector3 feetIkPosition, ref Quaternion feetIkRotations)
	{
		RaycastHit feetOutHit;
		if (showSolverDebug)
		{
			Debug.DrawLine(fromSkyPosition, fromSkyPosition + Vector3.down * (raycastDistance + heightFromGroundRaycast), Color.yellow);
		}
		if(Physics.Raycast(fromSkyPosition, Vector3.down, out feetOutHit, raycastDistance + heightFromGroundRaycast, envieronmantLayer))
		{
			feetIkPosition = fromSkyPosition;
			feetIkPosition.y = feetOutHit.point.y + pelvisOffset;
			feetIkRotations = Quaternion.FromToRotation(Vector3.up, feetOutHit.normal) * transform.rotation;
			
			return;
		}
		feetIkPosition = Vector3.zero;
	}
	private void AdjustFeetTarget(ref Vector3 feetPosition, HumanBodyBones foot)
	{
		feetPosition = anim.GetBoneTransform(foot).position;
		feetPosition.y = transform.position.y + heightFromGroundRaycast;
	}
	#endregion
}
