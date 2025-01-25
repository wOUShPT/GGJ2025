// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Audio
{
    [AddComponentMenu("QTI/Interactions/Audio/AudioSourceInteraction")]
    public class AudioSourceInteraction : Interaction
    {
        [Tooltip("AudioSource to affect")] public AudioSource audioSource;

        [Tooltip("Action to be performed")] public MusicAction action = MusicAction.Play;

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);

            switch (action)
            {
                case MusicAction.Play:
                    audioSource.Play();
                    break;
                case MusicAction.Pause:
                    audioSource.Pause();
                    break;
                case MusicAction.Resume:
                    audioSource.UnPause();
                    break;
                case MusicAction.Stop:
                    audioSource.Stop();
                    break;
                default:
                    break;
            }

            OnEnd();
        }

        public enum MusicAction
        {
            Play,
            Pause,
            Resume,
            Stop
        }
    }
}