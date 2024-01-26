using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Trouble_In_Company_Town.Gamemode
{
    internal class TCTNetworkHandler : NetworkBehaviour
    {
        public static TCTNetworkHandler Instance { get; private set; }
        internal ManualLogSource mls;

        public override void OnNetworkSpawn()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;
            mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");

            base.OnNetworkSpawn();
        }

        [ClientRpc]
        public void EventClientRpc(int id)
        {
            mls.LogInfo("Event recieved for " + id);
        }

        [ServerRpc(RequireOwnership = false)]
        public void EventServerRPC(int id)
        {
            EventServerRPC(id);
        }
    }
}
