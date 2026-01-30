using UnityEngine;
using EasyXRSystems;
using UnityEngine.Events;
using JetBrains.Annotations;

namespace EasyXRSystems
{
    public class EasyXRButtonUnityEventManager : XRButton
    {
        public enum TypesofTouch { Hand, Body, Both }
        [Header("Unity Event")]
        public UnityEvent OnPressSequence;

        [Header("General Settings")]
        public TypesofTouch ButtonTouchMode;

        public override void ButtonActivationWithBody(bool isPressed)
            => HandleButtonInitialization(isPressed, TypesofTouch.Body);

        public override void ButtonActivationWithHand(bool isLeftHand, bool isPressed)
            => HandleButtonInitialization(isPressed, TypesofTouch.Hand);



        public void HandleButtonInitialization(bool isPressed, TypesofTouch source)
        {
            if (!isPressed)
                return;


            if (ButtonTouchMode != TypesofTouch.Both && ButtonTouchMode != source)
                return;


            OnPressSequence?.Invoke();
        }

    }
}
