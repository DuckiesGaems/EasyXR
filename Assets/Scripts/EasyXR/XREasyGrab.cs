using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace EasyXRSystems
{
    public class XREasyGrab : MonoBehaviour
    {
        [Header("Place onto your Left/Right Controller")]
        public Rigidbody Player;
        public Transform HandTransform;
        public XRDirectInteractor HandInteractor;

        [Header("Input")]
        public XRNode xrHand = XRNode.RightHand; 
        public InteractionType Type = InteractionType.Trigger;

        [Header("Climbing")]
        public LayerMask ClimbLayer;
        public float GrabRange = 0.08f;

        [Header("Optional Settings")]
        public bool UseNonRBVel = true;

        [Header("Events")]
        public UnityEngine.Events.UnityEvent OnGrab;
        public UnityEngine.Events.UnityEvent OnRelease;

        private InputDevice device;
        private Transform grabPoint;
        private Collider grabbedCollider;
        private bool isGrabbing = false;

        private Collider[] overlapBuffer = new Collider[10];
        private Vector3 LastPosition;
        private Vector3 Velocity;

        public enum InteractionType { Trigger, Grip }

        private void Awake()
        {
            if (HandTransform == null)
                HandTransform = transform;

            if (Player == null)
                Player = GetComponentInParent<Rigidbody>();

            if (HandInteractor == null)
                HandInteractor = GetComponent<XRDirectInteractor>();

            AcquireDevice();
            LastPosition = transform.position;
        }

        private void Update()
        {
            if (!device.isValid)
                AcquireDevice();

            if (IsHoldingInteractable())
            {
                if (isGrabbing)
                    EndGrab();
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
                Velocity = transform.hasChanged
                    ? (transform.position - LastPosition) / Time.deltaTime
                    : Vector3.zero;

                LastPosition = transform.position;
                transform.hasChanged = false;
            }

            if (isGrabbing && grabPoint != null && Player != null)
            {
                Vector3 velocity = (grabPoint.position - HandTransform.position) / Time.fixedDeltaTime;
                Player.linearVelocity = velocity;
            }
        }
        private void AcquireDevice()
        {
            device = InputDevices.GetDeviceAtXRNode(xrHand);
        }

        private bool IsInputPressed()
        {
            if (!device.isValid)
                return false;

            if (Type == InteractionType.Trigger)
            {
                device.TryGetFeatureValue(CommonUsages.trigger, out float trigger);
                return trigger > 0.75f;
            }
            else
            {
                device.TryGetFeatureValue(CommonUsages.grip, out float grip);
                return grip > 0.75f;
            }
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

            grabbedCollider.enabled = false;

            OnGrab?.Invoke();
        }

        private void EndGrab()
        {
            isGrabbing = false;

            if (grabPoint != null)
                grabPoint.gameObject.SetActive(false);

            if (grabbedCollider)
                grabbedCollider.enabled = true;

            grabbedCollider = null;
            grabPoint = null;

            OnRelease?.Invoke();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Transform t = HandTransform != null ? HandTransform : transform;
            Gizmos.DrawWireSphere(t.position, GrabRange);
        }
    }
}
