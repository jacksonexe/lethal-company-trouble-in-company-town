using GameNetcodeStuff;
using HarmonyLib;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using LethalConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Trouble_In_Company_Town.Gamemode;
using Trouble_In_Company_Town.UI;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePatch(RoundManager __instance)
        {
            __instance.scrapAmountMultiplier = 10f;
            __instance.mapSizeMultiplier = 5f;
            TimeOfDay.Instance.globalTimeSpeedMultiplier = 0.2f;
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            if (terminal != null)
            {
                terminal.groupCredits = 500;
            }
            TimeOfDay.Instance.daysUntilDeadline = 1;
        }
    }
}
