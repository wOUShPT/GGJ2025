// Copyright (c) AstralShift. All rights reserved.

using System;
using System.Collections;
using UnityEngine;

namespace AstralShift.QTI.Helpers
{
    public static class Wait
    {
        /// <summary>
        /// Waits and then executes an action
        /// </summary>
        /// <param name="timeout">Time to wait before executing action (in seconds)</param>
        /// <param name="action">Action to execute</param>
        /// <returns></returns>
        public static IEnumerator SetTimeout(float timeout, Action action)
        {
            yield return new WaitForSeconds(timeout);
            action?.Invoke();
        }

        /// <summary>
        /// Waits and then executes an action
        /// </summary>
        /// <param name="frames">Time to wait before executing action (in seconds)</param>
        /// <param name="action">Action to execute</param>
        /// <returns></returns>
        public static IEnumerator SetFrameTimeout(int frames, Action action)
        {
            int frameCounter = 0;
            while (frameCounter < frames)
            {
                frameCounter++;
                yield return null;
            }

            action?.Invoke();
        }

        /// <summary>
        /// Waits for realtime (unscaledTime) and then executes an action
        /// </summary>
        /// <param name="timeout">Time to wait before executing action</param>
        /// <param name="action">Action to execute</param>
        /// <returns></returns>
        public static IEnumerator SetUnscaledTimeout(float timeout, Action action)
        {
            yield return new WaitForSecondsRealtime(timeout);
            action?.Invoke();
        }
    }
}