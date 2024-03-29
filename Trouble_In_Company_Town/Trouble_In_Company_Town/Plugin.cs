﻿using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using LethalConfig;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode;
using Trouble_In_Company_Town.Input;
using Trouble_In_Company_Town.Patches;
using Trouble_In_Company_Town.UI;
using UnityEngine;
using UnityEngine.Assertions;

namespace Trouble_In_Company_Town
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("ainavt.lc.lethalconfig", BepInDependency.DependencyFlags.HardDependency)]
    public class TownBase : BaseUnityPlugin
    {
        private const string modGUID = "jackexe.TroubleInCompanyTown";
        private const string modName = "Trouble In Company Town";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static TownBase Instance;

        public static ManualLogSource mls;
        public GameObject netManagerPrefab;
        public static AssetBundle bundle;

        public static TCTInputManagement InputActionsInstance = new TCTInputManagement();

        void Awake()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
            if (Instance == null)
            {
                Instance = this;
            }
            var dllFolderPath = System.IO.Path.GetDirectoryName(Info.Location);
            var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "tctassets");
            bundle = AssetBundle.LoadFromFile(assetBundleFilePath);
            netManagerPrefab = bundle.LoadAsset<GameObject>("Assets/TCAssets/TCTAssets.prefab");
            netManagerPrefab.AddComponent<TCTNetworkHandler>();

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Town Base has awakened");

            harmony.PatchAll(typeof(TownBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(NetworkObjectManagerPatch));
            harmony.PatchAll(typeof(HudManagerPatches));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(EnemyAIPatch));
            harmony.PatchAll(typeof(WalkieTalkiePatch));

            LethalConfigManager.SetModDescription("Configuration for Trouble in Company Town");
            TCTRoundManager.Init();
        }
    }
}
