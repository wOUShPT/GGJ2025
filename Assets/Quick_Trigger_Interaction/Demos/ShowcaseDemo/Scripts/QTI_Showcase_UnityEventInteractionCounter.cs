// Copyright (c) AstralShift. All rights reserved.

using TMPro;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.Showcase
{
    public class QTI_Showcase_UnityEventInteractionCounter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI[] _text;
        private int _counter;

        private void Awake()
        {
            foreach (var elemement in _text)
            {
                elemement.SetText(_counter.ToString());
            }
        }

        public void IncreaseCounter()
        {
            _counter++;
            foreach (var elemement in _text)
            {
                elemement.SetText(_counter.ToString());
            }
        }
    }
}