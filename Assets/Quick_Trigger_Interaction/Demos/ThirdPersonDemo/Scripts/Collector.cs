// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.TPDemo
{
    public class Collector : MonoBehaviour
    {
        private int collectedItems = 0;

        public void Collect()
        {
            collectedItems++;
        }

        public int CollectedItems => collectedItems;

        void OnGUI()
        {
            float resMult = (Screen.width * Screen.height) / (1920 * 1080);
            resMult = Mathf.Clamp(resMult, 1, 1.75f);

            GUIStyle labelStyle = GUI.skin.label;
            labelStyle.fontSize = (int)(32 * resMult);

            GUI.Label(new Rect(Screen.width - 170 * resMult, 180 * resMult, 250 * resMult, 45 * resMult),
                "Gems: " + collectedItems);
        }
    }
}