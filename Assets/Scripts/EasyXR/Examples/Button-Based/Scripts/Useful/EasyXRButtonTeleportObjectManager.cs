using UnityEngine;
using EasyXRSystems;
using PrimeTween;
using System.Threading.Tasks;

namespace EasyXRSystems
{
    public class EasyXRButtonTeleportObjectManager : XRButton
    {
        [Header("Teleportation Destination")]
        public Transform TeleportDestination;
        [Header("General Settings")]
        public Transform GorillaPlayer;

        public GameObject[] EnabledObjects;
        public GameObject[] DisabledObjects;
        [Header("Miscellaneous")]
        public float TeleportationDelay = 0f;
        public float gravityDisableTime = 1f;
        private GameObject ThisGameobject;

        private Rigidbody gorillaRb;
        private Transform gorillaTransform;

        private bool isTeleporting = false;
        private Collider[] sceneColliders;
        protected override void Start()
        {
            base.Start();
            ThisGameobject = this.gameObject;

            if (TeleportDestination == null)
            {
                Debug.LogError($"TeleportDestination not assigned! Please look at gameobject {this.gameObject.name}");
                return;
            }

            if (GorillaPlayer != null)
            {
                gorillaTransform = GorillaPlayer;
                gorillaRb = GorillaPlayer.GetComponent<Rigidbody>();

                if (gorillaRb == null)
                {
                    Debug.LogWarning("GorillaPlayer has no Rigidbody component.");
                }
            }
            else
            {
                Debug.LogError("GorillaPlayer not assigned!");
            }
            sceneColliders = FindObjectsByType<Collider>(FindObjectsSortMode.None); // required for player model to properly go through walls :)
        }

        public override void ButtonActivationWithBody(bool isPressed)
            => HandleButton(isPressed);
        public override void ButtonActivationWithHand(bool isLeftHand, bool isPressed)
            => HandleButton(isPressed);

        private void HandleButton(bool isPressed)
        {
            if (isPressed)
            {
                if (isTeleporting) return;

                isTeleporting = true;



                foreach (var col in sceneColliders)
                {
                    if (col != null)
                        col.enabled = false;
                }

                BeginTeleporationSequence();

            }
        }

        private async void BeginTeleporationSequence()
        {
            await TeleportAfterDelayAsync(TeleportDestination.position,TeleportDestination.rotation, TeleportationDelay);



            foreach (var col in sceneColliders)
            {
                if (col != null)
                    col.enabled = true;
            }

            isTeleporting = false;
        }

        private async Task TeleportAfterDelayAsync(Vector3 target,Quaternion targetrotation, float delay)
        {

            await Tween.Delay(delay);

            if (gorillaTransform != null)
            {
                if (gorillaRb != null)
                {
                    gorillaRb.isKinematic = true;
                    gorillaRb.useGravity = false;
                    gorillaRb.linearVelocity = Vector3.zero;

                    ObjectSetting();

                    await TeleportGorilla(target, targetrotation);

                    Debug.Log("Teleported GorillaPlayer to: " + target);


                    await Tween.Delay(gravityDisableTime);
                    gorillaRb.useGravity = true;


                    await Tween.Delay(0.1f);
                    gorillaRb.isKinematic = false;
                }
                else
                {

                    await Tween.Position(gorillaTransform, target, 0.005f, Ease.Default);
                             await Tween.Rotation(gorillaTransform, targetrotation, 0.005f, Ease.Default);
                    Debug.Log("Teleported GorillaPlayer to: " + target);
                }
            }
        }

        private async Task TeleportGorilla(Vector3 target, Quaternion targetrotation)
        {
            await Tween.Delay(0.1f);

            await Tween.Position(gorillaTransform, target, 0.005f, Ease.Default);
            await Tween.Rotation(gorillaTransform, targetrotation, 0.005f, Ease.Default);

            await Tween.Delay(0.05f);
            await Tween.Position(gorillaTransform, target, 0.005f, Ease.Default);  // just to make extra sure call again.
                     await Tween.Rotation(gorillaTransform, targetrotation, 0.005f, Ease.Default);
        }

        private void ObjectSetting()
        {
            foreach (var obj in EnabledObjects)
                if (obj) obj.SetActive(true);

            foreach (var obj in DisabledObjects)
                if (obj) obj.SetActive(false);

        }
    }
}
