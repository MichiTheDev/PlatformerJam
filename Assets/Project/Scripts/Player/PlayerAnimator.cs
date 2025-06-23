using UnityEngine;

[RequireComponent(
	typeof(Animator),
	typeof(PlayerMovement)
)]
public sealed class PlayerAnimator : MonoBehaviour
{
	[SerializeField] private float _pushRunAnimationSpeed = 1.5f;
	[SerializeField] private Vector2 _pushAnimationColliderOffset;
	[SerializeField] private Vector2 _pushAnimationColliderSize;
	
	private Animator _animator;
	private PlayerMovement _playerMovement;

	private Vector2 _defaultColliderOffset;
	private Vector2 _defaultColliderSize;
	
	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_playerMovement = GetComponent<PlayerMovement>();
	}

	private void Start()
	{
		_playerMovement.OnJumpStarted += () => _animator.SetTrigger("Jump");
		_playerMovement.OnRunStateChanged += OnRunStateChanged;
		_playerMovement.OnObstacleTouched += OnObstacleTouched;
		_playerMovement.OnObstacleReleased += OnObstacleReleased;
		
		GravityManager.Instance.OnGravityDirectionChanged += OnGravityDirectionChanged;

		_defaultColliderSize = _playerMovement.Collider.size;
		_defaultColliderOffset = _playerMovement.Collider.offset;
	}

	private void OnDestroy()
	{
		_playerMovement.OnRunStateChanged -= OnRunStateChanged;
		_playerMovement.OnObstacleTouched -= OnObstacleTouched;
		_playerMovement.OnObstacleReleased -= OnObstacleReleased;
		
		GravityManager.Instance.OnGravityDirectionChanged -= OnGravityDirectionChanged;
	}

	private void Update()
	{
		_animator.SetFloat("YVelocity", _playerMovement.VerticalVelocity);
		_animator.SetFloat("Speed", Mathf.Abs(_playerMovement.HorizontalVelocity));
		_animator.SetBool("IsGrounded", _playerMovement.IsGrounded);
		_animator.SetBool("IsObstacleInFront", _playerMovement.IsObstacleInFront);
	}
	
	private void OnGravityDirectionChanged(GravityDirection gravityDirection)
	{
		_animator.SetInteger("GravityDirection", gravityDirection == GravityDirection.Down ? -1 : 1);
	}

	private void OnRunStateChanged(bool running)
	{
		_animator.SetBool("IsRunning", running);

		if(_playerMovement.IsObstacleInFront && running)
		{
			_animator.speed = _pushRunAnimationSpeed;
		}
		else
		{
			_animator.speed = 1f;
		}
	}
	
	private void OnObstacleTouched(GameObject obstacle)
	{
		_playerMovement.Collider.size = _pushAnimationColliderSize;
		_playerMovement.Collider.offset = _pushAnimationColliderOffset;

		if(_playerMovement.IsRunning)
		{
			_animator.speed = _pushRunAnimationSpeed;
		}
	}
	
	private void OnObstacleReleased(GameObject obstacle)
	{
		_playerMovement.Collider.size = _defaultColliderSize;
		_playerMovement.Collider.offset = _defaultColliderOffset;
		
		_animator.speed = 1f;
	}
}