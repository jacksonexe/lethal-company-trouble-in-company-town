using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace Trouble_In_Company_Town.Gamemode.Sabotages
{
    public class SpawnWeaponSabotage : TimedSabotage
    {
        public SpawnWeaponSabotage(DateTime startTime) : base(startTime)
        {
        }

        public SpawnWeaponSabotage() : base() { }
        
        public static new string SABOTAGE_NAME { get; private set; } = "Weapon";

        public static void SpawnItemForClient(ulong clientId)
        {
            PlayerControllerB player = null;
            for(int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == clientId)
                {
                    player = StartOfRound.Instance.allPlayerScripts[i];
                }
            }
            if (TCTRoundManager.Instance.IsPlayerTraitor(player))
            {
                Vector3 spawnPos = player.transform.position;

                if (player.isPlayerDead)
                {
                    spawnPos = player.spectatedPlayerScript.transform.position;
                }
                spawnPos.y += 0.5f;
                GameObject obj = UnityEngine.Object.Instantiate(StartOfRound.Instance.allItemsList.itemsList[52].spawnPrefab, spawnPos, Quaternion.identity);
                obj.GetComponent<GrabbableObject>().fallTime = 5f;

                obj.AddComponent<ScanNodeProperties>().scrapValue = 0;
                obj.GetComponent<GrabbableObject>().SetScrapValue(0);
                obj.GetComponent<NetworkObject>().Spawn();
            }
        }

        public override void StartSabotage()
        {
            if (StartOfRound.Instance.IsHost || StartOfRound.Instance.IsServer)
            {
                if (TCTRoundManager.Instance.LocalPlayerIsTraitor()) //Stop signs for everyone
                {
                    SpawnItemForClient(StartOfRound.Instance.localPlayerController.playerClientId);
                }
            }
            else
            {
                TCTNetworkHandler.Instance.RequestItemSpawnServerRpc(StartOfRound.Instance.localPlayerController.playerClientId);
            }
        }

        public override int GetTimeout()
        {
            return 99999; //Only one per game
        }

        public override string GetSabotageName()
        {
            return SABOTAGE_NAME;
        }

        public override string GetSabotageActiveText()
        {
            return "Weapon is already deployed";
        }

        public override string GetSabotageStartedText()
        {
            return "Weapons have been deployed";
        }

        public override void EndSabotage()
        {
        }
    }
}
