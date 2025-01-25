// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;
using UnityEngine.SceneManagement;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    public class QTIDemoManager : MonoBehaviour
    {
        public bool enableMouseCursor = false;
        private const string showcase = "ShowcaseDemo";
        private const string thirdPerson = "ThirdPersonDemo";
        private const string firstPerson = "FirstPersonPuzzleDemo";
        private const string platformer2D = "Platformer2D";

        private void Awake()
        {
            if (enableMouseCursor)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        private void Update()
        {
            if (Input.GetButtonDown("Cancel") || Input.GetKeyDown(KeyCode.R))
            {
                RestartScene();
            }
#if QTI_PROJECT
            else if (Input.GetKey(KeyCode.Alpha1))
            {
                SceneManager.LoadScene(0);
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                SceneManager.LoadScene(1);
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                SceneManager.LoadScene(2);
            }
            else if (Input.GetKey(KeyCode.Alpha4))
            {
                SceneManager.LoadScene(3);
            }
#endif
        }
#if QTI_PROJECT
        private void OnGUI()
        {
            float resMult = (Screen.width * Screen.height) / (1920 * 1080);
            resMult = Mathf.Clamp(resMult, 1, 1.75f);

            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.fontSize = (int)(12 * resMult);


            GUI.Box(new Rect(30 * resMult, 10 * resMult, 250 * resMult, 180 * resMult), "");
            GUI.Label(new Rect(45 * resMult, 30 * resMult, 250 * resMult, 30 * resMult), "Shortcuts" + showcase,
                labelStyle);
            GUI.Label(new Rect(45 * resMult, 60 * resMult, 250 * resMult, 30 * resMult), "1 - " + showcase, labelStyle);
            GUI.Label(new Rect(45 * resMult, 90 * resMult, 250 * resMult, 30 * resMult), "2 - " + thirdPerson,
                labelStyle);
            GUI.Label(new Rect(45 * resMult, 120 * resMult, 250 * resMult, 30 * resMult), "3 - " + firstPerson,
                labelStyle);
            GUI.Label(new Rect(45 * resMult, 150 * resMult, 250 * resMult, 30 * resMult), "4 - " + platformer2D,
                labelStyle);
        }
#endif

        public void RestartScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}