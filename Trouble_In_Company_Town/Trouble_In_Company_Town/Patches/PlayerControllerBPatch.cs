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
        private static float DefaultIntensity;
        private static float DefaultRange;
        private static float DefaultShadowStrength;
        private static LightShadows DefaultShadows;
        private static LightShape DefaultShape;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePatch(PlayerControllerB __instance)
        {
            DefaultIntensity = __instance.nightVision.intensity;
            DefaultRange = __instance.nightVision.range;
            DefaultShadowStrength = __instance.nightVision.shadowStrength;
            DefaultShadows = __instance.nightVision.shadows;
            DefaultShape = __instance.nightVision.shape;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void infiniteSprintPatch(ref float  ___sprintMeter, PlayerControllerB __instance)
        {
            //For debug
            ___sprintMeter = 1f;
            if(TCTRoundManager.Instance.LocalPlayerIsTraitor() && __instance.isInsideFactory && TCTRoundManager.Instance.IsRunning && !TCTRoundManager.Instance.IsRoundEnding)
            {
                __instance.nightVision.enabled = true;
                __instance.nightVision.intensity = 10000f;
                __instance.nightVision.range = 100000f;
                __instance.nightVision.shadowStrength = 0f;
                __instance.nightVision.shadows = (LightShadows)0;
                __instance.nightVision.shape = (LightShape)2;
            }
            else
            {
                __instance.nightVision.enabled = false;
                __instance.nightVision.intensity = DefaultIntensity;
                __instance.nightVision.range = DefaultRange;
                __instance.nightVision.shadowStrength = DefaultShadowStrength;
                __instance.nightVision.shadows = DefaultShadows;
                __instance.nightVision.shape = DefaultShape;
            }
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
        public static bool KillPlayerPatch(Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, int deathAnimation)
        {
            if ((causeOfDeath == CauseOfDeath.Mauling || causeOfDeath == CauseOfDeath.Strangulation || causeOfDeath == CauseOfDeath.Mauling || causeOfDeath == CauseOfDeath.Crushing) && TCTRoundManager.Instance.LocalPlayerIsTraitor())
            {
                return false;
            }
            return true;
        }
            
    }
}
