// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Xml.Serialization
{
    // These classes represent a mapping between classes and a particular XML format.
    // There are two class of mapping information: accessors (such as elements and
    // attributes), and mappings (which specify the type of an accessor).

    internal abstract class Mapping
    {
        private bool _isSoap;

        internal Mapping() { }

        protected Mapping(Mapping mapping)
        {
            _isSoap = mapping._isSoap;
        }

        internal bool IsSoap
        {
            get { return _isSoap; }
            set { _isSoap = value; }
        }
    }

    internal sealed class ConstantMapping : Mapping
    {
        private string? _xmlName;
        private string? _name;
        private long _value;

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

        internal long Value
        {
            get { return _value; }
            set { _value = value; }
        }
    }
}
