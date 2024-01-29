using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trouble_In_Company_Town.Gamemode.Sabotages
{
    public abstract class TimedSabotage : Sabotage
    {
        public static new string SABOTAGE_NAME { get; private set; } = "Timed Sabotage"; //Implement this properly

        public DateTime TriggeredAt { get; private set; }

        public TimedSabotage()
        {
            this.TriggeredAt = DateTime.Now;
        }

        public TimedSabotage(DateTime startTime)
        {
            this.TriggeredAt = startTime;
        }

        public virtual int GetTimeout()
        {
            return 30;
        }

        public override bool CheckIfExpired()
        {
            DateTime now = DateTime.Now;
            double diff = now.Subtract(TriggeredAt).TotalSeconds;
            return diff >= GetTimeout();
        }
    }
}
