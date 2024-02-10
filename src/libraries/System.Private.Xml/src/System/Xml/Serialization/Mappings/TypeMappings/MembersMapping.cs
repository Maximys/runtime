// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Xml.Serialization.Mappings.AccessorMappings;

namespace System.Xml.Serialization.Mappings.TypeMappings
{
    internal sealed class MembersMapping : TypeMapping
    {
        internal MemberMapping[]? Members { get; set; }

        internal MemberMapping? XmlnsMember { get; set; }

        internal bool HasWrapperElement { get; set; } = true;

        internal bool ValidateRpcWrapperElement { get; set; }

        internal bool WriteAccessors { get; set; } = true;
    }
}
