using System;
using UnityEngine;

[RequireComponent(
    typeof(Rigidbody2D),
    typeof(Collider2D)
)]
public sealed class PlayerMovement : MonoBehaviour
{
    public event Action OnJumpStarted;
    public event Action<bool> OnRunStateChanged;
    
    public float HorizontalVelocity => _rb.linearVelocityX;
    public float VerticalVelocity => _rb.linearVelocityY;
    
    public bool IsRunning { get; private set; }
    public bool IsGrounded { get; private set; }

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
    
    private Rigidbody2D _rb;
    private Collider2D _collider;
    
    private float _movementSpeed;
    private float _movementInput;
    private float _jumpStartY;
    private float _coyoteTimer;

    private bool _isJumping;
    private bool _jumpHeld;

    private float _gravity => Mathf.Abs(Physics2D.gravity.y * _rb.gravityScale);
    private float _yDirection => transform.localScale.y;
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        _movementSpeed = _walkSpeed;
    }

    private void FixedUpdate()
    {
        UpdateGroundCheck();
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
        
        _rb.linearVelocityY = Mathf.Sqrt(2f * _gravity * _maxJumpHeight) * _yDirection;
        OnJumpStarted?.Invoke();
    }

    public void StopJump()
    {
        if(!_isJumping || !_jumpHeld) return;

        _jumpHeld = false;
    }

    public void FlipX(bool flip)
    {
        transform.localScale = new Vector3(flip ? -1f : 1f, transform.localScale.y, 1f);
    }

    public void FlipY(bool flip)
    {
        transform.localScale = new Vector3(transform.localScale.x, flip ? -1f : 1f, 1f);
    }
    
    private void UpdateMovement()
    {
        _rb.linearVelocityX = _movementInput * _movementSpeed;
    }

    private void UpdateJump()
    {
        if(!_isJumping) return;

        float jumpHeightSinceStart = transform.position.y - _jumpStartY * _yDirection;
        if(jumpHeightSinceStart >= _minJumpHeight)
        {
            if (!_jumpHeld)
            {
                _rb.linearVelocityY *= _jumpEndGravityMultiplier;
            }
            
            // If we start to fall, we aren't jumping anymore
            if(_yDirection > 0)
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

    private void UpdateGroundCheck()
    {
        Bounds bounds = _collider.bounds;
        Vector2 checkOrigin = new Vector2(bounds.center.x, bounds.center.y - bounds.extents.y * _yDirection);
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

    private void Landed()
    {
        _coyoteTimer = 0;
    }
}