// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Generations.Building
{
    internal sealed class ArgBuilder
    {
        internal string Name;
        internal int Index;
        internal Type ArgType;
        internal ArgBuilder(string name, int index, Type argType)
        {
            Name = name;
            Index = index;
            ArgType = argType;
        }
    }
}
