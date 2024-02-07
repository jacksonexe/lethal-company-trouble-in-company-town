using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trouble_In_Company_Town.UI
{
    public class TCTKillCooldown : MonoBehaviour
    {
        public PlayerControllerB Player;

        private TextMeshPro _timer;

        private void Awake()
        {
            FindComponents();
        }

        private void FindComponents()
        {
            _timer = ((Component)((Component)this).transform.Find("CharsLeft")).GetComponent<TextMeshPro>();
        }

        public void SetTimer(int currentCooldown)
        {
            if (currentCooldown <= 0)
            {
                _timer.text = "Kill: Ready";
            }
            _timer.text = "Kill: " + currentCooldown + "s";
        }

        public void Show(bool show)
        {
            ((Component)this).gameObject.SetActive(show);
        }
    }
}
