// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Reflection;

namespace System.Xml.Serialization.Mappings.Accessors
{
    internal sealed class ChoiceIdentifierAccessor : Accessor
    {
        internal string? MemberName { get; set; }

        internal string[]? MemberIds { get; set; }

        internal MemberInfo? MemberInfo { get; set; }
    }
}
