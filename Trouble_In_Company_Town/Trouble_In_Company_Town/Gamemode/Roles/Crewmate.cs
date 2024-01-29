using GameNetcodeStuff;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace Trouble_In_Company_Town.Gamemode
{
    public enum Faction
    {
        CREWMATE,
        TRAITOR
    }

    public class Crewmate
    {
        public ulong playerId {  get; protected set; }
        public PlayerControllerB controller { get; protected set; }

        public Faction Faction { get; protected set; }

        public Crewmate(ulong clientId, PlayerControllerB controller)
        {
            this.playerId = clientId;
            this.controller = controller;
            this.Faction = Faction.CREWMATE;
        }

        public virtual string GetRoleName() => "Crewmate";

        public virtual bool GetWarningTipSetting() => false;
        public virtual string GetTextColor() => "008000";

        public virtual string GetRoleGoal() => "collect scrap to win";

        public virtual void NotifyOfRole()
        {
            Utilities.DisplayTips("You are a ", GetRoleName(), GetWarningTipSetting());
            Utilities.AddChatMessage("You are a " + GetRoleName() + " " + GetRoleGoal() + ".", GetTextColor());
        }
        public override bool Equals(object o)
        {
            return (o as Crewmate)?.playerId == this.playerId;
        }
        public override int GetHashCode()
        {
            return playerId.GetHashCode();
        }
    }
}
