// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Interactors;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Audio
{
    [AddComponentMenu("QTI/Interactions/Audio/AudioPlayOneShotInteraction")]
    public class AudioPlayOneShotInteraction : Interaction
    {
        [Tooltip("USE AUDIO SOURCE: uses supplied audio source to play the clip.\n" +
                 "POSITION2D: Creates a one time use Audio Source to play the audio clip at AudioListener position.\n" +
                 "POSITION3D: Creates a one time use Audio Source to play the audio clip at clip position.")]
        public AudioPlayOneShotInteractionMode mode = AudioPlayOneShotInteractionMode.AudioSource;

        [Tooltip("Audio Clip to be played")] public AudioClip audioClip;
        public float volume = 1.0f;

        [Tooltip(
            "Audio Source to use to play the clip, if no Audio Source is present, one will be created at clip position")]
        public AudioSource audioSource;

        [Tooltip(
            "World space position to play the clip, only used if no Audio Source is provided, otherwise will use Audio Source Position")]
        public Transform clipPosition;

        private AudioListener _audioListener;

        private void Awake()
        {
            if (mode == AudioPlayOneShotInteractionMode.Position2D)
            {
#if UNITY_6000_0_OR_NEWER
                _audioListener = FindFirstObjectByType<AudioListener>();
#else
                _audioListener = FindObjectOfType<AudioListener>();
#endif
                clipPosition = _audioListener.transform;
            }
        }

        public override void Interact(IInteractor interactor)
        {
            base.Interact(interactor);

            switch (mode)
            {
                //Use existing Audio Source
                case AudioPlayOneShotInteractionMode.AudioSource:
                    if (audioSource == null)
                    {
                        Debug.LogError(
                            "No AudioSource found, AudioPlayOneShotInteraction can not play, if you want an Audio Source to be created in runtime use one of the Transient modes instead");
                        break;
                    }

                    audioSource.PlayOneShot(audioClip, volume);
                    break;

                //Create and destroy new Audio Source to play the clip
                case AudioPlayOneShotInteractionMode.Position2D:
                    if (_audioListener == null)
                    {
#if UNITY_6000_0_OR_NEWER
                        _audioListener = FindFirstObjectByType<AudioListener>();
#else
                        _audioListener = FindObjectOfType<AudioListener>();
#endif
                        if (_audioListener == null)
                        {
                            Debug.LogError("No AudioListener found, AudioPlayOneShotInteraction can not play");
                            break;
                        }

                        clipPosition = _audioListener.transform;
                    }

                    AudioSource.PlayClipAtPoint(audioClip, clipPosition.position, volume);
                    break;

                //Create and destroy new Audio Source to play the clip
                case AudioPlayOneShotInteractionMode.Position3D:
                    if (clipPosition == null)
                    {
                        Debug.LogError("No Clip Position found, AudioPlayOneShotInteraction can not play");
                        break;
                    }

                    AudioSource.PlayClipAtPoint(audioClip, clipPosition.position, volume);
                    break;
                default:
                    break;
            }

            OnEnd();
        }

        public enum AudioPlayOneShotInteractionMode
        {
            AudioSource,
            Position2D,
            Position3D
        }
    }
}