using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trouble_In_Company_Town.Gamemode.Sabotages
{
    internal class SabotageFactory
    {
        public static Sabotage GetSabotageFromName(string name)
        {
            if(name.Equals(LightsSabotage.SABOTAGE_NAME))
            {
                return new LightsSabotage();
            }
            else if (name.Equals(VOIPSabotage.SABOTAGE_NAME))
            {
                return new VOIPSabotage();
            }
            else if(name.Equals(SpawnWeaponSabotage.SABOTAGE_NAME))
            {
                return new SpawnWeaponSabotage();
            }
            else if (name.Equals(SpawnLandmineSabotage.SABOTAGE_NAME))
            {
                return new SpawnLandmineSabotage();
            }
            return null;
        }
    }
}
