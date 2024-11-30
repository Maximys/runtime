// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection.Emit;

namespace System.Xml.Serialization.Generations.CodeGenerations.Scopes
{
    internal sealed class LocalScope
    {
        public readonly LocalScope? parent;
        private readonly Dictionary<string, LocalBuilder> _locals;

        // Root scope
        public LocalScope()
        {
            _locals = new Dictionary<string, LocalBuilder>();
        }

        public LocalScope(LocalScope? parent) : this()
        {
            this.parent = parent;
        }

        public bool ContainsKey(string key)
        {
            return _locals.ContainsKey(key) || parent != null && parent.ContainsKey(key);
        }

        public bool TryGetValue(string key, [NotNullWhen(true)] out LocalBuilder? value)
        {
            if (_locals.TryGetValue(key, out value))
            {
                return true;
            }
            else if (parent != null)
            {
                return parent.TryGetValue(key, out value);
            }
            else
            {
                value = null;
                return false;
            }
        }

        [DisallowNull]
        public LocalBuilder? this[string key]
        {
            get
            {
                LocalBuilder? value;
                TryGetValue(key, out value);
                return value;
            }
            set
            {
                _locals[key] = value;
            }
        }

        public void AddToFreeLocals(Dictionary<(Type, string), Queue<LocalBuilder>> freeLocals)
        {
            foreach (var item in _locals)
            {
                (Type, string) key = (item.Value.LocalType, item.Key);
                Queue<LocalBuilder>? freeLocalQueue;
                if (freeLocals.TryGetValue(key, out freeLocalQueue))
                {
                    // Add to end of the queue so that it will be re-used in
                    // FIFO manner
                    freeLocalQueue.Enqueue(item.Value);
                }
                else
                {
                    freeLocalQueue = new Queue<LocalBuilder>();
                    freeLocalQueue.Enqueue(item.Value);
                    freeLocals.Add(key, freeLocalQueue);
                }
            }
        }
    }
}
