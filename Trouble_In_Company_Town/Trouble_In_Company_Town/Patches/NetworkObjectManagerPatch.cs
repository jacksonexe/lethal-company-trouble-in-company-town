using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode;
using Unity.Netcode;
using UnityEngine;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch]
    public class NetworkObjectManagerPatch
    {
        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init(GameNetworkManager __instance)
        {
            __instance.GetComponent<NetworkManager>().AddNetworkPrefab(TownBase.Instance.netManagerPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = UnityEngine.Object.Instantiate(TownBase.Instance.netManagerPrefab, Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
        }

    }
}
