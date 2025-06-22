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
    }

    private void OnDisable()
    {
        InputManager.Unregister("Movement", MovementInput);
        InputManager.Unregister("Run", RunInput);
        InputManager.Unregister("Jump", JumpInput);
    }

    private void MovementInput(InputAction.CallbackContext context)
    {
        _playerMovement.Move(context.ReadValue<float>());
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
}