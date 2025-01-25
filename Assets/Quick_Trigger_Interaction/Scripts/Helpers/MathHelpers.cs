// Copyright (c) AstralShift. All rights reserved.

using UnityEngine;

namespace AstralShift.QTI.Helpers
{
    public static class Math
    {
        /// <summary>
        /// Counts the number of trailing zeros of a binary value
        /// </summary>
        /// <param name="n">The number the count the zeros from</param>
        /// <returns></returns>
        public static int NumberOfTraillingZeros(int n)
        {
            int cnt;
            for (cnt = 0; (n & 1) != 1; cnt++, n >>= 1)
            {
            }

            return cnt;
        }

        public static int ToDigit(this int value, int index)
        {
            return (int)(value / Mathf.Pow(10, index)) % 10;
        }

        public static int DigitCount(this int value)
        {
            float result = value == 0 ? 1 : Mathf.Floor(Mathf.Log10(Mathf.Abs(value)) + 1);
            return (int)result;
        }

        /// <summary>
        /// Remaps a given value from a set range to a different range
        /// </summary>
        /// <param name="from">value</param>
        /// <param name="fromMin">from range min value</param>
        /// <param name="fromMax">from range max value</param>
        /// <param name="toMin">to range min value</param>
        /// <param name="toMax">to range max value</param>
        /// <returns></returns>
        public static float Remap(this float from, float fromMin, float fromMax, float toMin, float toMax)
        {
            var fromAbs = from - fromMin;
            var fromMaxAbs = fromMax - fromMin;

            var normal = fromAbs / fromMaxAbs;

            var toMaxAbs = toMax - toMin;
            var toAbs = toMaxAbs * normal;

            var to = toAbs + toMin;

            return to;
        }

        public static Vector3 ZeroZVector3(Vector3 source)
        {
            return new Vector3(source.x, source.y, 0f);
        }

        public static Vector2 To2D(this Vector3 vec)
        {
            return new Vector2(vec.x, vec.z);
        }

        public static Vector2 GetDirectionAtoB(Vector2 positionA, Vector2 positionB, bool normalize = false)
        {
            if (normalize)
            {
                return (positionB - positionA).normalized;
            }

            return positionB - positionA;
        }

        public static Vector3 GetDirectionAtoB(Vector3 positionA, Vector3 positionB, bool normalize = false)
        {
            if (normalize)
            {
                return (positionB - positionA).normalized;
            }

            return positionB - positionA;
        }

        public static Vector2 Rotate(this Vector2 direction, float delta)
        {
            return new Vector2(
                direction.x * Mathf.Cos(delta) - direction.y * Mathf.Sin(delta),
                direction.x * Mathf.Sin(delta) + direction.y * Mathf.Cos(delta)
            );
        }
    }
}