using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using Trouble_In_Company_Town.Gamemode;
using UnityEngine;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HudManagerPatches
    {
        [HarmonyPatch("SetSpectatingTextToPlayer")]
        [HarmonyPostfix]
        static public void SetSpectatingTextToPlayerPatch(PlayerControllerB playerScript, HUDManager __instance)
        {
            if (playerScript == null)
            {
                __instance.spectatingPlayerText.text = "";
            }
            else
            {
                Crewmate role = TCTRoundManager.Instance.GetPlayerRole(playerScript.playerClientId);
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
                    Crewmate role = TCTRoundManager.Instance.GetPlayerRole(playerScript.playerClientId);
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
                Crewmate role = TCTRoundManager.Instance.GetPlayerRole(player.playerClientId);
                if (role != null)
                {
                    playerStats.playerNotes.Clear();
                    playerStats.playerNotes.Add("Role: " + role.GetRoleName());
                }
            }
        }
    }
}
