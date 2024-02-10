// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Xml.Serialization.Mappings.Accessors.Comparers
{
    internal sealed class AccessorComparer : IComparer<ElementAccessor>
    {
        public int Compare(ElementAccessor? a1, ElementAccessor? a2)
        {
            if (a1 == a2)
            {
                return 0;
            }

            Debug.Assert(a1 != null);
            Debug.Assert(a2 != null);
            int w1 = a1.Mapping!.TypeDesc!.Weight;
            int w2 = a2.Mapping!.TypeDesc!.Weight;

            if (w1 == w2)
            {
                return 0;
            }

            if (w1 < w2)
            {
                return 1;
            }

            return -1;
        }
    }
}
