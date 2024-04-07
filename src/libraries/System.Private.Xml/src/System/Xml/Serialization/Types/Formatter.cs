// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Types
{
    internal sealed class Formatter
    {
        public Formatter(string name)
        {
            Name = name;
        }

        internal string Name { get; }
    }
}
