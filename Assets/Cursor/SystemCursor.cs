using System.Runtime.InteropServices;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

// adapted from http://answers.unity.com/answers/1692565/view.html
public class SystemCursor : MonoBehaviour {

    [System.Serializable]
    public class WrapEvent : UnityEvent<Vector2> { }
    public WrapEvent OnWrap;

    [System.Serializable]
    public class CursorMoveEvent : UnityEvent<Vector2> { }
    public CursorMoveEvent CursorMove;

    [HideInInspector]
    public bool mouseLocked = false;

    public bool alwaysShowCursor = false;
    public float cursorSpeed = 1;

    private bool ignoreNextMouseMove = false;

    private Vector2 pmp = Vector2.zero;

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN

    public class Win32 {
        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT {
            public int X;
            public int Y;

            public POINT(int x, int y) {
                this.X = x;
                this.Y = y;
            }
        }
    }
    Vector2Int getRealPos() {
        Point p = System.Windows.Forms.Cursor.Position;
        return new Vector2Int(p.X, Screen.currentResolution.height - p.Y - 1);          // correct cursor position to unity units
    }

    private void setRealPos(Vector2Int p) {
        Vector2Int c = new Vector2Int(p.x, Screen.currentResolution.height - p.y - 1);    // correct cursor position to windows units
        System.Windows.Forms.Cursor.Position = new Point(c.x, c.y);
    }

    private Vector2 mouseMoveAndWrap(bool shouldWrap) {

        Vector2 mp = Mouse.current.position.ReadValue();
        Vector2 screen = new Vector2(Screen.width, Screen.height);
        int d = (int)(Mathf.Min(screen.x, screen.y) / 6);

        int wrapHorizontal = 0;
        if (mp.x <= d)
            wrapHorizontal = 1;
        else if (mp.x >= screen.x - d)
            wrapHorizontal = -1;

        int wrapVertical = 0;
        if (mp.y <= d)
            wrapVertical = -1;
        else if (mp.y >= screen.y - d)
            wrapVertical = 1;

        Vector2 delta = mp - pmp;
        pmp = mp;
        if (shouldWrap && (wrapHorizontal != 0 || wrapVertical != 0)) {
            Win32.GetCursorPos(out Win32.POINT p);
            Vector2 wrapDelta = new Vector2(wrapHorizontal, wrapVertical);
            wrapDelta.Scale(screen - (d + 10) * 2 * Vector2.one);

            Win32.SetCursorPos(p.X + (int)wrapDelta.x, p.Y + (int)wrapDelta.y);

            wrapDelta.Scale(new Vector2(1f, -1f));
            OnWrap.Invoke(wrapDelta);

            pmp = mp + wrapDelta;
        }

        return delta;
    }

    private Vector2Int vpDim() {
        return new Vector2Int(Screen.width, Screen.height);
    }

    private Vector2Int dpDim() {
        return new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
    }

    private Vector2Int botLeftVPPos() {
        Vector2Int viewportDim = vpDim();
        Vector2Int displayDim = dpDim();

        Vector2Int DPTopToVPTop = Screen.mainWindowPosition;
        Vector2Int DPBotToVPTop = new Vector2Int(DPTopToVPTop.x, displayDim.y - DPTopToVPTop.y - 1);
        Vector2Int DPBotToVPBot = DPBotToVPTop - new Vector2Int(0, viewportDim.y);

        return DPBotToVPBot;
    }

    private Vector2 captureMouse(bool shouldCapture) {
        if (!shouldCapture) return Vector2.zero;

        Vector2Int displayMP = getRealPos();

        Vector2Int viewportDim = vpDim();
        Vector2Int viewportPos = botLeftVPPos();
        Vector2Int viewportCenter = viewportPos + viewportDim / 2;

        Vector2 delta = displayMP - viewportCenter;
        setRealPos(viewportCenter);

        return delta;
    }
#endif


    private void FixedUpdate() {
#if UNITY_EDITOR
        Vector2 delta = mouseMoveAndWrap(mouseLocked);
#else
        Vector2 delta = captureMouse(mouseLocked);
#endif
        if(delta != Vector2.zero && !ignoreNextMouseMove) {
            CursorMove.Invoke(delta * cursorSpeed);
        }
        ignoreNextMouseMove = false;
    }

    private void OnApplicationFocus(bool focus) {
        if (focus) ignoreNextMouseMove = true;
    }

    public void toggleMouseLock(InputAction.CallbackContext context) {
        if (!context.performed) return;

        Debug.Log("mouse lock toggle");
        if (mouseLocked) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        } else {
            Cursor.visible = alwaysShowCursor;
            Cursor.lockState = CursorLockMode.Confined;
        }

        mouseLocked = !mouseLocked;
    }

    public void LMB(InputAction.CallbackContext context) {
        if (!context.performed) return;

        if (!mouseLocked) {
            toggleMouseLock(context);
            return;
        }
    }
}