using BepInEx.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode.Sabotages;
using Trouble_In_Company_Town.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Trouble_In_Company_Town.Gamemode
{

    public class TraitorSabotageManager
    {
        private static List<Sabotage> ActiveSabotages;

        public static readonly TraitorSabotageManager Instance = new TraitorSabotageManager();
        internal ManualLogSource mls;
        public static bool VoicesMuted = false; //For client side

        private static ArrayList Landmines = new ArrayList();

        private TraitorSabotageManager() {
            ActiveSabotages = new List<Sabotage>();
            mls = BepInEx.Logging.Logger.CreateLogSource("TraitorSabotageManager");
        }

        public void LightsSaboKeyListener()
        {
            if(!TCTRoundManager.Instance.LocalPlayerIsTraitor() || !TCTRoundManager.Instance.IsRunning) return;
            mls.LogDebug("received request to start a lights sabo");
            Sabotage sabotage = new LightsSabotage();
            if (StartOfRound.Instance.IsHost || StartOfRound.Instance.IsServer)
            {
                TriggerSabo(StartOfRound.Instance.localPlayerController.playerClientId, sabotage.GetSabotageName());
            }
            else
            {
                TCTNetworkHandler.Instance.RequestSabotageServerRpc(StartOfRound.Instance.localPlayerController.playerClientId, sabotage.GetSabotageName());
            }
        }

        public void VOIPSaboKeyListener()
        {
            if(!TCTRoundManager.Instance.LocalPlayerIsTraitor() || !TCTRoundManager.Instance.IsRunning) return;
            mls.LogDebug("received request to start a voip sabo");
            Sabotage sabotage = new VOIPSabotage();
            if (StartOfRound.Instance.IsHost || StartOfRound.Instance.IsServer)
            {
                TriggerSabo(StartOfRound.Instance.localPlayerController.playerClientId, sabotage.GetSabotageName());
            }
            else
            {
                TCTNetworkHandler.Instance.RequestSabotageServerRpc(StartOfRound.Instance.localPlayerController.playerClientId, sabotage.GetSabotageName());
            }
        }

        public void WeaponSaboKeyListener()
        {
            if(!TCTRoundManager.Instance.LocalPlayerIsTraitor() || !TCTRoundManager.Instance.IsRunning) return;
            mls.LogDebug("received request to start a spawn weapons sabo");
            Sabotage sabotage = new SpawnWeaponSabotage();
            if (StartOfRound.Instance.IsHost || StartOfRound.Instance.IsServer)
            {
                TriggerSabo(StartOfRound.Instance.localPlayerController.playerClientId, sabotage.GetSabotageName());
            }
            else
            {
                TCTNetworkHandler.Instance.RequestSabotageServerRpc(StartOfRound.Instance.localPlayerController.playerClientId, sabotage.GetSabotageName());
            }
        }

        public void LandmineSaboKeyListener()
        {
            if (!TCTRoundManager.Instance.LocalPlayerIsTraitor() || !TCTRoundManager.Instance.IsRunning) return;
            mls.LogDebug("received request to start a spawn landmine sabo");
            Sabotage sabotage = new SpawnLandmineSabotage();
            if (StartOfRound.Instance.IsHost || StartOfRound.Instance.IsServer)
            {
                TriggerSabo(StartOfRound.Instance.localPlayerController.playerClientId, sabotage.GetSabotageName());
            }
            else
            {
                TCTNetworkHandler.Instance.RequestSabotageServerRpc(StartOfRound.Instance.localPlayerController.playerClientId, sabotage.GetSabotageName());
            }
        }

        public void TriggerSabo(ulong clientId, string sabo)
        {
            Sabotage type = SabotageFactory.GetSabotageFromName(sabo);
            if (StartOfRound.Instance.IsHost || StartOfRound.Instance.IsServer)
            {
                Sabotage activeSabo = null;
                mls.LogDebug("checking if sabo is active");
                for (int i = 0; i < ActiveSabotages.Count; i++)
                {
                    if (ActiveSabotages[i].GetSabotageName().Equals(type.GetSabotageName()))
                    {
                        if (!ActiveSabotages[i].CheckIfExpired())
                        {
                            activeSabo = ActiveSabotages[i];
                            break;
                        }
                    }
                }

                if (activeSabo == null)
                {
                    ActiveSabotages.Add(type);
                    TCTNetworkHandler.Instance.NotifyOfSabotageServerRpc(type.GetSabotageName());
                }
                else
                {
                    TCTNetworkHandler.Instance.NotifyOfSabotageOnCooldownServerRpc(clientId, type.GetSabotageName());
                }
            }
        }

        public void HandleOnSabotageTypes(string sabo)
        {
            Sabotage types = SabotageFactory.GetSabotageFromName(sabo);
            types.StartSabotage();
        }

        public void HandleOffSabotageTypes(string sabo)
        {
            Sabotage types = SabotageFactory.GetSabotageFromName(sabo);
            types.EndSabotage();
        }

        public void NotifyOfSaboCooldown(string sabo)
        {
            Sabotage types = SabotageFactory.GetSabotageFromName(sabo);
            Utilities.DisplayTips("Sabotage", types.GetSabotageActiveText(), true);
        }

        public void NotifyTraitorsOfSabo(string sabo)
        {
            Sabotage types = SabotageFactory.GetSabotageFromName(sabo);
            Utilities.DisplayTips("Sabotage", types.GetSabotageStartedText(), false);
        }

        public void ResetRound()
        {
            for (int i = 0; i < ActiveSabotages.Count; i++)
            {
                if (ActiveSabotages[i] != null)
                {
                    TCTNetworkHandler.Instance.NotifyOfSabotageEndServerRpc(ActiveSabotages[i].GetSabotageName());
                }
            }
            foreach (GameObject landmine in Landmines)
            {
                GameObject val = landmine;
                UnityEngine.Object.Destroy((UnityEngine.Object)(object)val);
            }
            Landmines.Clear();
            ActiveSabotages.Clear();
            VoicesMuted = false;
        }

        public void RegisterLandmine(GameObject landmine)
        {
            Landmines.Add(landmine);
        }

        public void Update()
        {
            if (ActiveSabotages != null)
            {
                List<Sabotage> toDelete = new List<Sabotage>();
                for (int i = 0; i < ActiveSabotages.Count; i++)
                {
                    if (ActiveSabotages[i] != null && ActiveSabotages[i].CheckIfExpired())
                    {
                        toDelete.Add(ActiveSabotages[i]);
                        TCTNetworkHandler.Instance.NotifyOfSabotageEndServerRpc(ActiveSabotages[i].GetSabotageName());
                    }
                }
                for (int i = 0; i < toDelete.Count; i++) //Cleanup old sabos
                {
                    ActiveSabotages.Remove(toDelete[i]);
                }
            }
            
            if (VoicesMuted)
            {
                StartOfRound.Instance.localPlayerController.voicePlayerState.IsLocallyMuted = true;
                StartOfRound.Instance.localPlayerController.voicePlayerState.Volume = 0;
            }
            else
            {
                StartOfRound.Instance.localPlayerController.voicePlayerState.Volume = 100;
                StartOfRound.Instance.localPlayerController.voicePlayerState.IsLocallyMuted = false;
            }
        }
    }
}
