using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using easyInputs;


namespace EasyXRSystems
{
    public class XREasyGrab : MonoBehaviour
    {

        [Header("Place onto your Left/Right Controller\n\nReferences")]
        public Rigidbody Player;
        public Transform HandTransform;
        public XRDirectInteractor HandInteractor;

        [Header("Input")]
        public EasyHand TriggerHand = EasyHand.RightHand;
        public InteractionType Type = InteractionType.Trigger;

        [Header("Climbing")]
        public LayerMask ClimbLayer;
        public float GrabRange = 0.08f;
        [Header("Optional Settings")]
        [Tooltip("If true, uses Non-Rigidbody Velocity and creates a more smoother result.")] public bool UseNonRBVel = true;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnGrab;
        public UnityEngine.Events.UnityEvent OnRelease;

        private Transform grabPoint;
        private Collider grabbedCollider;
        private bool isGrabbing = false;
        private Collider[] overlapBuffer = new Collider[10]; // or whatever max you expect


        public enum InteractionType { Trigger, Grip }

        private Vector3 Velocity;

        private Vector3 LastPosition;

        private void Awake()
        {
            if (UseNonRBVel)
            {
                LastPosition = transform.position;
            }

            if (HandTransform == null)
                HandTransform = transform;

            if (Player == null)
                Player = GetComponentInParent<Rigidbody>();

            if (HandInteractor == null)
                HandInteractor = GetComponent<XRDirectInteractor>();
        }

        private void Update()
        {

            if (IsHoldingInteractable())
            {
                if (isGrabbing) EndGrab();
                return;
            }

            bool inputPressed = IsInputPressed();

            if (inputPressed && !isGrabbing && TryGetClosestClimbable(out Collider climbable))
            {
                StartGrab(climbable);
            }
            else if (!inputPressed && isGrabbing)
            {
                EndGrab();
            }
        }

        private void FixedUpdate()
        {
            if (UseNonRBVel)
            {
                Velocity = transform.hasChanged ? (transform.position - LastPosition) / Time.deltaTime : Vector3.zero;
                LastPosition = transform.position;
                transform.hasChanged = false;
            }

            if (isGrabbing && grabPoint != null && Player != null)
            {
                Vector3 velocity = (grabPoint.position - HandTransform.position) / Time.fixedDeltaTime;
                Player.linearVelocity = velocity;
            }
        }

        private bool IsInputPressed()
        {
            return Type == InteractionType.Trigger
                ? EasyInputs.GetTriggerButtonDown(TriggerHand)
                : EasyInputs.GetGripButtonDown(TriggerHand);
        }

        private bool IsHoldingInteractable()
        {
            return HandInteractor != null && HandInteractor.hasSelection;
        }

        private bool TryGetClosestClimbable(out Collider climbable)
        {
            int count = Physics.OverlapSphereNonAlloc(
                HandTransform.position,
                GrabRange,
                overlapBuffer,
                ClimbLayer
            );

            float bestDist = float.MaxValue;
            Collider best = null;

            for (int i = 0; i < count; i++)
            {
                var c = overlapBuffer[i];
                if (c == grabbedCollider) continue;

                float d = (HandTransform.position - c.ClosestPoint(HandTransform.position)).sqrMagnitude;
                if (d < bestDist)
                {
                    bestDist = d;
                    best = c;
                }
            }

            climbable = best;
            return climbable != null;
        }

        private void StartGrab(Collider climbable)
        {
            isGrabbing = true;
            grabbedCollider = climbable;

            if (grabPoint == null)
                grabPoint = new GameObject("GrabPoint").transform;

            grabPoint.SetParent(climbable.transform, true);
            grabPoint.position = HandTransform.position;
            grabPoint.rotation = HandTransform.rotation;

            grabPoint.SetParent(climbable.transform, true);

            grabbedCollider.enabled = false;

            OnGrab?.Invoke();
            Debug.Log($"Grab started with {TriggerHand} on {climbable.name}.");
        }

        private void EndGrab()
        {
            isGrabbing = false;

            grabPoint.gameObject.SetActive(false);
            grabPoint = null;

            if (grabbedCollider) grabbedCollider.enabled = true;
            grabbedCollider = null;

            OnRelease?.Invoke();
            Debug.Log($"Grab ended with {TriggerHand}.");
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Transform t = HandTransform != null ? HandTransform : transform;
            Gizmos.DrawWireSphere(t.position, GrabRange);
        }
    }
}
