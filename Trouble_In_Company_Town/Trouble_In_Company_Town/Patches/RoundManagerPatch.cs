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
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static TCTRoundManager roundManager;
        [HarmonyPatch("LoadNewLevelWait")]
        [HarmonyPostfix]
        static void LoadNewLevelWaitPatch(ref StartOfRound ___playersManager)
        {
            roundManager = new TCTRoundManager(___playersManager, GameNetworkManager.Instance.isHostingGame);
        }
    }
}
