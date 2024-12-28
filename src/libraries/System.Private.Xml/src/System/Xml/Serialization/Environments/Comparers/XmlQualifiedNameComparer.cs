// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace System.Xml.Serialization.Environments.Comparers
{
    internal sealed class XmlQualifiedNameComparer : IEqualityComparer<XmlQualifiedName>
    {
        public bool Equals(XmlQualifiedName? x, XmlQualifiedName? y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if ((x is null) || (y is null))
            {
                return false;
            }

            if ((x.Name == y.Name) && (x.Namespace == y.Namespace))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public int GetHashCode([DisallowNull] XmlQualifiedName obj)
        {
            return HashCode.Combine(
                obj.Name,
                obj.Namespace);
        }
    }
}
