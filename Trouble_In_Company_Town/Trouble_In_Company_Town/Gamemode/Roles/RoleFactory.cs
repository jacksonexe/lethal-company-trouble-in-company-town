using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trouble_In_Company_Town.Gamemode.Roles
{
    internal class RoleFactory
    {
        public static Crewmate GetRole(string role, ulong clientId, PlayerControllerB player)
        {
            if (role == "Traitor")
            {
                return new Traitor(clientId, player);
            }
            else
            {
                return new Crewmate(clientId, player);
            }
        }
    }
}
