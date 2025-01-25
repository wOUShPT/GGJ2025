// Copyright (c) AstralShift. All rights reserved.

using System.Collections.Generic;
using UnityEngine;

namespace AstralShift.Helpers.DynamicEnum
{
    [CreateAssetMenu(fileName = "DynamicEnum", menuName = "ScriptableObjects/DynamicEnum", order = 1)]
    public class DynamicEnum : ScriptableObject
    {
        public List<string> enumValues = new List<string>();
    }
}