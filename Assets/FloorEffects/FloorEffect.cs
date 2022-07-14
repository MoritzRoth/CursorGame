using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class FloorEffect : MonoBehaviour
{

    [SerializeField]
    public enum EffectType {
        DoNothing,
        ApplyForce,
        InvokeEvent,
        ChangePhysics,
        SetPhysicsMaterial
    }

    [SerializeField]
    public enum EventType {
        SelectEvent,
        OnEnter,
        OnExit,
        OnStay,
        OnEnterAndExit
    }

    [SerializeField]
    public enum BoolChanges {
        Toggle,
        TurnOn,
        TurnOff
    }

    [SerializeField]
    public enum Direction {
        Vector,
        Movement,
        CounterMovement,
        Inwards,
        Outwards,
        North,
        NorthEast,
        East,
        SouthEast,
        South,
        SouthWest,
        West,
        NorthWest
    }

    [System.Serializable]
    public class Effect{
        public EffectType effectType;
        public EventType executeOn;
        public bool enabled;

        // apply force variables
        public Direction forceDir;
        public Vector2 forceDirVec;
        public float forceStrength;
        public ForceMode2D forceMode;

        // invoke event variables
        public UnityEvent someEvent; 

        // change physics variables
        public BoolChanges physicsChanges;

        // set physics material
        public PhysicsMaterial2D physMat;

        public Effect(EffectType t, EventType on) {
            effectType = t;
            executeOn = on;
            enabled = true;
        }

        public void execute(EventType et, Collider2D col, Collider2D floorTile) {
            if (!enabled) return;
            if (!correctEvent(et)) return;

            switch(effectType) {
                case EffectType.ApplyForce:
                    applyForce(col, floorTile);
                    break;
                case EffectType.InvokeEvent:
                    someEvent.Invoke();
                    break;
                case EffectType.ChangePhysics:
                    changePhysics(col);
                    break;
                case EffectType.SetPhysicsMaterial:
                    setPhysicsMat(col);
                    break;
            }
        }

        bool correctEvent(EventType et) {
            if (executeOn == et) return true;
            return executeOn == EventType.OnEnterAndExit && (et == EventType.OnEnter || et == EventType.OnExit);
        }

        private void changePhysics(Collider2D col) {
            CursorController cc = col.gameObject.GetComponent<CursorController>();
            if(cc) {
                switch(physicsChanges) {
                    case BoolChanges.Toggle:
                        cc.togglePhysics();
                        break;
                    case BoolChanges.TurnOn:
                        cc.setUsePhysics(true);
                        break;
                    case BoolChanges.TurnOff:
                        cc.setUsePhysics(false);
                        break;
                }
            }
        }

        private void applyForce(Collider2D col, Collider2D floorTile) {
            Rigidbody2D rb = col.gameObject.GetComponent<Rigidbody2D>();
            if (rb) {

                // determine force direction
                if (forceDir != Direction.Vector)           forceDirVec = Vector2.zero;
                if (forceDir == Direction.Movement)         forceDirVec = rb.velocity;
                if (forceDir == Direction.CounterMovement)  forceDirVec = -rb.velocity;
                if (forceDir == Direction.Inwards)          forceDirVec = floorTile.attachedRigidbody.position - rb.position;
                if (forceDir == Direction.Outwards)         forceDirVec = rb.position - floorTile.attachedRigidbody.position;

                if (forceDir == Direction.North || forceDir == Direction.NorthEast || forceDir == Direction.NorthWest) forceDirVec += Vector2.up;
                if (forceDir == Direction.East  || forceDir == Direction.NorthEast || forceDir == Direction.SouthEast) forceDirVec += Vector2.right;
                if (forceDir == Direction.South || forceDir == Direction.SouthEast || forceDir == Direction.SouthWest) forceDirVec += Vector2.down;
                if (forceDir == Direction.West  || forceDir == Direction.NorthWest || forceDir == Direction.SouthWest) forceDirVec += Vector2.left;

                // determine force strength
                // TODO

                rb.AddForce(forceDirVec.normalized * forceStrength * Time.fixedDeltaTime, forceMode);
            }
        }

        private void setPhysicsMat(Collider2D col) {
            Rigidbody2D rb = col.gameObject.GetComponent<Rigidbody2D>();
            if (rb) {
                rb.sharedMaterial = physMat;
            }
        }
    }

    public List<Effect> effects = new List<Effect>();

    #region Editor
#if UNITY_EDITOR
    // adapted from https://forum.unity.com/threads/display-a-list-class-with-a-custom-editor-script.227847/
    [CustomEditor(typeof(FloorEffect))]
    public class FloorEffectEditor : Editor {

        FloorEffect t;
        SerializedObject GetTarget;
        SerializedProperty ThisList;
        int ListSize;

        void OnEnable() {
            t = (FloorEffect)target;
            GetTarget = new SerializedObject(t);
            ThisList = GetTarget.FindProperty("effects"); // Find the List in our script and create a refrence of it
        }

        public override void OnInspectorGUI() {
            //base.OnInspectorGUI();

            GetTarget.Update();

            if (GUILayout.Button("Add New Effect")) {
                t.effects.Add(new Effect(EffectType.DoNothing, EventType.SelectEvent));
            }

            for (int i = 0; i < ThisList.arraySize; i++) {
                SerializedProperty EffectRef = ThisList.GetArrayElementAtIndex(i);
                
                SerializedProperty EffectActive = EffectRef.FindPropertyRelative("enabled");
                SerializedProperty EffectTyp = EffectRef.FindPropertyRelative("effectType");
                SerializedProperty ExecuteOn = EffectRef.FindPropertyRelative("executeOn");
                
                SerializedProperty ForceDir = EffectRef.FindPropertyRelative("forceDir");
                SerializedProperty ForceDirVec = EffectRef.FindPropertyRelative("forceDirVec");
                SerializedProperty ForceStrength = EffectRef.FindPropertyRelative("forceStrength");
                SerializedProperty ForceMode = EffectRef.FindPropertyRelative("forceMode");

                SerializedProperty SomeEvent = EffectRef.FindPropertyRelative("someEvent");

                SerializedProperty PhysicsCh = EffectRef.FindPropertyRelative("physicsChanges");
                SerializedProperty PhysicsMat = EffectRef.FindPropertyRelative("physMat");

                if (i > 0) { 
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }

                EditorGUILayout.LabelField("Effect " + i);
                EditorGUILayout.BeginHorizontal();
                EffectTyp.enumValueIndex = EditorGUILayout.Popup(/*new GUIContent(""),*/EffectTyp.enumValueIndex,EffectTyp.enumDisplayNames);
                ExecuteOn.enumValueIndex = EditorGUILayout.Popup(/*new GUIContent(""),*/ExecuteOn.enumValueIndex,ExecuteOn.enumDisplayNames);
                EffectActive.boolValue = EditorGUILayout.Toggle(EffectActive.boolValue, GUILayout.MaxWidth(15));
                EditorGUILayout.EndHorizontal();

                switch ((EffectType)EffectTyp.enumValueIndex) {
                    case EffectType.ApplyForce:
                        ForceMode.enumValueIndex = EditorGUILayout.Popup("Force Mode", ForceMode.enumValueIndex, ForceMode.enumDisplayNames);
                        ForceDir.enumValueIndex = EditorGUILayout.Popup("Force Direction", ForceDir.enumValueIndex, ForceDir.enumDisplayNames);
                        if((Direction)ForceDir.enumValueIndex == Direction.Vector)
                            ForceDirVec.vector2Value = EditorGUILayout.Vector2Field("Direction Vector", ForceDirVec.vector2Value);
                        ForceStrength.floatValue = Mathf.Max(0, EditorGUILayout.FloatField(new GUIContent("Force Strength"), ForceStrength.floatValue));
                        break;

                    case EffectType.InvokeEvent:
                        EditorGUILayout.PropertyField(SomeEvent);
                        break;
                    case EffectType.ChangePhysics:
                        PhysicsCh.enumValueIndex = EditorGUILayout.Popup(new GUIContent("Change Mode"), PhysicsCh.enumValueIndex, PhysicsCh.enumDisplayNames);
                        break;
                    case EffectType.SetPhysicsMaterial:
                        EditorGUILayout.PropertyField(PhysicsMat);
                        break;
                }

                EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("Remove This Effect")) {
                    ThisList.DeleteArrayElementAtIndex(i);
                }
                if(GUILayout.Button("^",GUILayout.MaxWidth(30))) {

                }
                if (GUILayout.Button("v", GUILayout.MaxWidth(30))) {

                }
                EditorGUILayout.EndHorizontal();
            }

            GetTarget.ApplyModifiedProperties();
        }

        Effect showEffectEditor(Effect effect) {


            return effect;
        }
    }

#endif
            #endregion


    private Collider2D ourCollider;

    // Start is called before the first frame update
    void Start() {
        ourCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update() {
        
    }

    private void OnTriggerEnter2D(Collider2D collision) {
        foreach(Effect e in effects) {
            e.execute(EventType.OnEnter, collision, ourCollider);
        }
    }

    private void OnTriggerExit2D(Collider2D collision) {
        foreach (Effect e in effects) {
            e.execute(EventType.OnExit, collision, ourCollider);
        }
    }

    private void OnTriggerStay2D(Collider2D collision) {
        foreach (Effect e in effects) {
            e.execute(EventType.OnStay, collision, ourCollider);
        }
    }
}
