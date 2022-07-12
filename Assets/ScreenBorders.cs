using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ScreenBorders : MonoBehaviour
{

    public GameObject borderPrefab;
    public float borderThickness = 2f;
    public BoxCollider2D[] keepInside;

    [System.Serializable] 
    public class ScreenResizeEvent : UnityEvent<Vector2> { }
    public ScreenResizeEvent OnScreenResize;

    [System.Serializable]
    public class ViewChangeEvent : UnityEvent<Bounds> { }
    public ViewChangeEvent OnViewChange;

    private Vector2Int res;
    private List<GameObject> borders = new List<GameObject>();
    private BoxCollider2D screenCollider;
    private Matrix4x4 viewProjMat;

    // Start is called before the first frame update
    void Start()
    {
        // get / create inactive box collider to test if key elements have left the screen area after resizing
        screenCollider = GetComponent<BoxCollider2D>();
        if(!screenCollider) {
            screenCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        screenCollider.isTrigger = true;

        // initialize border obstacles
        for (int i = 0; i < 4; i++) {
            borders.Add(Instantiate(borderPrefab, transform));
        }
        borders[0].name = "LeftBorder";
        borders[1].name = "RightBorder";
        borders[2].name = "TopBorder";
        borders[3].name = "BottomBorder";
        updateBorders();

        res = getRes();
        viewProjMat = getViewProjMat();
    }

    // Update is called once per frame
    void Update()
    {
        Camera cam = Camera.main;


        // detect resolution change
        if(res != getRes() || viewProjMat != getViewProjMat()) {
            updateBorders();

            if (res != getRes()) {
                OnScreenResize.Invoke(res);
                res = getRes();
            }

            if(viewProjMat != getViewProjMat()) {
                OnViewChange.Invoke(screenCollider.bounds);
                viewProjMat = getViewProjMat();
            }
        }
    }

    private void updateBorders() {
        Camera cam = Camera.main;

        Vector2 screenDim = new Vector2(cam.pixelWidth, cam.pixelHeight);

        Vector2 min = cam.ScreenToWorldPoint(Vector2.zero);
        Vector2 max = cam.ScreenToWorldPoint(screenDim);
        Vector2 span = new Vector2(Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));

        screenCollider.offset = (min + max) / 2;
        screenCollider.size = span;
        Bounds screenBounds = screenCollider.bounds;

        //left border
        borders[0].transform.localScale = new Vector3(borderThickness, borderThickness * 2 + span.y, 1);
        borders[0].transform.position = new Vector3(min.x - borderThickness / 2, (min.y + max.y) / 2, 0);

        //right border
        borders[1].transform.localScale = new Vector3(borderThickness, borderThickness * 2 + span.y, 1);
        borders[1].transform.position = new Vector3(max.x + borderThickness / 2, (min.y + max.y) / 2, 0);

        //top border
        borders[2].transform.localScale = new Vector3(borderThickness * 2 + span.x, borderThickness, 1);
        borders[2].transform.position = new Vector3((min.x + max.x) / 2, max.y + borderThickness / 2, 0);

        //bottom border
        borders[3].transform.localScale = new Vector3(borderThickness * 2 + span.x, borderThickness, 1);
        borders[3].transform.position = new Vector3((min.x + max.x) / 2, min.y - borderThickness / 2, 0);


        // keep freely moveable elements on sceen that should stay on screen
        /*foreach(BoxCollider2D col in keepInside) {
            GameObject go = col.gameObject;
            Bounds b = col.bounds;

            if(!screenBounds.Contains(b.min) || !screenBounds.Contains(b.max)) {
                Vector2 closestPointOnCol = col.ClosestPoint(screenBounds.center);
                Vector2 closetsPointOnScreen = screenCollider.ClosestPoint(b.center);

                Vector2 closestPointsOffset = closetsPointOnScreen - closestPointOnCol;

                // move game object directly next to screen
                go.transform.position += (Vector3)closestPointsOffset;
                // move game object inside screen
                Vector3 moveInsideVec = new Vector3(Mathf.Sign(closestPointsOffset.x), Mathf.Sign(closestPointsOffset.y), 0);
                moveInsideVec.Scale(b.size);
                go.transform.position += moveInsideVec;

            }
        }*/
    }

    Vector2Int getRes() {
        return new Vector2Int(Screen.width, Screen.height);
    }

    // actually did not test if this is really the view projection matrix
    // i only need this to check if the camera moved or the projection changed since last frame
    // since all the relevant parameters are used in this calculation the result should only change if the input changes... this is enough for the current purpose
    Matrix4x4 getViewProjMat() {
        Camera cam = Camera.main;
        return cam.projectionMatrix * cam.transform.worldToLocalMatrix;
    }
}
