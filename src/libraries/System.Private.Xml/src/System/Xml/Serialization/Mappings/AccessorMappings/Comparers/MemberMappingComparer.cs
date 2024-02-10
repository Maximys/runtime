// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;

namespace System.Xml.Serialization.Mappings.AccessorMappings.Comparers
{
    internal sealed class MemberMappingComparer : IComparer<MemberMapping>
    {
        public int Compare(MemberMapping? m1, MemberMapping? m2)
        {
            Debug.Assert(m1 != null);
            Debug.Assert(m2 != null);
            bool m1Text = m1.IsText;
            if (m1Text)
            {
                if (m2.IsText)
                {
                    return 0;
                }

                return 1;
            }
            else
            {
                if (m2.IsText)
                {
                    return -1;
                }
            }

            if (m1.SequenceId < 0 && m2.SequenceId < 0)
            {
                return 0;
            }

            if (m1.SequenceId < 0)
            {
                return 1;
            }

            if (m2.SequenceId < 0)
            {
                return -1;
            }

            if (m1.SequenceId < m2.SequenceId)
            {
                return -1;
            }

            if (m1.SequenceId > m2.SequenceId)
            {
                return 1;
            }

            return 0;
        }
    }
}
