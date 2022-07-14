using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CursorController : MonoBehaviour
{

    private Rigidbody2D rb;
    private PolygonCollider2D col;
    private SystemCursor sysC;

    public bool usePhysics;
    Vector2 previousPos;

#region Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(CursorController))]
    public class CursorControllerEditor : Editor {
        public override void OnInspectorGUI() {
            base.OnInspectorGUI();

            CursorController controller = (CursorController)target;

            
        }
    }
#endif
#endregion

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody2D>();
        sysC = GetComponent<SystemCursor>();
        col = GetComponent<PolygonCollider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!usePhysics && rb.velocity != Vector2.zero) {
            usePhysics = true;
        }
    }

    private void FixedUpdate() {
        /*velocity += rb.velocity;
        rb.MovePosition(rb.position + velocity * Time.deltaTime);

        rb.velocity = Vector2.zero;*/

        //DetectAndRelolveCollision();

        previousPos = rb.position;
    }

    public void cursorMove(Vector2 delta) {
        if (!sysC.mouseLocked) return;

        Vector2 pxSz = pxSize();
        float pxS = Mathf.Min(pxSz.x, pxSz.y);

        //float corr = Vector2.Dot(rb.velocity.normalized, delta.normalized);
        //Debug.Log(System.String.Format("Delta: {0}; Vel: {1}; scaledDelta: {2}, Corr: {3}", delta, rb.velocity, delta * pxSz, corr));
        //rb.velocity *= Mathf.Abs(corr * Mathf.Min(1, (delta.magnitude * pxS) / (rb.velocity.magnitude + 0.00001f)) );
        //velocity += delta * pxSz * physicsInfluence;
        if (usePhysics) {
            Vector2 moveVec = delta * pxSz;
            Vector2 velocity = moveVec / Time.fixedDeltaTime;
            Vector2 requiredAcceleration = rb.mass * velocity;

            rb.AddForce(requiredAcceleration, ForceMode2D.Force);
            //rb.AddForce(delta * Time.deltaTime * pxSz, ForceMode2D.Force);
        } else {
            rb.MovePosition(rb.position + delta * pxSz);
        }

        //DetectAndRelolveCollision();

        /*if (rb.velocity.magnitude > pxS * 3) {
            rb.velocity += delta * pxSize();
        }else {
            rb.velocity = Vector2.zero;
            rb.MovePosition(v2(transform.position) + delta * pxSz);
        }*/
    }

    public void togglePhysics() {
        setUsePhysics(!usePhysics);
    }

    public void setUsePhysics(bool nowUsePhysics) {
        if(!usePhysics && nowUsePhysics) {
            // rb.velocity = (rb.position - previousPos) * Time.fixedDeltaTime;
            Vector2 targetVelocity = (rb.position - previousPos) / Time.fixedDeltaTime;
            Vector2 requiredAcceleration = rb.mass * targetVelocity / Time.fixedDeltaTime;
            rb.AddForce(requiredAcceleration, ForceMode2D.Force);
        }
        if(usePhysics && !nowUsePhysics) {
            rb.velocity = Vector2.zero;
        }

        usePhysics = nowUsePhysics;
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

    /*void DetectAndRelolveCollision() {
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(getLayerMaskFromCollisionMatrix(gameObject.layer));

        for(int iter = 0; iter < maxCollisionIterations; iter++) {

            List<Collider2D> overlaps = new List<Collider2D>();
            Physics2D.OverlapCollider(col, filter, overlaps);

            // if no overlaps were found terminate
            if (overlaps.Count == 0) return;

            // otherwise resolve collisions and check again next loop
            foreach(Collider2D other in overlaps) {
                ColliderDistance2D offset = Physics2D.Distance(col, other);

                // check again if colliders still overlap
                // (collision resolve with previous collider in list could have fixed this overlap as well)
                if(offset.distance < 0) {
                    // resolve collision
                    rb.position += offset.normal * offset.distance;
                }
            }
        }
    }

    LayerMask getLayerMaskFromCollisionMatrix(int layer) {
        LayerMask m = 0;

        for(int i = 0; i < 32; i++) {
            if (!Physics2D.GetIgnoreLayerCollision(layer, i)) 
                m |= (1 << i);
        }

        return m;
    }*/
}