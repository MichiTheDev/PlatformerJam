using UnityEngine;
using UnityEngine.InputSystem;

public class InputExample : MonoBehaviour
{
    // Subscribe on input events
    private void OnEnable()
    {
        GameInput gameInput = InputManager.Input;
        
        gameInput.Player.Movement.performed += MovementInput;
        gameInput.Player.Movement.canceled += MovementInput;
    }

    // Unsubscribe on input events
    private void OnDisable()
    {
        GameInput gameInput = InputManager.Input;
        
        gameInput.Player.Movement.performed -= MovementInput;
        gameInput.Player.Movement.canceled -= MovementInput;
    }
    
    private void MovementInput(InputAction.CallbackContext context)
    {
        print(context.ReadValue<float>());
    }
}