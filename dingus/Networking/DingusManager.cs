using System.Collections.Generic;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace dingus.Networking
{
    public class DingusManager : MonoBehaviourPunCallbacks
    {
        public static DingusManager Instance;

        private readonly Dictionary<int, GameObject> remoteDinguses = new();
        private readonly List<int>                   dingusPlayers  = [];

        private void Awake()
        {
            Instance = this;

            if (PhotonNetwork.InRoom)
                ScanPlayersForDingus();
        }

        private void ScanPlayersForDingus()
        {
            foreach (Player p in PhotonNetwork.PlayerList)
            {
                if (p == PhotonNetwork.LocalPlayer) continue;
                if (DingusNetworkManager.PlayerHasDingus(p))
                {
                    if (!dingusPlayers.Contains(p.ActorNumber))
                        dingusPlayers.Add(p.ActorNumber);
                    SpawnRemoteDingus(p);
                }
            }
        }

        public override void OnJoinedRoom()
        {
            ScanPlayersForDingus();
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            if (newPlayer == PhotonNetwork.LocalPlayer) return;

            if (DingusNetworkManager.PlayerHasDingus(newPlayer))
            {
                if (!dingusPlayers.Contains(newPlayer.ActorNumber))
                    dingusPlayers.Add(newPlayer.ActorNumber);

                SpawnRemoteDingus(newPlayer);
            }
        }

        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            bool hasDingus = DingusNetworkManager.PlayerHasDingus(targetPlayer);

            if (hasDingus)
            {
                if (!dingusPlayers.Contains(targetPlayer.ActorNumber))
                    dingusPlayers.Add(targetPlayer.ActorNumber);

                SpawnRemoteDingus(targetPlayer);
            }
            else
            {
                dingusPlayers.Remove(targetPlayer.ActorNumber);

                if (remoteDinguses.TryGetValue(targetPlayer.ActorNumber, out GameObject d))
                {
                    Destroy(d);
                    remoteDinguses.Remove(targetPlayer.ActorNumber);
                }
            }
        }

        public override void OnPlayerLeftRoom(Player otherPlayer)
        {
            dingusPlayers.Remove(otherPlayer.ActorNumber);

            if (!remoteDinguses.TryGetValue(otherPlayer.ActorNumber, out GameObject dingus))
                return;

            Destroy(dingus);
            remoteDinguses.Remove(otherPlayer.ActorNumber);
        }

        public int[] GetDingusActorList() => dingusPlayers.ToArray();

        private void SpawnRemoteDingus(Player player)
        {
            if (player == PhotonNetwork.LocalPlayer) return;

            if (remoteDinguses.ContainsKey(player.ActorNumber))
                return;

            GameObject dingus = Instantiate(Plugin.dingusPrefab);

            foreach (AudioSource src in dingus.GetComponentsInChildren<AudioSource>())
                src.enabled = false;

            Rigidbody rb = dingus.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity  = false;
            }

            Collider coll = dingus.GetComponent<Collider>();
            if (coll != null) coll.enabled = false;

            dingus.AddComponent<RemoteDingus>();
            remoteDinguses[player.ActorNumber] = dingus;
        }

        public bool TryGetRemoteDingus(int actorNumber, out GameObject dingus) =>
            remoteDinguses.TryGetValue(actorNumber, out dingus);

        public void UpdateRemoteDingus(int actorNumber, Vector3 pos, Quaternion rot)
        {
            if (!remoteDinguses.TryGetValue(actorNumber, out GameObject dingus))
                return;

            dingus.transform.position = pos;
            dingus.transform.rotation = rot;
        }

        public override void OnLeftRoom()
        {
            foreach (GameObject dingus in remoteDinguses.Values)
                Destroy(dingus);

            remoteDinguses.Clear();
            dingusPlayers.Clear();
        }
    }
}
