// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace System.Xml.Serialization.Mappings.Navigation
{
    internal sealed class NavigationNameTable<T> : INavigationNameScope<T>
    {
        private readonly Dictionary<NavigationNameKey, T> _table = new Dictionary<NavigationNameKey, T>();

        internal void Add(XmlQualifiedName qname, T value)
        {
            Add(qname.Name, qname.Namespace, value);
        }

        internal void Add(string? name, string? ns, T value)
        {
            NavigationNameKey key = new NavigationNameKey(name, ns);
            _table.Add(key, value);
        }

        internal T? this[XmlQualifiedName qname]
        {
            get
            {
                T? obj;
                return _table.TryGetValue(new NavigationNameKey(qname.Name, qname.Namespace), out obj) ? obj : default;
            }
            set
            {
                switch (value)
                {
                    case T valueT:
                        _table[new NavigationNameKey(qname.Name, qname.Namespace)] = valueT;
                        break;
                    case null:
                        _table.Remove(new NavigationNameKey(qname.Name, qname.Namespace));
                        break;
                    default:
                        throw new InvalidCastException();
                }
            }
        }

        public T? this[string? name, string? ns]
        {
            get
            {
                T? obj;
                _table.TryGetValue(new NavigationNameKey(name, ns), out obj);
                return obj;
            }
            set
            {
                switch (value)
                {
                    case T valueT:
                        _table[new NavigationNameKey(name, ns)] = valueT;
                        break;
                    case null:
                        _table.Remove(new NavigationNameKey(name, ns));
                        break;
                    default:
                        throw new InvalidCastException();
                }
            }
        }

        internal ICollection<T> Values
        {
            get { return _table.Values; }
        }

        internal T[] ToArray()
        {
            T[] a = new T[_table.Count];
            _table.Values.CopyTo(a, 0);
            return a;
        }
    }
}
