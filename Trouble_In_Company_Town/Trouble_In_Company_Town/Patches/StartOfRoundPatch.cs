using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode;
using static Steamworks.InventoryItem;
using Unity.Netcode;
using UnityEngine;
using System.Data;
using Trouble_In_Company_Town.UI;
using static UnityEngine.GraphicsBuffer;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        [HarmonyPatch("OnShipLandedMiscEvents")]
        [HarmonyPostfix]
        static void OnShipLandedMiscEventsPatch(StartOfRound __instance)
        {
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            if (terminal != null)
            {
                terminal.groupCredits = 500;
            }
            if (__instance.IsHost || __instance.IsServer) 
            {
                TCTRoundManager.Instance.StartRound(__instance);
                /*Vector3 spawnPos = GameNetworkManager.Instance.localPlayerController.transform.position;

                if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                {
                    spawnPos = GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.transform.position;
                }
                //This is for debug
                GameObject obj = UnityEngine.Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[52].spawnPrefab, spawnPos, Quaternion.identity);
                obj.GetComponent<GrabbableObject>().fallTime = 0f;

                obj.AddComponent<ScanNodeProperties>().scrapValue = 500;
                obj.GetComponent<GrabbableObject>().SetScrapValue(500);
                obj.GetComponent<NetworkObject>().Spawn();*/
            }
        }
        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void UpdatePatch(StartOfRound __instance)
        {
            if (__instance.IsHost || __instance.IsServer)
            {
                if (!TCTRoundManager.Instance.IsInitializing)
                {
                    if (TCTRoundManager.Instance.IsRunning && !TCTRoundManager.Instance.IsRoundEnding)
                    {
                        StartMatchLever startMatchLever = GameObject.FindObjectOfType<StartMatchLever>();
                        startMatchLever.triggerScript.enabled = false;
                        TCTRoundManager.Instance.checkIfRoundOver(__instance);
                    }
                    else if (TCTRoundManager.Instance.IsRoundEnding && !TCTRoundManager.Instance.IsRoundOver)
                    {
                        TCTRoundManager.Instance.SetRoundOver();
                        TCTNetworkHandler.Instance.NotifyOfRoundEndingServerRpc();
                        StartOfRound.Instance.EndGameServerRpc(0);
                    }

                    if (TCTRoundManager.Instance.IsRunning && __instance.shipIsLeaving && TimeOfDay.Instance.currentDayTime / TimeOfDay.Instance.totalTime >= TimeOfDay.Instance.shipLeaveAutomaticallyTime)
                    {
                        TCTRoundManager.Instance.HandleShipLeaveMidnight(__instance);
                    }
                }
            }
            if (__instance.inShipPhase)
            {
                StartMatchLever startMatchLever = GameObject.FindObjectOfType<StartMatchLever>();
                int numConnected = 0;
                for (int i = 0; i < __instance.allPlayerScripts.Length; i++)
                {
                    PlayerControllerB controller = __instance.allPlayerScripts[i];
                    if (controller.isActiveAndEnabled && (controller.isPlayerControlled || controller.isPlayerDead))
                    {
                        numConnected++;
                    }
                }
                if (numConnected < 2)
                {
                    startMatchLever.triggerScript.enabled = false;
                    startMatchLever.triggerScript.hoverTip = "At least 2 players are required";
                    startMatchLever.triggerScript.disableTriggerMesh = true;
                }
                else
                {
                    startMatchLever.triggerScript.enabled = true;
                    startMatchLever.triggerScript.hoverTip = "Start Round";
                    startMatchLever.triggerScript.disableTriggerMesh = false;
                }
            }
        }

        [HarmonyPatch("SetMapScreenInfoToCurrentLevel")]
        [HarmonyPostfix]
        static void SetMapScreenInfoToCurrentLevelPatch(StartOfRound __instance)
        {
            __instance.screenLevelDescription.SetText("Traitor Wins: " + TCTRoundManager.NumTraitorWins + "\nCrewmate Wins: " + TCTRoundManager.NumCrewmateWins);
        }

        [HarmonyPatch("PassTimeToNextDay")]
        [HarmonyPostfix]
        static void PassTimeToNextDayPatch(StartOfRound __instance)
        {
            int levelSelection = StartOfRound.Instance.currentLevelID;
            StartOfRound.Instance.ResetShip();
            __instance.ChangeLevel(levelSelection);
            __instance.ChangePlanet();
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            if (terminal != null)
            {
                terminal.groupCredits = 500;
            }
            TimeOfDay.Instance.daysUntilDeadline = 1;
            __instance.screenLevelDescription.SetText("Traitor Wins: " + TCTRoundManager.NumTraitorWins + "\nCrewmate Wins: " + TCTRoundManager.NumCrewmateWins);
            StartMatchLever startMatchLever = GameObject.FindObjectOfType<StartMatchLever>();
            startMatchLever.triggerScript.enabled = true;
        }

    }
}
