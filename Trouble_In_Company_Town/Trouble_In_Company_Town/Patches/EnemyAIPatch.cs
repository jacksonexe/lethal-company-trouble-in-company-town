using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.InputSystem.XR;
using UnityEngine;
using Trouble_In_Company_Town.Gamemode;

namespace Trouble_In_Company_Town.Patches
{
    [HarmonyPatch(typeof(EnemyAI))]
    internal class EnemyAIPatch
    {
        [HarmonyPatch("CheckLineOfSightForPlayer")]
        [HarmonyPostfix]
        public static PlayerControllerB CheckLineOfSightForPlayerPatch(PlayerControllerB __ret, float width, int range, int proximityAwareness, EnemyAI __instance)
        {
            if (__instance.isOutside && !__instance.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
            {
                range = Mathf.Clamp(range, 0, 30);
            }
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                Crewmate role = TCTRoundManager.Instance.GetPlayerRole(StartOfRound.Instance.allPlayerScripts[i]);
                if ( role != null && role.Faction == Faction.TRAITOR) //Traitors are invulnerable
                {
                    continue;
                }
                Vector3 position = StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position;
                if (Vector3.Distance(position, __instance.eye.position) < (float)range && !Physics.Linecast(__instance.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                {
                    Vector3 to = position - __instance.eye.position;
                    if (Vector3.Angle(__instance.eye.forward, to) < width || (proximityAwareness != -1 && Vector3.Distance(__instance.eye.position, position) < (float)proximityAwareness))
                    {
                        return StartOfRound.Instance.allPlayerScripts[i];
                    }
                }
            }
            return null;
        }

        [HarmonyPatch("CheckLineOfSightForClosestPlayer")]
        [HarmonyPostfix]
        public static PlayerControllerB CheckLineOfSightForClosestPlayerPatch(PlayerControllerB __ret, float width, int range, int proximityAwareness, float bufferDistance, EnemyAI __instance)
        {
            if (__instance.isOutside && !__instance.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
            {
                range = Mathf.Clamp(range, 0, 30);
            }
            float num = 1000f;
            float num2 = 1000f;
            int num3 = -1;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                Crewmate role = TCTRoundManager.Instance.GetPlayerRole(StartOfRound.Instance.allPlayerScripts[i]);
                if (role != null && role.Faction == Faction.TRAITOR) //Traitors are invulnerable
                {
                    continue;
                }
                Vector3 position = StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position;
                if (!Physics.Linecast(__instance.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                {
                    Vector3 to = position - __instance.eye.position;
                    num = Vector3.Distance(__instance.eye.position, position);
                    if ((Vector3.Angle(__instance.eye.forward, to) < width || (proximityAwareness != -1 && num < (float)proximityAwareness)) && num < num2)
                    {
                        num2 = num;
                        num3 = i;
                    }
                }
            }
            if (__instance.targetPlayer != null && num3 != -1 && __instance.targetPlayer != StartOfRound.Instance.allPlayerScripts[num3] && bufferDistance > 0f && Mathf.Abs(num2 - Vector3.Distance(__instance.transform.position, __instance.targetPlayer.transform.position)) < bufferDistance)
            {
                return null;
            }
            if (num3 < 0)
            {
                return null;
            }
            __instance.mostOptimalDistance = num2;
            return StartOfRound.Instance.allPlayerScripts[num3];
        }

        [HarmonyPatch("GetAllPlayersInLineOfSight")]
        [HarmonyPostfix]
        public static PlayerControllerB[] GetAllPlayersInLineOfSightPatch(PlayerControllerB[]__ret, float width, int range, Transform eyeObject, float proximityCheck, int layerMask, EnemyAI __instance)
        {
            if (layerMask == -1)
            {
                layerMask = StartOfRound.Instance.collidersAndRoomMaskAndDefault;
            }
            if (eyeObject == null)
            {
                eyeObject = __instance.eye;
            }
            if (__instance.isOutside && !__instance.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
            {
                range = Mathf.Clamp(range, 0, 30);
            }
            List<PlayerControllerB> list = new List<PlayerControllerB>(4);
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (!__instance.PlayerIsTargetable(StartOfRound.Instance.allPlayerScripts[i]))
                {
                    continue;
                }
                Crewmate role = TCTRoundManager.Instance.GetPlayerRole(StartOfRound.Instance.allPlayerScripts[i]);
                if (role != null && role.Faction == Faction.TRAITOR) //Traitors are invulnerable
                {
                    continue;
                }
                Vector3 position = StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position;
                if (Vector3.Distance(__instance.eye.position, position) < (float)range && !Physics.Linecast(eyeObject.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                {
                    Vector3 to = position - eyeObject.position;
                    if (Vector3.Angle(eyeObject.forward, to) < width || Vector3.Distance(__instance.transform.position, StartOfRound.Instance.allPlayerScripts[i].transform.position) < proximityCheck)
                    {
                        list.Add(StartOfRound.Instance.allPlayerScripts[i]);
                    }
                }
            }
            if (list.Count == 4)
            {
                return StartOfRound.Instance.allPlayerScripts;
            }
            if (list.Count > 0)
            {
                return list.ToArray();
            }
            return null;
        }

        [HarmonyPatch("PlayerIsTargetable")]
        [HarmonyPostfix]
        public static bool PlayerIsTargetablePatch(bool __ret, PlayerControllerB playerScript, bool cannotBeInShip, bool overrideInsideFactoryCheck, EnemyAI __instance)
        {
            if (cannotBeInShip && playerScript.isInHangarShipRoom)
            {
                return false;
            }
            Crewmate role = TCTRoundManager.Instance.GetPlayerRole(playerScript);
            if (role != null && role.Faction == Faction.TRAITOR) //Traitors are invulnerable
            {
                return false;
            }
            if (playerScript.isPlayerControlled && !playerScript.isPlayerDead && playerScript.inAnimationWithEnemy == null && (overrideInsideFactoryCheck || playerScript.isInsideFactory != __instance.isOutside) && playerScript.sinkingValue < 0.73f)
            {
                if (__instance.isOutside && StartOfRound.Instance.hangarDoorsClosed)
                {
                    return playerScript.isInHangarShipRoom == __instance.isInsidePlayerShip;
                }
                return true;
            }
            return false;
        }
    }
}
