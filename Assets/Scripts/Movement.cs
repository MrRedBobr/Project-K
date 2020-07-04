using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

interface IMoveState
{
	void FixedUpdate();
	void Update();
}
[RequireComponent(typeof(Animator), typeof(CharacterController), typeof(PlayerSettings))]
public class Movement : MonoBehaviour
{
	//all private var
	[SerializeField] private Animator anim;
	[SerializeField] private CharacterController controller;
	[SerializeField] private PlayerSettings playerSettings;
	private float InputX;
	private float InputZ;
	private Camera cam;
	private bool _blockRotationPlayer = true;
	private IMoveState State { get; set; }

	[SerializeField] private LayerMask _sprintingLayerMasck;

	public Movement()
	{
		State = new GroundState(this);
	}

	/*Статус движения*/
	private void Start()
	{
		anim = GetComponent<Animator>();
		controller = GetComponent<CharacterController>();
		cam = Camera.main;
	}

	private void Update()
	{
		InputX = Input.GetAxis("Horizontal");
		InputZ = Input.GetAxis("Vertical");

		State.Update();
	}
	private void FixedUpdate()
	{
		State.FixedUpdate();
	}

	#region state region

	#region state without sword

	#region defauld state
	class GroundState : IMoveState
	{
		private Movement character;

		private Vector3 _disireMoveDirection;
		private bool _blockRotationPlayer;
		private float _speed;
		public GroundState(Movement character)
		{
			this.character = character;
		}
		public void FixedUpdate()
		{
			Gravity();
			FenceChek();
			PitChek();
		}
		public void Update()
		{
			InputMagnitude();
			return;
		}

		void MoveAndRot()
		{
			Camera camera = Camera.main;
			Vector3 forward = character.cam.transform.forward;
			var right = character.cam.transform.right;

			forward.y = 0f;
			right.Normalize();
			_disireMoveDirection = forward * character.InputZ + right * character.InputX;

			if (_blockRotationPlayer && character._blockRotationPlayer)
			{
				character.transform.rotation = Quaternion.Slerp(character.transform.rotation, Quaternion.LookRotation(_disireMoveDirection), character.playerSettings.directionRotationSpeed);
			}
		}
		void InputMagnitude()
		{
			
			character.anim.SetFloat("InputZ", character.InputZ, 0.0f, Time.deltaTime * 2f);

			_speed = new Vector2(character.InputX, character.InputZ).magnitude;
			if (_speed > 1) _speed = 1;


			if (_speed > character.playerSettings.allowRotation)
			{
				character.anim.SetFloat("InputManitide", _speed, 0f, Time.deltaTime);
				_blockRotationPlayer = Input.GetButton("Horizontal") || Input.GetButton("Vertical");
				MoveAndRot();
			}
			else if (_speed < character.playerSettings.allowRotation)
			{
				character.anim.SetFloat("InputManitide", _speed, 0f, Time.deltaTime);
			}
		}
		void Gravity()
		{
			character.controller.Move(Vector3.down * character.playerSettings.gravity * Time.deltaTime);
		}
		void FenceChek()
		{
			Vector3 position = character.transform.position;
			Vector3 forward = character.transform.forward;
			Vector3 lazyChaker = position + new Vector3(0, 0.501f, 0);
			Ray ray = new Ray(lazyChaker, forward);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 0.6f, character._sprintingLayerMasck) && Vector3.Angle(hit.normal, Vector3.up) > 80 && Input.GetButton("Sprint"))
			{
				if (hit.collider.bounds.max.y - lazyChaker.y < 0.8)
				{
					Vector3 hitPoint = hit.point;

					if (Vector3.Distance(hit.collider.bounds.ClosestPoint(hitPoint - hit.normal), hitPoint) < 0.5f)
					{
						character.anim.SetTrigger("Jumping Over Into Combat");
						character.State = new OverFence(character);
					}
					else
					{
						RaycastHit climbHit;
						Ray climbRay = new Ray(lazyChaker + forward + Vector3.up * 0.9f, Vector3.down);
						Physics.Raycast(climbRay, out climbHit, 0.9f, character._sprintingLayerMasck);
						if(climbHit.collider != null)
						{
							character.anim.SetTrigger("ClimbingFence");
							character.State = new FenceClimbState(character, climbHit.point);
						}
					}
				}
			}
			
		}
		void PitChek()
		{
			if (Input.GetButton("Sprint") && Input.GetButton("Vertical") && character.controller.isGrounded)
			{
				Vector3 position = character.transform.position;
				Vector3 forward = character.transform.forward * 0.9f;

				Vector3 chekPos = position + new Vector3(0f, 1f, 0f) + forward;
				Debug.DrawRay(chekPos, Vector3.down, Color.green);
				if(!Physics.Raycast(chekPos, Vector3.down*2, 2f))
				{
					for (int i = 0; i < 5; i++)
					{
						Debug.DrawRay(chekPos, forward, Color.black);
						chekPos += forward;
						RaycastHit hit;
						if (Physics.Raycast(chekPos, Vector3.down, out hit, 2f))
						{
							Vector3 point = hit.point;

							Debug.DrawRay(chekPos, Vector3.down*2, Color.green);
							character.anim.SetTrigger("Jumping Over Pit");
							float time = Vector3.Distance(character.transform.position, point) > 3 ? 0.8f : 1.25f;
							character.anim.speed = time;
							character.controller.enabled = false;
							character.State = new JumpOverPit(character, point);
							break;
						}
					}
				}
				
			}

		}
	}
	#endregion


	class OverFence : IMoveState
	{
		private Movement character;
		private Vector3 forward;
		private Vector3 pelvisPos;
		public OverFence(Movement character)
		{
			forward = character.transform.forward;
			pelvisPos = character.transform.position;
			character.controller.enabled = false;
			this.character = character;
		}
		public void FixedUpdate()
		{
			Moving();
			character.anim.SetFloat("InputManitide", character.InputZ, 0f, Time.deltaTime);
		}

		void Moving()
		{
			if (Vector3.Distance(pelvisPos, character.transform.position) < 1f)
			{
				Vector3 stepTo = Vector3.Lerp(character.transform.position, pelvisPos + forward, 0.1f);
				character.transform.localPosition = stepTo;
			}
			else if(character._blockRotationPlayer) character.StartCoroutine(character.OverfenchToGroundMove(0.5f));
		}

		public void Update()
		{

		}

	}
	class JumpOverPit : IMoveState
	{
		private Movement character;
		private Vector3 endPos;

		private Vector3 force;
		private Vector3 gravity = new Vector3(0f, 0.9f, 9f);

		public JumpOverPit(Movement chracter, Vector3 onJumpPos)
		{
			chracter.controller.enabled = false;
			this.character = chracter;
			endPos = onJumpPos;
		}
		public void FixedUpdate()
		{
			float dist = Vector3.Distance(character.transform.position, endPos);
			if (dist > 0.1f) character.transform.position = Vector3.Lerp(character.transform.position, endPos, 0.15f / dist);
			else 
			{ 
				character.StartCoroutine(character.OverfenchToGroundMove(0.1f));
			}
		}
		public void Update()
		{

		}
	}

	class FenceClimbState : IMoveState
	{
		private Movement _character;
		private Vector3 _endPosition;
		private float dist;

		public FenceClimbState(Movement character, Vector3 newPoint)
		{
			character.controller.enabled = false;
			_character = character;
			_endPosition = newPoint;

			_character.anim.SetFloat("InputZ", 0);
			_character.anim.SetFloat("InputX", 0);
			_character.anim.SetFloat("InputManitide", 0);
		}

		public void FixedUpdate()
		{
			if (dist > 0.1f) _character.transform.position = Vector3.Lerp(_character.transform.position, _endPosition, 0.1f);
			else _character.StartCoroutine(_character.OverfenchToGroundMove(0.1f));
		}
		public void Update()
		{
			dist = Vector3.Distance(_character.transform.position, _endPosition);
		}
	}

	#endregion state without sword

	#region state with sword

	#endregion state with sword

	#endregion state region

	#region korutine
	IEnumerator OverfenchToGroundMove(float seconds)
	{
		_blockRotationPlayer = false;
		State = new GroundState(this);
		controller.enabled = true;
		yield return new WaitForSeconds(seconds);
		anim.speed = 1;
		_blockRotationPlayer = true;
	}
	IEnumerator FenceClimbCoroutine(Vector3 pos, float seconds)
	{
		transform.position = pos;
		yield return new WaitForSeconds(seconds);
		State = new GroundState(this);
		controller.enabled = true;
		anim.speed = 1;
	}
	#endregion

	#region animation action

	#endregion
}
