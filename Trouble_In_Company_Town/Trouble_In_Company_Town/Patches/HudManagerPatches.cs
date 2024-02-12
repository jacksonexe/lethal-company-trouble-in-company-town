using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Trouble_In_Company_Town.Gamemode;
using Trouble_In_Company_Town.UI;
using UnityEngine;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HudManagerPatches
    {
        public static TCTRolesUI RolesUI;
        public static TCTKillCooldown KillCooldownUI;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPatch(ref HUDManager __instance)
        {
            KillCooldownUI = new TCTKillCooldown(__instance);
            RolesUI = new TCTRolesUI(__instance);
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void UpdatePatch(ref HUDManager __instance)
        {
            foreach (PlayerControllerB player in StartOfRound.Instance.allPlayerScripts)
            {
                ScanNodeProperties props = player.gameObject.GetComponent<ScanNodeProperties>();
                if (props == null)
                {
                    props = player.gameObject.AddComponent<ScanNodeProperties>();
                    UnityEngine.GameObject.Instantiate(UnityEngine.GameObject.Find("ScanNode")).transform.parent = player.transform;
                    props.headerText = player.name;
                    props.scrapValue = 0;
                    props.requiresLineOfSight = false;
                    props.subText = player.name;
                    props.maxRange = 9999;
                    props.minRange = 0;
                    props.nodeType = 1;
                }
                if (props != null)
                {
                    List<ScanNodeProperties> __nodesOnScreen = Traverse.Create(__instance).Field("nodesOnScreen").GetValue() as List<ScanNodeProperties>;
                    Dictionary<RectTransform, ScanNodeProperties> scanNodes = Traverse.Create(__instance).Field("scanNodes").GetValue() as Dictionary<RectTransform, ScanNodeProperties>;
                    if (TCTRoundManager.Instance.IsRunning && TCTRoundManager.Instance.LocalPlayerIsTraitor() && !__nodesOnScreen.Contains(props) && !scanNodes.ContainsValue(props) && !player.isPlayerDead && !player.disconnectedMidGame && !player.IsLocalPlayer && player.isPlayerControlled && player.isActiveAndEnabled)
                    {
                        __nodesOnScreen.Add(props);
                        Traverse.Create(__instance).Field("nodesOnScreen").SetValue(__nodesOnScreen);
                        MethodInfo AssignNodeToUIElement = __instance.GetType().GetMethod("AssignNodeToUIElement",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                        AssignNodeToUIElement.Invoke(__instance, new object[] { props });
                    }
                    else if((!TCTRoundManager.Instance.IsRunning || !TCTRoundManager.Instance.LocalPlayerIsTraitor() || player.isPlayerDead || player.disconnectedMidGame || !player.isPlayerControlled || !player.isActiveAndEnabled) && __nodesOnScreen.Contains(props))
                    {
                        __nodesOnScreen.Remove(props);
                        Traverse.Create(__instance).Field("nodesOnScreen").SetValue(__nodesOnScreen);
                        MethodInfo UpdateScanNodes = __instance.GetType().GetMethod("NodeIsNotVisible",
                        BindingFlags.NonPublic | BindingFlags.Instance);
                        UpdateScanNodes.Invoke(__instance, new object[] { props, scanNodes.});
                    }
                }
            }

            if (TCTRoundManager.Instance.IsRunning)
            {
                Crewmate role = TCTRoundManager.Instance.GetPlayerRole(StartOfRound.Instance.localPlayerController);
                if (role != null)
                {
                    RolesUI.SetRole(role.GetRoleName(), role.GetRoleColor());
                }
                else
                {
                    RolesUI.SetRole("Preparing", Color.black);
                }
                if (!StartOfRound.Instance.localPlayerController.isPlayerDead && TCTRoundManager.Instance.LocalPlayerIsTraitor())
                {
                    Traitor localPlayer = (Traitor)TCTRoundManager.LocalPlayersRole;
                    if (TCTRoundManager.TraitorKillCooldown != null)
                    {
                        KillCooldownUI.SetTimer((int)(TCTRoundManager.TraitorKillCooldown.Value - DateTime.Now.Subtract(localPlayer.LastKillTime).TotalSeconds));
                    }
                    else
                    {
                        KillCooldownUI.HideTimer();
                    }
                }
            }
            else
            {
                RolesUI.SetRole("Preparing", Color.black);
                KillCooldownUI.HideTimer();
            }
        }

        [HarmonyPatch("SetSpectatingTextToPlayer")]
        [HarmonyPostfix]
        static public void SetSpectatingTextToPlayerPatch(PlayerControllerB playerScript, ref HUDManager __instance)
        {
            if (playerScript == null)
            {
                __instance.spectatingPlayerText.text = "";
            }
            else
            {
                Crewmate role = TCTRoundManager.Instance.GetPlayerRole(playerScript);
                if (role != null)
                {
                    __instance.spectatingPlayerText.text = "(Spectating: " + playerScript.playerUsername + " - " + role.GetRoleName() + ")";
                }
            }
        }

        [HarmonyPatch("UpdateBoxesSpectateUI")]
        [HarmonyPostfix]
        static public void UpdateBoxesSpectateUI(HUDManager __instance)
        {
            Dictionary<Animator, PlayerControllerB> ___spectatingPlayerBoxes = Traverse.Create(__instance).Field("spectatingPlayerBoxes").GetValue() as Dictionary<Animator, PlayerControllerB>;
            PlayerControllerB playerScript;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                playerScript = StartOfRound.Instance.allPlayerScripts[i];
                if (___spectatingPlayerBoxes.Values.Contains(playerScript))
                {
                    GameObject gameObject = ___spectatingPlayerBoxes.FirstOrDefault((KeyValuePair<Animator, PlayerControllerB> x) => x.Value == playerScript).Key.gameObject;
                    Crewmate role = TCTRoundManager.Instance.GetPlayerRole(playerScript);
                    if (role != null)
                    {
                        gameObject.GetComponentInChildren<TextMeshProUGUI>().text = playerScript.playerUsername + " - " + role.GetRoleName();
                    }
                }
            }
        }

        [HarmonyPatch("FillEndGameStats")]
        [HarmonyPrefix]
        public static void FillEndGameStatsPatch(object[] __args)
        {
            EndOfGameStats stats = (EndOfGameStats)__args[0];
            for (int i = 0; i < stats.allPlayerStats.Length; i++)
            {
                PlayerStats playerStats = stats.allPlayerStats[i];
                PlayerControllerB player = StartOfRound.Instance.allPlayerScripts[i];
                Crewmate role = TCTRoundManager.Instance.GetPlayerRole(player);
                if (role != null)
                {
                    playerStats.playerNotes.Clear();
                    playerStats.playerNotes.Add("Role: " + role.GetRoleName());
                }
            }
        }
    }
}
