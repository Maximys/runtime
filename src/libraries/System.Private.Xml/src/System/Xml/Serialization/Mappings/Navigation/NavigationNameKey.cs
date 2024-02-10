// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.CodeAnalysis;

namespace System.Xml.Serialization.Mappings.Navigation
{
    internal sealed class NavigationNameKey : IEquatable<NavigationNameKey>
    {
        private readonly string? _ns;
        private readonly string? _name;

        internal NavigationNameKey(string? name, string? ns)
        {
            _name = name;
            _ns = ns;
        }

        public override bool Equals([NotNullWhen(true)] object? other)
        {
            if (other is NavigationNameKey navigationNameKey)
            {
                return Equals(navigationNameKey);
            }
            return false;
        }

        public bool Equals([NotNullWhen(true)] NavigationNameKey? other)
        {
            if (other == null)
            {
                return false;
            }
            return _name == other._name && _ns == other._ns;
        }

        public override int GetHashCode()
        {
            return (_ns == null ? "<null>".GetHashCode() : _ns.GetHashCode()) ^ (_name == null ? 0 : _name.GetHashCode());
        }
    }
}
