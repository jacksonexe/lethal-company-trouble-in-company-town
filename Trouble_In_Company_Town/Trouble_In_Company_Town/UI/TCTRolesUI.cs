using GameNetcodeStuff;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Trouble_In_Company_Town.UI
{
    public class TCTRolesUI : MonoBehaviour
    {
        public PlayerControllerB Player;

        private TextMeshProUGUI _roleText;

        public TCTRolesUI(HUDManager hudManager)
        {
            this.FindComponents(hudManager);
        }

        // Based on https://github.com/Treyo1928/TreysHealthText-Lethal-Company-Mod
        private void FindComponents(HUDManager hudManager)
        {
            GameObject topLeftCorner = GameObject.Find("Systems/UI/Canvas/IngamePlayerHUD/TopLeftCorner");

            GameObject textObj = new GameObject("RoleUIText");
            textObj.transform.SetParent(topLeftCorner.transform, false); // Parent to 'TopLeftCorner'

            _roleText = textObj.AddComponent<TextMeshProUGUI>();

            if (hudManager.weightCounter != null)
            {
                TextMeshProUGUI weightText = hudManager.weightCounter;
                _roleText.font = weightText.font;
                _roleText.fontSize = weightText.fontSize;
                _roleText.color = Color.black;
                _roleText.alignment = TextAlignmentOptions.Center;
                _roleText.enableAutoSizing = weightText.enableAutoSizing;
                _roleText.fontSizeMin = weightText.fontSizeMin;
                _roleText.fontSizeMax = weightText.fontSizeMax;

                if (weightText.fontMaterial != null)
                {
                    _roleText.fontSharedMaterial = new Material(weightText.fontMaterial);
                }

                if (weightText.transform.parent != null)
                {
                    RectTransform weightCounterParentRect = weightText.transform.parent.GetComponent<RectTransform>();
                    if (weightCounterParentRect != null)
                    {
                        RectTransform roleTextRect = _roleText.GetComponent<RectTransform>();
                        roleTextRect.localRotation = weightCounterParentRect.localRotation;
                    }
                }
            }
            else
            {
                _roleText.fontSize = 24;
                _roleText.color = Color.black;
                _roleText.alignment = TextAlignmentOptions.Center;
            }

            RectTransform rectTransform = textObj.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);

            int XOffset = 0;
            int YOffset = -90;
            rectTransform.anchoredPosition = new Vector2(-53 + XOffset, -95 + YOffset);
        }

        public void SetRole(string role, Color roleColor)
        {
            _roleText.text = role;
            _roleText.color = roleColor;
        }
    }
}
