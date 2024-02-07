using GameNetcodeStuff;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Trouble_In_Company_Town.Gamemode
{
    public class Traitor : Crewmate
    {
        public DateTime LastKillTime { get; set; }

        public Traitor(ulong id, PlayerControllerB controller) : base(id, controller)
        {
            this.Faction = Faction.TRAITOR;
        }
        public override string GetRoleName() => "Traitor";

        public override bool GetWarningTipSetting() => true;

        public override string GetTextColor() => "FF0000";

        public override string GetRoleGoal() => "kill them all";

        public override Color GetRoleColor()
        {
            return new Color(255, 0, 0, 255);
        }

        public Crewmate GetCremateInRange()
        {
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                Crewmate role = TCTRoundManager.Instance.GetPlayerRole(StartOfRound.Instance.allPlayerScripts[i]);
                if (role != null && role.Faction == Faction.TRAITOR) //Traitors are invulnerable
                {
                    continue;
                }
                Vector3 position = StartOfRound.Instance.allPlayerScripts[i].gameplayCamera.transform.position;
                if (Vector3.Distance(position, this.controller.playerEye.position) < 10 && !Physics.Linecast(this.controller.playerEye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                {
                    Vector3 to = position - this.controller.playerEye.position;
                    if (Vector3.Angle(this.controller.playerEye.forward, to) < 10)
                    {
                        return role;
                    }
                }
            }
            return null;
        }

        public virtual void ExecuteKillingAbility()
        {
            Crewmate visibleCrew = GetCremateInRange();
            if(visibleCrew != null)
            {
                if (!visibleCrew.controller.isPlayerDead)
                {
                    LastKillTime = DateTime.Now;
                    TCTNetworkHandler.Instance.SyncTraitorKillServerRpc(visibleCrew.controller.playerClientId, this.controller.playerClientId, LastKillTime);
                }
            }
        }
    }

}
