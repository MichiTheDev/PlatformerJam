using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    
    private static GameInput _input;
    
    private void Awake()
    {
        if(Instance is not null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        _input = new GameInput();
        _input.Enable();
    }

    public static void Register(string actionName, Action<InputAction.CallbackContext> context)
    {
        InputAction action = _input.asset.FindAction(actionName);
        if (action is null) return;

        action.performed += context;
        action.canceled += context;
    }


    public static void Unregister(string actionName, Action<InputAction.CallbackContext> context)
    {
        InputAction action = _input.asset.FindAction(actionName);
        if (action is null) return;

        action.performed -= context;
        action.canceled -= context;
    }
}