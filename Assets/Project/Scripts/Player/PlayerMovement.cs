using System;
using UnityEngine;

[RequireComponent(
    typeof(Rigidbody2D),
    typeof(BoxCollider2D)
)]
public sealed class PlayerMovement : MonoBehaviour
{
    public event Action OnJumpStarted;
    public event Action OnLanded;
    public event Action<GameObject> OnObstacleTouched;
    public event Action<GameObject> OnObstacleReleased;
    public event Action<bool> OnRunStateChanged;
    
    public float HorizontalVelocity => _rb.linearVelocityX;
    public float VerticalVelocity => _rb.linearVelocityY;
    
    public GameObject Obstacle { get; private set; }
    public BoxCollider2D Collider { get; private set; }
    
    public float FacingDirection { get; private set; }
    
    public bool IsRunning { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsObstacleInFront { get; private set; }

    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _runSpeed = 6f;

    [Header("Jump")] 
    [SerializeField] private float _minJumpHeight = 1f;
    [SerializeField] private float _maxJumpHeight = 3.5f;
    [Tooltip("Used to smooth out jump when stop jumping. Set to 0 to disable smoothing. " +
             "Set to 1 to disable Min Jump Height. The lower this value, the closer it will get to Min Jump Height.")]
    [SerializeField, Range(0.01f, 1f)] private float _jumpEndGravityMultiplier = 0.8f;
    [SerializeField] private float _coyoteTime = 0.1f;
    
    [Header("Ground Check")]
    [SerializeField] private LayerMask _excludedGroundCheckLayers;
    [SerializeField] private float _groundCheckHeight = 0.05f;

    [Header("Obstacle Check")] 
    [SerializeField] private LayerMask _obstacleLayer;
    [SerializeField] private float _obstacleCheckDistance = 0.1f;
    [Tooltip("This value will be multiplied by the height of the collider to determine the check origin. " +
             "I.e. if the value is 0.8f, it will be at 80% of the characters height, which could be around the head.")]
    [SerializeField, Range(0.01f, 1f)] private float _obstacleCheckHeight = 0.8f;
    
    private Rigidbody2D _rb;
    
    private float _movementSpeed;
    private float _movementInput;
    private float _jumpStartY;
    private float _coyoteTimer;
    private float _flipY = 1f;

    private bool _isJumping;
    private bool _jumpHeld;

    private float _gravity => Mathf.Abs(Physics2D.gravity.y * _rb.gravityScale);
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        Collider = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        _movementSpeed = _walkSpeed;
    }

    private void FixedUpdate()
    {
        UpdateGroundCheck();
        UpdateObstacleCheck();
        UpdateMovement();
        UpdateJump();
    }

    public void SetRunning(bool running)
    {
        IsRunning = running;
        _movementSpeed = running ? _runSpeed : _walkSpeed;
        OnRunStateChanged?.Invoke(IsRunning);
    }
    
    public void Move(float direction)
    {
        if(direction != 0f)
        {
            _movementInput = Mathf.Sign(direction);
            FlipX(_movementInput < 0);
            return;
        }

        _movementInput = 0f;
    }

    public void StartJump()
    {
        if(_isJumping || !IsGrounded) return;

        _isJumping = true;
        _jumpHeld = true;
        _jumpStartY = transform.position.y;
        
        _rb.linearVelocityY = Mathf.Sqrt(2f * _gravity * _maxJumpHeight) * _flipY;
        OnJumpStarted?.Invoke();
    }

    public void StopJump()
    {
        _jumpHeld = false;
    }

    public void FlipX(bool flip)
    {
        FacingDirection = flip ? -1f : 1f;
        transform.localScale = new Vector3(FacingDirection, _flipY, 1f);
    }

    public void FlipY(bool flip)
    {
        _flipY = flip ? -1f : 1f;
        transform.localScale = new Vector3(FacingDirection, _flipY, 1f);
    }

    private void UpdateGroundCheck()
    {
        Bounds bounds = Collider.bounds;
        Vector2 checkOrigin = new Vector2(bounds.center.x, bounds.center.y - bounds.extents.y * _flipY);
        Vector2 checkSize = new Vector2(bounds.size.x - 0.05f, _groundCheckHeight);

        RaycastHit2D hit = Physics2D.CapsuleCast(
            checkOrigin, checkSize, CapsuleDirection2D.Horizontal, 0f, 
            Vector2.zero, 0f, ~_excludedGroundCheckLayers
        );

        if(hit.collider is null)
        {
            _coyoteTimer += Time.fixedDeltaTime;
            if(_coyoteTimer >= _coyoteTime)
            {
                IsGrounded = false;
            }
            return;
        }

        if(!IsGrounded)
        {
            Landed();
        }
        IsGrounded = true;
    }
    
    private void UpdateObstacleCheck()
    {
        Bounds bounds = Collider.bounds;
        float originYStart = _flipY > 0 ? bounds.min.y : bounds.max.y; 
        Vector2 origin = new Vector2(
            bounds.center.x + bounds.extents.x * FacingDirection,
            originYStart + bounds.size.y * _obstacleCheckHeight * _flipY
        );

        RaycastHit2D hit = Physics2D.Raycast(
            origin, FacingDirection * Vector2.right, _obstacleCheckDistance, _obstacleLayer
        );

        if(Obstacle is null && hit.collider is not null)
        {
            Obstacle = hit.collider.gameObject;
            IsObstacleInFront = true;
            OnObstacleTouched?.Invoke(Obstacle);
        }
        else if(IsObstacleInFront && hit.collider is null)
        {
            IsObstacleInFront = false;
            OnObstacleReleased?.Invoke(Obstacle);
            Obstacle = null;
        }
    }
    
    private void UpdateMovement()
    {
        _rb.linearVelocityX = _movementInput * _movementSpeed;
    }

    private void UpdateJump()
    {
        if(!_isJumping) return;

        float jumpHeightSinceStart = transform.position.y - _jumpStartY * _flipY;
        if(jumpHeightSinceStart >= _minJumpHeight)
        {
            if (!_jumpHeld)
            {
                _rb.linearVelocityY *= _jumpEndGravityMultiplier;
            }
            
            // If we start to fall, we aren't jumping anymore
            if(_flipY > 0)
            {
                if(_rb.linearVelocityY <= 0)
                {
                    _isJumping = false;
                } 
            }
            else
            {
                if(_rb.linearVelocityY >= 0)
                {
                    _isJumping = false;
                } 
            }
        }
    }

    private void Landed()
    {
        _isJumping = false;
        _coyoteTimer = 0;
        OnLanded?.Invoke();
    }
}