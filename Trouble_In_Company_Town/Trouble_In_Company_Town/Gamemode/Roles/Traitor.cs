using GameNetcodeStuff;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trouble_In_Company_Town.Gamemode
{
    public class Traitor : Crewmate
    {
        public Traitor(ulong id, PlayerControllerB controller) : base(id, controller) {
            this.Faction = Faction.TRAITOR;
        }
        public override string GetRoleName() => "Traitor";

        public override bool GetWarningTipSetting() => true;

        public override string GetTextColor() => "FF0000";

        public override string GetRoleGoal() => "kill them all";
    }

}
