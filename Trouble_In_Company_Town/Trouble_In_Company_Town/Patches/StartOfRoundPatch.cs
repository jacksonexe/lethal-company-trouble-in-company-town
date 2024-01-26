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
            TCTRoundManager.Instance.startRound(__instance, __instance.IsHost || __instance.IsServer);
            if(__instance.IsHost || __instance.IsServer) 
            {
                Vector3 spawnPos = GameNetworkManager.Instance.localPlayerController.transform.position;

                if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
                {
                    spawnPos = GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.transform.position;
                }
                //This is for debug
                GameObject obj = UnityEngine.Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[52].spawnPrefab, spawnPos, Quaternion.identity);
                obj.GetComponent<GrabbableObject>().fallTime = 0f;

                obj.AddComponent<ScanNodeProperties>().scrapValue = 500;
                obj.GetComponent<GrabbableObject>().SetScrapValue(500);
                obj.GetComponent<NetworkObject>().Spawn();
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
                        TCTRoundManager.Instance.checkIfRoundOver(__instance);
                    }
                    else if (TCTRoundManager.Instance.IsRoundEnding && !TCTRoundManager.Instance.IsRoundOver)
                    {
                        TCTRoundManager.Instance.SetRoundOver();
                        StartOfRound.Instance.EndGameServerRpc(0);
                        StartOfRound.Instance.SetDoorsClosedServerRpc(true);
                       
                    }
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
            StartOfRound.Instance.ResetShip();
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            if (terminal != null)
            {
                terminal.groupCredits = 500;
            }
            __instance.screenLevelDescription.SetText("Traitor Wins: " + TCTRoundManager.NumTraitorWins + "\nCrewmate Wins: " + TCTRoundManager.NumCrewmateWins);
        }
    }
}
