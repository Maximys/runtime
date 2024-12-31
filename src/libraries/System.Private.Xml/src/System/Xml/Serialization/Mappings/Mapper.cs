// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Mappings
{
    internal sealed class Mapper
    {
        public object? NullValue { get; }

        public Type PrimitiveType { get; }

        public Func<object?> ReadPrimitiveValue { get; }

        public Mapper(
            object? nullValue,
            Type primitiveType,
            Func<object?> readPrimitiveValue)
        {
            NullValue = nullValue;
            PrimitiveType = primitiveType;
            ReadPrimitiveValue = readPrimitiveValue;
        }
    }
}
