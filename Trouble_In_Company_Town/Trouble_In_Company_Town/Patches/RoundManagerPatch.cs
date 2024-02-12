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

        public static GameObject landmine;

        public static GameObject turret;

        [HarmonyPatch("Awake")]
        [HarmonyPostfix]
        public static void AwakePatch(RoundManager __instance)
        {
            __instance.scrapAmountMultiplier = 10f;
            __instance.mapSizeMultiplier = 5f;
            Terminal terminal = UnityEngine.Object.FindObjectOfType<Terminal>();
            if (terminal != null)
            {
                terminal.groupCredits = 500;
            }
            if (TimeOfDay.Instance != null)
            {
                TimeOfDay.Instance.globalTimeSpeedMultiplier = 0.2f;
                TimeOfDay.Instance.daysUntilDeadline = 1;
            }
        }

        [HarmonyPatch("LoadNewLevel")]
        [HarmonyPrefix]
        public static bool LoadNewLevelPatch(ref SelectableLevel newLevel)
        {
            SpawnableMapObject[] spawnableMapObjects = newLevel.spawnableMapObjects;
            SpawnableMapObject[] array = spawnableMapObjects;
            foreach (SpawnableMapObject val in array)
            {
                if ((UnityEngine.Object)(object)val.prefabToSpawn.GetComponentInChildren<Landmine>() != (UnityEngine.Object)null)
                {
                    landmine = val.prefabToSpawn;
                    val.numberToSpawn = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
                    {
                    new Keyframe(0f, 100f),
                    new Keyframe(1f, 100f)
                    });
                }
                else if ((UnityEngine.Object)(object)val.prefabToSpawn.GetComponentInChildren<Turret>() != (UnityEngine.Object)null)
                {
                    turret = val.prefabToSpawn;
                    val.numberToSpawn = new AnimationCurve((Keyframe[])(object)new Keyframe[2]
                    {
                    new Keyframe(0f, 40f),
                    new Keyframe(1f, 40f)
                    });
                }
            }
            return true;
        }
    }
}
