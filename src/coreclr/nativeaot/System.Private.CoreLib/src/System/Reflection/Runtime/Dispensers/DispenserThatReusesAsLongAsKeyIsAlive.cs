// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System.Reflection.Runtime.Dispensers
{
    internal sealed class DispenserThatReusesAsLongAsKeyIsAlive<K, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)] V> : Dispenser<K, V>
        where K : class, IEquatable<K>
        where V : class
    {
        public DispenserThatReusesAsLongAsKeyIsAlive(Func<K, V> factory)
        {
            _conditionalWeakTable = new ConditionalWeakTable<K, V>();
            _factory = factory;
        }

        public sealed override V GetOrAdd(K key)
        {
            return _conditionalWeakTable.GetOrAdd(key, _factory);
        }

        private readonly Func<K, V> _factory;
        private readonly ConditionalWeakTable<K, V> _conditionalWeakTable;
    }
}
