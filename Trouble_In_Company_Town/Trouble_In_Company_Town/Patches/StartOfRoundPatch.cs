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
            if (TownBase.RolePrefab != null || TownBase.KillCooldownPrefab != null)
            {
                GameObject gameObject = ((Component)((Component)GameObject.Find("Systems").gameObject.transform.Find("UI")).gameObject.transform.Find("Canvas")).gameObject;

                if (TownBase.RolePrefab != null)
                {
                    TownBase.RolePrefab = TownBase.bundle.LoadAsset<GameObject>("Assets/TCAssets/RoleUI.prefab");
                    TownBase.RolesUI = UnityEngine.Object.Instantiate(TownBase.RolePrefab, gameObject.transform).AddComponent<TCTRolesUI>();
                }

                if (TownBase.KillCooldownPrefab != null)
                {
                    TownBase.KillCooldownPrefab = TownBase.bundle.LoadAsset<GameObject>("Assets/TCAssets/KillCooldownUI.prefab");
                    TownBase.KillCooldownUI = UnityEngine.Object.Instantiate(TownBase.KillCooldownPrefab, gameObject.transform).AddComponent<TCTKillCooldown>();
                }
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
                        TCTRoundManager.Instance.checkIfRoundOver(__instance);
                    }
                    else if (TCTRoundManager.Instance.IsRoundEnding && !TCTRoundManager.Instance.IsRoundOver)
                    {
                        TCTRoundManager.Instance.SetRoundOver();
                        StartOfRound.Instance.EndGameServerRpc(0);
                        StartOfRound.Instance.SetDoorsClosedServerRpc(true);
                    }
                    if (TownBase.KillCooldownUI != null)
                    {
                        if (TCTRoundManager.Instance.IsRunning && !__instance.localPlayerController.isPlayerDead && TCTRoundManager.Instance.LocalPlayerIsTraitor())
                        {
                            Traitor localPlayer = (Traitor)TCTRoundManager.LocalPlayersRole;
                            if (TCTRoundManager.TraitorKillCooldown != null)
                            {
                                TownBase.KillCooldownUI.Show(true);
                                TownBase.KillCooldownUI.SetTimer((int)(TCTRoundManager.TraitorKillCooldown.Value - DateTime.Now.Subtract(localPlayer.LastKillTime).TotalSeconds));
                            }
                            else
                            {
                                TownBase.KillCooldownUI.Show(false);
                            }
                        }
                        else if (TownBase.KillCooldownUI != null)
                        {
                            TownBase.KillCooldownUI.Show(false);
                        }
                    }

                    if (TCTRoundManager.Instance.IsRunning && __instance.shipIsLeaving && TimeOfDay.Instance.currentDayTime / TimeOfDay.Instance.totalTime >= TimeOfDay.Instance.shipLeaveAutomaticallyTime)
                    {
                        TCTRoundManager.Instance.HandleShipLeaveMidnight(__instance);
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
        }

    }
}
