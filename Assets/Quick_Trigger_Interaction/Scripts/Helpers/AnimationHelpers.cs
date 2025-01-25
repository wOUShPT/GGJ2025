// Copyright (c) AstralShift. All rights reserved.

using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor.Animations;

#endif

namespace AstralShift.QTI.Helpers
{
    public static class Animation
    {
#if UNITY_EDITOR

        public static List<string> GetAnimatorStates(AnimatorController controller)
        {
            AnimatorControllerLayer[] allLayer = controller.layers;
            List<string> stateNames = new List<string>();

            for (int i = 0; i < allLayer.Length; i++)
            {
                ChildAnimatorState[] states = allLayer[i].stateMachine.states;

                for (int j = 0; j < states.Length; j++)
                {
                    stateNames.Add(states[j].state.name);
                }
            }

            return stateNames;
        }

        public static List<string> GetAnimatorStates(AnimatorController controller, int layer)
        {
            AnimatorControllerLayer[] allLayer = controller.layers;
            List<string> stateNames = new List<string>();
            ChildAnimatorState[] states = allLayer[layer].stateMachine.states;

            for (int i = 0; i < states.Length; i++)
            {
                stateNames.Add(states[i].state.name);
            }

            return stateNames;
        }

        public static List<string> GetAnimatorLayers(AnimatorController controller)
        {
            AnimatorControllerLayer[] allLayer = controller.layers;
            List<string> layerNames = new List<string>();

            for (int i = 0; i < allLayer.Length; i++)
            {
                layerNames.Add(allLayer[i].name);
            }

            return layerNames;
        }

#endif
    }
}