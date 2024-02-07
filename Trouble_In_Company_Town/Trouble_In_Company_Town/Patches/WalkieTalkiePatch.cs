using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode;
using Trouble_In_Company_Town.Input;
using UnityEngine;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch(typeof(WalkieTalkie))]
    internal class WalkieTalkiePatch
    {
        public static Color prevColor = Color.green;

        [HarmonyPatch("Start")]
        [HarmonyPostfix]
        public static void StartPatch(WalkieTalkie __instance)
        {
            prevColor = __instance.walkieTalkieLight.color;
        }

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        public static void UpdatePatch(WalkieTalkie __instance)
        {
            if (TownBase.InputActionsInstance.TraitorWalkieChannel.IsPressed() && __instance.isHeld && __instance.playerHeldBy != null && TCTRoundManager.Instance.IsPlayerTraitor(__instance.playerHeldBy)) //Change color to indicate that they are talking to only traitors
            {
                __instance.walkieTalkieLight.color = Color.red;
            }
            else
            {
                __instance.walkieTalkieLight.color = prevColor;
            }
        }
    }
}
