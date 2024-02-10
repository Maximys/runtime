// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Xml.Serialization.Mappings
{
    internal sealed class ConstantMapping : Mapping
    {
        private string? _xmlName;
        private string? _name;

        [AllowNull]
        internal string XmlName
        {
            get { return _xmlName ?? string.Empty; }
            set { _xmlName = value; }
        }

        [AllowNull]
        internal string Name
        {
            get { return _name ?? string.Empty; }
            set { _name = value; }
        }

        internal long Value { get; set; }
    }
}
