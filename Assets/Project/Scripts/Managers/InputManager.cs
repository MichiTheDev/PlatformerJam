using System;
using UnityEngine.InputSystem;

public sealed class InputManager : Singleton<InputManager>
{
    public static float InputScale { get; private set; } = 1f;
    
    private static GameInput _input;
    
    protected override void Awake()
    {
        base.Awake();
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

    public static void SetInversedInput(bool reversed)
    {
        InputScale = reversed ? -1f : 1f;
    }
}