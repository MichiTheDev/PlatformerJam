using System;
using UnityEngine.InputSystem;

public sealed class InputManager : Singleton<InputManager>
{
    public static event Action<bool> OnInputInverted;
    
    public static float InputScale { get; private set; } = 1f;
    public static bool InputInverted { get; private set; }
    
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

    public static void SetInputInverted(bool inverted)
    {
        InputScale = inverted ? -1f : 1f;
        InputInverted = inverted;
        OnInputInverted?.Invoke(inverted);
    }
}