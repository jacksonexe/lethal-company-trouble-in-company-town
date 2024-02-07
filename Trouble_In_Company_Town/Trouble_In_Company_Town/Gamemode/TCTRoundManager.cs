using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using LethalConfig.ConfigItems.Options;
using LethalConfig.ConfigItems;
using LethalConfig;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Trouble_In_Company_Town.Gamemode
{
    public class TCTRoundManager
    {
        private static List<Crewmate> players;
        private static readonly int MAX_TRAITORS = 3;
        internal ManualLogSource mls;
        public static TCTRoundManager Instance { get; private set; }
        public static Crewmate LocalPlayersRole { get; private set; }

        public bool IsRunning { get; private set; }
        public bool IsInitializing { get; private set; }
        public bool IsRoundEnding { get; private set; }
        public bool IsRoundOver { get; private set; }
        public static int NumTraitorWins { get; private set; } = 0;
        public static int NumCrewmateWins { get; private set; } = 0;
        public static ConfigEntry<int> NumberOfTraitors { get; private set; }
        public static ConfigEntry<int> TraitorKillCooldown { get; private set; }

        public static List<Crewmate> TraitorsTransmittingOnTraitorChannel { get; private set; } = new List<Crewmate>();

        public TCTRoundManager() {
            players = new List<Crewmate>();
            mls = BepInEx.Logging.Logger.CreateLogSource("TCTNetworkManager");
        }

        public static void Init()
        {
            if(Instance == null)
            {
                Instance = new TCTRoundManager();
                Instance.InitConfig();
            }
        }

        public void StartRound(StartOfRound playersManager)
        {
            if (IsRunning) return;
            this.resetRound();
            IsInitializing = true;
            populatePlayers(playersManager);
            IsInitializing = false;
            IsRunning = true;
        }

        public void StopRound() {  IsRunning = false; }

        private List<ulong> getTrators(StartOfRound playersManager)
        {
            List<ulong> playersList = new List<ulong>();
            List<ulong> trators = new List<ulong>();
            for (int i = 0; i < playersManager.allPlayerScripts.Length; i++)
            {
                PlayerControllerB controller = playersManager.allPlayerScripts[i];
                if (controller.isActiveAndEnabled && (controller.isPlayerControlled || controller.isPlayerDead))
                {
                    mls.LogDebug("Getting player info for " + controller.playerClientId);
                    playersList.Add(controller.playerClientId);
                }
            }
            int numTrators = NumberOfTraitors.Value;
            /*if (playersList.Count > 6)
            {
                Math.Max(1, Math.Min((int) Math.Floor((double)(playersList.Count / 3)), MAX_TRAITORS));
            }*/
            if(numTrators > players.Count || numTrators == 0)
            {
                numTrators = 1;
            }
            for (int i = 1; i <= numTrators; i++)
            {
                System.Random rand = new System.Random(playersManager.randomMapSeed);
                ulong tratorId = playersList[rand.Next(0, playersList.Count)];
                mls.LogDebug("Picked " + tratorId + " as a traitor from between 0 and " + (playersList.Count));
                playersList.Remove(tratorId);
                trators.Add(tratorId);
            }
            return trators;
        }

        private void populatePlayers(StartOfRound playersManager)
        {
            ulong localPlayer = GameNetworkManager.Instance.localPlayerController.playerClientId;
            List<ulong> trators = this.getTrators(playersManager);
            TCTNetworkHandler.Instance.NotifyRoundStartServerRpc(trators.Count);
            for (int i = 0; i < playersManager.allPlayerScripts.Length; i++)
            {
                PlayerControllerB controller = playersManager.allPlayerScripts[i];
                if (controller.isActiveAndEnabled && (controller.isPlayerControlled || controller.isPlayerDead))
                {
                    ulong id = controller.playerClientId;
                    if (controller != null)
                    {
                        Crewmate role;
                        if (trators.Contains(id))
                        {
                            role = new Traitor(id, controller);
                        }
                        else
                        {
                            role = new Crewmate(id, controller);
                        }
                        players.Add(role);
                        if (id == localPlayer)
                        {
                            LocalPlayersRole = role;
                        }
                        mls.LogDebug("Picked " + role.GetRoleName() + " for " + id);
                    }
                }
            }
            for (int i = 0; i < players.Count; i++)
            {
                Crewmate role = players[i];
                TCTNetworkHandler.Instance.sendRoleServerRpc(role.playerId, role.GetRoleName());
            }
        }

        public void registerLocalPlayersRole(Crewmate role)
        {
            mls.LogDebug("Registering Role for local player " + role.GetRoleName());
            if (!players.Contains(role))
            {
                players.Add(role);
            }
            LocalPlayersRole = role;
            LocalPlayersRole.NotifyOfRole();
        }

        public void registerOtherPlayersRole(Crewmate role)
        {
            if (!players.Contains(role))
            {
                players.Add(role);
            }
        }

        public void resetRound()
        {
            players.Clear();
            TraitorSabotageManager.Instance.ResetRound();
            LocalPlayersRole = null;
            IsRoundOver = false;
            TraitorsTransmittingOnTraitorChannel.Clear();
        }

        public void SetRoundOver() { 
            IsRoundOver = true;
            IsRoundEnding = false;
            IsRunning = false;
        }

        public void checkIfRoundOver(StartOfRound playersManager)
        {
            if (IsInitializing || IsRoundEnding || IsRoundOver) return;
            bool isOnlyOneFactionAlive = true;
            Crewmate startRole = null;
            int activeCrew = 0;
            List<string> traitors = new List<string>();
            for (int i = 0; i < playersManager.allPlayerScripts.Length; i++)
            {
                PlayerControllerB controller = playersManager.allPlayerScripts[i];
                if (controller != null && controller.isActiveAndEnabled && (controller.isPlayerControlled || controller.isPlayerDead))
                {
                    activeCrew++;
                    Crewmate foundRole = null;
                    for (int j = 0; j < players.Count; j++)
                    {
                        if (players[j].playerId == controller.playerClientId)
                        {
                            foundRole = players[j];
                            break;
                        }
                    }
                    if (foundRole != null)
                    {
                        if(foundRole.Faction == Faction.TRAITOR)
                        {
                            traitors.Add(controller.playerUsername);
                        }
                        if (startRole == null)
                        {
                            if (foundRole.controller.isPlayerDead)
                            {
                                continue;
                            }
                            startRole = foundRole;
                        }
                        if(startRole.Faction != foundRole.Faction && !foundRole.controller.isPlayerDead)
                        {
                            isOnlyOneFactionAlive = false;
                        }
                    }
                }
            }
            if (isOnlyOneFactionAlive && activeCrew == players.Count && startRole != null) //Make sure players are done loading
            {
                TCTNetworkHandler.Instance.NotifyRoundOverServerRpc(startRole.GetRoleName(), String.Join(", ", traitors.ToArray()), startRole.Faction == Faction.TRAITOR, startRole.Faction);
                if(startRole.Faction == Faction.TRAITOR)
                {
                    TCTRoundManager.NumTraitorWins = TCTRoundManager.NumTraitorWins + 1;
                }
                else
                {
                    TCTRoundManager.NumCrewmateWins = TCTRoundManager.NumCrewmateWins + 1;
                }
                IsRoundEnding = true;
            }
            else
            {
                int scrap = StartOfRound.Instance.GetValueOfAllScrap();
                if (scrap >= 500)
                {
                    TCTNetworkHandler.Instance.NotifyRoundOverServerRpc("Crewmates", String.Join(", ", traitors.ToArray()), false, Faction.CREWMATE);
                    TCTRoundManager.NumCrewmateWins = TCTRoundManager.NumCrewmateWins + 1;
                    IsRoundEnding = true;
                }
            }
        }

        public void HandleShipLeaveMidnight(StartOfRound playersManager)
        {
            List<string> traitors = new List<string>();
            for (int i = 0; i < playersManager.allPlayerScripts.Length; i++)
            {
                Crewmate foundRole = GetPlayerRole(playersManager.allPlayerScripts[i]);
                if (foundRole != null)
                {
                    if (foundRole.Faction == Faction.TRAITOR)
                    {
                        traitors.Add(playersManager.allPlayerScripts[i].playerUsername);

                    }
                }
            }
            TCTNetworkHandler.Instance.NotifyRoundOverServerRpc("Crewmates", String.Join(", ", traitors.ToArray()), false, Faction.CREWMATE);
            TCTRoundManager.NumCrewmateWins = TCTRoundManager.NumCrewmateWins + 1;
            IsRoundEnding = true;
        
    }

        public Crewmate GetPlayerRole(PlayerControllerB player)
        {
            for (int i = 0; i < players.Count; i++)
            {
                mls.LogDebug("Checking player " + players[i].playerId + " " + player.playerClientId);
                if (players[i].playerId == player.playerClientId) { return players[i]; }
            }
            return null;
        }

        internal void UpdateClientScore(Faction faction)
        {
            if (faction == Faction.TRAITOR)
            {
                TCTRoundManager.NumTraitorWins = TCTRoundManager.NumTraitorWins + 1;
            }
            else
            {
                TCTRoundManager.NumCrewmateWins = TCTRoundManager.NumCrewmateWins + 1;
            }
        }

        public bool LocalPlayerIsTraitor()
        {
            return LocalPlayersRole != null && LocalPlayersRole.Faction == Faction.TRAITOR;
        }

        public void JoinTraitorWalkieChannel(bool joinedChannel, Crewmate role = null)
        {
            if (role == null && LocalPlayerIsTraitor())
            {
                role = LocalPlayersRole;
            }
            else if (role == null)
            {
                return;
            }
            if (role.Faction == Faction.TRAITOR)
            {
                if(!joinedChannel && TraitorsTransmittingOnTraitorChannel.Contains(role))
                {
                    TraitorsTransmittingOnTraitorChannel.Remove(role);
                }
                else if (!TraitorsTransmittingOnTraitorChannel.Contains(role)) {
                    TraitorsTransmittingOnTraitorChannel.Add(role);
                }
            }
        }

        public bool IsPlayerTraitor(PlayerControllerB playerHeldBy)
        {
            if(playerHeldBy== null)
            {
                return false;
            }
            Crewmate role = this.GetPlayerRole(playerHeldBy);
            return role != null && role.Faction == Faction.TRAITOR;
        }

        public bool TraitorCanKill(Traitor traitor)
        {
            if (traitor != null && traitor.LastKillTime != null) {
                double diff = DateTime.Now.Subtract(traitor.LastKillTime).TotalSeconds;
                return diff >= TraitorKillCooldown.Value;
            }
            return true;
        }

        internal Crewmate GetPlayerRoleById(ulong playerId)
        {
            PlayerControllerB player = null;
            for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
            {
                if (StartOfRound.Instance.allPlayerScripts[i].playerClientId == playerId)
                {
                    player = StartOfRound.Instance.allPlayerScripts[i];
                }
            }

            return this.GetPlayerRole(player);
        }

        internal void HandleLocalPlayerKill()
        {
            if (LocalPlayersRole != null && this.LocalPlayerIsTraitor())
            {
                TCTNetworkHandler.Instance.InitateKillRequestServerRpc(LocalPlayersRole.playerId);
            }
        }

        internal void HandlePlayerKill(ulong playerId)
        {   
            Crewmate c = GetPlayerRoleById(playerId);
            if (c.Faction == Faction.TRAITOR && TraitorCanKill(c as Traitor))
            {
                (c as Traitor).ExecuteKillingAbility();
            }
        }

        internal void InitConfig()
        {
            NumberOfTraitors = TownBase.Instance.Config.Bind("Traitors", "Number of Traitors", 1, "Number of traitors, if exceeds number of players, 1 is used.");
            LethalConfigManager.AddConfigItem(new IntInputFieldConfigItem(NumberOfTraitors, new IntInputFieldOptions
            {
                Min = 1,
                Max = MAX_TRAITORS,
                RequiresRestart = true,
            }));
            TraitorKillCooldown = TownBase.Instance.Config.Bind("Traitors", "Kill Cooldown", 30, "Kill cooldown of the traitors in seconds.");
            LethalConfigManager.AddConfigItem(new IntInputFieldConfigItem(TraitorKillCooldown, new IntInputFieldOptions
            {
                Min = 1,
                Max = 300,
                RequiresRestart = true,
            }));
        }
    }
}
