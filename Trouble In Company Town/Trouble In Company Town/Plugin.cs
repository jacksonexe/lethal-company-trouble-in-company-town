using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private static TownBase Instance;

        internal ManualLogSource mls;
        public static AssetBundle MainAssetBundle;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            var dllFolderPath = System.IO.Path.GetDirectoryName(Info.Location);
            var assetBundleFilePath = System.IO.Path.Combine(dllFolderPath, "TCTAssets");
            MainAssetBundle = AssetBundle.LoadFromFile(assetBundleFilePath);

            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);

            mls.LogInfo("Town Base has awakened");

            harmony.PatchAll(typeof(TownBase));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(NetworkObjectManagerPatch));
        }
    }
}
