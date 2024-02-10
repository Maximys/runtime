// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System.Xml.Serialization.Mappings.Navigation
{
    internal interface INavigationNameScope
    {
        object? this[string? name, string? ns] { get; set; }
    }
}
