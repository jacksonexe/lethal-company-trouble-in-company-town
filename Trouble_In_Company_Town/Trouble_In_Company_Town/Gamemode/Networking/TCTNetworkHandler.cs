﻿using BepInEx.Logging;
using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode.Roles;
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

        [ServerRpc(RequireOwnership =false)]
        public void sendRoleServerRpc(ulong id, string role)
        {
            if (mls == null)
            {
                mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");
            }
            mls.LogDebug("Sending role " + role + " to " + id);
            sendRoleClientRpc(id, role);
        }

        [ClientRpc]
        public void sendRoleClientRpc(ulong id, string role)
        {
            if(mls == null)
            {
                mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");
            }
            mls.LogDebug("recieved role " + role + " to " + id);
            PlayerControllerB player = null;
            for(int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
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
                EnemyAI[] array = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
                if (crewmate.Faction == Faction.TRAITOR) //Traitors are immune to monsters
                {
                    for (int i = 0; i < array.Length; i++)
                    {
                        array[i].EnableEnemyMesh(enable: false);
                    }
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
            NotifyRoundOverClientRpc(winner, traitors, warn, faction);
        }

        [ClientRpc]
        public void NotifyRoundOverClientRpc(string winner, string traitors, bool warn, Faction faction)
        {
            if (mls == null)
            {
                mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");
            }
            mls.LogDebug("Round ended received, winner " + winner + " " + traitors);
            PlayerControllerB player = GameNetworkManager.Instance.localPlayerController;
            HUDManager.Instance.DisplayTip("Round Over ", "Winner: " + winner + " Traitors: " + traitors, warn);
            if(!player.isPlayerDead) {
                StartOfRound.Instance.ForcePlayerIntoShip();
                Crewmate crew = TCTRoundManager.Instance.GetPlayerRole(StartOfRound.Instance.localPlayerController.playerClientId);
                if (crew != null && faction != crew.Faction)
                {
                    StartOfRound.Instance.localPlayerController.KillPlayer(new UnityEngine.Vector3(0,0,0), false);
                }
            }
            EnemyAI[] array = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
            for (int i = 0; i < array.Length; i++)
            {
                array[i].EnableEnemyMesh(enable: true);
            }

            if (StartOfRound.Instance.IsClient)
            {
                TCTRoundManager.Instance.UpdateClientScore(faction);
            }
        }
    }
}
