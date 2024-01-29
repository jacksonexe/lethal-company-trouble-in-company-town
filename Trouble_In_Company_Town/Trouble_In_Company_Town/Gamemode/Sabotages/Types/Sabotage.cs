using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;

namespace Trouble_In_Company_Town.Gamemode.Sabotages
{
    public abstract class Sabotage
    {
        public static int SABO_UNIQUE_ID = 0;
        public static string SABOTAGE_NAME { get; private set; } = "Sabotage"; //Implement this properly

        public virtual bool IsFireAndForget()
        {
            return false;
        }

        public virtual bool CanHaveMultiple()
        {
            return true;
        }

        public virtual bool CheckIfExpired()
        {
            return false;
        }

        public abstract void StartSabotage();

        public abstract void EndSabotage();

        public virtual string GetSabotageName()
        {
            return SABOTAGE_NAME;
        }

        public string GetUniqueID()
        {
            int id = SABO_UNIQUE_ID++;
            return "_" + id;
        }

        public virtual string GetSabotageActiveText()
        {
            return "Sabotage is active";
        }

        public virtual string GetSabotageStartedText()
        {
            return "Sabotage has begun";
        }
    }
}
