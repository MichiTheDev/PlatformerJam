using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public sealed class PlayerController : MonoBehaviour
{
    private PlayerMovement _playerMovement;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
    }

    private void OnEnable()
    {
        InputManager.Register("Movement", MovementInput);
        InputManager.Register("Run", RunInput);
        InputManager.Register("Jump", JumpInput);
        InputManager.Register("Test", TestInput);
        
        GravityManager.Instance.OnGravityDirectionChanged += OnGravityDirectionChanged;
    }

    private void OnDisable()
    {
        InputManager.Unregister("Movement", MovementInput);
        InputManager.Unregister("Run", RunInput);
        InputManager.Unregister("Jump", JumpInput);
        InputManager.Unregister("Test", TestInput);
        
        GravityManager.Instance.OnGravityDirectionChanged -= OnGravityDirectionChanged;
    }

    private void MovementInput(InputAction.CallbackContext context)
    {
        _playerMovement.Move(context.ReadValue<float>() * InputManager.InputScale);
    }

    private void RunInput(InputAction.CallbackContext context)
    {
        _playerMovement.SetRunning(context.performed);
    }
    
    private void JumpInput(InputAction.CallbackContext context)
    {
        if(context.performed) _playerMovement.StartJump();
        else if(context.canceled) _playerMovement.StopJump();
    }
    
    private void TestInput(InputAction.CallbackContext context)
    {
        if(!context.performed) return;

        if(GravityManager.Instance.GravityDirection == GravityDirection.Down)
        {
            GravityManager.Instance.ChangeGravity(GravityDirection.Top);
        }
        else if(GravityManager.Instance.GravityDirection == GravityDirection.Top)
        {
            GravityManager.Instance.ChangeGravity(GravityDirection.Down);
        }
    }
    
    private void OnGravityDirectionChanged(GravityDirection gravityDirection)
    {
        switch(gravityDirection)
        {
            case GravityDirection.Top:
                _playerMovement.FlipY(-1f);
                break;
            case GravityDirection.Down:
                _playerMovement.FlipY(1f);
                break;
        }
    }
}