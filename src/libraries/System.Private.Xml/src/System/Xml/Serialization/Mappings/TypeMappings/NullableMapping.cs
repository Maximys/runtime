// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Mappings.TypeMappings
{
    internal sealed class NullableMapping : TypeMapping
    {
        internal TypeMapping? BaseMapping { get; set; }

        internal override string DefaultElementName
        {
            get { return BaseMapping!.DefaultElementName; }
        }
    }
}
