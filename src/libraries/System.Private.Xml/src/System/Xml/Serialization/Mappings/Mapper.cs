// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Mappings
{
    internal sealed class Mapper
    {
        public Type PrimitiveType { get; }

        public Mapper(Type primitiveType)
        {
            PrimitiveType = primitiveType;
        }
    }
}
