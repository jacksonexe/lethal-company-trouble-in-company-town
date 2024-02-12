﻿using BepInEx.Logging;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode.Roles;
using Trouble_In_Company_Town.Gamemode.Sabotages;
using Trouble_In_Company_Town.UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

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

            if (!StartOfRound.Instance.IsHost && !StartOfRound.Instance.IsServer)
            {
                TCTRoundManager.Instance.UpdateClientScore(faction);
                TCTRoundManager.Instance.SetRoundOver();
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
                TCTRoundManager.Instance.SetRouteStarted();
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

        [ServerRpc(RequireOwnership = false)]
        public void RequestLandmineSpawnServerRpc(ulong clientId)
        {
            SpawnLandmineSabotage.SpawnItemForClient(clientId);
        }

        [ServerRpc(RequireOwnership = false)]
        public void GiveTraitorsWalkieTalkieServerRpc(ulong clientId)
        {
            PlayerControllerB player = null;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == clientId)
                {
                    player = StartOfRound.Instance.allPlayerScripts[i];
                }
            }
            Vector3 spawnPos = player.transform.position;

            if (!player.isPlayerDead)
            {
                spawnPos.y -= 5f;
                GameObject obj = UnityEngine.Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[14].spawnPrefab, spawnPos, Quaternion.identity);
                obj.GetComponent<GrabbableObject>().fallTime = 0f;

                obj.AddComponent<ScanNodeProperties>().scrapValue = 0;
                obj.GetComponent<GrabbableObject>().SetScrapValue(0);
                obj.GetComponent<NetworkObject>().Spawn();

                EquipRadioClientRpc(obj.GetComponent<NetworkObject>().NetworkObjectId, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new ulong[] { clientId },
                    }
                });
            }
        }

        [ClientRpc]
        public void EquipRadioClientRpc(ulong objectId, ClientRpcParams clientRpcParans = default)
        {
            GrabbableObject radio = null;
            GrabbableObject[] array = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
            for (int i = 0; i < array.Length; i++)
            {
                if (array[i].NetworkObjectId == objectId)
                {
                    radio = (GrabbableObject)array[i];
                }
            }
            mls.LogDebug("Found radio " + radio + " for id " + objectId);
            if (radio != null)
            {
                InteractTrigger trigger = radio.gameObject.GetComponent<InteractTrigger>();
                if (trigger != null)
                {
                    trigger.Interact(StartOfRound.Instance.localPlayerController.transform);
                    trigger.onInteract.Invoke(StartOfRound.Instance.localPlayerController);
                }
                radio.GrabItem();
                radio.PocketItem();
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void SyncTraitorChannelServerRpc(bool joinedChannel, ulong playerId)
        {
            ClientRpcParams clientRpcParams = createBroadcastConfig();
            SyncTraitorChannelClientRpc(joinedChannel, playerId, Faction.TRAITOR, clientRpcParams);
        }

        [ClientRpc]
        public void SyncTraitorChannelClientRpc(bool joinedChannel, ulong clientId, Faction faction, ClientRpcParams clientRpcParans = default)
        {
            PlayerControllerB player = null;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == clientId)
                {
                    player = StartOfRound.Instance.allPlayerScripts[i];
                    break;
                }
            }
            if (player != null)
            {
                Crewmate crewmate = RoleFactory.GetRoleByFaction(faction, clientId, player);
                TCTRoundManager.Instance.JoinTraitorWalkieChannel(joinedChannel, crewmate);
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void SyncTraitorKillServerRpc(ulong killedId, ulong killerId, DateTime performedAt)
        {
            PlayerControllerB killer = null;
            PlayerControllerB killed = null;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == killedId)
                {
                    killed = StartOfRound.Instance.allPlayerScripts[i];
                }
                if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == killerId)
                {
                    killer = StartOfRound.Instance.allPlayerScripts[i];
                }
            }
            EquipMaskClientRpc(new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { killed.playerClientId },
                }
            });
            ClientRpcParams clientRpcParams = createBroadcastConfig();
            SyncTraitorKillClientRpc(killer.playerClientId, performedAt, clientRpcParams);
        }

        [ClientRpc]
        internal void EquipMaskClientRpc(ClientRpcParams clientRpcParans = default)
        {
            StartOfRound.Instance.localPlayerController.KillPlayer(Vector2.zero, true, CauseOfDeath.Suffocation, 5);
        }

        [ClientRpc]
        internal void SyncTraitorKillClientRpc(ulong killerId, DateTime performedAt, ClientRpcParams clientRpcParans = default)
        {
            Crewmate c = TCTRoundManager.Instance.GetPlayerRoleById(killerId);
            if(c != null && c.Faction == Faction.TRAITOR)
            {
                (c as Traitor).LastKillTime = performedAt;
            }
        }

        [ServerRpc(RequireOwnership = false)]
        internal void InitateKillRequestServerRpc(ulong playerId)
        {
            TCTRoundManager.Instance.HandlePlayerKill(playerId);
        }

        [ServerRpc(RequireOwnership = false)]
        internal void NotifyOfRoundEndingServerRpc()
        {
            ClientRpcParams clientRpcParams = createBroadcastConfig();
            NotifyOfRoundEndingClientRpc(clientRpcParams);
        }

        [ClientRpc]
        internal void NotifyOfRoundEndingClientRpc(ClientRpcParams clientRpcParans = default)
        {
            StartMatchLever startMatchLever = GameObject.FindObjectOfType<StartMatchLever>();
            startMatchLever.triggerScript.animationString = "SA_PushLeverBack";
            startMatchLever.leverHasBeenPulled = false;
            startMatchLever.triggerScript.interactable = false;
            startMatchLever.leverAnimatorObject.SetBool("pullLever", false);
            StartOfRound.Instance.SetShipDoorsClosed(true);
        }
    }
}
