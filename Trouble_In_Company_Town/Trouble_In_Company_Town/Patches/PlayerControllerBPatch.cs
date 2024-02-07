using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private static CauseOfDeath LastCauseOfDeath;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void infiniteSprintPatch(ref float  ___sprintMeter)
        {
            //For debug
            ___sprintMeter = 1f;
        }

        [HarmonyPatch("PlayerIsHearingOthersThroughWalkieTalkie")]
        [HarmonyPostfix]
        private static bool PlayerIsHearingOthersThroughWalkieTalkiePatch(bool __ret, PlayerControllerB playerScript, PlayerControllerB __instance)
        {
            if (playerScript == null)
            {
                playerScript = __instance;
            }
            if (!playerScript.holdingWalkieTalkie)
            {
                return false;
            }
            Crewmate transmitRole = TCTRoundManager.Instance.GetPlayerRole(playerScript);
            for (int i = 0; i < WalkieTalkie.allWalkieTalkies.Count; i++)
            {
                if (WalkieTalkie.allWalkieTalkies[i].clientIsHoldingAndSpeakingIntoThis && WalkieTalkie.allWalkieTalkies[i] != playerScript.currentlyHeldObjectServer as WalkieTalkie)
                {
                    PlayerControllerB owner = WalkieTalkie.allWalkieTalkies[i].playerHeldBy;
                    Crewmate role = TCTRoundManager.Instance.GetPlayerRole(owner);
                    return owner == null || transmitRole == null || role == null || transmitRole.Faction == Faction.CREWMATE || (TCTRoundManager.TraitorsTransmittingOnTraitorChannel.Contains(transmitRole) && role.Faction == Faction.TRAITOR); //Only transmit to traitors
                }
            }
            return false;
        }

        [HarmonyPatch("KillPlayer")]
        [HarmonyPrefix]
        public static void KillPlayerPatch(Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, int deathAnimation)
        {
            LastCauseOfDeath = causeOfDeath;
        }
            
        [HarmonyPatch("AllowPlayerDeath")]
        [HarmonyPostfix]
        private static bool AllowPlayerDeathPatch(bool __ret)
        {
            if ((LastCauseOfDeath == CauseOfDeath.Mauling || LastCauseOfDeath == CauseOfDeath.Strangulation || LastCauseOfDeath == CauseOfDeath.Mauling || LastCauseOfDeath == CauseOfDeath.Crushing) && TCTRoundManager.Instance.LocalPlayerIsTraitor())
            {
                return false;
            }
            return __ret;
        }
    }
}
