using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ToggleFullscreen : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnToggleFullscreen(InputAction.CallbackContext context) {
        if (!context.performed) return;
        if(Screen.fullScreen) {
            Screen.fullScreen = false;
            return;
        }

        Screen.SetResolution(Screen.currentResolution.width, Screen.currentResolution.height, true);
    }
}
