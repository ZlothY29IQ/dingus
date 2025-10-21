/*
MIT License

Copyright (c) 2023 dev9998

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using dingus;
using GorillaLocomotion;
using UnityEngine;

namespace DevHoldableEngine
{
    public class DevHoldable : HoldableObject
    {
        public bool        InHand;
        public bool        InLeftHand;
        public bool        PickUp;
        public Rigidbody   Rigidbody;
        public AudioClip   clip;
        public AudioSource audioSource;
        public Collider    boxColl;

        public float Distance   = 0.2f;
        public float ThrowForce = 1.75f;

        public void Update()
        {
            bool leftGrip  = ControllerInputPoller.instance.leftGrab;
            bool rightGrip = ControllerInputPoller.instance.rightGrab;

            if (PickUp && leftGrip &&
                Vector3.Distance(GTPlayer.Instance.leftControllerTransform.position, transform.position) < Distance &&
                !InHand && EquipmentInteractor.instance.leftHandHeldEquipment == null)
            {
                InLeftHand = true;
                InHand     = true;
                transform.SetParent(GorillaTagger.Instance.offlineVRRig.leftHandTransform.parent);

                GorillaTagger.Instance.StartVibration(true, 0.1f, 0.05f);
                EquipmentInteractor.instance.leftHandHeldEquipment = this;

                OnGrab(true);
            }
            else if (!leftGrip && InHand && InLeftHand)
            {
                InLeftHand = true;
                InHand     = false;
                transform.SetParent(null);

                GorillaTagger.Instance.StartVibration(true, 0.1f, 0.05f);
                EquipmentInteractor.instance.leftHandHeldEquipment = null;

                OnDrop(true);
            }

            if (PickUp && rightGrip &&
                Vector3.Distance(GTPlayer.Instance.rightControllerTransform.position, transform.position) < Distance &&
                !InHand && EquipmentInteractor.instance.rightHandHeldEquipment == null)
            {
                InLeftHand = false;
                InHand     = true;
                transform.SetParent(GorillaTagger.Instance.offlineVRRig.rightHandTransform.parent);

                GorillaTagger.Instance.StartVibration(false, 0.1f, 0.05f);
                EquipmentInteractor.instance.rightHandHeldEquipment = this;

                OnGrab(false);
            }
            else if (!rightGrip && InHand && !InLeftHand)
            {
                InLeftHand = false;
                InHand     = false;
                transform.SetParent(null);

                GorillaTagger.Instance.StartVibration(false, 0.1f, 0.05f);
                EquipmentInteractor.instance.rightHandHeldEquipment = null;

                OnDrop(false);
            }
        }

        public override void OnGrab(InteractionPoint point, GameObject grabbingHand)
        {
            bool isLeft = grabbingHand == GTPlayer.Instance.leftControllerTransform.gameObject;
            OnGrab(isLeft);
        }

        public override void OnHover(InteractionPoint point, GameObject hoveringHand) { }

        public override void DropItemCleanup()
        {
            if (InHand)
            {
                OnDrop(InLeftHand);
                InHand     = false;
                InLeftHand = false;
            }
        }

        public virtual void OnGrab(bool isLeft)
        {
            if (Rigidbody != null)
            {
                Rigidbody.isKinematic = true;
                Rigidbody.useGravity  = false;
            }

            if (clip == null || audioSource == null || boxColl == null)
            {
                audioSource = gameObject.GetComponent<AudioSource>();
                boxColl     = gameObject.GetComponent<BoxCollider>();
                clip        = Plugin.bundle.LoadAsset<AudioClip>("wpn_select");
            }

            audioSource.PlayOneShot(clip, 1f);
        }

        public virtual void OnDrop(bool isLeft)
        {
            if (Rigidbody != null)
            {
                GorillaVelocityEstimator gorillaVelocityEstimator = (isLeft
                                                                             ? GTPlayer.Instance.leftControllerTransform
                                                                                    .GetComponentInChildren<
                                                                                             GorillaVelocityEstimator>()
                                                                             : GTPlayer.Instance
                                                                                    .rightControllerTransform
                                                                                    .GetComponentInChildren<
                                                                                             GorillaVelocityEstimator>()) ??
                                                                    null;

                if (gorillaVelocityEstimator != null)
                {
                    Rigidbody.isKinematic = false;
                    Rigidbody.useGravity  = true;
                    Rigidbody.velocity = gorillaVelocityEstimator.linearVelocity * ThrowForce
                                       + GTPlayer.Instance.GetComponent<Rigidbody>().velocity;

                    Rigidbody.angularVelocity = gorillaVelocityEstimator.angularVelocity;
                }
            }
        }
    }
}