using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trouble_In_Company_Town.UI
{
    public class TCTRolesUI : MonoBehaviour
    {
        public PlayerControllerB Player;

        private TextMeshPro _roleText;
        private Image _background;

        private void Awake()
        {
            FindComponents();
        }

        private void FindComponents()
        {
            _roleText = ((Component)((Component)this).transform.Find("CharsLeft")).GetComponent<TextMeshPro>();
            _background = ((Component)((Component)this).transform.Find("CharsLeft")).GetComponent<Image>();
        }

        public void SetRole(string role, Color roleColor)
        {
            _background.color = roleColor;
            _roleText.text = role;
        }

        public void Show(bool show)
        {
            ((Component)this).gameObject.SetActive(show);
        }
    }
}
