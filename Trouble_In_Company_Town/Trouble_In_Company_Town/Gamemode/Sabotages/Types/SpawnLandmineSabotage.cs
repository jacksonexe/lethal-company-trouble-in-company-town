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
    public class SpawnLandmineSabotage : TimedSabotage
    {
        public SpawnLandmineSabotage(DateTime startTime) : base(startTime)
        {
        }

        public SpawnLandmineSabotage() : base() { }
        
        public static new string SABOTAGE_NAME { get; private set; } = "Landmine";

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
            if (TCTRoundManager.Instance.IsPlayerTraitor(player) && !player.isPlayerDead)
            {
                Vector3 playerPos = player.transform.position;
                Vector3 playerDirection = player.transform.forward;
                Quaternion playerRotation = player.transform.rotation;
                float spawnDistance = 2;

                Vector3 spawnPos = playerPos + playerDirection * spawnDistance;

                GameObject val = ((IEnumerable<GameObject>)Resources.FindObjectsOfTypeAll<GameObject>()).FirstOrDefault((Func<GameObject, bool>)((GameObject x) => ((UnityEngine.Object)x).name == "Landmine"));
                val.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                if ((System.Object)(object)val != (UnityEngine.Object)null)
                {
                    TraitorSabotageManager.Instance.RegisterLandmine(UnityEngine.Object.Instantiate<GameObject>(val, spawnPos, val.transform.rotation));
                }
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
                TCTNetworkHandler.Instance.RequestLandmineSpawnServerRpc(StartOfRound.Instance.localPlayerController.playerClientId);
            }
        }

        public override int GetTimeout()
        {
            return 90; //Only one per game
        }

        public override string GetSabotageName()
        {
            return SABOTAGE_NAME;
        }

        public override string GetSabotageActiveText()
        {
            return "Landmine is already deployed";
        }

        public override string GetSabotageStartedText()
        {
            return "Landmine have been deployed";
        }

        public override void EndSabotage()
        {
        }
    }
}
