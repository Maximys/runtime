// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Mappings.TypeMappings.PrimitiveMappings
{
    internal sealed class EnumMapping : PrimitiveMapping
    {
        internal bool IsFlags{ get; set; }

        internal ConstantMapping[]? Constants { get; set; }
    }
}
