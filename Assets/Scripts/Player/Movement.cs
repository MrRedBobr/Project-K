using PlayerPridSet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;


interface IMoveState
{
	void FixedUpdate();
	void Update();
}
[RequireComponent(typeof(Animator), typeof(CharacterController))]
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

	[Header("Phisycs components")]
	[SerializeField] private Rigidbody rbody;
	[SerializeField] private CapsuleCollider cCollider;

	[Header("Parkour obg")]
	public List<Collider> Handing = new List<Collider>();
	public List<Collider> Colliders = new List<Collider>();

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
		rbody.useGravity = false;
		cCollider.enabled = false;
		rbody.isKinematic = true;
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

	private bool Grounded()
	{
		Ray ray = new Ray(transform.position + Vector3.up * 0.4f, Vector3.down);
		return Physics.SphereCast(ray, 0.2f, 0.4f);
	}

	#region state region

	#region state without sword

	#region defauld state
	class GroundState : IMoveState
	{
		private Movement _character;

		private Vector3 _disireMoveDirection;
		private bool _blockRotationPlayer;
		private float _speed;
		public GroundState(Movement character)
		{
			this._character = character;
		}
		public void FixedUpdate()
		{
			Gravity();
			FenceChek();
			PitChek();
		}
		public void Update()
		{
			_character.anim.SetBool("isGrounded", _character.Grounded());
			InputMagnitude();
			if (Input.GetKeyDown(KeyCode.Q)) _character.State = new ArmoredSwordMovement(_character);
			if (Input.GetKeyDown(KeyCode.Space))
			{
				_character.controller.enabled = false;
				_character.cCollider.enabled = true;

				_character.rbody.useGravity = true;
				_character.rbody.isKinematic = false;

				_character.anim.applyRootMotion = false;
				Vector3 forward = _character.transform.forward;
				Vector3 force;
				if (Physics.Raycast(_character.transform.position + Vector3.up * 0.3f, forward, 0.4f))
				{
					force = (forward * 0.2f + Vector3.up * 2f) * 3f;
				}
				else
				{
					force = (forward + Vector3.up * 2f) * 3f;
				}

				_character.rbody.AddForce((forward + Vector3.up * 2f) * 3f, ForceMode.VelocityChange);

				_character.anim.SetBool("isGrounded", false);

				_character.State = new Jumping(_character);
			}
			
			return;
		}

		private void MoveAndRot()
		{
			Camera camera = Camera.main;
			Vector3 forward = _character.cam.transform.forward;
			var right = _character.cam.transform.right;

			forward.y = 0f;
			right.Normalize();
			_disireMoveDirection = forward * _character.InputZ + right * _character.InputX;

			if (_blockRotationPlayer && _character._blockRotationPlayer)
			{
				_character.transform.rotation = Quaternion.Slerp(_character.transform.rotation, Quaternion.LookRotation(_disireMoveDirection), _character.playerSettings.directionRotationSpeed);
			}
		}
		private void InputMagnitude()
		{
			
			_character.anim.SetFloat("InputZ", _character.InputZ, 0.0f, Time.deltaTime * 2f);

			_speed = new Vector2(_character.InputX, _character.InputZ).magnitude;
			if (_speed > 1) _speed = 1;


			if (_speed > _character.playerSettings.allowRotation)
			{
				_character.anim.SetFloat("InputManitide", _speed, 0f, Time.deltaTime);
				_blockRotationPlayer = Input.GetButton("Horizontal") || Input.GetButton("Vertical");
				MoveAndRot();
			}
			else if (_speed < _character.playerSettings.allowRotation)
			{
				_character.anim.SetFloat("InputManitide", _speed, 0f, Time.deltaTime);
			}
		}
		private void Gravity()
		{
			_character.controller.Move(Vector3.down * _character.playerSettings.gravity * Time.deltaTime);
		}
		private void FenceChek()
		{
			Vector3 position = _character.transform.position;
			Vector3 forward = _character.transform.forward;
			Vector3 lazyChaker = position + new Vector3(0, 0.501f, 0);
			Ray ray = new Ray(lazyChaker, forward);
			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 0.3f, _character._sprintingLayerMasck) && Vector3.Angle(hit.normal, Vector3.up) > 80 && Input.GetButton("Sprint"))
			{
				if (hit.collider.bounds.max.y - lazyChaker.y < 0.8)
				{
					Vector3 hitPoint = hit.point;

					if (Vector3.Distance(hit.collider.bounds.ClosestPoint(hitPoint - hit.normal), hitPoint) < 0.5f)
					{
						_character.anim.SetTrigger("Jumping Over Into Combat");
						_character.State = new OverFence(_character);
					}
					else
					{
						RaycastHit climbHit;
						Ray climbRay = new Ray(lazyChaker + forward + Vector3.up * 0.9f, Vector3.down);
						Physics.Raycast(climbRay, out climbHit, 0.9f, _character._sprintingLayerMasck);
						if(climbHit.collider != null)
						{
							_character.anim.SetTrigger("ClimbingFence");
							_character.State = new FenceClimbState(_character, climbHit.point);
						}
					}
				}
			}
			
		}
		private void PitChek()
		{
			if (Input.GetButton("Sprint") && Input.GetButton("Vertical") && _character.controller.isGrounded)
			{
				Vector3 position = _character.transform.position;
				Vector3 forward = _character.transform.forward * 0.3f;

				Vector3 chekPos = position + new Vector3(0f, 1f, 0f) + forward;
				Debug.DrawRay(chekPos, Vector3.down, Color.green);
				forward = _character.transform.forward * 0.9f;
				if (!Physics.Raycast(chekPos, Vector3.down*2, 2f))
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
							_character.anim.SetTrigger("Jumping Over Pit");
							float time = Vector3.Distance(_character.transform.position, point) > 3 ? 0.8f : 1.25f;
							_character.anim.speed = time;
							_character.controller.enabled = false;
							_character.State = new JumpOverPit(_character, point);
							return; // end of this procces
						}
					}
					//not fount place, but is pit. JUMP!
					//_character.State = new JumpOnCliff(_character);
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
			float dist = Vector3.Distance(pelvisPos, character.transform.position);
			Debug.Log(dist);
			if (dist < 0.6f)
			{
				Vector3 stepTo = Vector3.Lerp(character.transform.position, pelvisPos + forward, dist / 2f);
				character.transform.localPosition = stepTo;
			}
			else if(character._blockRotationPlayer) character.StartCoroutine(character.OverfenchToGroundMove(0.1f));
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

	class Jumping : IMoveState
	{
		private Movement _character;
		public Jumping(Movement character)
		{
			_character = character;
		}
		public void FixedUpdate()
		{
			if (_character.Handing[0])
			{
				_character.rbody.velocity = Vector3.zero;
				_character.transform.rotation = _character.Handing[0].gameObject.transform.rotation;
				_character.transform.position = _character.Handing[0].gameObject.transform.position;
				_character.transform.localPosition += new Vector3(0f, -2.046f, 0f);
				_character.transform.localPosition -= _character.transform.forward * 0.204f;
				_character.State = new HandingState(_character, false);

				return;
			}
			if (_character.Handing[1])
			{
				_character.rbody.velocity = Vector3.zero;
				_character.transform.rotation = _character.Handing[1].gameObject.transform.rotation;
				_character.transform.position = _character.Handing[1].gameObject.transform.position;
				_character.transform.localPosition += new Vector3(0f, -1f, 0f);
				_character.transform.localPosition -= _character.transform.forward * 0.204f;

				_character.State = new HandingState(_character, true);
				return;
			}
			//ground
			Vector3 pos = _character.transform.position + Vector3.up * 0.1f;
			Debug.DrawLine(pos, pos - Vector3.up * 0.2f, Color.black, 2f);
			if (Physics.Raycast(pos, Vector3.down, 0.2f) && _character.rbody.velocity != Vector3.zero)
			{
				_character.controller.enabled = !false;
				_character.cCollider.enabled = !true;
				_character.rbody.useGravity = !true;
				_character.rbody.isKinematic = !false;
				_character.anim.applyRootMotion = !false;

				_character.State = new GroundState(_character);
			}
		}
		public void Update()
		{
			
		}
	}

	class HandingState : IMoveState
	{
		private Movement _character;
		private bool _fenceClimb;
		public HandingState(Movement character, bool ok)
		{
			character.anim.applyRootMotion = false;
			character.cCollider.enabled = false;
			character.rbody.useGravity = false;
			character.rbody.isKinematic = true;

			if(!ok) character.anim.SetTrigger("Handing");
			_character = character;
			_fenceClimb = ok;
		}
		public void FixedUpdate()
		{
			if (_fenceClimb)
			{
				_character.anim.applyRootMotion = true;
				_character.StartCoroutine(_character.FenceClimbCoroutine(1f));
				_fenceClimb = false;
			}
		}
		public void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				_character.anim.applyRootMotion = true;

				
				_character.StartCoroutine(_character.FenceClimbCoroutine(1.5f));
			}
		}
	}


	#endregion state without sword

	#region state with sword
	class ArmoredSwordMovement : IMoveState
	{
		private Movement _character;

		private Vector3 _disireMoveDirection;
		private bool _blockRotationPlayer;
		private float _speed;
		public ArmoredSwordMovement(Movement character)
		{
			character.anim.SetTrigger("Sword");
			this._character = character;
		}
		public void FixedUpdate()
		{
			InputMagnitude();
		}
		public void Update()
		{
			Gravity();
			if(Input.GetMouseButtonDown(0)) _character.anim.SetTrigger("Attack1");
			if(Input.GetKeyDown(KeyCode.Q))
			{
				_character.anim.SetTrigger("Sword");
				_character.State = new GroundState(_character);
			}
			Debug.Log(Input.GetAxis("Mouse X") + " " + Input.GetAxis("Mouse Y"));
		}

		void MoveAndRot()
		{
			Camera camera = Camera.main;
			Vector3 forward = _character.cam.transform.forward;
			var right = _character.cam.transform.right;

			forward.y = 0f;
			right.Normalize();
			_disireMoveDirection = forward * _character.InputZ + right * _character.InputX;

			if (_blockRotationPlayer && _character._blockRotationPlayer)
			{
				_character.transform.rotation = Quaternion.Slerp(_character.transform.rotation, Quaternion.LookRotation(_disireMoveDirection), _character.playerSettings.directionRotationSpeed);
			}
		}
		void InputMagnitude()
		{
			_character.anim.SetFloat("InputZ", _character.InputZ, 0.0f, Time.deltaTime * 2f);

			_speed = new Vector2(_character.InputX, _character.InputZ).magnitude;
			if (_speed > 1) _speed = 1;


			if (_speed > _character.playerSettings.allowRotation)
			{
				_character.anim.SetFloat("InputManitide", _speed, 0f, Time.deltaTime);
				_blockRotationPlayer = Input.GetButton("Horizontal") || Input.GetButton("Vertical");
				MoveAndRot();
			}
			else if (_speed < _character.playerSettings.allowRotation)
			{
				_character.anim.SetFloat("InputManitide", _speed, 0f, Time.deltaTime);
			}
		}
		void Gravity()
		{
			_character.controller.Move(Vector3.down * _character.playerSettings.gravity * Time.deltaTime);
		}
	}
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
	IEnumerator FenceClimbCoroutine(float seconds)
	{
		anim.applyRootMotion = true;
		anim.SetFloat("InputManitide", 0);
		anim.SetTrigger("UpOnWall");
		yield return new WaitForEndOfFrame();
		anim.SetBool("isGrounded", true);
		yield return new WaitForSeconds(seconds);
		State = new GroundState(this);
		controller.enabled = true;
	}
	#endregion

	#region animation action

	#endregion
}
