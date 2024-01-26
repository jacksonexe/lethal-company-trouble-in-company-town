using GameNetcodeStuff;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;

namespace Trouble_In_Company_Town.Gamemode
{
    public enum Faction
    {
        CREWMATE,
        TRAITOR
    }

    public class Crewmate
    {
        public ulong playerId {  get; protected set; }
        public PlayerControllerB controller { get; protected set; }

        public Faction Faction { get; protected set; }

        public Crewmate(ulong clientId, PlayerControllerB controller)
        {
            this.playerId = clientId;
            this.controller = controller;
            this.Faction = Faction.CREWMATE;
        }

        private void AddChatMessage(string chatMessage, string textColor = "FF0000") //Copy of the client side only message
        {
            if (!(HUDManager.Instance.lastChatMessage == chatMessage))
            {
                HUDManager.Instance.lastChatMessage = chatMessage;
                HUDManager.Instance.PingHUDElement(HUDManager.Instance.Chat, 4f);
                if (HUDManager.Instance.ChatMessageHistory.Count >= 4)
                {
                    HUDManager.Instance.chatText.text.Remove(0, HUDManager.Instance.ChatMessageHistory[0].Length);
                    HUDManager.Instance.ChatMessageHistory.Remove(HUDManager.Instance.ChatMessageHistory[0]);
                }
                StringBuilder stringBuilder = new StringBuilder(chatMessage);
                stringBuilder.Replace("[playerNum0]", StartOfRound.Instance.allPlayerScripts[0].playerUsername);
                stringBuilder.Replace("[playerNum1]", StartOfRound.Instance.allPlayerScripts[1].playerUsername);
                stringBuilder.Replace("[playerNum2]", StartOfRound.Instance.allPlayerScripts[2].playerUsername);
                stringBuilder.Replace("[playerNum3]", StartOfRound.Instance.allPlayerScripts[3].playerUsername);
                chatMessage = stringBuilder.ToString();
                string item = "<color=#" + textColor + ">'" + chatMessage + "'</color>";
                HUDManager.Instance.ChatMessageHistory.Add(item);
                HUDManager.Instance.chatText.text = "";
                for (int i = 0; i < HUDManager.Instance.ChatMessageHistory.Count; i++)
                {
                    TextMeshProUGUI textMeshProUGUI = HUDManager.Instance.chatText;
                    textMeshProUGUI.text = textMeshProUGUI.text + "\n" + HUDManager.Instance.ChatMessageHistory[i];
                }
            }
        }

        public virtual string GetRoleName() => "Crewmate";

        public virtual bool GetWarningTipSetting() => false;
        public virtual string GetTextColor() => "008000";

        public virtual string GetRoleGoal() => "collect scrap to win";

        public virtual void NotifyOfRole()
        {
            HUDManager.Instance.DisplayTip("You are a ", GetRoleName(), GetWarningTipSetting());
            AddChatMessage("You are a " + GetRoleName() + " " + GetRoleGoal() + ".", GetTextColor());
        }
        public override bool Equals(object o)
        {
            return (o as Crewmate)?.playerId == this.playerId;
        }
        public override int GetHashCode()
        {
            return playerId.GetHashCode();
        }
    }
}
