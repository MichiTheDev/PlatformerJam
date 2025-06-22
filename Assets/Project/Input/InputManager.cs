using UnityEngine;

public sealed class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }
    public static GameInput Input { get; private set; }
    
    private void Awake()
    {
        if(Instance is not null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        Input = new GameInput();
        Input.Enable();
    }
}