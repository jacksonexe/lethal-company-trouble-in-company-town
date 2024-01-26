using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode;
using Trouble_In_Company_Town.Patches;
using UnityEngine;

namespace Trouble_In_Company_Town
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class TownBase : BaseUnityPlugin
    {
        private const string modGUID = "jackexe.TroubleInCompanyTown";
        private const string modName = "Trouble In Company Town";
        private const string modVersion = "1.0.0";

        private readonly Harmony harmony = new Harmony(modGUID);
        public static TownBase Instance;

        internal ManualLogSource mls;
        public GameObject netManagerPrefab;

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
            AssetBundle bundle = AssetBundle.LoadFromFile(assetBundleFilePath);
            netManagerPrefab = bundle.LoadAsset<GameObject>("Assets/TCAssets/TCTAssets.prefab");
            netManagerPrefab.AddComponent<TCTNetworkHandler>();

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Town Base has awakened");

            harmony.PatchAll(typeof(TownBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(NetworkObjectManagerPatch));
        }
    }
}
