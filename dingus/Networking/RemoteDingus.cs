using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace dingus.Networking
{
    public class RemoteDingus : MonoBehaviour, IOnEventCallback
    {
        private readonly float      lerpSpeed = 15f;
        private          Vector3    targetPos;
        private          Quaternion targetRot;

        private void LateUpdate()
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime    * lerpSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * lerpSpeed);
        }

        private void OnEnable()  => PhotonNetwork.AddCallbackTarget(this);
        private void OnDisable() => PhotonNetwork.RemoveCallbackTarget(this);

        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code != 41)
                return;

            object[] data = (object[])photonEvent.CustomData;

            int        sender = photonEvent.Sender;
            Vector3    pos    = (Vector3)data[0];
            Quaternion rot    = (Quaternion)data[1];

            if (!DingusManager.Instance.TryGetRemoteDingus(sender, out GameObject dingus) || dingus != gameObject)
                return;

            targetPos = pos;
            targetRot = rot;
        }
    }
}