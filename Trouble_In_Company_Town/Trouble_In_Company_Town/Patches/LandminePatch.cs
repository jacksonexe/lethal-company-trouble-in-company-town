using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode;
using UnityEngine;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch(typeof(Landmine))]
    internal class LandminePatch
    {
        //Traitor landmine immunity disabled for now
        /*[HarmonyPatch("OnTriggerEnter")]
        [HarmonyPrefix]
        public static bool OnTriggerEnterPatch(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                return !TCTRoundManager.Instance.IsPlayerTraitor(component);
            }
            return true;
        }

        [HarmonyPatch("OnTriggerExit")]
        [HarmonyPrefix]
        public static bool OnTriggerExitPatch(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                return !TCTRoundManager.Instance.IsPlayerTraitor(component);
            }
            return true;
        }

        [HarmonyPatch("TriggerMineOnLocalClientByExiting")]
        [HarmonyPrefix]
        public static bool TriggerMineOnLocalClientByExitingPatch()
        {
            return !TCTRoundManager.Instance.LocalPlayerIsTraitor();
        }*/
    }
}
