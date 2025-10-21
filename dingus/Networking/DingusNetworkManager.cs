using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;

namespace dingus.Networking
{
    public static class DingusNetworkManager
    {
        private const string DINGUS_KEY = "Dingus";

        public static void SetHasDingus()
        {
            Hashtable props = new Hashtable
            {
                    { DINGUS_KEY, true },
            };

            PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        }

        public static bool PlayerHasDingus(Player player) =>
                player.CustomProperties.TryGetValue(DINGUS_KEY, out object value) && value is bool b && b;
    }
}