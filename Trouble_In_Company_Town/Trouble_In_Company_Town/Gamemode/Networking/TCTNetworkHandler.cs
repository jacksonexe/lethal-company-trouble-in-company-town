using BepInEx.Logging;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode.Roles;
using Trouble_In_Company_Town.Gamemode.Sabotages;
using Unity.Netcode;
using UnityEngine;

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
            if (mls == null)
            {
                mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");
            }

            base.OnNetworkSpawn();
        }

        private ClientRpcParams createBroadcastConfig()
        {
            List<ulong> clients = new List<ulong>();
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                clients.Add(StartOfRound.Instance.allPlayerScripts[i].playerClientId);
            }
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = clients.ToArray(),
                }
            };
            return clientRpcParams;
        }

        [ServerRpc(RequireOwnership = false)]
        public void sendRoleServerRpc(ulong id, string role)
        {
            if (mls == null)
            {
                mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");
            }
            mls.LogDebug("Sending role " + role + " to " + id);
            ClientRpcParams clientRpcParams = createBroadcastConfig();
            sendRoleClientRpc(id, role, clientRpcParams);
        }

        [ClientRpc]
        public void sendRoleClientRpc(ulong id, string role, ClientRpcParams clientRpcParams = default)
        {
            if (mls == null)
            {
                mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");
            }
            mls.LogDebug("recieved role " + role + " to " + id);
            PlayerControllerB player = null;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == id)
                {
                    player = StartOfRound.Instance.allPlayerScripts[i];
                    break;
                }
            }
            if (player != null)
            {
                Crewmate crewmate = RoleFactory.GetRole(role, id, player);
                if (id == StartOfRound.Instance.localPlayerController.playerClientId)
                {
                    TCTRoundManager.Instance.registerLocalPlayersRole(crewmate);
                }
                else
                {
                    TCTRoundManager.Instance.registerOtherPlayersRole(crewmate);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void NotifyRoundOverServerRpc(string winner, string traitors, bool warn, Faction faction)
        {
            if (mls == null)
            {
                mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");
            }
            mls.LogDebug("Round ended, winner " + winner + " " + traitors);

            HUDManager.Instance.AddTextToChatOnServer("Round over winner is: " + winner + " " + traitors);
            ClientRpcParams clientRpcParams = createBroadcastConfig();
            NotifyRoundOverClientRpc(winner, traitors, warn, faction, clientRpcParams);
        }

        [ClientRpc]
        public void NotifyRoundOverClientRpc(string winner, string traitors, bool warn, Faction faction, ClientRpcParams clientRpcParams = default)
        {
            if (mls == null)
            {
                mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");
            }
            mls.LogDebug("Round ended received, winner " + winner + " " + traitors);
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            Utilities.DisplayTips("Round Over ", "Winner: " + winner + "\nTraitors: " + traitors, warn);
            if (!player.isPlayerDead)
            {
                StartOfRound.Instance.ForcePlayerIntoShip();
                Crewmate crew = TCTRoundManager.Instance.GetPlayerRole(StartOfRound.Instance.localPlayerController);
                if (crew != null && faction != crew.Faction)
                {
                    StartOfRound.Instance.localPlayerController.KillPlayer(new UnityEngine.Vector3(0, 0, 0), false);
                }
            }
            EnemyAI[] array = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
            for (int i = 0; i < array.Length; i++)
            {
                array[i].EnableEnemyMesh(enable: true);
            }

            if (!StartOfRound.Instance.IsHost && !StartOfRound.Instance.IsServer)
            {
                TCTRoundManager.Instance.UpdateClientScore(faction);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void NotifyRoundStartServerRpc(int numTraitors)
        {
            ClientRpcParams clientRpcParams = createBroadcastConfig();
            NotifyRoundStartClientRpc(numTraitors, clientRpcParams);
        }

        [ClientRpc]
        public void NotifyRoundStartClientRpc(int numTraitors, ClientRpcParams clientRpcParans = default)
        {
            HUDManager.Instance.planetIntroAnimator.SetTrigger("introAnimation");
            HUDManager.Instance.planetInfoHeaderText.text = "There are traitors among us";
            HUDManager.Instance.planetInfoSummaryText.text = "There are " + numTraitors + " traitors";
            HUDManager.Instance.planetRiskLevelText.text = "Company Town is in Trouble";
            if (!StartOfRound.Instance.IsHost && !StartOfRound.Instance.IsServer) {
                TCTRoundManager.Instance.resetRound();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void RequestSabotageServerRpc(ulong clientId, string saboType)
        {
            mls.LogDebug("received request to start a sabo " + saboType.ToString());
            TraitorSabotageManager.Instance.TriggerSabo(clientId, saboType);
        }

        [ServerRpc(RequireOwnership = false)]
        public void NotifyOfSabotageServerRpc(string saboType)
        {
            mls.LogDebug("sending request to notify of sabo " + saboType.ToString());
            ClientRpcParams clientRpcParams = createBroadcastConfig();
            NotifyOfSabotageClientRpc(saboType, clientRpcParams);
        }

        [ClientRpc]
        public void NotifyOfSabotageClientRpc(string saboType, ClientRpcParams clientRpcParans = default)
        {
            mls.LogDebug("recieved request to notify of sabo " + saboType.ToString());
            if (TCTRoundManager.Instance.LocalPlayerIsTraitor())
            {
                TraitorSabotageManager.Instance.NotifyTraitorsOfSabo(saboType);
            }
            TraitorSabotageManager.Instance.HandleOnSabotageTypes(saboType);
        }

        [ServerRpc(RequireOwnership = false)]
        public void NotifyOfSabotageOnCooldownServerRpc(ulong clientId, string sabo)
        {
            mls.LogDebug("sending info to " + clientId + " that sabo is on cooldown");
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { clientId }
                }
            };
            NotifyOfSabotageOnCooldownClientRpc(sabo, clientRpcParams);
        }

        [ClientRpc]
        public void NotifyOfSabotageOnCooldownClientRpc(string saboType, ClientRpcParams clientRpcParans = default)
        {
            TraitorSabotageManager.Instance.NotifyOfSaboCooldown(saboType);
        }

        [ServerRpc(RequireOwnership = false)]
        public void NotifyOfSabotageEndServerRpc(string saboType)
        {
            mls.LogDebug("sending request to notify of sabo end " + saboType.ToString());
            ClientRpcParams clientRpcParams = createBroadcastConfig();
            NotifyOfSabotageEndClientRpc(saboType, clientRpcParams);
        }

        [ClientRpc]
        public void NotifyOfSabotageEndClientRpc(string saboType, ClientRpcParams clientRpcParans = default)
        {
            mls.LogDebug("recieved request to notify of sabo end " + saboType.ToString());
            TraitorSabotageManager.Instance.HandleOffSabotageTypes(saboType);
        }

        [ServerRpc(RequireOwnership =false)]
        public void RequestItemSpawnServerRpc(ulong clientId)
        {
            SpawnWeaponSabotage.SpawnItemForClient(clientId);
        }
    }
}
