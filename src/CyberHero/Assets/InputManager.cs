using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    public bool MenuCloseOpenInput { get; private set; }

    public PlayerInput playerInput;

    private InputAction menuCloseOpenAction;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        menuCloseOpenAction = playerInput.actions["MenuButton"];

    }

    void Update()
    {
        MenuCloseOpenInput = menuCloseOpenAction.WasPressedThisFrame();
    }
}
