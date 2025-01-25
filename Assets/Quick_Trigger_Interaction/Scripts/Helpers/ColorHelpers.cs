// Copyright (c) AstralShift. All rights reserved.

namespace AstralShift.QTI.Helpers
{
    public static class ColorHelpers
    {
        public static string ToHexString(this UnityEngine.Color c) => $"#{(int)c.r:X2}{(int)c.g:X2}{(int)c.b:X2}";
        public static string ToRgbString(this UnityEngine.Color c) => $"RGB({c.r}, {c.g}, {c.b})";
    }
}