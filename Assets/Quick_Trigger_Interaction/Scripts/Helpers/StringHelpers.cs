// Copyright (c) AstralShift. All rights reserved.

using System;

namespace AstralShift.QTI.Helpers
{
    public static class String
    {
        /// <summary>
        /// Replaces strings first occurence of a certain Find value by the given Replace value
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Find"></param>
        /// <param name="Replace"></param>
        /// <returns></returns>
        public static string ReplaceFirstOccurrence(string Source, string Find, string Replace)
        {
            int Place = Source.IndexOf(Find);
            string result = Source.Remove(Place, Find.Length).Insert(Place, Replace);
            return result;
        }

        public static int StringToInt(string value)
        {
            Int32.TryParse(value, out int result);
            return result;
        }

        public static string Reverse(this string value)
        {
            char[] charArray = value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }
    }
}