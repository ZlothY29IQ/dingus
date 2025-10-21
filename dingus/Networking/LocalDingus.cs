using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace dingus.Networking;

public class LocalDingus : MonoBehaviour
{
    private const float      sendRate      = 0.1f;
    private const float      moveThreshold = 0.001f;
    private       Vector3    lastPos;
    private       Quaternion lastRot;

    private float timer;
    private bool  wasMoving;

    private void Start()
    {
        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < sendRate)
            return;

        timer = 0f;

        if (!NetworkSystem.Instance.InRoom)
            return;

        bool isMoving = HasMoved();

        if (isMoving || wasMoving)
        {
            SendTransform();
            wasMoving = isMoving;
        }
    }

    private bool HasMoved()
    {
        float posDiff = (transform.position - lastPos).sqrMagnitude;
        float rotDiff = Quaternion.Angle(transform.rotation, lastRot);

        bool moved = posDiff > moveThreshold || rotDiff > 0.1f;

        if (moved)
        {
            lastPos = transform.position;
            lastRot = transform.rotation;
        }

        return moved;
    }

    private void SendTransform()
    {
        int[] targetActors = DingusManager.Instance.GetDingusActorList();

        if (targetActors.Length == 0)
            return;

        object[] data =
        [
                transform.position,
                transform.rotation,
        ];

        PhotonNetwork.RaiseEvent(
                41,
                data,
                new RaiseEventOptions
                {
                        TargetActors = targetActors,
                },
                SendOptions.SendUnreliable
        );
    }
}