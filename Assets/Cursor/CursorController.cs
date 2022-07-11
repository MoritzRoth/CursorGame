using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    [SerializeField] private InputActionReference cursorMovement;

    Rigidbody2D rb;
    SystemCursor sysC;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        sysC = GetComponent<SystemCursor>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void cursorMove(Vector2 delta) {
        if (!sysC.mouseLocked) return;
        rb.MovePosition(v2(transform.position) + delta * pxSize());
    }

    public void LMB(InputAction.CallbackContext context) {
        
    }

    Vector2 pxSize() {
        Camera cam = Camera.main;

        Vector2 screenDim = new Vector2(cam.pixelWidth, cam.pixelHeight);

        Vector3 min = cam.ScreenToWorldPoint(Vector2.zero);
        Vector3 max = cam.ScreenToWorldPoint(screenDim);
        Vector3 span = new Vector3(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y), Mathf.Abs(max.z - min.z));

        return span / screenDim;
    }

    Vector2 v2(Vector3 v) {
        return v;
    }
}