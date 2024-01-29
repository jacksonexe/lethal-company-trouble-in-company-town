using BepInEx.Logging;
using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trouble_In_Company_Town.Gamemode;
using UnityEngine.InputSystem;

namespace Trouble_In_Company_Town.Input
{
    internal class TraitorInputManagement : LcInputActions
    {
        [InputAction("<Keyboard>/k", Name = "Lights Sabo")]
        public InputAction LightsSaboKey { get; set; }

        [InputAction("<Keyboard>/j", Name = "No Talking Sabo")]
        public InputAction VOIPSabo { get; set; }
        internal ManualLogSource mls = BepInEx.Logging.Logger.CreateLogSource("TraitorInputManagement");

        [InputAction("<Keyboard>/v", Name = "Spawn Weapon Sabo")]
        public InputAction WeaponSabo { get; set; }

        public TraitorInputManagement()
        {
            LightsSaboKey.performed += LightsSaboKeyListener;
            VOIPSabo.performed += VOIPSaboKeyListener;
            WeaponSabo.performed += WeaponSaboKeyListener;
        }

        public void LightsSaboKeyListener(InputAction.CallbackContext saboKey)
        {
            mls.LogDebug("Checking if the sabo key is pressed" + saboKey.performed);
            if (!saboKey.performed) return;
            TraitorSabotageManager.Instance.LightsSaboKeyListener();
        }

        public void VOIPSaboKeyListener(InputAction.CallbackContext saboKey)
        {
            mls.LogDebug("Checking if the sabo key is pressed" + saboKey.performed);
            if (!saboKey.performed) return;
            TraitorSabotageManager.Instance.VOIPSaboKeyListener();
        }

        public void WeaponSaboKeyListener(InputAction.CallbackContext saboKey)
        {
            mls.LogDebug("Checking if the sabo key is pressed" + saboKey.performed);
            if (!saboKey.performed) return;
            TraitorSabotageManager.Instance.WeaponSaboKeyListener();
        }
    }
}
