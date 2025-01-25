// Copyright (c) AstralShift. All rights reserved.

using AstralShift.QTI.Helpers.Attributes;
using UnityEngine;

namespace AstralShift.QTI.Interactions.Demos.Platformer2D
{
    public class PlayRandomSFX : MonoBehaviour
    {
        public AudioClip[] sfx;
        public bool useAudioSource;
        [ConditionalHide("useAudioSource")] public AudioSource audioSource;
        [ConditionalHide("useAudioSource")] public bool playOneShot;

        public void Play()
        {
            int randomIndex = Random.Range(0, sfx.Length);

            if (useAudioSource)
            {
                if (playOneShot)
                {
                    audioSource.PlayOneShot(sfx[randomIndex]);
                }
                else
                {
                    audioSource.Stop();
                    audioSource.clip = sfx[randomIndex];
                    audioSource.Play();
                }
            }
            else
            {
                AudioSource.PlayClipAtPoint(sfx[randomIndex], transform.position);
            }
        }

        public void Stop()
        {
            audioSource.Stop();
        }
    }
}