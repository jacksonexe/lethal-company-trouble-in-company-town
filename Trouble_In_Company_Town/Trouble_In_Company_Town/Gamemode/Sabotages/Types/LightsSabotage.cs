using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using LethalConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BepInEx.Configuration;

namespace Trouble_In_Company_Town.Gamemode.Sabotages
{
    public class LightsSabotage : TimedSabotage
    {
        public static ConfigEntry<int> LightsSabotageCooldown { get; private set; }


        public LightsSabotage(DateTime startTime) : base(startTime)
        {
            SetupConfig();
        }

        public LightsSabotage()
        {
            SetupConfig();
        }

        private void SetupConfig()
        {
            LightsSabotageCooldown = TownBase.Instance.Config.Bind("Sabotages", "Lights", 60, "Number of seconds before the next sabo can be called");
            LethalConfigManager.AddConfigItem(new IntInputFieldConfigItem(LightsSabotageCooldown, new IntInputFieldOptions
            {
                Min = 1,
                Max = 300,
                RequiresRestart = true,
            }));
        }

        public override int GetTimeout()
        {
            return LightsSabotageCooldown.Value;
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
