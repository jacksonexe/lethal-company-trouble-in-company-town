using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trouble_In_Company_Town.Gamemode.Sabotages
{
    public class LightsSabotage : TimedSabotage
    {
        public LightsSabotage(DateTime startTime) : base(startTime)
        {
        }

        public LightsSabotage()
        {
        }

        public static new string SABOTAGE_NAME { get; private set; } = "Lights";
        public override void StartSabotage()
        {
            RoundManager.Instance.TurnOnAllLights(false);
        }

        public override string GetSabotageName()
        {
            return SABOTAGE_NAME;
        }

        public override string GetSabotageActiveText()
        {
            return "Lights are already active";
        }

        public override string GetSabotageStartedText()
        {
            return "Lights have been sabotaged";
        }

        public override void EndSabotage()
        {
            RoundManager.Instance.TurnOnAllLights(true);
        }
    }
}
