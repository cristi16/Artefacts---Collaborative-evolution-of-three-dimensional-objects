using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Reflection.Emit;

namespace UnityStandardAssets.Network
{
    public class LobbyTopPanel : MonoBehaviour
    {
        public bool isInGame = false;

        public GameObject quitPanel;
        public GameObject backButton;

        protected bool isDisplayed = true;

        void Start()
        {
        }


        void Update()
        {
            if (!isInGame)
                return;

            backButton.SetActive(!isInGame);

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ToggleVisibility(!isDisplayed);
            }

        }

        public void ToggleVisibility(bool visible)
        {
            Cursor.visible = visible;
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;

            isDisplayed = visible;
            foreach (Transform t in transform)
            {
                t.gameObject.SetActive(isDisplayed);
            }

            if (isInGame && quitPanel != null)
            {
                quitPanel.SetActive(isDisplayed);
            }
        }
    }
}