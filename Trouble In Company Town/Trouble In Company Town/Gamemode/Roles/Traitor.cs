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
        public Traitor(SteamId id, PlayerControllerB controller) : base(id, controller) { }
    }
}
