using UnityEngine;
using PrimeTween;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace EasyXRSystems
{
    [DisallowMultipleComponent]
    public abstract class XRButton : MonoBehaviour
    {
        [Header("General Settings")]
        public bool leftHandOnly = false;
        public float cooldown = 0.15f;
        public float maxCheckDistance = 0.05f;

        [Header("Hold Settings - only used when hold-able button")]
        public float handHoldDuration = 0.5f;
        public float bodyHoldDuration = 0.5f;

        private float sqrMaxDist;
        private float lastPressTime = 0f;

        private Vector3 boxHalfExtents;
        private Collider[] overlapBuffer = new Collider[4];
        private int handLayerMask = (1 << 6) | (1 << 7) | (1 << 8);

        private Transform self;

        // Cached references (assigned via tags)
        private Transform leftHand;
        private Transform rightHand;
        private Transform body;

        // Per‑source inside state
        private bool leftInside = false;
        private bool rightInside = false;
        private bool bodyInside = false;

        // Hold timers
        private float leftHoldTimer = 0f;
        private float rightHoldTimer = 0f;
        private float bodyHoldTimer = 0f;

        // Hold state flags
        private bool leftHolding = false;
        private bool rightHolding = false;
        private bool bodyHolding = false;

        // ---------------------------------------------------------
        //  UNITY LIFECYCLE
        // ---------------------------------------------------------
        protected virtual void Awake()
        {
            self = transform;
            sqrMaxDist = maxCheckDistance * maxCheckDistance;

            AssignTaggedReferences();
        }

        protected virtual void Start()
        {
            UpdateBoxExtents();
        }

        // ---------------------------------------------------------
        //  TAG-BASED HAND/BODY ASSIGNMENT
        // ---------------------------------------------------------
        private void AssignTaggedReferences()
        {
            GameObject[] handObjects = GameObject.FindGameObjectsWithTag("HandTag");

            foreach (var obj in handObjects)
            {
                int layer = obj.layer;

                if (layer == 7) leftHand = obj.transform;
                if (layer == 6) rightHand = obj.transform;
            }

            GameObject bodyObj = GameObject.FindWithTag("Body");
            if (bodyObj != null)
                body = bodyObj.transform;
        }

        // ---------------------------------------------------------
        //  DISTANCE GATE
        // ---------------------------------------------------------
        private bool IsHandCloseEnough()
        {
            Renderer r = GetComponentInChildren<Renderer>();
            if (r == null) return true;

            Bounds b = r.bounds;

            if (leftHand != null)
            {
                Vector3 closest = b.ClosestPoint(leftHand.position);
                if ((closest - leftHand.position).sqrMagnitude < sqrMaxDist)
                    return true;
            }

            if (!leftHandOnly && rightHand != null)
            {
                Vector3 closest = b.ClosestPoint(rightHand.position);
                if ((closest - rightHand.position).sqrMagnitude < sqrMaxDist)
                    return true;
            }

            if (body != null)
            {
                Vector3 closest = b.ClosestPoint(body.position);
                if ((closest - body.position).sqrMagnitude < sqrMaxDist)
                    return true;
            }

            return false;
        }


        // ---------------------------------------------------------
        //  UPDATE LOOP
        // ---------------------------------------------------------
        protected virtual void Update()
        {
            if (!IsHandCloseEnough())
                return;

            int count = Physics.OverlapBoxNonAlloc(
                self.position,
                boxHalfExtents,
                overlapBuffer,
                self.rotation,
                handLayerMask
            );

            bool leftFound = false;
            bool rightFound = false;
            bool bodyFound = false;

            float time = Time.time;

            // PRESS DETECTION
            for (int i = 0; i < count; i++)
            {
                Collider other = overlapBuffer[i];
                if (other == null) continue;

                int layer = other.gameObject.layer;

                if (layer == 7) // left hand
                {
                    leftFound = true;
                    if (!leftInside && time - lastPressTime >= cooldown)
                    {
                        leftInside = true;
                        lastPressTime = time;
                        ButtonActivationWithHand(true, true);
                    }
                }
                else if (layer == 6 && !leftHandOnly) // right hand
                {
                    rightFound = true;
                    if (!rightInside && time - lastPressTime >= cooldown)
                    {
                        rightInside = true;
                        lastPressTime = time;
                        ButtonActivationWithHand(false, true);
                    }
                }
                else if (layer == 8) // body
                {
                    bodyFound = true;
                    if (!bodyInside && time - lastPressTime >= cooldown)
                    {
                        bodyInside = true;
                        lastPressTime = time;
                        ButtonActivationWithBody(true);
                    }

                }
            }

            // RELEASE DETECTION
            if (leftInside && !leftFound)
            {
                leftInside = false;
                leftHoldTimer = 0f;
                leftHolding = false;
                ButtonActivationWithHand(true, false);
            }

            if (rightInside && !rightFound)
            {
                rightInside = false;
                rightHoldTimer = 0f;
                rightHolding = false;
                ButtonActivationWithHand(false, false);
            }

            if (bodyInside && !bodyFound)
            {
                bodyInside = false;
                bodyHoldTimer = 0f;
                bodyHolding = false;
                ButtonActivationWithBody(false);
            }

            // HOLD DETECTION
            float dt = Time.deltaTime;

            // LEFT HOLD
            if (leftInside)
            {
                leftHoldTimer += dt;
                if (!leftHolding && leftHoldTimer >= handHoldDuration)
                {
                    leftHolding = true;
                    OnHandHold(true, leftHoldTimer);
                }
            }

            // RIGHT HOLD
            if (rightInside)
            {
                rightHoldTimer += dt;
                if (!rightHolding && rightHoldTimer >= handHoldDuration)
                {
                    rightHolding = true;
                    OnHandHold(false, rightHoldTimer);
                }
            }

            // BODY HOLD
            if (bodyInside)
            {
                bodyHoldTimer += dt;
                if (!bodyHolding && bodyHoldTimer >= bodyHoldDuration)
                {
                    bodyHolding = true;
                    OnBodyHold(bodyHoldTimer);
                }
            }
        }

        // ---------------------------------------------------------
        //  EXTENTS
        // ---------------------------------------------------------
        private void UpdateBoxExtents()
        {
            MeshFilter mf = GetComponentInChildren<MeshFilter>();

            if (mf != null && mf.sharedMesh != null)
            {
                // Local mesh bounds
                Bounds local = mf.sharedMesh.bounds;

                // Convert local extents to world extents using lossyScale
                Vector3 scaledExtents = Vector3.Scale(local.extents, mf.transform.lossyScale);

                // Add a small padding
                boxHalfExtents = scaledExtents + new Vector3(0.02f, 0.02f, 0.02f);
                return;
            }

            // Fallback to renderer if mesh is missing
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null)
            {
                boxHalfExtents = r.bounds.extents + new Vector3(0.02f, 0.02f, 0.02f);
                return;
            }

            // Final fallback
            boxHalfExtents = transform.lossyScale * 0.5f;
        }



        // ---------------------------------------------------------
        //  OVERRIDABLE EVENTS
        // ---------------------------------------------------------
        public virtual void ButtonActivationWithHand(bool isLeftHand, bool isPressed) { }
        public virtual void ButtonActivationWithBody(bool isPressed) { }

        public virtual void OnHandHold(bool isLeftHand, float duration) { }
        public virtual void OnBodyHold(float duration) { }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, boxHalfExtents * 2f);
        }
#endif
    }
#if UNITY_EDITOR
    [CustomEditor(typeof(XRButton), true)]
    public class EasyXRButtonEditor : Editor
    {
        // XRButton base fields
        private readonly string[] baseFields =
        {
        "leftHandOnly",
        "cooldown",
        "maxCheckDistance",
        "handHoldDuration",
        "bodyHoldDuration"
    };

        // Foldout states
        private static bool showBase = false;
        private static bool showSubclass = false;
        private static bool showDebug = false;


        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            XRButton xRButton = (XRButton)target;





            string subclassName = target.GetType().Name;
            string prettyName = ObjectNames.NicifyVariableName(subclassName);

            showSubclass = EditorGUILayout.Foldout(showSubclass, prettyName + " Settings", true);


            if (showSubclass)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                SerializedProperty iterator = serializedObject.GetIterator();
                bool enterChildren = true;

                while (iterator.NextVisible(enterChildren))
                {
                    enterChildren = false;

                    if (iterator.name == "m_Script") continue;

                    bool isBaseField = false;
                    foreach (string field in baseFields)
                    {
                        if (iterator.name == field)
                        {
                            isBaseField = true;
                            break;
                        }
                    }
                    if (isBaseField) continue;

                    EditorGUILayout.PropertyField(iterator, true);
                }

                GUILayout.Space(10);
            }
            GUILayout.Space(10);

            showBase = EditorGUILayout.Foldout(showBase, "XR Button Settings", true);

            if (showBase)
            {
                EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                foreach (string field in baseFields)
                {
                    SerializedProperty prop = serializedObject.FindProperty(field);
                    if (prop != null)
                        EditorGUILayout.PropertyField(prop, true);
                }

                GUILayout.Space(10);
            }


            GUILayout.Space(10);


            string debugLabel = EditorApplication.isPlaying
                ? "Debug Tools"
                : "Debug Tools - Works ONLY in play-test";


            if (!EditorApplication.isPlaying)
                showDebug = false;


            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                showDebug = EditorGUILayout.Foldout(showDebug, debugLabel, true);

                if (showDebug && EditorApplication.isPlaying)
                {
                    EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

                    if (GUILayout.Button("Test button press - Hand"))
                    {
                        xRButton.ButtonActivationWithHand(false, true);
                        Tween.Delay(0.85f);
                        xRButton.ButtonActivationWithHand(false, false);
                    }

                    GUILayout.Space(10);

                    if (GUILayout.Button("Test button press - Body"))
                    {
                        xRButton.ButtonActivationWithBody(true);
                        Tween.Delay(0.85f);
                        xRButton.ButtonActivationWithBody(false);
                    }

                    GUILayout.Space(10);
                }
            }



            GUIStyle richText = new GUIStyle(GUI.skin.label)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter
            };

            GUILayout.Label("<b>Tip:</b> Add layers 6 - Right, 7 - Left, 8 - Body", richText);

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}
