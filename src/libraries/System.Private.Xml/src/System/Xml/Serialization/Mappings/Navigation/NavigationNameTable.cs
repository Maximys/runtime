// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;
using System.Collections.Generic;

namespace System.Xml.Serialization.Mappings.Navigation
{
    internal sealed class NavigationNameTable : INavigationNameScope
    {
        private readonly Dictionary<NavigationNameKey, object?> _table = new Dictionary<NavigationNameKey, object?>();

        internal void Add(XmlQualifiedName qname, object value)
        {
            Add(qname.Name, qname.Namespace, value);
        }

        internal void Add(string? name, string? ns, object value)
        {
            NavigationNameKey key = new NavigationNameKey(name, ns);
            _table.Add(key, value);
        }

        internal object? this[XmlQualifiedName qname]
        {
            get
            {
                object? obj;
                return _table.TryGetValue(new NavigationNameKey(qname.Name, qname.Namespace), out obj) ? obj : null;
            }
            set
            {
                _table[new NavigationNameKey(qname.Name, qname.Namespace)] = value;
            }
        }

        internal object? this[string? name, string? ns]
        {
            get
            {
                object? obj;
                return _table.TryGetValue(new NavigationNameKey(name, ns), out obj) ? obj : null;
            }
            set
            {
                _table[new NavigationNameKey(name, ns)] = value;
            }
        }

        object? INavigationNameScope.this[string? name, string? ns]
        {
            get
            {
                object? obj;
                _table.TryGetValue(new NavigationNameKey(name, ns), out obj);
                return obj;
            }
            set
            {
                _table[new NavigationNameKey(name, ns)] = value;
            }
        }

        internal ICollection Values
        {
            get { return _table.Values; }
        }

        [RequiresDynamicCode(XmlSerializer.AotSerializationWarning)]
        internal Array ToArray(Type type)
        {
            Array a = Array.CreateInstance(type, _table.Count);
            ((ICollection)_table.Values).CopyTo(a, 0);
            return a;
        }
    }
}
