using UnityEngine;

[RequireComponent(
    typeof(Rigidbody2D),
    typeof(Animator),
    typeof(Collider2D)
)]
public sealed class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _walkSpeed = 3f;
    [SerializeField] private float _runSpeed = 6f;

    [Header("Jump")] 
    [SerializeField] private float _minJumpHeight = 1f;
    [SerializeField] private float _maxJumpHeight = 3.5f;
    [Tooltip("Used to smooth out jump when stop jumping. Set to 0 to disable smoothing. " +
             "Set to 1 to disable Min Jump Height. The lower this value, the closer it will get to Min Jump Height.")]
    [SerializeField, Range(0.01f, 1f)] private float _jumpEndGravityMultiplier = 0.8f;
    
    [Header("Ground Check")]
    [SerializeField] private LayerMask _excludedGroundCheckLayers;
    [SerializeField] private float _groundCheckHeight = 0.05f;
    
    private Rigidbody2D _rb;
    private Animator _animator;
    private Collider2D _collider;

    private float _movementSpeed;
    private float _movementInput;
    private float _jumpStartY;

    private bool _isRunning;
    private bool _isJumping;
    private bool _jumpHeld;
    private bool _isGrounded;
    
    private float _gravity => -(Physics2D.gravity.y * _rb.gravityScale);
    
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _collider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        _movementSpeed = _walkSpeed;
    }

    private void FixedUpdate()
    {
        GroundCheck();
        UpdateMovement();
        UpdateJump();
    }

    private void Update()
    {
        UpdateAnimations();
    }

    public void SetRunning(bool running)
    {
        _isRunning = running;
        _movementSpeed = running ? _runSpeed : _walkSpeed;
        
        _animator.SetBool("IsRunning", _isRunning);
    }
    
    public void Move(float direction)
    {
        _movementInput = direction;

        if(_movementInput != 0f)
        {
            Flip(_movementInput);
        }
    }

    public void StartJump()
    {
        if(_isJumping || !_isGrounded) return;

        _isJumping = true;
        _jumpHeld = true;
        _jumpStartY = transform.position.y;
        
        _animator.SetTrigger("Jump");
        
        _rb.linearVelocityY = Mathf.Sqrt(2f * _gravity * _maxJumpHeight);
    }

    public void StopJump()
    {
        if(!_isJumping || !_jumpHeld) return;

        _jumpHeld = false;
    }

    public void Flip(float direction)
    {
        direction = Mathf.Sign(direction);
        transform.localScale = new Vector3(direction, 1f, 1f);
    }

    private void UpdateMovement()
    {
        _rb.linearVelocityX = _movementInput * _movementSpeed;
    }

    private void UpdateJump()
    {
        if(!_isJumping) return;

        float jumpHeightSinceStart = transform.position.y - _jumpStartY;
        if(jumpHeightSinceStart >= _minJumpHeight)
        {
            if (!_jumpHeld)
            {
                _rb.linearVelocityY *= _jumpEndGravityMultiplier;
            }

            // If we start to fall, we aren't jumping anymore
            if(_rb.linearVelocityY <= 0)
            {
                _isJumping = false;
            } 
        }
    }

    private void GroundCheck()
    {
        Bounds bounds = _collider.bounds;
        Vector2 checkOrigin = new Vector2(bounds.center.x, bounds.center.y - bounds.extents.y);
        Vector2 checkSize = new Vector2(bounds.size.x - 0.05f, _groundCheckHeight);

        RaycastHit2D hit = Physics2D.CapsuleCast(
            checkOrigin, checkSize, CapsuleDirection2D.Horizontal, 0f, 
            Vector2.zero, 0f, ~_excludedGroundCheckLayers
        );

        _isGrounded = hit.collider is not null;
    }
    
    private void UpdateAnimations()
    {
        _animator.SetFloat("YVelocity", _rb.linearVelocityY);
        _animator.SetFloat("Speed", Mathf.Abs(_rb.linearVelocityX));
    }
}