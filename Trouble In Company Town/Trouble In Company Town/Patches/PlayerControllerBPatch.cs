using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        private Crewmate role;

        [HarmonyPatch("Update")]
        [HarmonyPostfix]
        static void infiniteSprintPatch(ref float  ___sprintMeter, ref string ___playerUsername)
        {
            ___sprintMeter = 1f;
            ___playerUsername = "With stupid ->";
        }
    }
}
