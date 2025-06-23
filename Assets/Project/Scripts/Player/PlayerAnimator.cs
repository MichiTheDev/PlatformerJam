using UnityEngine;

[RequireComponent(
	typeof(Animator),
	typeof(PlayerMovement)
)]
public sealed class PlayerAnimator : MonoBehaviour
{
	private Animator _animator;
	private PlayerMovement _playerMovement;
	
	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_playerMovement = GetComponent<PlayerMovement>();
	}

	private void Start()
	{
		_playerMovement.OnJumpStarted += () => _animator.SetTrigger("Jump");
		_playerMovement.OnRunStateChanged += running => _animator.SetBool("IsRunning", running);
		
		GravityManager.Instance.OnGravityDirectionChanged += OnGravityDirectionChanged;
	}

	private void OnDestroy()
	{
		GravityManager.Instance.OnGravityDirectionChanged -= OnGravityDirectionChanged;
	}

	private void OnGravityDirectionChanged(GravityDirection gravityDirection)
	{
		_animator.SetInteger("GravityDirection", gravityDirection == GravityDirection.Down ? -1 : 1);
	}

	private void Update()
	{
		_animator.SetFloat("YVelocity", _playerMovement.VerticalVelocity);
		_animator.SetFloat("Speed", Mathf.Abs(_playerMovement.HorizontalVelocity));
		_animator.SetBool("IsGrounded", _playerMovement.IsGrounded);
	}
}