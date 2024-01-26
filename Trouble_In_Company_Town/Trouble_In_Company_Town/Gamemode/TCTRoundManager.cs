using GameNetcodeStuff;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trouble_In_Company_Town.Gamemode
{
    internal class TCTRoundManager
    {
        private List<Crewmate> players;
        private static int MAX_TRAITORS = 3;
        public TCTRoundManager(StartOfRound playersManager, bool isServer) {
            players = new List<Crewmate>();
            if (isServer)
            {
                populatePlayers(playersManager);
            }
        }

        private PlayerControllerB getMatchingPlayer(SteamId steamId, StartOfRound playersManager)
        {
            foreach(PlayerControllerB player in playersManager.allPlayerScripts)
            {
                if(player.playerSteamId == steamId)
                {
                    return player;
                }
            }
            return null;
        }

        private List<SteamId> getTrators(StartOfRound playersManager)
        {
            List<SteamId> players = new List<SteamId>();
            List<SteamId> trators = new List<SteamId>();
            foreach (SteamId id in GameNetworkManager.Instance.steamIdsInLobby)
            {
                players.Add(id);
            }
            Random rand = new System.Random(playersManager.randomMapSeed);
            int numTrators = 1;
            if (players.Count > 6)
            {
                Math.Max(1, Math.Min((int) Math.Floor((double)(players.Count / 3)), MAX_TRAITORS));
            }
            if(numTrators > players.Count)
            {
                numTrators = 1;
            }
            for (int i = 0; i < numTrators; i++)
            {
                SteamId tratorId = players[rand.Next(0, trators.Count)];
                players.Remove(tratorId);
                trators.Add(tratorId);
            }
            return trators;
        }

        private void populatePlayers(StartOfRound playersManager)
        {
            List<SteamId> trators = this.getTrators(playersManager);
            foreach( SteamId id in GameNetworkManager.Instance.steamIdsInLobby)
            {
                PlayerControllerB controller = getMatchingPlayer(id, playersManager);
                if (controller != null)
                {
                    if (trators.Contains(id))
                    {
                        players.Add(new Traitor(id, controller));
                    }
                    else
                    {
                        players.Add(new Crewmate(id, controller));
                    }
                }
            }
        }
    }
}
