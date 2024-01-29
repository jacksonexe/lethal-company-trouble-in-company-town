using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trouble_In_Company_Town.Gamemode.Sabotages
{
    public class VOIPSabotage : TimedSabotage
    {
        public VOIPSabotage(DateTime startTime) : base(startTime)
        {
        }

        public VOIPSabotage()
        {
        }

        public static new string SABOTAGE_NAME { get; private set; } = "Players Voices";

        public override void StartSabotage()
        {
            if (!TCTRoundManager.Instance.LocalPlayerIsTraitor())
            {
                TraitorSabotageManager.VoicesMuted = true;
            }
            else
            {
                TraitorSabotageManager.VoicesMuted = false;
            }
        }
        public override string GetSabotageName()
        {
            return SABOTAGE_NAME;
        }

        public override string GetSabotageActiveText()
        {
            return "Players voices have already been silenced";
        }

        public override string GetSabotageStartedText()
        {
            return "Players voices have been silenced";
        }

        public override void EndSabotage()
        {
            TraitorSabotageManager.VoicesMuted = false;
        }
    }
}
