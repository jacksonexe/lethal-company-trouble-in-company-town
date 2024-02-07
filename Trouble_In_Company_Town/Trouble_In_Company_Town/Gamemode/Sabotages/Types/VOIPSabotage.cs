using BepInEx.Configuration;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using LethalConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trouble_In_Company_Town.Gamemode.Sabotages
{
    public class VOIPSabotage : TimedSabotage
    {
        public static ConfigEntry<int> VOIPSabotageCooldown { get; private set; }

        public VOIPSabotage(DateTime startTime) : base(startTime)
        {
            SetupConfig();
        }

        public VOIPSabotage()
        {
            SetupConfig();
        }

        public static new string SABOTAGE_NAME { get; private set; } = "Players Voices";

        private void SetupConfig()
        {
            VOIPSabotageCooldown = TownBase.Instance.Config.Bind("Sabotages", "Player Voices", 60, "Number of seconds before the next sabo can be called");
            LethalConfigManager.AddConfigItem(new IntInputFieldConfigItem(VOIPSabotageCooldown, new IntInputFieldOptions
            {
                Min = 1,
                Max = 300,
                RequiresRestart = true,
            }));
        }

        public override int GetTimeout()
        {
            return VOIPSabotageCooldown.Value;
        }

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
