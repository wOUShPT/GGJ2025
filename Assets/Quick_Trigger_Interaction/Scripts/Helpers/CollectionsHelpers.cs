// Copyright (c) AstralShift. All rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;

namespace AstralShift.QTI.Helpers
{
    public static class Collections
    {
        /// <summary>
        /// Shuffles IEnumerables
        /// </summary>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, System.Random generator = null)
        {
            if (generator == null)
            {
                generator = new System.Random();
            }

            var elements = source.ToArray();
            for (var i = elements.Length - 1; i >= 0; i--)
            {
                var swapIndex = generator.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }

        /// <summary>
        /// Compare two arrays of Generic Type and return true if they have the same elements
        /// </summary>
        /// <param name="array1"></param>
        /// <param name="array2"></param>
        /// <typeparam name="T">Type</typeparam>
        /// <returns></returns>
        public static bool HaveSameElements<T>(T[] array1, T[] array2)
        {
            if (array1 == null || array2 == null)
            {
                return array1 == null && array2 == null;
            }

            // Check if the arrays are of the same length
            if (array1.Length != array2.Length)
            {
                return false;
            }

            // Sort both arrays
            var sortedArray1 = array1.OrderBy(item => item).ToArray();
            var sortedArray2 = array2.OrderBy(item => item).ToArray();

            // Compare sorted arrays
            return sortedArray1.SequenceEqual(sortedArray2);
        }
    }
}